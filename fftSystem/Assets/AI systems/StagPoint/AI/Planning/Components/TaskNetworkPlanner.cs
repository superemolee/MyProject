// Copyright (c) 2014 StagPoint Consulting

#if UNITY_WINRT
#error HTN Planner for Unity does not support the Windows App Store and Windows Mobile platforms
#endif

#define TRACK_FAILED_NODES

using System;
using System.Collections.Generic;

using UnityEngine;

namespace StagPoint.Planning.Components
{

	using StagPoint.Core;

	[AddComponentMenu( "StagPoint/Task Network Planner", -1 )]
	public partial class TaskNetworkPlanner : MonoBehaviour
	{

		#region Debugging 

		public static MonoBehaviour DebugTarget;

		#endregion 

		#region Public delegates and events

		public delegate void PlannerCallback( TaskNetworkPlanner planner );
		public delegate void PlanCallback( TaskNetworkPlanner planner, TaskNetworkPlan plan );
		public delegate void TaskCallback( TaskNetworkPlanner planner, PlannerTask task );

		#region Planning events 

		/// <summary>
		/// This event is raised whenever the planner is attempting to generate a new plan
		/// </summary>
		public event PlannerCallback PlanningStarted;

		/// <summary>
		/// This event is raised whenever a planning attempt has completed
		/// </summary>
		public event PlannerCallback PlanningEnded;

		/// <summary>
		/// This event is raised whenever a new plan is generated
		/// </summary>
		public event PlanCallback PlanGenerated;

		/// <summary>
		/// This event is raised whenever a plan is discarded due to being lower priority than the 
		/// currently running plan
		/// </summary>
		public event PlannerCallback PlanDiscarded;

		/// <summary>
		/// This event is raised whenever the planner fails to generate a valid plan
		/// </summary>
		public event PlannerCallback PlanningFailed;

		#endregion 

		#region Plan execution events 

		/// <summary>
		/// This event is raised whenever a plan starts executing 
		/// </summary>
		public event PlanCallback PlanExecuted;

		/// <summary>
		/// This event is raised whenever a running plan is aborted due to a new plan being exeuted
		/// </summary>
		public event PlanCallback PlanAborted;

		/// <summary>
		/// This event is raised whenever a running plan has completed successfully
		/// </summary>
		public event PlanCallback PlanCompleted;

		/// <summary>
		/// This event is raised whenever a running plan has returned a failure status code
		/// </summary>
		public event PlanCallback PlanFailed;

		#endregion 

		#region Task Execution events 

		/// <summary>
		/// This event is raised whenever a new task has begun execution
		/// </summary>
		public event TaskCallback TaskExecuted;

		/// <summary>
		/// This event is raised whenever a task has successfully completed execution
		/// </summary>
		public event TaskCallback TaskSucceeded;

		/// <summary>
		/// This event is raised whenever a task fails during execution
		/// </summary>
		public event TaskCallback TaskFailed;

		#endregion 

		#endregion

		#region Public fields

		/// <summary>
		/// Gets or sets the TaskNetworkGraph that this planner will use for planning
		/// </summary>
		public TaskNetworkGraph Graph;

		/// <summary>
		/// Gets or sets the agent script instance that this planner will be generating plans for
		/// </summary>
		public MonoBehaviour Agent;

		/// <summary>
		/// Gets or sets whether the planner will automatically perform periodic replanning
		/// </summary>
		public bool AutoReplan = true;

		/// <summary>
		/// Gets or sets the amount of time between replanning attempts
		/// </summary>
		public float AutoReplanInterval = 0.2f;

		#endregion

		#region Public properties

		/// <summary>
		/// Returns a reference to the Blackboard definition that is associated with this planner. This 
		/// is only used during design and editing, and should not be used by agents, sensors, and other
		/// components at runtime.
		/// </summary>
		public Blackboard BlackboardDefinition
		{
			get
			{

				if( Graph == null )
					return null;

				return Graph.BlackboardDefinition;

			}
		}

		/// <summary>
		/// Returns a reference to the Blackboard instance used during runtime planning. This is the 
		/// instance that should be used by agents, sensors, etc.
		/// </summary>
		public Blackboard RuntimeBlackboard
		{
			get
			{

				if( Application.isPlaying )
				{

					if( runtimeState == null )
					{
						runtimeState = BlackboardDefinition.Clone();
					}

					return runtimeState;

				}

				return BlackboardDefinition;

			}
		}

		/// <summary>
		/// Returns the current planner status
		/// </summary>
		public PlannerStatus Status { get; private set; }

		/// <summary>
		/// Gets a reference to the plan currently being executed
		/// </summary>
		public TaskNetworkPlan Plan { get; private set; }

		#endregion

		#region Private variables 

		private Blackboard runtimeState;
		private PlannerTask lastTaskExecuted = null;

		private float timeTillReplan = 0f;

		#endregion 

		#region Monobehaviour events 

#if UNITY_EDITOR

		public void Start()
		{
			
			OnValidate();

		}

		public void OnDisable()
		{
			this.Plan = null;
			this.Status = PlannerStatus.NoPlan;
		}

		public void OnValidate()
		{

			try
			{

				if( this.Graph != null )
				{
					this.Graph.Validate();
				}

			}
			catch( Exception err )
			{
				Debug.LogError( string.Format( "Validation failure for graph {0} : {1}", this.name, err.ToString() ) );
			}

		}

#endif

		public void Update()
		{

			if( this.Agent == null )
			{
				this.enabled = false;
				throw new InvalidOperationException( "The TaskNetworkPlanner.Agent field has not been assigned: " + this.name );
			}

			doAutoPlanning();

			if( Plan == null )
			{
				this.Status = PlannerStatus.NoPlan;
				return;
			}

			var taskStatus = Plan.Tick();
			switch( taskStatus )
			{
				case TaskStatus.Breakpoint:
					handleBreakpoint();
					break;
				case TaskStatus.Running:
					this.Status = PlannerStatus.Running;
					if( Plan.CurrentTask != lastTaskExecuted )
					{
						raiseTaskSucceeded( lastTaskExecuted );
						raiseTaskExecuted();
						lastTaskExecuted = Plan.CurrentTask;
					}
					break;
				case TaskStatus.Succeeded:
					this.Status = PlannerStatus.Succeeded;
					raisePlanCompleted( Plan );
					break;
				case TaskStatus.Failed:
					this.Status = PlannerStatus.Failed;
					raisePlanFailed( Plan );
					break;
				default:
					throw new InvalidOperationException( "Unhandled TaskStatus value: " + taskStatus );
			}

		}

		#endregion

		#region Public methods

		/// <summary>
		/// Generate a new plan for the <paramref name="agent"/> based on the current world state as
		/// described by the <paramref name="blackboard"/>.
		/// </summary>
		/// <param name="agent">The agent for which the plan is being generated. This object instance must be
		/// of the same type as the type for which the TaskNetworkGraph was developed</param>
		/// <param name="blackboard">The current world state required by the planner</param>
		/// <returns></returns>
		public TaskNetworkPlan GeneratePlan()
		{

			// If the planner is currently executing a task marked NotInterruptable, do not generate
			// any new plans.
			if( !canInterruptCurrentPlan() )
			{
				raisePlanDiscarded();
				return null;
			}

			try
			{

				raisePlanningStarted();

				// Initialize graph on first use. This is an opportunity for the graph to compile any
				// script expressions, allocate any pooled objects, etc.
				if( !Graph.IsInitialized )
				{
					Graph.Initialize( this.Agent, this.BlackboardDefinition );
				}

				// Obtain a reference to the Blackboard instance that is used at runtime
				var blackboard = this.RuntimeBlackboard;

				using( var builder = PlanBuilder.Obtain( Agent, blackboard.Clone() ) )
				{

					var planNodes = builder.BuildPlan( Graph.RootNode );

					if( builder.PlanWasDiscarded )
					{
						raisePlanDiscarded();
						return null;
					}

					if( planNodes == null )
					{
						raisePlanningFailed();
						return null;
					}

					if( !TaskNetworkPlan.IsHigherPriority( planNodes, this.Plan ) )
					{
						raisePlanDiscarded();
						return null;
					}

					var newPlan = TaskNetworkPlan.Obtain( planNodes, this.Agent, this.RuntimeBlackboard );

#if UNITY_EDITOR && TRACK_FAILED_NODES
					newPlan.failedNodes.AddRange( builder.FailedNodes );
#endif

					raisePlanGenerated( newPlan );

					return newPlan;

				}
			}
			finally
			{
				raisePlanningEnded();
			}

		}

		public void ExecutePlan( TaskNetworkPlan plan )
		{

			if( Plan != null )
			{

				if( Plan.Status == TaskStatus.Running )
				{
					raisePlanAborted( this.Plan );
				}

				this.Plan.Release();

			}

			this.Plan = plan;
			this.Status = ( Plan != null && Plan.TaskCount > 0 ) ? PlannerStatus.Running : PlannerStatus.NoPlan;
			this.lastTaskExecuted = null;

			if( plan != null )
			{
				raisePlanExecuted( plan );
			}

			if( DebugTarget == this.Agent )
			{
				plan.tasks[ 0 ].PauseOnRun = true;
			}

		}

		/// <summary>
		/// Aborts the current plan (if any) and sets the planner's Status to NoPlan.
		/// This can be used, for instance, to force the planner to regenerate a new plan.
		/// </summary>
		public void ClearPlan()
		{

			if( this.Status == PlannerStatus.Running )
				raisePlanDiscarded();

			if( this.Plan != null )
			{
				this.Plan.Release();
			}

			this.Plan = null;
			this.Status = PlannerStatus.NoPlan;

		}

		#endregion

		#region Event wrappers 

		private void raiseTaskSucceeded( PlannerTask task )
		{
			if( task != null && this.TaskSucceeded != null )
			{
				this.TaskSucceeded( this, task );
			}
		}

		private void raiseTaskExecuted()
		{
			if( this.TaskExecuted != null )
			{
				this.TaskExecuted( this, Plan.CurrentTask );
			}
		}

		private void raisePlanningStarted()
		{
			if( this.PlanningStarted != null )
			{
				this.PlanningStarted( this );
			}
		}

		private void raisePlanningEnded()
		{
			if( this.PlanningEnded != null )
			{
				this.PlanningEnded( this );
			}
		}

		private void raisePlanCompleted( TaskNetworkPlan plan )
		{
			if( this.PlanCompleted != null )
			{
				this.PlanCompleted( this, plan );
			}
		}

		private void raisePlanFailed( TaskNetworkPlan plan )
		{

			if( this.TaskFailed != null )
			{
				this.TaskFailed( this, plan.CurrentTask );
			}

			if( this.PlanFailed != null )
			{
				this.PlanFailed( this, plan );
			}

		}

		private void raisePlanGenerated( TaskNetworkPlan plan )
		{
			if( this.PlanGenerated != null )
			{
				this.PlanGenerated( this, plan );
			}
		}

		private void raisePlanDiscarded()
		{
			if( this.PlanDiscarded != null )
			{
				this.PlanDiscarded( this );
			}
		}

		private void raisePlanExecuted( TaskNetworkPlan plan )
		{
			if( this.PlanExecuted != null )
			{
				this.PlanExecuted( this, plan );
			}
		}

		private void raisePlanAborted( TaskNetworkPlan plan )
		{
			if( this.PlanAborted != null )
			{
				this.PlanAborted( this, plan );
			}
		}

		private void raisePlanningFailed()
		{
			if( this.PlanningFailed != null )
			{
				this.PlanningFailed( this );
			}
		}

		#endregion 

		#region Private utility methods

		private bool canInterruptCurrentPlan()
		{
			//Plan != null && Plan.CurrentTask != null && Plan.CurrentTask.NotInterruptable

			if( this.Plan == null )
				return true;

			if( Plan.Status != TaskStatus.Running && Plan.Status != TaskStatus.Breakpoint )
				return true;

			var task = Plan.CurrentTask;
			if( task == null || !task.NotInterruptable )
				return true;

			return task.Status == TaskStatus.Failed || task.Status == TaskStatus.Succeeded;

		}

		private void handleBreakpoint()
		{

#if UNITY_EDITOR

			// If the user is currently in debug step mode, and this planner's agent is not the 
			// one being debugged, ignore breakpoints.
			if( DebugTarget != null && DebugTarget.gameObject != null && DebugTarget != Agent )
				return;

			var behavior = Plan.CurrentTask;

			Debug.LogWarning( string.Format( "Planner breakpoint triggered: Task [{0}] on object {1}", behavior.Name, Agent.name ) );

			UnityEditor.Selection.activeObject = Agent.gameObject;
			UnityEditor.EditorApplication.isPaused = true;

			UnityEditor.EditorApplication.ExecuteMenuItem( "Window/Task Network Graph Editor" );

#endif

		}

		private void doAutoPlanning()
		{

			if( !this.AutoReplan )
				return;

			this.timeTillReplan -= Time.deltaTime;

			var noPlan = this.Plan == null || this.Status != PlannerStatus.Running;

			if( noPlan || timeTillReplan <= 0 )
			{

				timeTillReplan += AutoReplanInterval;

				var plan = GeneratePlan();
				if( plan != null )
				{
					this.ExecutePlan( plan );
				}

			}

		}

		#endregion

		#region Nested types

		private class PlanBuilder : IDisposable
		{

			#region Object pooling 

			private static ListEx<PlanBuilder> pool = new ListEx<PlanBuilder>();

			public static PlanBuilder Obtain( object agent, Blackboard blackboard )
			{

				var instance = pool.Count > 0 ? pool.Pop() : new PlanBuilder();
				instance.blackboard = blackboard;
				instance.agent = agent;

				return instance;

			}

			public void Release()
			{

#if UNITY_EDITOR && TRACK_FAILED_NODES
				this.FailedNodes.Clear();
#endif

				this.agent = null;

				this.blackboard.Release();
				this.blackboard = null;

				this.activePlan = null;

				this.plan.Clear();

				pool.Add( this );

			}

			#endregion 

			#region Public properties 

#if UNITY_EDITOR && TRACK_FAILED_NODES
			/// <summary>
			/// While debugging in the editor, this list will contain the nodes that were searched
			/// but had conditions which failed. This field will be removed at runtime via
			/// conditional compilation directives, and is only included when running in the editor.
			/// </summary>
			public ListEx<GraphNodeBase> FailedNodes = new ListEx<GraphNodeBase>();
#endif

			public bool PlanWasDiscarded { get { return this.planWasDiscarded; } }

			#endregion 

			#region Private variables

			private Blackboard blackboard;
			private object agent;

			private TaskNetworkPlan activePlan;
			private ListEx<GraphNodeBase> plan = new ListEx<GraphNodeBase>( 24 );

			private bool planWasDiscarded = false;

			#endregion 

			#region Public methods 

			public IList<GraphNodeBase> BuildPlan( GraphNodeBase root )
			{

				this.planWasDiscarded = false;

#if UNITY_EDITOR && TRACK_FAILED_NODES
				this.FailedNodes.Clear();
#endif

				if( searchForPlan( root, 0 ) && this.plan.Count > 0 )
				{
					return this.plan;
				}

				return null;

			}

			#endregion 

			#region Private utility methods 

			private bool searchForPlan( GraphNodeBase node, int depth )
			{

				// Ensure that the planner does not get stuck in an infinite loop if the 
				// graph is malformed (user unconditionally linked a LinkNode to an
				// ancestor node, etc)
				if( depth >= 256 )
				{
					Debug.LogError( "Exceeded plan nesting depth. Does the graph contain an invalid cycle?", this.agent as UnityEngine.Object );
					return false;
				}

				// If the current node would result in a plan with a lower priority than the current
				// plan, then don't bother searching.
				if( canPruneThisNode( activePlan, node ) )
				{
					this.planWasDiscarded = true;
					return false;
				}

				// If this node's preconditions cannot be satisfied then the current plan
				// needs to be aborted.
				try
				{
					if( !node.ArePreconditionsSatisfied( this.agent, this.blackboard ) )
					{
#if UNITY_EDITOR && TRACK_FAILED_NODES
						FailedNodes.Add( node );
#endif
						return false;
					}
				}
				catch( Exception err )
				{
					var errorMessage = string.Format( "Exception thrown while evaluating conditions on task [{0}]: {1}", node.GetPath(), err.Message );
					Debug.LogError( errorMessage, this.agent as UnityEngine.Object );
					return false;
				}

				// Links just redirect to another part of the graph
				if( node is LinkNode )
				{

					var link = (LinkNode)node;

					if( link.LinkedNode == null )
						return false;

					plan.Add( node );

					node = link.LinkedNode;

				}

				// Operators cannot be decomposed, just add them to the plan in progress
				if( node is OperatorNode )
				{

					if( node.ApplyEffects( this.agent, this.blackboard, false ) != TaskStatus.Succeeded )
						return false;

					plan.Add( node );

					return true;

				}

				// Decompose compound nodes according to node type
				if( isSelectorNode( node ) )
				{
					// A selector ('OR' mode) will go through each child in turn, until it 
					// finds one that results in a successful plan, and returns that plan.
					if( decomposeSelector( node, depth + 1 ) )
					{
						return true;
					}
				}
				else if( node is CompositeNode )
				{
					// A method ('AND' mode) will only return a plan if all children can
					// be successfully evaluated and included in the plan
					if( decomposeSequence( node, depth + 1 ) )
					{
						return true;
					}
				}

				return false;

			}

			private bool decomposeSequence( GraphNodeBase node, int depth )
			{

				using( var currentState = this.blackboard.Push() )
				{

					var subTasks = node.ChildNodes;

					if( subTasks.Count == 0 )
						return false;

					var startIndex = plan.Count;
					plan.Add( node );

					for( int i = 0; i < subTasks.Count; i++ )
					{

						if( !subTasks[ i ].IsEnabled )
							continue;

						var planGenerated = searchForPlan( subTasks[ i ], depth + 1 );
						if( !planGenerated )
						{
							rollback( startIndex );
							return false;
						}

					}

					if( plan.Count == startIndex )
					{
						rollback( startIndex );
						return false;
					}

					return true;

				}

			}

			private bool decomposeSelector( GraphNodeBase node, int depth )
			{

				using( var currentState = this.blackboard.Push() )
				{

					var subTasks = node.ChildNodes;

					if( subTasks.Count == 0 )
						return false;

					var startIndex = plan.Count;

					for( int i = 0; i < subTasks.Count; i++ )
					{

						if( !subTasks[ i ].IsEnabled )
							continue;

						currentState.Clear();
						this.plan.Add( node );

						var planGenerated = searchForPlan( subTasks[ i ], depth + 1 );
						if( planGenerated )
						{
							return true;
						}

						rollback( startIndex );

					}

					return false;

				}

			}

			private void rollback( int startOfBranch )
			{
				while( this.plan.Count > startOfBranch )
				{
					this.plan.Pop();
				}
			}

			private bool isSelectorNode( GraphNodeBase node )
			{

				if( node is RootNode )
					return true;

				if( node is CompositeNode )
				{
					if( ( (CompositeNode)node ).Mode == DecompositionMode.SelectOne )
						return true;
				}

				return false;

			}

			private bool canPruneThisNode( TaskNetworkPlan activePlan, GraphNodeBase node )
			{

				if( activePlan == null || activePlan.Status != TaskStatus.Running )
					return false;

				if( plan.Count > 1 )
					return false;

				if( !( node is CompositeNode ) )
					return false;

				var activeMTR = activePlan.getMTR();
				if( activeMTR.Count < 2 )
					return false;

				return activeMTR[ 1 ] < node.Index;

			}

			#endregion 

			#region IDisposable Members

			public void Dispose()
			{
				this.Release();
			}

			#endregion

		}

		#endregion 


	}

}
