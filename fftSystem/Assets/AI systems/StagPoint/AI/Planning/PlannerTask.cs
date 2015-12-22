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

	public class PlannerTask
	{

		#region Public properties

		/// <summary>
		/// Returns a reference to the HTN Node associated with this task
		/// </summary>
		public OperatorNode Node { get; private set; }

		/// <summary>
		/// Returns the name of the task
		/// </summary>
		public string Name { get { return Node.Name; } }

		/// <summary>
		/// Returns a unique identifier for the task
		/// </summary>
		public string UID { get { return Node.UID; } }

		/// <summary>
		/// Returns the task's current status
		/// </summary>
		public TaskStatus Status { get; private set; }

		/// <summary>
		/// Gets or sets whether the editor will be paused when this task begins excuting (breakpoint)
		/// </summary>
		public bool PauseOnRun { get; set; }

		/// <summary>
		/// Gets or sets whether the task may be interrupted by generating a new plan. 
		/// </summary>
		public bool NotInterruptable { get; set; }

		/// <summary>
		/// Returns the list of preconditions that must be satisfied for this task to be executed
		/// </summary>
		public List<NodePreconditionBase> Conditions { get; private set; }

		#endregion

		#region Private runtime variables

		private IEnumerator enumerator;
		private object agent;
		private Blackboard blackboard;
		private bool isActionInitialized;

#pragma warning disable 414
		private bool isBreakpointTriggered;
#pragma warning restore 414

		#endregion

		#region Constructor

		public PlannerTask( OperatorNode node, object agent, Blackboard blackboard )
		{

			if( node == null )
				throw new ArgumentNullException( "node" );

			if( agent == null )
				throw new ArgumentNullException( "agent" );

			if( blackboard == null )
				throw new ArgumentNullException( "blackboard" );

			this.Conditions = new List<NodePreconditionBase>();

			this.Node = node;
			this.agent = agent;
			this.blackboard = blackboard;

			this.Status = TaskStatus.Running;
			this.PauseOnRun = node.PauseOnRun;
			this.NotInterruptable = node.Method.IsDefined( typeof( NotInterruptableAttribute ), true );

			this.isActionInitialized = false;
			this.isBreakpointTriggered = false;

		}

		#endregion

		#region Public methods

		public TaskStatus Tick()
		{

#if UNITY_EDITOR
			// If a breakpoint is set on this Task, then notify observers by returning a special status code
			if( this.PauseOnRun && !this.isBreakpointTriggered )
			{
				this.isBreakpointTriggered = true;
				this.Status = TaskStatus.Running;
				return TaskStatus.Breakpoint;
			}
#endif

			#region Initialize task on first run 

			// Only initialize the iterator stack on the first execution of this PlannerTask
			if( !this.isActionInitialized )
			{

				this.isActionInitialized = true;

				// Conditions must be evaluated on each tick
				if( !arePreconditionsSatisfied( this.agent, this.blackboard ) )
				{
					return this.Status = TaskStatus.Failed;
				}

				try
				{

					// Attempt to generate the task's iterator
					this.enumerator = this.Node.BuildAction( this.agent, this.blackboard );

					// If nothing was returned, raise an exception
					if( this.enumerator == null )
					{
						Debug.LogError( string.Format( "Error creating runtime task [{0}] - Unknown error", this.Node.GetPath() ), this.agent as UnityEngine.Object );
						return TaskStatus.Failed;
					}

				}
				catch( Exception err )
				{
					Debug.LogError( string.Format( "Error executing task [{0}] - {1}", this.Node.GetPath(), err.ToString() ), this.agent as UnityEngine.Object );
					return this.Status = TaskStatus.Failed;
				}

			}

			#endregion 

			var moveNext = false;

			try
			{
				moveNext = this.enumerator.MoveNext();
			}
			catch( Exception err )
			{
				Debug.LogError( string.Format( "Error executing task [{0}] - {1}", this.Node.GetPath(), err.ToString() ), this.agent as UnityEngine.Object );
				return this.Status = TaskStatus.Failed;
			}

			if( moveNext )
			{

				var taskResult = this.enumerator.Current;

				if( taskResult is TaskStatus )
				{

					var status = (TaskStatus)taskResult;

					if( status == TaskStatus.Failed )
					{
						return this.Status = TaskStatus.Failed;
					}
					else if( status == TaskStatus.Succeeded )
					{

						Node.ApplyEffects( this.agent, this.blackboard, true );

						return this.Status = TaskStatus.Succeeded;

					}

					return this.Status = TaskStatus.Running;

				}
				else
				{

					// If the value cannot be intelligently converted to another
					// TaskStatus value, then consider the current PlannerTask
					// to have failed.
					Debug.LogError( string.Format( "Unexpected return value from function {0} in Task [{1}]", this.Node.Method, this.Node.GetPath() ), this.agent as UnityEngine.Object );
					return this.Status = TaskStatus.Failed;

				}

			}
			else
			{

				// If the task does not explicitly return TaskStatus.Succeeded before it 
				// finishes executing, then the task has implicitly failed.
				Debug.LogError( string.Format( "Function {0} in Task [{1}] did not return a success or failure status code", this.Node.GetPath(), this.Node.Name ), this.agent as UnityEngine.Object );
				this.Status = TaskStatus.Failed;

			}

			return this.Status;

		}

		#endregion

		#region Private utility methods 

		private bool arePreconditionsSatisfied( object agent, Blackboard state )
		{

			try
			{

				for( int i = 0; i < this.Conditions.Count; i++ )
				{

					var condition = this.Conditions[ i ];

					if( !condition.IsConditionSatisfied( agent, state ) )
						return false;
				}

			}
			catch( Exception err )
			{
				var errorMessage = string.Format( "Exception thrown while evaluating conditions on task [{0}]: {1}", this.Node.GetPath(), err.Message );
				throw new Exception( errorMessage, err );
			}

			return true;

		}

		#endregion 

	}

}
