// Copyright (c) 2014 StagPoint Consulting
				
using UnityEngine;
using System.Collections;

using StagPoint.Core;
using StagPoint.Planning;

namespace StagPoint.Examples
{

	[TypeRename( "PlannerFunctionLibrary" )]
	[FunctionLibrary( "Capture the flag" )]
	public class PlannerFunctionLibrary
	{

		[Operator( "NOP", "The NOP action stands for (No OPeration), and does nothing." )]
		public static TaskStatus NOP()
		{
			return TaskStatus.Succeeded;
		}

		[Operator( "Log Warning", "Prints a message to the Unity debug console" )]
		public static TaskStatus LogWarning( [DefaultValue( "Text" )] string text )
		{

			if( string.IsNullOrEmpty( text ) )
				return TaskStatus.Failed;

#if UNITY_EDITOR
			DebugMessages.LogWarning( text );
#endif

			return TaskStatus.Succeeded;

		}

		[Operator( "Log Expression", "Prints the results of an expression to the Unity debug console" )]
		public static TaskStatus LogExpression( [ScriptParameter][DefaultValue( "'Enter expression'" )] object value )
		{
			return Log( value != null ? value.ToString() : "(null)" );
		}

		[Operator( "Log Message", "Prints a message to the Unity debug console" )]
		public static TaskStatus Log( [DefaultValue( "Text" )] string text )
		{

			if( string.IsNullOrEmpty( text ) )
				return TaskStatus.Failed;

#if UNITY_EDITOR
			DebugMessages.Log( text );
#endif

			return TaskStatus.Succeeded;

		}

		[Operator( "Wait", "Waits for the specified number of seconds" )]
		public static IEnumerator Wait( [DefaultValue( 1f )] float seconds )
		{

			var timeout = Time.realtimeSinceStartup + seconds;
			while( Time.realtimeSinceStartup < timeout )
			{
				yield return TaskStatus.Running;
			}

			yield return TaskStatus.Succeeded;

		}

		[Operator( "Error", "Prints an error message to the console and causes the plan to fail" )]
		public static TaskStatus Error( [DefaultValue( "Unknown Error" )] string message )
		{
			Debug.LogError( message );
			return TaskStatus.Failed;
		}

		#region Library conditions

		[Condition( "Is Variable Defined" )]
		public static bool IsVariableDefined( Blackboard blackboard, string variableName )
		{
			return blackboard.Contains( variableName );
		}

		#endregion

		#region Library effects

		/// <summary>
		/// For testing purposes only 
		/// </summary>
		[Effect( "Randomize Variable" )]
		public static TaskStatus RandomizeVariable( Blackboard blackboard, string variableName, int minValue, int maxValue )
		{
			blackboard.SetValue<int>( variableName, Random.Range( minValue, maxValue ) );
			return TaskStatus.Succeeded;
		}

		#endregion

	}

}
