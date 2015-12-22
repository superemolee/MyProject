// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using StagPoint.Core;

using UnityEngine;

namespace StagPoint.Planning
{

#if UNITY_EDITOR
	using UnityEditor;
#endif

	public partial class OperatorNode : GraphNodeBase
	{

		#region Public fields

		/// <summary>
		/// Indicates the method that will be called on the agent when this operation 
		/// is invoked.
		/// </summary>
		public MethodInfo Method;

		/// <summary>
		/// Holds the parameter values that will be passed to the method when this operation
		/// is invoked
		/// </summary>
		public List<object> Arguments = new List<object>();

		/// <summary>
		/// When set to TRUE, the Unity Editor will pause immediately before this Task is executed
		/// </summary>
		public bool PauseOnRun = false;

		#endregion 

		#region Private runtime variables

		/// <summary>
		/// Holds the actual arguments array that will be passed to the target function.
		/// This array will be re-used in order to prevent frequent memory allocations.
		/// </summary>
		private object[] runtimeArguments;

		/// <summary>
		/// Holds the ParameterInfo instance for each of the method's defined parameters.
		/// This array is cached to prevent frequent memory allocations when performing 
		/// type conversion.
		/// </summary>
		private ParameterInfo[] definedParameters;

		/// <summary>
		/// Holds script evaluators for function arguments whose values are being 
		/// supplied by an evaluated script expressions.
		/// </summary>
		private Dictionary<ParameterInfo, ExpressionEvaluator> argumentEvaluators = null;

		#endregion

		#region Base class overrides 

		public override void Validate()
		{

			if( Method == null )
				throw new InvalidOperationException( string.Format( "Missing method in node '{0}'", this.Name ) );

			var validReturnType =
				typeof( TaskStatus ).IsAssignableFrom( Method.ReturnType ) ||
				typeof( IEnumerator ).IsAssignableFrom( Method.ReturnType );

			if( !validReturnType )
				throw new InvalidOperationException( string.Format( "Invalid return type specified for method '{0}'", this.Method ) );

		}

		public override bool CanAttach( GraphNodeBase other )
		{
			return false;
		}

		public override bool CanAttach<T>()
		{
			return false;
		}

		public override string ToString()
		{

			if( Method != null )
				return Method.ToString();

			return base.ToString();

		}

		#endregion 

		#region Public methods 

		internal IEnumerator BuildAction( object agent, Blackboard blackboard )
		{

			if( agent == null )
				throw new ArgumentNullException( "agent" );

			if( blackboard == null )
				throw new ArgumentNullException( "blackboard" );

			// Ensure that argument script evaluators are properly initialized
			if( argumentEvaluators == null )
			{
				initializeEvaluators( agent, blackboard );
			}

			// Perform any necessary data conversion, and store argument values in the runtimeArguments array
			for( int i = 0; i < definedParameters.Length; i++ )
			{

				var argument = Arguments[ i ];
				var parameter = definedParameters[ i ];

				// If this argument is supplied via script evaluation, perform that evaluation now
				ExpressionEvaluator evaluator = null;
				if( argumentEvaluators.TryGetValue( parameter, out evaluator ) && evaluator != null )
				{
					argument = evaluator.Execute( agent, blackboard );
				}

				// Perform type conversion when necessary. This should not normally be needed, since the 
				// design-time interface for editing the arguments should enforce the use of compatible 
				// types, but this allows for other cases (such as building the HTN through script, etc)
				var argumentType = ( argument == null ) ? parameter.ParameterType : argument.GetType();
				if( !parameter.ParameterType.IsAssignableFrom( argumentType ) )
				{
					argument = Convert.ChangeType( argument, parameter.ParameterType );
				}

				runtimeArguments[ i ] = argument;

			}

			if( typeof( TaskStatus ).IsAssignableFrom( Method.ReturnType ) )
			{
				return createEnumerator( agent, runtimeArguments );
			}
			else if( typeof( IEnumerator ).IsAssignableFrom( Method.ReturnType ) )
			{

				try
				{
					var iterator = (IEnumerator)Method.Invoke( Method.IsStatic ? null : agent, runtimeArguments );
					return iterator;
				}
				catch
				{
					Debug.LogError( string.Format( "Error initializing task '{0}'", this.Name ), agent as UnityEngine.Object );
					throw;
				}

			}
			else
			{
				throw new Exception( "Invalid return type specified for function: " + this.Method );
			}

		}

		internal override void Initialize( object agent, Blackboard state )
		{

			base.Initialize( agent, state );

			initializeArguments();
			initializeEvaluators( agent, state );

		}

		#endregion 

		#region Private utility methods

		private IEnumerator createEnumerator( object agent, object[] runtimeArguments )
		{

			var tickStatus = TaskStatus.Running;
			do
			{

				try
				{
					tickStatus = (TaskStatus)Method.Invoke( Method.IsStatic ? null : agent, runtimeArguments );
				}
				catch( Exception err )
				{
					Debug.LogError( string.Format( "Error executing task [{0}] - {1}", this.Name, err.ToString() ), agent as UnityEngine.Object );
					tickStatus = TaskStatus.Failed;
				}

				yield return tickStatus;

			} while( tickStatus == TaskStatus.Running );

			yield break;

		}

		private void initializeEvaluators( object agent, Blackboard state )
		{

			argumentEvaluators = new Dictionary<ParameterInfo, ExpressionEvaluator>();

			for( int i = 0; i < definedParameters.Length; i++ )
			{

				var parameter = definedParameters[ i ];
				if( parameter.IsDefined( typeof( ScriptParameterAttribute ), false ) )
				{
					var evaluator = argumentEvaluators[ parameter ] = new ExpressionEvaluator();
					evaluator.Compile( Arguments[ i ].ToString(), agent, state );
				}
				else
				{
					argumentEvaluators[ parameter ] = null;
				}

			}
		}

		private void initializeArguments()
		{

			if( Method == null )
				return;

			// Pre-cache the list of defined parameters and runtime arguments to prevent per-evaluation 
			// memory allocations.
			definedParameters = Method.GetParameters();
			runtimeArguments = new object[ definedParameters.Length ];

			// If the target method's parameter list has been changed since the last time the HTN
			// graph was edited, this object may have too many arguments defined. That might be fine
			// since extra arguments will be ignored during invocation, but the user experience is
			// nicer when we remove the extras at design time.
			while( Arguments.Count > definedParameters.Length )
			{
				Arguments.RemoveAt( Arguments.Count - 1 );
			}

			// If the target method has been changed, the argument list might be shorter than the 
			// parameter list, so ensure that there is an argument for every parameter
			while( Arguments.Count < definedParameters.Length )
			{
				int lastIndex = Arguments.Count;
				Arguments.Add( getDefaultParameterValue( definedParameters[ lastIndex ] ) );
			}

		}

		private static object getDefaultParameterValue( ParameterInfo parameter )
		{

			if( parameter.IsDefined( typeof( ScriptParameterAttribute ), false ) )
			{
				return "/* Script */";
			}

			var defaultValueAttribute = parameter.GetCustomAttributes( typeof( DefaultValueAttribute ), false ).FirstOrDefault() as DefaultValueAttribute;
			if( defaultValueAttribute != null )
			{
				return defaultValueAttribute.Value;
			}

			if( parameter.IsOptional )
				return parameter.RawDefaultValue;

			var parameterType = parameter.ParameterType;

			if( parameterType.IsValueType )
				return Activator.CreateInstance( parameterType );

			if( parameterType == typeof( string ) )
				return string.Empty;

			return null;

		}

		#endregion

		#region ISerializationCallbackReceiver Members

		public override void OnAfterDeserialize()
		{

			this.IsExpanded = true;
			base.OnAfterDeserialize();

			initializeArguments();

		}

		#endregion

	}

#if UNITY_EDITOR

	/*****************************
	 * Design-time support only *
	 ****************************/

	public partial class OperatorNode : GraphNodeBase
	{

		public override void OnInspectorGUI( TaskNetworkGraph graph )
		{

			if( this.Method == null )
			{

				DesignUtil.Header( "Missing Method" );

				EditorGUILayout.HelpBox( "\nThe target method that was previously stored could not be found (was it renamed?).\n\nPlease select one of the following methods.\n", MessageType.Error );

				EditorGUILayout.Space();
				selectTargetMethod( graph.DomainType );
				EditorGUILayout.Space();

				if( this.Method == null )
					return;

			}

			base.OnInspectorGUI( graph );

			if( DesignUtil.SectionFoldout( "node-operator-arguments", "Function" ) )
			{

				string sectionLabel = string.Format( "{0}.{1}()", Method.DeclaringType.Name, Method.Name );
				EditorGUILayout.LabelField( "Function", sectionLabel );
				EditorGUILayout.Space();
	
				var parameters = this.Method.GetParameters();

				editArguments( graph.BlackboardDefinition, parameters );
				EditorGUILayout.Space();

				showEditScriptButton();

			}

			if( DesignUtil.SectionFoldout( "node-operator-breakpoint", "Debugging" ) )
			{
				this.PauseOnRun = EditorGUILayout.Toggle( "Breakpoint", this.PauseOnRun );
			}

			GUILayout.Space( 5 );

		}

		private void selectTargetMethod( System.Type agentType )
		{

			var methods = agentType
				.GetMethods()
				.Where( x =>
					!x.IsSpecialName &&
					!x.IsGenericMethod &&
					!x.IsGenericMethodDefinition &&
					isOperatorMethod( agentType, x )
				)
				.OrderBy( x => x.Name )
				.ToArray();

			var methodNames = methods.Select( x => x.Name ).ToArray();

			var index = EditorGUILayout.Popup( "Operator", -1, methodNames );
			if( index != -1 )
			{

				this.Method = methods[ index ];

				Arguments.Clear();
				initializeArguments();

			}
				
		}

		private bool isOperatorMethod( System.Type agentType, MethodInfo method )
		{

			// Only public instance methods allowed. Presumably, instance methods
			// will already have access to any information needed to perform their 
			// task, including the active Blackboard instance.
			if( !method.IsPublic || method.IsStatic )
				return false;

			// Tasks must always return a status code that can be used to determine
			// the current state of the task (Success or failure, running, paused, etc)
			if( method.ReturnType != typeof( TaskStatus ) )
			{
				// Alternatively, tasks can be defined as an IEnumerator<Status> 
				// function. This would be used for long-running tasks rather than
				// instantaneous tasks.
				var iteratorType = typeof( IEnumerator );
				if( !iteratorType.IsAssignableFrom( method.ReturnType ) )
				{
					return false;
				}
			}

			// We don't want to list methods that are defined on base classes 
			// such as Monobehaviour.
			if( method.DeclaringType != agentType )
				return false;

			if( !string.IsNullOrEmpty( method.DeclaringType.Namespace ) && method.DeclaringType.Namespace.Contains( "StagPoint" ) )
				return false;

			// Ensure that there are no array, list, or dictionary types defined as parameters
			var parameters = method.GetParameters();
			foreach( var parameter in parameters )
			{

				if( typeof( IList ).IsAssignableFrom( parameter.ParameterType ) )
					return false;

				if( typeof( IDictionary ).IsAssignableFrom( parameter.ParameterType ) )
					return false;

				if( !string.IsNullOrEmpty( parameter.ParameterType.Namespace ) && parameter.ParameterType.Namespace.Contains( "StagPoint" ) )
					return false;

			}

			// Assume that the method is valid
			return true;

		}

		private void showEditScriptButton()
		{

			try
			{

				var selectedScript = MonoScriptHelper.GetMonoScriptFromType( Method.DeclaringType );
				if( selectedScript != null && GUILayout.Button( "Edit Script" ) )
				{
					DesignUtil.Defer( () =>
					{
						var lineNumber = getLineNumberForMethod( selectedScript, Method );
						AssetDatabase.OpenAsset( selectedScript, lineNumber );
					} );
				}

			}
			catch
			{
				// Just ignore any errors in this design-time code
			}

		}

		private void editArguments( Blackboard blackboard, ParameterInfo[] parameters )
		{

			for( int i = 0; i < parameters.Length; i++ )
			{

				var parameter = parameters[ i ];
				var parameterType = parameter.ParameterType;

				// If the user target method has been changed, the argument list might be shorter than the 
				// parameter list, so ensure that there is an argument for every parameter
				if( Arguments.Count <= i )
				{
					Arguments.Add( getDefaultParameterValue( parameter ) );
				}

				var label = ObjectNames.NicifyVariableName( parameter.Name );

				if( parameter.IsDefined( typeof( ScriptParameterAttribute ), false ) )
				{
					GUI.color = Color.cyan;
					Arguments[ i ] = DesignUtil.EditValue( label, typeof( string ), Arguments[ i ] );
					GUI.color = Color.white;
				}
				else
				{
					Arguments[ i ] = DesignUtil.EditValue( label, parameterType, Arguments[ i ] );
				}

			}

		}

		private int getLineNumberForMethod( MonoScript selectedScript, MethodInfo method )
		{
			
			var text = selectedScript.text;
			if( string.IsNullOrEmpty( text ) )
				return 0;

			var signature = string.Format( "Status {0}(", method.Name );
			
			var index = text.IndexOf( signature );
			if( index == -1 )
			{
				signature = string.Format( "IEnumerator {0}(", method.Name );
				index = text.IndexOf( signature );
				if( index == -1 )
					return 0;
			}

			int lineNumber = 1;
			for( int i = 0; i < index; i++ )
			{
				if( text[ i ] == '\n' )
					lineNumber += 1;
			}

			return lineNumber;

		}

		protected override string getNodeType()
		{
			return "Task Node";
		}

		protected override string getHelpDescription()
		{
			return "Task nodes are used to call functions defined in your agent class (operators). They cannot contain subtasks.";
		}

		protected override string getLabel()
		{
			return "<size=14><b>" + this.Name + "</b></size>";
		}

		protected override GUIStyle getBoxStyle()
		{

			var result = DesignUtil.GetStyle( "htn_operator_node", (GUIStyle)"flow node 5" );
			if( this.Method == null )
			{
				result = DesignUtil.GetStyle( "htn_operator_node_err", (GUIStyle)"flow node 6" );
			}

			result.fontStyle = FontStyle.Bold;
			result.richText = true;

			return result;

		}

	}

#endif

}
