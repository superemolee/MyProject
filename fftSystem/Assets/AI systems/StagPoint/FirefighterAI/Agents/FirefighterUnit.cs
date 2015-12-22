using System;
using System.Linq;
using System.Collections;

using UnityEngine;

using StagPoint.Core;
using StagPoint.Planning;
using StagPoint.Planning.Components;

[RequireComponent( typeof(TaskNetworkPlanner) )]
public class FirefighterUnit : MonoBehaviour
{

	private NavMeshAgent nav;
	private float turnDirection = 1f;
	private float baseNavSpeed = 6f;
	private float baseNavAcceleration = 12f;

	[BlackboardVariable]
	[NonSerialized]
	private TestCubeScript TestCube;

	private TaskNetworkPlanner planner;

	[NonSerialized]
	public Blackboard Blackboard;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	void OnEnable ()
	{
		planner = GetComponent<TaskNetworkPlanner>();
		var testCubeTarget = FindObjectsOfType<TestCubeScript> ();
		// cannot use it this way
		this.TestCube = testCubeTarget.FirstOrDefault ();

		this.nav = GetComponent<NavMeshAgent> ();

		// Obtain a reference to the runtime Blackboard instance
		this.Blackboard = planner.RuntimeBlackboard;
		
		// Data bind blackboard variables
		Blackboard.DataBind( this, BlackboardBindingMode.AttributeControlled );
		
	}

#region Memberfunctions

	private float distance2D (Vector3 lhs, Vector3 rhs)
	{
		rhs.y = lhs.y;
		return Vector3.Distance (lhs, rhs);
	}

	private Vector3 restrictToNavmesh (Vector3 position)
	{
		NavMeshHit navHit;
		NavMesh.SamplePosition (position, out navHit, 10, 0xff);
		
		return navHit.position;
		
	}

#endregion

#region Generic actions
	[Operator( "Go To Object", "Causes the agent to navigate to the object's position. If the object moves, this action will cause the agent to follow the object." )]
	public IEnumerator GoToObject ([ScriptParameter] MonoBehaviour target, [DefaultValue( 1f )] float speedMultiplier)
	{

		while (target != null) {

			nav.SetDestination (target.transform.position);
		
			Vector3 disVec = target.transform.position - gameObject.transform.position;

			if (disVec.magnitude < 2.5f) {
				target = null;
				yield return TaskStatus.Succeeded;
			}
			yield return TaskStatus.Running;
		}
		yield return TaskStatus.Failed;

		
	}

	[Operator( "Walk Clock Wise", "Causes the agent to navigate to the object's position. If the object moves, this action will cause the agent to follow the object." )]
	public IEnumerator walkClockwise ([ScriptParameter] MonoBehaviour target, [DefaultValue( 1f )] float speedMultiplier)
	{
		
		// Note: I would never abuse Unity's navigation in production for a task like this, but it 
		// didn't seem prudent to further complicate the example by also adding better navigation.
		
//		var targetPosition = Vector3.one * float.MaxValue;
		
		//		nav.autoBraking = false;
		//		nav.speed = baseNavSpeed * speedMultiplier;
		//		nav.acceleration = baseNavAcceleration * speedMultiplier;
		
		while (target != null && target.gameObject != null) {
			
			//			// Need to periodically re-path. We'll do that whenever the object changes position
			//			if( distance2D( target.transform.position, targetPosition ) > 1f )
			//			{
			//				targetPosition = target.transform.position;
			////				nav.destination = restrictToNavmesh( targetPosition );
			//			}
			
			// If we've reached (close enough to) the destination, return success
			//			if( distance2D( transform.position, target.transform.position ) <= 1f )
			//			{
			////				nav.autoBraking = true;
			//				yield return TaskStatus.Succeeded;
			//			}
			
			yield return TaskStatus.Running;
			
		}
		
		yield return TaskStatus.Failed;
		
		
	}

#endregion



#region Scene assessment and safety

	public TaskStatus moveObjectAway ()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus placeToolInActionCircle ()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus placeSalvageSheet ()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus placeToolInSalvageSheet ()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus wearProtectiveClothing ()
	{
		
		return TaskStatus.Succeeded;
		
	}

#endregion
}
