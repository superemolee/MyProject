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

	public abstract partial class NodePreconditionBase
	{

		#region Static variables 
		
		protected static List<ConditionType> unaryComparisons = new List<ConditionType>()
		{
			ConditionType.IsNull,
			ConditionType.IsNotNull,
			ConditionType.IsTrue,
			ConditionType.IsFalse
		};

		#endregion 

		#region Private serialized fields

		[SerializeField]
		protected string uid = System.Guid.NewGuid().ToString();

		#endregion 

		#region Public methods

		public abstract bool IsConditionSatisfied( object agent, Blackboard blackboard );

		public virtual void Initialize( object agent, Blackboard state ) { }

		#endregion 

		#region Constructor

		public NodePreconditionBase()
		{
			// Stub - Required for serialization
		}

		#endregion

	}

	public partial class VariableCondition : NodePreconditionBase
	{

		#region Public fields 

		/// <summary>
		/// Represents the name of the BlackboardVariable that you wish to check.
		/// </summary>
		public string VariableName;

		/// <summary>
		/// Indicates which type of comparison to use
		/// </summary>
		public ConditionType Comparison;

		/// <summary>
		/// Indicates the value to be compared against the BlackboardVariable
		/// </summary>
		public object Value;

		#endregion 

		#region Constructor 

		public VariableCondition()
		{
			// Stub - Required for serialization
		}

		public VariableCondition( string variableName, ConditionType comparison, object value )
		{
			this.VariableName = variableName;
			this.Comparison = comparison;
			this.Value = value;
		}

		#endregion 

		#region Base class overrides

		public override bool IsConditionSatisfied( object agent, Blackboard blackboard )
		{

			var variable = blackboard.GetVariable( this.VariableName );

			switch( this.Comparison )
			{
				case ConditionType.EqualTo:
					return variable.EqualTo( this.Value );
				case ConditionType.NotEqualTo:
					return variable.NotEqualTo( this.Value );
				case ConditionType.GreaterThan:
					return variable.GreaterThan( this.Value );
				case ConditionType.GreaterThanOrEqual:
					return variable.GreaterThanOrEqual( this.Value );
				case ConditionType.LessThan:
					return variable.LessThan( this.Value );
				case ConditionType.LessThanOrEqual:
					return variable.LessThanOrEqual( this.Value );
				case ConditionType.IsNull:
					return variable.IsNull();
				case ConditionType.IsNotNull:
					return variable.IsNotNull();
				case ConditionType.IsTrue:
					return variable.IsTrue();
				case ConditionType.IsFalse:
					return variable.IsFalse();
			}

			return false;

		}

		public override string ToString()
		{

			string comparisonType = ObjectNames.NicifyVariableName( Comparison.ToString() );

			if( unaryComparisons.Contains( Comparison ) )
			{
				return string.Format( "<b>${0}</b> {1}", VariableName, comparisonType );
			}

			return string.Format( "<b>${0}</b> {1} <b>{2}</b>", VariableName, comparisonType, Value );

		}

		#endregion 

	}

	[TypeRename( "StagPoint.Planning.ExpressionCondition" )]
	public partial class EvalCondition : NodePreconditionBase
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

		public override bool IsConditionSatisfied( object agent, Blackboard blackboard )
		{

			try
			{

				if( eval == null )
				{
					eval = new ExpressionEvaluator();
					eval.Compile( this.Script, agent, blackboard );
				}

				return (bool)eval.Execute( agent, blackboard );

			}
			catch( CompileException compileError )
			{
				Debug.LogError( string.Format( "Script: {0}, Compile error : {1}", this.Script, compileError.Message ) );
				return false;
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

	public partial class MethodCondition : NodePreconditionBase, ISerializationCallbackReceiver
	{

		#region Public fields 

		/// <summary>
		/// Indicates which method will be called when this condition is evaluated
		/// </summary>
		public MethodInfo Method;

		/// <summary>
		/// Contains the list of arguments that will be passed to the target function 
		/// </summary>
		public List<object> Arguments = new List<object>();

		/// <summary>
		/// Contains the expected return value of the function
		/// </summary>
		public bool ExpectedValue = true;

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

		#region Public methods

		public override bool IsConditionSatisfied( object agent, Blackboard blackboard )
		{

			// Ensure that we have a valid array for runtime arguments
			if( runtimeArguments == null || definedParameters == null )
			{
				definedParameters = Method.GetParameters();
				runtimeArguments = new object[ definedParameters.Length ];
			}

			// The first argument is *always* the active Blackboard
			runtimeArguments[ 0 ] = blackboard;

			// Ensure that argument script evaluators are properly initialized
			if( argumentEvaluators == null )
			{
				initializeEvaluators( agent, blackboard );
			}

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

			// Invoke the target method and return the result to the caller
			var returnValue = (bool)Method.Invoke( Method.IsStatic ? null : agent, runtimeArguments );
			return returnValue == this.ExpectedValue;

		}

		#endregion 

		#region Base class overrides 

		public override void Initialize( object agent, Blackboard state )
		{
			base.Initialize( agent, state );
			this.initializeArguments();
		}

		public override string ToString()
		{

			if( Method == null )
			{
				return "Method not defined";
			}

			var expected = ( this.ExpectedValue ? "TRUE" : "FALSE" );

			if( Arguments.Count == 0 )
			{
				return ( Method != null ) ? string.Format( "<b>{0}</b>() is <b>{1}</b>", Method.Name, expected ) : "** ERROR **";
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

			builder.Append( " ) returns " );
			builder.Append( expected );

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
			initializeArguments();
		}

		public void OnBeforeSerialize()
		{
			// Not needed
		}

		#endregion 

	}

#if UNITY_EDITOR

	/*****************************
	 * Design-time support only *
	 ****************************/

	public abstract partial class NodePreconditionBase
	{

		#region Static design-time variables 

		protected static List<ConditionType> booleanComparisons = new List<ConditionType>()
		{
			ConditionType.IsTrue,
			ConditionType.IsFalse
		};

		protected static List<ConditionType> equalityComparisons = new List<ConditionType>()
		{
			ConditionType.EqualTo,
			ConditionType.NotEqualTo
		};

		protected static List<ConditionType> objectComparisons = new List<ConditionType>()
		{
			ConditionType.IsNull,
			ConditionType.IsNotNull
		};

		protected static List<ConditionType> primitiveComparisons = new List<ConditionType>()
		{
			ConditionType.EqualTo,
			ConditionType.NotEqualTo,
			ConditionType.GreaterThan,
			ConditionType.GreaterThanOrEqual,
			ConditionType.LessThan,
			ConditionType.LessThanOrEqual,
		};

		#endregion 

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

	public partial class EvalCondition : NodePreconditionBase
	{

		public override void OnInspectorGUI( int width, object agent, Blackboard blackboard )
		{

			if( !isExpanded )
				return;

			if( blackboard == null )
				return;

			this.Script = EditorGUILayout.TextField( "Expression", this.Script );

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

	public partial class MethodCondition : NodePreconditionBase
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
				initializeArguments();
			}

			if( definedParameters.Length > 0 )
			{

				for( int i = 1; i < definedParameters.Length; i++ )
				{

					var parameter = definedParameters[ i ];

					var label = ObjectNames.NicifyVariableName( parameter.Name );
					var parameterType = parameter.ParameterType;

					// If the user target method has been changed, the argument list might be shorter than the 
					// parameter list, so ensure that there is an argument for every parameter
					if( Arguments.Count < i )
					{
						Arguments.Add( getDefaultParameterValue( parameter ) );
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

			}

			this.ExpectedValue = EditorGUILayout.Toggle( "Returns TRUE", this.ExpectedValue );

		}

	}

	public partial class VariableCondition : NodePreconditionBase
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
				editComparison( blackboard );

				if( !unaryComparisons.Contains( Comparison ) )
				{
					this.Value = editValue( blackboard );
				}

			}
			GUILayout.EndVertical();

		}

		private object editValue( Blackboard blackboard )
		{

			if( !blackboard.Contains( this.VariableName ) )
			{
				GUILayout.Label( "Variable not found", (GUIStyle)"ErrorLabel" );
				return this.Value;
			}

			var variableType = blackboard.GetVariableType( this.VariableName );
			var myType = (this.Value != null) ? this.Value.GetType() : typeof( object );

			if( !variableType.IsAssignableFrom( myType ) )
			{
				this.Value = variableType.IsValueType ? Activator.CreateInstance( variableType ) : null;
			}

			return DesignUtil.EditValue( "Value", variableType, this.Value );

		}

		private void editComparison( Blackboard blackboard )
		{

			if( !blackboard.Contains( VariableName ) )
			{
				this.Comparison = (ConditionType)EditorGUILayout.EnumPopup( "Comparison", this.Comparison );
				return;
			}

			List<ConditionType> valid = null;

			var variableType = blackboard.GetVariableType( this.VariableName );
			if( variableType == typeof( bool ) )
			{
				valid = booleanComparisons;
			}
			else if( variableType.IsPrimitive )
			{
				valid = primitiveComparisons;
			}
			else if( variableType.IsValueType && !variableType.IsPrimitive )
			{
				valid = equalityComparisons;
			}
			else if( variableType == typeof( string ) )
			{
				valid = equalityComparisons;
			}
			else if( variableType.IsClass )
			{
				valid = objectComparisons;
			}

			var values = valid.Select( x => x.ToString() ).ToList();
			values.Sort();

			var selectedIndex = Mathf.Max( 0, values.IndexOf( this.Comparison.ToString() ) );
			selectedIndex = EditorGUILayout.Popup( "Comparison", selectedIndex, values.ToArray() );

			this.Comparison = (ConditionType)Enum.Parse( typeof( ConditionType ), values[ selectedIndex ] );

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
				this.Value = blackboard.GetValue<object>( this.VariableName );
			}

		}

	}

#endif

}
