﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using StagPoint.Core;
using StagPoint.Planning;
using StagPoint.Planning.Components;

using Firefighter.Utilities;

[RequireComponent( typeof(TaskNetworkPlanner) )]

public class FirefighterCasualtyCarer : RescuerGeneral {
	private NavMeshAgent nav;
	private Animator m_animator;
	private float turnDirection = 1f;
	private float baseNavSpeed = 6f;
	private float baseNavAcceleration = 12f;
	
	#region Blackboard vaiables
	[BlackboardVariable]
	[NonSerialized]
	private TestCubeScript TestCube;
	
	[BlackboardVariable]
	[NonSerialized]
	private GameObject Car1;
	
	[BlackboardVariable]
	[NonSerialized]
	private GameObject Car2;
	
	[BlackboardVariable]
	[NonSerialized]
	private Vector3 InnerCircleSurveyTarget;
	
	[BlackboardVariable]
	[NonSerialized]
	private Tasks Task;
	
	#endregion
	
	
	
	private TaskNetworkPlanner planner;
	
	[NonSerialized]
	public Blackboard Blackboard;
	
//	public void setcurrentTask(int id){
//		switch (id){
//		case 0:
//			Task = Tasks.Free;
//			break;
//		case 1:
//			Task = Tasks.SurveyInnerCircle;
//			break;
//		case 2:
//			Task = Tasks.SurveyOuterCircle;
//			break;
//		case 3:
//			Task = Tasks.TriageCasualties;
//			break;
//		case 4:
//			Task = Tasks.EstablishCasualtyContact;
//			break;
//		case 5:
//			Task = Tasks.StabiliseVehicles;
//			break;
//		case 6:
//			Task = Tasks.EstablishAToolStagingArea;
//			break;
//		default:
//			Task = Tasks.Free;
//			break;
//			
//		}
//	}
	
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		Task = runningTask;
	}
	
	void OnEnable ()
	{
		planner = GetComponent<TaskNetworkPlanner>();
		
		var testCubeTarget = FindObjectsOfType<TestCubeScript> ();
		// cannot use it this way
		this.TestCube = testCubeTarget.FirstOrDefault ();
		
		this.nav = GetComponent<NavMeshAgent> ();
		
		this.m_animator = GetComponent<Animator> ();
		
		// TODO: use visual to sence these objects, however, this init here atm.
		Car1 = GameObject.Find ("Car1");
		Car2 = GameObject.Find ("Car2");
		
		InnerCircleSurveyTarget = calculateActionCircle ();
		
//		Task = Tasks.Free;
		
		// Obtain a reference to the runtime Blackboard instance
		this.Blackboard = planner.RuntimeBlackboard;
		
		// Data bind blackboard variables
		Blackboard.DataBind( this, BlackboardBindingMode.AttributeControlled );
		
	}
	
	
	#region Memberfunctions
	
	/// <summary>
	/// Calculates the action circle.
	/// How to calculate the position of the target?
	/// one car trasform.position
	/// one car with obstacles
	/// with tree transform.position
	/// with wall or something that not possible to go further transform.postion 
	/// two cars next to each other(test scene) (car1.transform.position + car2.transform.position)/2
	/// two cars have a little distance (car1.transform.position + car2.transform.position)/2
	/// two cars far away perform twice car1.transform.position and car2.transform.position
	/// TODO: only implement the test case 
	/// </summary>
	/// <returns>The center position of the action circle.</returns>
	private Vector3 calculateActionCircle(){
		// These two parameters need to be tested.
		float dist1 = 20f;
		float dist2 = 100f;
		
		// two cars accident
		if (Car1 != null && Car2 != null) {
			// two cars next to each other(test scene)
			if(Vector3.Distance(Car1.transform.position, Car2.transform.position) <= dist1)
				return new Vector3((Car1.transform.position.x + Car2.transform.position.x)/2,
				                   (Car1.transform.position.y + Car2.transform.position.y)/2,
				                   (Car1.transform.position.z + Car2.transform.position.z)/2 );
			// two cars have a little distance (car1.transform.position + car2.transform.position)/2
			else if(Vector3.Distance(Car1.transform.position, Car2.transform.position)>dist1 && 
			        Vector3.Distance(Car1.transform.position, Car2.transform.position)<=dist2)
				return Vector3.zero;
			// two cars far away perform twice car1.transform.position and car2.transform.position
			else if(Vector3.Distance(Car1.transform.position, Car2.transform.position)>dist2)
				return Vector3.zero;
		}
		// other cases
		else if(Car1 !=null && Car2 == null){
			return Vector3.zero;
		}
		return Vector3.one;
	}
	
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
	
	private float angleToValue(float angle){
		return (angle * 3.1415926f / 180f);
	}
	
	private float valueToAngle(float value){
		return (value * 180f / 3.1415926f);
	}
	
	/// <summary>
	/// Generate several points around the target for inner and outer circle survey.
	/// </summary>
	/// <returns>Points clockwise.</returns>
	/// <param name="x0">target, original point of the circle</param>
	/// <param name="x1">current position, arrived to the circle position</param>
	/// <param name="deltaAlpha">relative move angle per target.</param>
	private List<Vector3> pointsOnCircle(Vector3 x0, Vector3 x1, float deltaAlpha)
	{
		x1.y = x0.y;
		float r = Vector3.Distance (x0, x1);
		float alpha = valueToAngle((float)Math.Acos ((x1.x - x0.x)/r));
		float acumulateAlpha = alpha;
		List<Vector3> points = new List<Vector3>();
		
		while (acumulateAlpha - alpha <= 360){
			
			float x = x0.x + r * (float)Math.Cos (angleToValue(acumulateAlpha));
			float z = x0.z - r * (float)Math.Sin (angleToValue(acumulateAlpha));
			acumulateAlpha +=deltaAlpha;
			Vector3 point = new Vector3 (x, x1.y, z);
			// for debug
			//GameObject obj = new GameObject();
			//obj.transform.position = point;
			points.Add(point);
		}
		return points;
		
	}
	
	#endregion
	
	#region Generic actions
	/// <summary>
	/// Gos to object: this function is a generic operator function that controls the agent go to the target in the speed of speedMultiplier times default speed
	/// and will stop in a distance of stopDist between the target and the agent.
	/// </summary>
	/// <param name="target">Target: where the agent goes.</param>
	/// <param name="speedMultiplier">Speed multiplier: controls the speed of the agent, default value is 1f. </param>
	/// <param name="stopDist">Stop dist: controls distance where the agent should stop.</param>
	[Operator( "Go To Object", "This function is a generic operator function that controls the agent go to the target in the speed of speedMultiplier times default speed and will stop in a distance of stopDist between the target and the agent." )]
	public IEnumerator GoToObject ([ScriptParameter] MonoBehaviour target, [DefaultValue( 1f )] float speedMultiplier, [DefaultValue( 1f )] float stopDist)
	{
		
		while (target != null) {
			
			nav.SetDestination (target.transform.position);
			
			Vector3 disVec = target.transform.position - gameObject.transform.position;
			
			if (disVec.magnitude < stopDist) {
				target = null;
				yield return TaskStatus.Succeeded;
			}
			yield return TaskStatus.Running;
		}
		yield return TaskStatus.Failed;
		
	}
	
	/// <summary>
	/// Navigates to position.
	/// </summary>
	/// <returns>The to position.</returns>
	/// <param name="target">Target.</param>
	/// <param name="speedMultiplier">Speed multiplier.</param>
	/// <param name="stopDist">Stop dist.</param>
	[Operator( "Navigate to position", "This function is a generic operator function that controls the agent go to the target postion in the speed of speedMultiplier times default speed and will stop in a distance of stopDist between the target and the agent." )]
	public IEnumerator NavigateToPosition ([ScriptParameter] Vector3 target, [DefaultValue( 1f )] float speedMultiplier, [DefaultValue( 5f )] float stopDist)
	{
		nav.autoBraking = false;
		nav.destination = target;
		//nav.speed = baseNavSpeed * speedMultiplier;
		//nav.acceleration = baseNavAcceleration * speedMultiplier;
		
		while (true) {
			Vector3 disVec = target - gameObject.transform.position;
			
			if (disVec.magnitude < stopDist) {
				target = Vector3.zero;
				nav.Stop();
				yield return TaskStatus.Succeeded;
			}
			yield return TaskStatus.Running;
		}
		yield return TaskStatus.Failed;
		
	}
	
	/// <summary>
	/// Walks clockwisely and check, simply for inner circle survey and outer circle survey.
	/// </summary>
	/// <returns>The clockwise and check.</returns>
	/// <param name="target">Target.</param>
	/// <param name="speedMultiplier">Speed multiplier.</param>
	[Operator( "Walk Clock Wise", "Causes the avatar navigate the target position clockwise. " )]
	public IEnumerator walkClockwiseAndCheck ([ScriptParameter] Vector3 target, [DefaultValue( 1f )] float speedMultiplier)
	{
		
		List<Vector3> points = pointsOnCircle (target, transform.position, 30);
		int IsCheck_id= Animator.StringToHash ("IsCheck");
		bool isCheck = true;
		while (true) {
			m_animator.SetBool(IsCheck_id,false);
			nav.destination=points[0];
			if(Vector3.Distance(points[0],transform.position)<1f){
				points.RemoveAt(0);
				isCheck = true;
			}
			if((points.Count % 5 == 0) && points.Count!=0 && isCheck)
			{
				m_animator.SetBool(IsCheck_id,true);
				isCheck = false;
				transform.rotation = Quaternion.LookRotation(target-transform.position);
				Debug.Log(target);
				//nav.Stop();
			}
			if(points.Count == 0){
				nav.autoBraking = true;
				nav.destination = transform.position;
				nav.Stop();
				yield return TaskStatus.Succeeded;
			}
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
		
		return TaskStatus.Running;
		
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
	
	public TaskStatus Stop()
	{
		
		//nav.autoBraking = true;
		//nav.destination = transform.position;
		nav.Stop();
		InnerCircleSurveyTarget = Vector3.zero;
		return TaskStatus.Succeeded;
		
	}
	
	#endregion
}
