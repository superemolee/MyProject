// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagPoint.Core
{

	/// <summary>
	/// Describes the current state of a TaskNetwork instance
	/// </summary>
	public enum PlannerStatus
	{
		/// <summary>
		/// The planner does not currently have an active plan
		/// </summary>
		NoPlan = 0,
		/// <summary>
		/// The plan did not complete successfully
		/// </summary>
		Failed,
		/// <summary>
		/// The plan has completed successfully
		/// </summary>
		Succeeded,
		/// <summary>
		/// The plan requires more time to run to completion
		/// </summary>
		Running
	}

	/// <summary>
	/// Describes the current state of a PlannerTask instance
	/// </summary>
	public enum TaskStatus
	{
		/// <summary>
		/// The task did not complete successfully
		/// </summary>
		Failed = 0,
		/// <summary>
		/// The task has completed successfully
		/// </summary>
		Succeeded,
		/// <summary>
		/// The task requires more time to run to completion
		/// </summary>
		Running,
		/// <summary>
		/// Used internally to implement debug breakpoints in the planner
		/// </summary>
		Breakpoint
	}

	/// <summary>
	/// Specifies how an object will be data-bound to a Blackboard instance
	/// </summary>
	public enum BlackboardBindingMode
	{
		/// <summary>
		/// Only fields and properties with a DataBoundVariable attribute specified
		/// will be data-bound.
		/// </summary>
		AttributeControlled,
		/// <summary>
		/// All public fields and properties will be data-bound, except for those 
		/// that have a DoNotDatabind attribute defined.
		/// </summary>
		AllPublicFields
	}

	/// <summary>
	/// Indicates how a Composite Node should be evaluated - whether the node requires
	/// that all child nodes can be evaluated, or whether to stop evaluating and return
	/// success on the first child node that can be evaluated successfully.
	/// </summary>
	[TypeRename( "StagPoint.Core.MethodEvaluationMode" )]
	public enum DecompositionMode
	{
		/// <summary>
		/// Either all child nodes can be successfully evaluated, or the node
		/// being evaluated will fail validation
		/// </summary>
		SelectAll = 0,
		/// <summary>
		/// All child nodes are evaluated in order, until either one can be 
		/// successfully evaluated or all child nodes have failed evaluation.
		/// If any child node can be evaluated, then this node can be evaluated.
		/// </summary>
		SelectOne,
	}

}
