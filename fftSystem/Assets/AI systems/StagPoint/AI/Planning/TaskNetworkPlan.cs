// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using StagPoint.Core;
using StagPoint.Planning;

using UnityEngine;

namespace StagPoint.Core
{

	public class TaskNetworkPlan
	{

		#region Object pooling 

		private static ListEx<TaskNetworkPlan> pool = new ListEx<TaskNetworkPlan>();

		public static TaskNetworkPlan Obtain( IList<GraphNodeBase> nodes, object agent, Blackboard blackboard )
		{

			if( agent == null )
				throw new ArgumentNullException( "agent" );

			if( blackboard == null )
				throw new ArgumentNullException( "blackboard" );

			if( nodes == null )
				throw new ArgumentNullException( "nodes" );

			if( nodes.Count == 0 )
				throw new ArgumentException( "Plan does not contain any tasks" );

			if( !( nodes[ 0 ] is RootNode ) )
				throw new ArgumentException( "The plan does not start at the root" );

			var instance = pool.Count > 0 ? pool.Pop() : new TaskNetworkPlan();

			instance.Agent = agent;
			instance.nodes.AddRange( nodes );

			var compositePreconditionIndex = 0;

			for( int i = 1; i < nodes.Count; i++ )
			{

				var currentNode = nodes[ i ];

				var operatorNode = currentNode as OperatorNode;
				if( operatorNode != null )
				{

					var newTask = new PlannerTask( operatorNode, agent, blackboard );

					// Consolidate the preconditions for all composite nodes leading to the operator node
					while( ++compositePreconditionIndex < i )
					{

						var compositeNode = nodes[ compositePreconditionIndex ];

						if( !( compositeNode is CompositeNode || compositeNode is LinkNode ) )
						{
							var planString = string.Join( ", ", nodes.Select( x => x.Name ).ToArray() );
							throw new Exception( string.Format( "Expected a composite node or link node at index {0}, found {1} - [{2}]", compositePreconditionIndex, compositeNode.Name, planString ) );
						}

						newTask.Conditions.AddRange( compositeNode.Conditions );

					}

					// Only after all predecessor node's conditions have been added can we 
					// add the operator node's conditions
					newTask.Conditions.AddRange( operatorNode.Conditions );

					instance.tasks.Add( newTask );

				}

			}

			instance.TaskCount = instance.tasks.Count;
			instance.Status = ( instance.TaskCount > 0 ) ? TaskStatus.Running : TaskStatus.Failed;
			instance.CurrentTask = ( instance.TaskCount > 0 ) ? instance.tasks[ 0 ] : null;

			return instance;

		}

		public void Release()
		{

			this.Agent = null;
			this.CurrentTask = null;

			this.Status = TaskStatus.Failed;
			this.TaskCount = 0x00;

			this.currentTaskIndex = 0x00;

			this.mtr.Clear();
			this.nodes.Clear();
			this.tasks.Clear();

#if UNITY_EDITOR
			this.failedNodes.Clear();
#endif

		}

		#endregion 

		#region Public properties

		public ListEx<GraphNodeBase> nodes = new ListEx<GraphNodeBase>();
		public ListEx<PlannerTask> tasks = new ListEx<PlannerTask>();

#if UNITY_EDITOR
		/// <summary>
		/// While debugging in the editor, this list will contain the nodes that were searched
		/// but had conditions which failed. This field will be removed at runtime via
		/// conditional compilation directives, and is only included when running in the editor.
		/// </summary>
		public ListEx<GraphNodeBase> failedNodes = new ListEx<GraphNodeBase>( 24 );
#endif

		public object Agent { get; private set; }
		public int TaskCount { get; private set; }
		public TaskStatus Status { get; private set; }
		public PlannerTask CurrentTask { get; private set; }

		#endregion 

		#region Private variables 

		private static ListEx<int> tempMTR = new ListEx<int>( 12 );

		private ListEx<int> mtr = new ListEx<int>( 12 );
		private int currentTaskIndex = 0;

		#endregion 

		#region Constructors

		private TaskNetworkPlan()
		{
		}

		#endregion 

		#region Public methods 

		/// <summary>
		/// Execute exactly one step of the current plan
		/// </summary>
		public TaskStatus Tick()
		{

			if( this.Agent == null )
				throw new NullReferenceException( "The agent that created this plan has been destroyed" );

			if( this.Status != TaskStatus.Running )
				return Status;

			this.CurrentTask = tasks[ currentTaskIndex ];
			this.Status = CurrentTask.Tick();

			if( this.Status == TaskStatus.Breakpoint )
			{
				this.Status = TaskStatus.Running;
				return TaskStatus.Breakpoint;
			}

			if( Status == TaskStatus.Running )
			{
				return TaskStatus.Running;
			}

			if( Status == TaskStatus.Succeeded )
			{

				if( ++currentTaskIndex >= TaskCount )
					return TaskStatus.Succeeded;

				this.CurrentTask = tasks[ currentTaskIndex ];

				return Status = TaskStatus.Running;

			}

			return Status = TaskStatus.Failed;

		}

		internal static bool IsHigherPriority( IList<GraphNodeBase> proposedPlan, TaskNetworkPlan plan )
		{

			if( plan == null || plan.Status != TaskStatus.Running )
			{
				return true;
			}

			tempMTR.Clear();
			for( int i = 0; i < proposedPlan.Count; i++ )
			{
				var node = proposedPlan[ i ];
				if( node is RootNode || node is CompositeNode )
				{
					tempMTR.Add( node.Index );
				}
			}

			var otherMTR = plan.getMTR();

			// Compare the Method Traversal Record for each plan. If any method in this plan 
			// has a lower index (and therefor higher priority) than the matching method in 
			// the other plan, then this plan has a higher priority overall.
			var count = Mathf.Min( tempMTR.Count, otherMTR.Count );
			for( int i = 0; i < count; i++ )
			{

				if( tempMTR[ i ] < otherMTR[ i ] )
					return true;

			}

			return tempMTR.Count > otherMTR.Count;

		}

		/// <summary>
		/// Returns TRUE if this plan is a higher priority than <paramref name="otherPlan"/>. Priority is 
		/// determined implicitly through the position in the task network tree, where branches that are
		/// placed higher in the tree are considered to have higher priority than the branches below them.
		/// </summary>
		/// <param name="otherPlan">The plan to be compared to this plan</param>
		/// <returns></returns>
		public bool IsHigherPriority( TaskNetworkPlan otherPlan )
		{

			if( otherPlan == null )
				return true;
			
			// Running plans always have higher priority than plans that have already finished
			if( this.Status == TaskStatus.Running && otherPlan.Status != TaskStatus.Running )
				return true;

			// Obtain the Method Traversal Record for both plans
			var thisMTR = this.getMTR();
			var otherMTR = otherPlan.getMTR();

			// Compare the Method Traversal Record for each plan. If any method in this plan 
			// has a lower index (and therefor higher priority) than the matching method in 
			// the other plan, then this plan has a higher priority overall.
			var count = Mathf.Min( thisMTR.Count, otherMTR.Count );
			for( int i = 0; i < count; i++ )
			{

				if( thisMTR[ i ] < otherMTR[ i ] )
					return true;

			}

			return thisMTR.Count > otherMTR.Count;

		}

		#endregion

		#region Private utility methods 

		internal ListEx<int> getMTR()
		{

			if( this.mtr.Count == 0 )
			{

				// Create a Method Traversal Record by storing the index of 
				// each Method in the plan. Primitive tasks are ignored.
				for( int i = 0; i < this.nodes.Count; i++ )
				{

					var node = this.nodes[ i ];

					if( node is RootNode || node is CompositeNode )
					{
						this.mtr.Add( node.Index );
					}

				}

			}

			return this.mtr;

		}

		#endregion 

	}

}
