// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using StagPoint.Eval;
using StagPoint.Core;

using UnityEngine;

namespace StagPoint.Core
{

	using ScriptEnvironment = StagPoint.Eval.Environment;

	/// <summary>
	/// Contains values and functionality used for runtime script evaluation
	/// </summary>
	internal class EvalUtil
	{

		#region Static variables 

		/// <summary>
		/// Contains all default constants used by the script evaluation system. Can be used to 
		/// provide an alias for any desired System.Type, or any constant values that are not 
		/// defined by the script grammar.
		/// </summary>
		private static Dictionary<string, object> Constants = new Dictionary<string, object>()
		{
			{ "Random", typeof( UnityEngine.Random ) },
			{ "Type", typeof( System.Type ) },
			{ "Time", typeof( UnityEngine.Time ) },
			{ "Object", typeof( UnityEngine.Object ) },
			{ "GameObject", typeof( UnityEngine.GameObject ) },
			{ "Component", typeof( UnityEngine.Component ) },
			{ "ScriptableObject", typeof( UnityEngine.ScriptableObject ) },
			{ "MonoBehaviour", typeof( UnityEngine.MonoBehaviour ) },
			{ "Rect", typeof( UnityEngine.Rect ) },
			{ "Vector2", typeof( UnityEngine.Vector2 ) },
			{ "Vector3", typeof( UnityEngine.Vector3 ) },
			{ "Vector4", typeof( UnityEngine.Vector4 ) },
			{ "Quaternion", typeof( UnityEngine.Quaternion ) },
			{ "Matrix", typeof( UnityEngine.Matrix4x4 ) },
			{ "Mathf", typeof( UnityEngine.Mathf ) },
			{ "Math", typeof( UnityEngine.Mathf ) },
			{ "Debug", typeof( UnityEngine.Debug ) },
		};

		#endregion 

		#region Static contructor 

		static EvalUtil()
		{

			var assemblies = new List<Assembly>
			{
				typeof( MonoBehaviour ).Assembly,
				Assembly.GetExecutingAssembly(),
			};

			foreach( var assembly in assemblies )
			{

				foreach( var type in assembly.GetTypes() )
				{

					if( type.IsGenericType || type.IsSpecialName || !type.IsPublic )
						continue;

					Constants[ type.Name ] = type;

				}

			}

		}

		#endregion 

		#region Public methods

		/// <summary>
		/// Create a new script environment, with all constants and global variables already defined
		/// </summary>
		/// <returns></returns>
		public static ScriptEnvironment CreateEnvironment()
		{
			
			var newEnvironment = new ScriptEnvironment()
			{ 
				Constants = EvalUtil.Constants
			};

			initializeStandardFunctions( newEnvironment );

			return newEnvironment;

		}

		private static void initializeStandardFunctions( ScriptEnvironment env )
		{

			Func<Vector3, Vector3, float> dist2d = ( a, b ) =>
			{
				a.y = b.y;
				return Vector3.Distance( a, b );
			};

			Func<Vector3, Vector3, float> dist = ( a, b ) =>
			{
				return Vector3.Distance( a, b );
			};

			Func<object, bool> isNull = ( value ) =>
			{

				if( value is UnityEngine.Component )
				{
					var component = value as UnityEngine.Component;
					return component == null || component.gameObject == null;
				}

				return value == null;

			};

			Func<float, float, float> rand = ( min, max ) =>
			{
				return UnityEngine.Random.Range( min, max );
			};

			env.AddVariable( new BoundVariable( "isNull", isNull.Target, isNull.Method ) );
			env.AddVariable( new BoundVariable( "dist", dist.Target, dist.Method ) );
			env.AddVariable( new BoundVariable( "dist2d", dist2d.Target, dist2d.Method ) );
			env.AddVariable( new BoundVariable( "rand", rand.Target, rand.Method ) );

		}

		#endregion 

	}

	internal class ExpressionEvaluator
	{

		#region Public properties 

		public ScriptEnvironment Environment { get; private set; }

		#endregion 

		#region Private runtime variables

		private Expression expression = null;
		private Variable agentVariable = null;
		private Variable blackboardVariable = null;

		private List<VariableBase> scriptVariables = new List<VariableBase>();

		#endregion

		#region Public methods 

		public Expression Compile( string script, object agent, Blackboard blackboard )
		{

			this.agentVariable = new Variable( "agent", agent );
			this.blackboardVariable = new Variable( "blackboard", blackboard );

			this.Environment = EvalUtil.CreateEnvironment();
			this.Environment.ResolveEnvironmentVariable = resolveScriptVariables( agent, blackboard );
			this.Environment.AddVariable( agentVariable );
			this.Environment.AddVariable( blackboardVariable );

			this.expression = EvalEngine.Compile( script, Environment );

			return this.expression;

		}

		public object Execute( object agent, Blackboard blackboard )
		{

			#region Patch up variable data to match current planner state

			for( int i = 0; i < scriptVariables.Count; i++ )
			{

				var variable = scriptVariables[ i ];

				if( variable is ScriptBlackboardVariable )
				{
					( (ScriptBlackboardVariable)variable ).Blackboard = blackboard;
				}
				else if( variable is BoundVariable )
				{
					( (BoundVariable)variable ).Target = agent;
				}

			}

			agentVariable.Value = agent;
			blackboardVariable.Value = blackboard;

			#endregion

			return expression.Execute();

		}

		#endregion 

		#region Private utility methods

		private ScriptEnvironment.VariableResolutionCallback resolveScriptVariables( object agent, Blackboard blackboard )
		{
			return ( string name, out VariableBase variable ) =>
			{

				variable = null;

				if( name.StartsWith( "$" ) )
				{

					var blackboardVar = blackboard.GetVariable( name.Substring( 1 ) );
					if( blackboardVar == null )
						return false;

					variable = new ScriptBlackboardVariable( name, blackboardVar.DataType ) { Blackboard = blackboard };
					this.scriptVariables.Add( variable );

					return true;

				}

				var member = agent.GetType().GetMember( name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ).FirstOrDefault();
				if( member is FieldInfo || member is PropertyInfo || member is MethodInfo )
				{

					variable = new BoundVariable( name, agent, member );
					this.scriptVariables.Add( variable );

					return true;

				}

				return false;

			};
		}

		#endregion

	}

	/// <summary>
	/// Provides a bridge between the script evaluation engine and the host agent's Blackboard,
	/// allowing script expressions to use blackboard variables.
	/// </summary>
	internal class ScriptBlackboardVariable : Variable
	{

		#region Public fields 

		/// <summary>
		/// Contains a reference to the Blackboard instance referenced by the script expression.
		/// Because the Blackboard instance is necessarily different during the planning phase than
		/// the execution phase, you must set this field to the correct instance before evaluating
		/// the script expression.
		/// </summary>
		public Blackboard Blackboard;

		#endregion 

		#region Public properties 

		public override object Value
		{
			get
			{
				var value = Blackboard.GetValue<object>( this.Name.Substring( 1 ) );
				return value;
			}
			set
			{
				Blackboard.SetValue( this.Name.Substring( 1 ), value );
			}
		}

		#endregion 

		#region Constructor 

		public ScriptBlackboardVariable( string name, Type type )
			: base( name, type )
		{
		}

		#endregion 

	}

}