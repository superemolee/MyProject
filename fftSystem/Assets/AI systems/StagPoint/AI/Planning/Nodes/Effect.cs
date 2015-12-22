// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using StagPoint.Core;
using StagPoint.Eval;

using UnityEngine;

namespace StagPoint.Planning
{

#if UNITY_EDITOR
	using UnityEditor;
#endif

	public abstract partial class NodeEffectBase
	{

		#region Private serialized fields

		[SerializeField]
		protected string uid = System.Guid.NewGuid().ToString();

		#endregion

		#region Public fields 

		/// <summary>
		/// If this field set to TRUE (default), the effect will be applied at runtime as well 
		/// as during plan creation. If you do not want this Effect applied automatically
		/// at runtime, set this field to FALSE.
		/// </summary>
		public bool ApplyAtRuntime = true;

		#endregion 

		#region Public methods 

		public virtual void Initialize( object agent, Blackboard state ) { }

		public abstract TaskStatus Apply( object agent, Blackboard blackboard );

		#endregion 

	}

	public partial class EvalEffect : NodeEffectBase
	{

		#region Public fields 

		public string Script = string.Empty;

		#endregion

		#region Private runtime variables 

		private ExpressionEvaluator eval;

		#endregion 

		#region Base class overrides 

		public override void Initialize( object agent, Blackboard state )
		{

			base.Initialize( agent, state );

			if( eval == null )
			{
				eval = new ExpressionEvaluator();
				eval.Compile( this.Script, agent, state );
			}

		}

		public override TaskStatus Apply( object agent, Blackboard blackboard )
		{

			try
			{

				if( eval == null )
				{
					eval = new ExpressionEvaluator();
					eval.Compile( this.Script, agent, blackboard );
				}

				eval.Execute( agent, blackboard );

				return TaskStatus.Succeeded;

			}
			catch( CompileException compileError )
			{
				Debug.LogError( string.Format( "Script compile error : {0}", compileError.Message ) );
				return TaskStatus.Failed;
			}

		}

		public override string ToString()
		{

			if( string.IsNullOrEmpty( this.Script ) )
			{
				return "(no script)";
			}

			return this.Script;

		}

		#endregion 

	}

	public partial class MethodEffect : NodeEffectBase, ISerializationCallbackReceiver
	{

		#region Public fields 

		public MethodInfo Method;

		public List<object> Arguments = new List<object>();

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

		public override TaskStatus Apply( object agent, Blackboard blackboard )
		{

			// Ensure that the Method is still valid (this can happen if the user script has been
			// edited since the effect was first created)
			if( Method == null )
			{
				Debug.LogError( string.Format( "Failed to apply effect: {0}", this ) );
				return TaskStatus.Failed;
			}

			// Ensure that argument script evaluators are properly initialized
			if( argumentEvaluators == null )
			{
				initializeEvaluators( agent, blackboard );
			}

			// Ensure that we have a valid array for runtime arguments
			if( runtimeArguments == null || definedParameters == null )
			{
				definedParameters = Method.GetParameters();
				runtimeArguments = new object[ definedParameters.Length ];
			}

			// The first argument is *always* the active Blackboard
			runtimeArguments[ 0 ] = blackboard;

			// Perform any necessary data conversion, and store argument values in the runtimeArguments array
			for( int i = 1; i < definedParameters.Length; i++ )
			{

				var argument = Arguments[ i - 1 ];
				var parameter = definedParameters[ i ];

				// If this argument is supplied via script evaluation, perform that evaluation now
				ExpressionEvaluator evaluator = null;
				if( argumentEvaluators.TryGetValue( parameter, out evaluator ) && evaluator != null )
				{
					argument = evaluator.Execute( agent, blackboard );
				}

				// Perform type conversion when necessary. This should not normally be needed, since the 
				// design-time interface for editing the arguments should enforce the use of compatible 
				// types, but this allows for edge cases (such as building the HTN through script, etc)
				var argumentType = ( argument == null ) ? parameter.ParameterType : argument.GetType();
				if( !parameter.ParameterType.IsAssignableFrom( argumentType ) )
				{
					argument = Convert.ChangeType( argument, parameter.ParameterType );
				}

				runtimeArguments[ i ] = argument;

			}

			return (TaskStatus) Method.Invoke( Method.IsStatic ? null : agent, runtimeArguments );

		}

		public override void Initialize( object agent, Blackboard state )
		{
			base.Initialize( agent, state );
			initializeArgumentsArray();
		}

		public override string ToString()
		{

			if( Method == null )
			{
				return "Method not defined";
			}

			if( Arguments.Count == 0 )
			{
				return ( Method != null ) ? string.Format( "<b>{0}</b>() is <b>TRUE</b>", Method.Name ) : "** ERROR **";
			}

			var builder = new StringBuilder();
			builder.Append( string.Format( "<b>{0}</b>( ", Method.Name ) );

			for( int i = 0; i < Arguments.Count; i++ )
			{

				if( i > 0 )
					builder.Append( ", " );

				var value = Arguments[ i ];

				var text = value == null ? "null" : value.ToString();
				if( value is UnityEngine.Object )
				{
					text = ( (UnityEngine.Object)value ).name;
				}

				builder.Append( text );

			}

			builder.Append( " )" );

			return builder.ToString();

		}

		#endregion 

		#region Private utility methods 

		private void initializeEvaluators( object agent, Blackboard state )
		{

			argumentEvaluators = new Dictionary<ParameterInfo, ExpressionEvaluator>();

			for( int i = 1; i < definedParameters.Length; i++ )
			{

				var parameter = definedParameters[ i ];
				if( parameter.IsDefined( typeof( ScriptParameterAttribute ), false ) )
				{
					var evaluator = argumentEvaluators[ parameter ] = new ExpressionEvaluator();
					evaluator.Compile( Arguments[ i - 1 ].ToString(), agent, state );
				}
				else
				{
					argumentEvaluators[ parameter ] = null;
				}

			}
		}

		private void initializeArgumentsArray()
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
			while( Arguments.Count > definedParameters.Length - 1 )
			{
				Arguments.RemoveAt( Arguments.Count - 1 );
			}

			// If the target method has been changed, the argument list might be shorter than the 
			// parameter list, so ensure that there is an argument for every parameter
			while( Arguments.Count < definedParameters.Length - 1 )
			{
				int lastIndex = Arguments.Count;
				Arguments.Add( getDefaultParameterValue( definedParameters[ lastIndex ] ) );
			}

		}

		private static object getDefaultParameterValue( ParameterInfo parameter )
		{

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

		public void OnAfterDeserialize()
		{
			initializeArgumentsArray();
		}

		public void OnBeforeSerialize()
		{
			// Not needed
		}

		#endregion

	}

	public partial class VariableEffect : NodeEffectBase
	{

		#region Public fields 

		public string VariableName;
		public EffectType ActionType;
		public object Argument;

		#endregion 

		#region Class overrides

		public override TaskStatus Apply( object agent, Blackboard blackboard )
		{

			var variable = blackboard.GetVariable( this.VariableName );

			switch( ActionType )
			{
				case EffectType.Add:
					variable.Add( this.Argument );
					break;
				case EffectType.Subtract:
					variable.Subtract( this.Argument );
					break;
				case EffectType.SetValue:
					variable.Assign( this.Argument );
					break;
				default:
					return TaskStatus.Failed;
			}

			return TaskStatus.Succeeded;
			
		}

		public override string ToString()
		{

			switch( ActionType )
			{
				case EffectType.Add:
					return string.Format( "Add <b>{0}</b> to <b>${1}</b>", this.Argument, this.VariableName );
				case EffectType.Subtract:
					return string.Format( "Subtract <b>{0}</b> from <b>${1}</b>", this.Argument, this.VariableName );
				case EffectType.SetValue:
					return string.Format( "Set <b>${0}</b> to <b>{1}</b>", this.VariableName, ( this.Argument is UnityEngine.Object ) ? ( (UnityEngine.Object)Argument ).name : this.Argument );
			}

			return "** ERROR **";

		}

		#endregion 

	}

#if UNITY_EDITOR

	/*****************************
	 * Design-time support only *
	 ****************************/

	public abstract partial class NodeEffectBase
	{

		#region Private design-time variables

		protected bool isExpanded = false;

		#endregion

		#region Public design-time methods

		public virtual bool DoFoldout( int width )
		{
			isExpanded = DesignUtil.Foldout( this.isExpanded, this.ToString(), width );
			return isExpanded;
		}

		public abstract void OnInspectorGUI( int width, object agent, Blackboard blackboard );

		#endregion

	}

	public partial class EvalEffect : NodeEffectBase
	{

		public override void OnInspectorGUI( int width, object agent, Blackboard blackboard )
		{

			if( !isExpanded )
				return;

			if( blackboard == null )
				return;

			this.Script = EditorGUILayout.TextField( "Expression", this.Script );
			this.ApplyAtRuntime = EditorGUILayout.Toggle( "Apply at runtime", this.ApplyAtRuntime );

			if( !Application.isPlaying && agent != null && !string.IsNullOrEmpty( this.Script ) )
			{
				if( GUILayout.Button( "Validate" ) )
				{
					validateScript( agent, blackboard );
				}
			}

		}

		private void validateScript( object agent, Blackboard blackboard )
		{

			try
			{

				var eval = new ExpressionEvaluator();

				var expression = eval.Compile( this.Script, agent, blackboard );
				if( expression == null )
				{
					EditorUtility.DisplayDialog( "Script Validation", "Unknown error compiling script", "OK" );
					return;
				}

				EditorUtility.DisplayDialog( "Script Validation", "This script compiles correctly", "OK" );

			}
			catch( Exception error )
			{
				EditorUtility.DisplayDialog( "Script Error", error.Message, "OK" );
			}

		}

	}

	public partial class MethodEffect : NodeEffectBase
	{

		public override void OnInspectorGUI( int width, object agent, Blackboard blackboard )
		{

			if( !isExpanded )
				return;

			if( blackboard == null )
				return;

			// If the Method is null, most likely it did not deserialize properly. This is typically because
			// the developer renamed the target method, and the named method no longer exists.
			if( Method == null )
			{
				EditorGUILayout.HelpBox( "The method to be called is not defined or is invalid.", MessageType.Error );
				return;
			}

			// Make sure that the parameter information has been retrieved
			if( definedParameters == null || Arguments == null )
			{
				initializeArgumentsArray();
			}

			if( definedParameters.Length <= 1 )
			{
				EditorGUILayout.HelpBox( "No editable parameters defined for this method", MessageType.None );
				return;
			}

			for( int i = 1; i < definedParameters.Length; i++ )
			{

				var parameter = definedParameters[ i ];

				var label = ObjectNames.NicifyVariableName( definedParameters[ i ].Name );
				var parameterType = definedParameters[ i ].ParameterType;

				// If the user target method has been changed, the argument list might be shorter than the 
				// parameter list, so ensure that there is an argument for every parameter
				if( Arguments.Count < i )
				{
					Arguments.Add( getDefaultParameterValue( definedParameters[ i ] ) );
				}

				if( parameter.IsDefined( typeof( ScriptParameterAttribute ), false ) )
				{
					GUI.color = Color.cyan;
					Arguments[ i - 1 ] = DesignUtil.EditValue( label, typeof( string ), Arguments[ i - 1 ] );
					GUI.color = Color.white;
				}
				else
				{
					Arguments[ i - 1 ] = DesignUtil.EditValue( label, parameterType, Arguments[ i - 1 ] );
				}

			}

			this.ApplyAtRuntime = EditorGUILayout.Toggle( "Apply at runtime", this.ApplyAtRuntime );

		}

	}

	public partial class VariableEffect : NodeEffectBase
	{

		public override void OnInspectorGUI( int width, object agent, Blackboard blackboard )
		{

			if( !isExpanded )
				return;

			if( blackboard == null )
				return;

			EditorGUIUtility.GetControlID( this.GetHashCode(), FocusType.Native );
			GUILayout.BeginVertical( GUILayout.Width( width ) );
			{

				editVariableName( blackboard );
				if( !blackboard.Contains( this.VariableName ) )
				{
					GUILayout.Label( "Select a blackboard variable", (GUIStyle)"ErrorLabel" );
				}
				else
				{

					editActionType( blackboard );

					this.Argument = editValue( blackboard );
					this.ApplyAtRuntime = EditorGUILayout.Toggle( "Apply at runtime", this.ApplyAtRuntime );

				}

			}
			GUILayout.EndVertical();

		}

		private object editValue( Blackboard blackboard )
		{

			if( !blackboard.Contains( this.VariableName ) )
			{
				GUILayout.Label( "Variable not found", (GUIStyle)"ErrorLabel" );
				return this.Argument;
			}

			var variableType = blackboard.GetVariableType( this.VariableName );
			var myType = ( this.Argument != null ) ? this.Argument.GetType() : typeof( object );

			if( !variableType.IsAssignableFrom( myType ) )
			{
				this.Argument = variableType.IsValueType ? Activator.CreateInstance( variableType ) : null;
			}

			return DesignUtil.EditValue( "Value", variableType, this.Argument );

		}

		private void editActionType( Blackboard blackboard )
		{

			var variableType = blackboard.GetVariableType( this.VariableName );
			if( variableType != typeof( int ) && variableType != typeof( float ) )
			{
				this.ActionType = EffectType.SetValue;
				return;
			}

			this.ActionType = (EffectType)EditorGUILayout.EnumPopup( "Action", this.ActionType );

		}

		private void editVariableName( Blackboard blackboard )
		{

			var keys = blackboard.GetKeys( true );
			keys.Sort();

			if( keys.Count == 0 )
			{
				this.VariableName = EditorGUILayout.TextField( "Variable", this.VariableName );
				return;
			}

			var selectedIndex = keys.IndexOf( this.VariableName );
			var newSelection = EditorGUILayout.Popup( "Variable", selectedIndex, keys.ToArray() );

			if( newSelection != selectedIndex )
			{
				this.VariableName = keys[ newSelection ];
				this.Argument = blackboard.GetValue<object>( this.VariableName );
			}

		}

	}

#endif
}
