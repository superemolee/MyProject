// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

namespace StagPoint.Planning.Components
{

	using StagPoint.Core;
	using StagPoint.Planning;
	using StagPoint.DataCollection;

	using Debug = UnityEngine.Debug;

	[AddComponentMenu( "StagPoint/Examples/Capture the Flag/Planner Runtime Information" )]
	[RequireComponent( typeof( TaskNetworkPlanner ) )]
	public class RuntimePlannerInfo : MonoBehaviour
	{

		#region Public fields 

		public bool showDebugGUI;
		public bool showWhenSelected = true;
		public int numLinesToShow = 5;
		public TextAnchor debugGUIPosition;

		public TimedCounter numPlanningAttempts = new TimedCounter();
		public TimedCounter numPlansGenerated = new TimedCounter();
		public TimedCounter numPlansDiscarded = new TimedCounter();
		public TimedCounter numPlanningAttemptsFailed = new TimedCounter();

		public TimedCounter numSuccessfulPlans = new TimedCounter();
		public TimedCounter numAbortedPlans = new TimedCounter();
		public TimedCounter numCompletedPlans = new TimedCounter();
		public TimedCounter numExecutedPlans = new TimedCounter();
		public TimedCounter numFailedPlans = new TimedCounter();

		public DataVariable planningTime = new DataVariable();
		public DataVariable planLength = new DataVariable();
		public DataVariable searchDepth = new DataVariable();

		#endregion 

		#region Public properties 

		public float TotalRunTime { get; private set; }

		#endregion 

#if UNITY_STANDALONE || UNITY_EDITOR

		#region Private variables

		private List<string> messages = new List<string>() { "", "", "", "", "" };

		private float planningStartTime;
		private TaskNetworkPlanner planner;

		private GUIStyle debugMessageStyle;
		private Texture2D backgroundTexture;
		private string displayText = string.Empty;
		private bool rebuildDisplay = true;

		#endregion 

		#region Monobehaviour events

		[Conditional( "DEBUG" )]
		public void OnGUI()
		{

			if( !showDebugGUI )
				return;

#if UNITY_EDITOR
			if( showWhenSelected && !UnityEditor.Selection.gameObjects.Contains( this.gameObject ) )
				return;
#else
			if( showWhenSelected )
				return;
#endif

			if( debugMessageStyle == null )
			{
				initializeStyles();
			}

			var text = new GUIContent( buildLogContent() );

			var size = calculateRenderSize( text );
			var position = Vector2.zero;

			switch( this.debugGUIPosition )
			{
				case TextAnchor.LowerCenter:
					position.x = ( Screen.width - size.x ) * 0.5f;
					position.y = Screen.height - size.y;
					break;
				case TextAnchor.LowerLeft:
					position.y = Screen.height - size.y;
					break;
				case TextAnchor.LowerRight:
					position.x = Screen.width - size.x;
					position.y = Screen.height - size.y;
					break;
				case TextAnchor.MiddleCenter:
					position.x = ( Screen.width - size.x ) * 0.5f;
					position.y = ( Screen.height - size.y ) * 0.5f;
					break;
				case TextAnchor.MiddleLeft:
					position.y = ( Screen.height - size.y ) * 0.5f;
					break;
				case TextAnchor.MiddleRight:
					position.x = Screen.width - size.x;
					position.y = ( Screen.height - size.y ) * 0.5f;
					break;
				case TextAnchor.UpperCenter:
					position.x = ( Screen.width - size.x ) * 0.5f;
					break;
				case TextAnchor.UpperRight:
					position.x = Screen.width - size.x;
					break;
				default:
					break;
			}

			var rect = new Rect( position.x, position.y, size.x, size.y );

			GUI.DrawTexture( rect, backgroundTexture );
			GUI.Label( rect, text, debugMessageStyle );

		}

		[Conditional( "DEBUG" )]
		public void Update()
		{
			TotalRunTime += Time.deltaTime;
		}

		[Conditional( "DEBUG" )]
		public void OnEnable()
		{

			this.planner = GetComponent<TaskNetworkPlanner>();
			if( this.planner == null )
			{
				Debug.LogError( string.Format( "A {0} component should be paired with a {1} component", this.GetType().Name, typeof( TaskNetworkPlanner ).Name ) );
				return;
			}

			planner.PlanningStarted += planner_PlanningStarted;
			planner.PlanningEnded += planner_PlanningEnded;

			planner.PlanGenerated += planner_PlanGenerated;
			planner.PlanningFailed += planner_PlanningFailed;
			planner.PlanDiscarded += planner_PlanDiscarded;

			planner.PlanExecuted += planner_PlanExecuted;
			planner.PlanAborted += planner_PlanAborted;
			planner.PlanCompleted += planner_PlanCompleted;
			planner.PlanFailed += planner_PlanFailed;

			planner.TaskSucceeded += planner_TaskSucceeded;

		}

		[Conditional( "DEBUG" )]
		public void OnDisable()
		{

			planner.PlanningStarted -= planner_PlanningStarted;
			planner.PlanningEnded -= planner_PlanningEnded;

			planner.PlanGenerated -= planner_PlanGenerated;
			planner.PlanningFailed -= planner_PlanningFailed;
			planner.PlanDiscarded -= planner_PlanDiscarded;

			planner.PlanExecuted -= planner_PlanExecuted;
			planner.PlanAborted -= planner_PlanAborted;
			planner.PlanCompleted -= planner_PlanCompleted;
			planner.PlanFailed -= planner_PlanFailed;

			planner.TaskSucceeded -= planner_TaskSucceeded;

		}

		#endregion 

		#region Event handlers 

		void planner_TaskSucceeded( TaskNetworkPlanner planner, PlannerTask task )
		{
			this.rebuildDisplay = true;
			log( "Task complete: " + task.Name, "lime" );
		}

		void planner_PlanAborted( TaskNetworkPlanner planner, TaskNetworkPlan plan )
		{
			this.rebuildDisplay = true;
			var planName = plan.nodes.First( x => x is CompositeNode ).Name;
			log( "Plan aborted: " + planName, "orange" );
			numAbortedPlans.Increment();
		}

		void planner_PlanFailed( TaskNetworkPlanner planner, TaskNetworkPlan plan )
		{
			this.rebuildDisplay = true;
			log( "Plan failed", "red" );
			numFailedPlans.Increment();
		}

		void planner_PlanCompleted( TaskNetworkPlanner planner, TaskNetworkPlan plan )
		{
			this.rebuildDisplay = true;
			log( "Plan completed" );
			numCompletedPlans.Increment();
		}

		void planner_PlanExecuted( TaskNetworkPlanner planner, TaskNetworkPlan plan )
		{
			this.rebuildDisplay = true;
			var planName = plan.nodes.First( x => x is CompositeNode ).Name;
			log( "Plan started: " + planName );
			numExecutedPlans.Increment();
		}

		void planner_PlanDiscarded( TaskNetworkPlanner planner )
		{
			this.rebuildDisplay = true;
			numPlansDiscarded.Increment();
		}

		void planner_PlanningFailed( TaskNetworkPlanner planner )
		{
			this.rebuildDisplay = true;
			log( "Failed to generate plan" );
			numPlanningAttemptsFailed.Increment();
		}

		void planner_PlanGenerated( TaskNetworkPlanner planner, TaskNetworkPlan plan )
		{
			this.rebuildDisplay = true;
			numPlansGenerated.Increment();
			planLength.Update( plan.tasks.Count );
			searchDepth.Update( plan.nodes.Max( x => x.Depth ) );
		}

		void planner_PlanningEnded( TaskNetworkPlanner planner )
		{
			this.rebuildDisplay = true;
			planningTime.Update( Time.realtimeSinceStartup - planningStartTime );
		}

		void planner_PlanningStarted( TaskNetworkPlanner planner )
		{
			this.rebuildDisplay = true;
			numPlanningAttempts.Increment();
			planningStartTime = Time.realtimeSinceStartup;
		}

		#endregion 

		#region Private utility methods 

		private void initializeStyles()
		{

			initializeBackgroundTexture();

			debugMessageStyle = new GUIStyle();
			debugMessageStyle.richText = true;
			debugMessageStyle.padding = new RectOffset( 3, 3, 3, 3 );
			debugMessageStyle.fontSize = 12;
			debugMessageStyle.normal.textColor = Color.white;

		}

		private void initializeBackgroundTexture()
		{

			backgroundTexture = new Texture2D( 2, 2, TextureFormat.ARGB32, false )
			{
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Repeat
			};

			var backgroundColor = Color.black * 0.8f;

			backgroundTexture.SetPixels( new Color[] { backgroundColor, backgroundColor, backgroundColor, backgroundColor } );
			backgroundTexture.Apply( false );

		}

		private Vector2 calculateRenderSize( GUIContent text )
		{

			var height = debugMessageStyle.CalcHeight( text, 400 );

			return new Vector2( 400, height );

		}

		private string buildLogContent()
		{

			if( !this.rebuildDisplay )
				return this.displayText;

			this.rebuildDisplay = false;

			var header = string.Format( "<b>{0}</b>", this.name );
			var planText = buildPlanText();
			var logMessages = string.Join( "\n", messages.ToArray() );
			var text = string.Format( "{0}\n\n{1}\n\n{2}", header, planText, logMessages );

			return this.displayText = text;

		}

		private string buildPlanText()
		{

			var plan = planner.Plan;
			if( plan == null || plan.Status != TaskStatus.Running )
				return "<color=orange>No plan</color>";

			var taskNames = new List<string>();
			var taskIndex = plan.tasks.IndexOf( plan.CurrentTask );

			for( int i = taskIndex; i < plan.TaskCount; i++ )
			{

				var color = ( i < taskIndex ) ? "lime" : ( i == taskIndex ) ? "yellow" : "white";

				taskNames.Add( string.Format( "<color={0}>{1}</color>", color, plan.tasks[ i ].Name ) );

			}

			return "<b>Executing: </b>" + String.Join( ", ", taskNames.ToArray() );

		}

		private void log( string message )
		{
			log( message, "white" );
		}

		private void log( string message, string color )
		{

			messages.Add( string.Format( "<color=white>{0} -</color> <color={2}>{1}</color>", Time.realtimeSinceStartup.ToString( "F3" ), message, color ) );

			while( messages.Count > numLinesToShow )
			{
				messages.RemoveAt( 0 );
			}

		}

		#endregion 

#endif

	}

}

