using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using StagPoint.Core;
using StagPoint.Planning;
using StagPoint.Planning.Components;

using Firefighter.Utilities;

[RequireComponent( typeof(TaskNetworkPlanner) )]
public class FirefighterToolOperator : RescuerGeneral
{
	private NavMeshAgent nav;
	private Animator m_animator;
	private float turnDirection = 1f;
	private float baseNavSpeed = 6f;
	private float baseNavAcceleration = 12f;

#region Blackboard vaiables

	[BlackboardVariable]
	[NonSerialized]
	private GameObject
		Car1;
	
	[BlackboardVariable]
	[NonSerialized]
	private GameObject
		Car2;
	
	[BlackboardVariable]
	[NonSerialized]
	private Vector3
		InnerCircleSurveyTarget;

	[BlackboardVariable]
	[NonSerialized]
	private Tasks
		Task;

	[BlackboardVariable]
	[NonSerialized]
	private VehicleRestingOn
		VehicleOn;

	[BlackboardVariable]
	[NonSerialized]
	private int
		StablePointSillsCounter;

	[BlackboardVariable]
	[NonSerialized]
	private int
		StablePointWheelsCounter;

	[BlackboardVariable]
	[NonSerialized]
	private int
		ToughGlassCount;


//	[BlackboardVariable]
//	[NonSerialized]
//	private VectorList
//		StablePointSills;
//	
//	[BlackboardVariable]
//	[NonSerialized]
//	private VectorList
//		StablePointWheels;

	[BlackboardVariable]
	[NonSerialized]
	private bool
		IsToolInToolStageArea;

	[BlackboardVariable]
	[NonSerialized]
	private string
		TooltoFind;

	[BlackboardVariable]
	[NonSerialized]
	private GameObject
		Tool;

//	[BlackboardVariable]
//	[NonSerialized]
//	private bool
//		hasTool;

	[BlackboardVariable]
	[NonSerialized]
	private GameObject
		CurrentStablePointSill;

	[BlackboardVariable]
	[NonSerialized]
	private GameObject
		CurrentStablePointWheel;

	[BlackboardVariable]
	[NonSerialized]
	private Vector3
		SpareLoc;

#endregion



	private TaskNetworkPlanner planner;

	[NonSerialized]
	public Blackboard
		Blackboard;


	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{

		Task = runningTask;
		StablePointSillsCounter = Car1.GetComponent<CrashedCar> ().StablePointSills.Count;
		StablePointWheelsCounter = Car1.GetComponent<CrashedCar> ().StablePointWheels.Count;
		if (StablePointSillsCounter > 0)
			CurrentStablePointSill = Car1.GetComponent<CrashedCar> ().StablePointSills.First<GameObject> ();
		if (StablePointWheelsCounter > 0)
			CurrentStablePointWheel = Car1.GetComponent<CrashedCar> ().StablePointWheels.First<GameObject> ();
		if (runningTask == Tasks.StableSills) {
			TooltoFind = "StableTool_2";
		} else if (runningTask == Tasks.StableWheels) {
			TooltoFind = "StableTool_1";
		} else if(runningTask == Tasks.ManageGlass){
			TooltoFind = "Klebebandabroller_1";
		}
		taskSetter ();
	}

	void OnEnable ()
	{
		planner = GetComponent<TaskNetworkPlanner> ();


		this.nav = GetComponent<NavMeshAgent> ();

		this.m_animator = GetComponent<Animator> ();

		// TODO: use visual to sence these objects, however, this init here atm.
		Car1 = GameObject.Find ("Car1");
		Car2 = GameObject.Find ("Car2");
		InnerCircleSurveyTarget = calculateActionCircle ();
		Task = Tasks.Free;
		VehicleOn = VehicleRestingOn.Wheels; // TODO: defined the game logic later for how to control this information.
		//StablePointSills.points.Add(new Vector3(0,0,0)); // TODO: find a way to initialize this points list.
		//StablePointWheels.points.Add (new Vector3(0,0,0)); // TODO: find a way to initialize this points list.
		IsToolInToolStageArea = true; // TODO: this variable should be calculated together with tools. Currently all the tools are in tool stage area
		Tool = null; 
		//hasTool = false;
		TooltoFind = "";

		SpareLoc = transform.position;
		// Obtain a reference to the runtime Blackboard instance
		this.Blackboard = planner.RuntimeBlackboard;
		
		// Data bind blackboard variables
		Blackboard.DataBind (this, BlackboardBindingMode.AttributeControlled);
		
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
	private Vector3 calculateActionCircle ()
	{
		// These two parameters need to be tested.
		float dist1 = 20f;
		float dist2 = 100f;

		// two cars accident
		if (Car1 != null && Car2 != null) {
			// two cars next to each other(test scene)
			if (Vector3.Distance (Car1.transform.position, Car2.transform.position) <= dist1)
				return new Vector3 ((Car1.transform.position.x + Car2.transform.position.x) / 2,
				                   (Car1.transform.position.y + Car2.transform.position.y) / 2,
				                   (Car1.transform.position.z + Car2.transform.position.z) / 2);
			// two cars have a little distance (car1.transform.position + car2.transform.position)/2
			else if (Vector3.Distance (Car1.transform.position, Car2.transform.position) > dist1 && 
				Vector3.Distance (Car1.transform.position, Car2.transform.position) <= dist2)
				return Vector3.zero;
			// two cars far away perform twice car1.transform.position and car2.transform.position
			else if (Vector3.Distance (Car1.transform.position, Car2.transform.position) > dist2)
				return Vector3.zero;
		}
		// other cases
		else if (Car1 != null && Car2 == null) {
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

	private float angleToValue (float angle)
	{
		return (angle * 3.1415926f / 180f);
	}

	private float valueToAngle (float value)
	{
		return (value * 180f / 3.1415926f);
	}

	/// <summary>
	/// Generate several points around the target for inner and outer circle survey.
	/// </summary>
	/// <returns>Points clockwise.</returns>
	/// <param name="x0">target, original point of the circle</param>
	/// <param name="x1">current position, arrived to the circle position</param>
	/// <param name="deltaAlpha">relative move angle per target.</param>
	private List<Vector3> pointsOnCircle (Vector3 x0, Vector3 x1, float deltaAlpha)
	{
		x1.y = x0.y;
		float r = Vector3.Distance (x0, x1);
		float alpha = valueToAngle ((float)Math.Acos ((x1.x - x0.x) / r));
		float acumulateAlpha = alpha;
		List<Vector3> points = new List<Vector3> ();

		while (acumulateAlpha - alpha <= 360) {

			float x = x0.x + r * (float)Math.Cos (angleToValue (acumulateAlpha));
			float z = x0.z - r * (float)Math.Sin (angleToValue (acumulateAlpha));
			acumulateAlpha += deltaAlpha;
			Vector3 point = new Vector3 (x, x1.y, z);
			// for debug
			//GameObject obj = new GameObject();
			//obj.transform.position = point;
			points.Add (point);
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
				nav.Stop ();
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
		int IsCheck_id = Animator.StringToHash ("IsCheck");
		bool isCheck = true;
		while (true) {
			m_animator.SetBool (IsCheck_id, false);
			nav.destination = points [0];
			if (Vector3.Distance (points [0], transform.position) < 1f) {
				points.RemoveAt (0);
				isCheck = true;
			}
			if ((points.Count % 5 == 0) && points.Count != 0 && isCheck) {
				m_animator.SetBool (IsCheck_id, true);
				isCheck = false;
				transform.rotation = Quaternion.LookRotation (target - transform.position);
				Debug.Log (target);
				//nav.Stop();
			}
			if (points.Count == 0) {
				nav.autoBraking = true;
				nav.destination = transform.position;
				nav.Stop ();
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

	public TaskStatus Stop ()
	{
		
		//nav.autoBraking = true;
		//nav.destination = transform.position;
		nav.Stop ();
		InnerCircleSurveyTarget = Vector3.zero;
		int speed_id = Animator.StringToHash ("Speed");
		int direction_id = Animator.StringToHash ("Direction");
		m_animator.SetFloat (speed_id, 0.0f);
		m_animator.SetFloat (direction_id, 0.0f);
		setcurrentTask (0);
		return TaskStatus.Succeeded;
		
	}


#endregion


#region VehicleStablilization
	/// <summary>
	/// This methods is used to find the relative tool for stabilizing vehicle, manage glasses, space creation and rescue casualty.
	/// Use Raycast to find whether the tool is in the tool stage area. 
	/// </summary>
	/// <param name="tool">Tool.</param>
	public IEnumerator FindToolInToolStageArea ([ScriptParameter] string toolname)
	{
		// How to get a tool.
		// First, all the tools in the tool stage area are with the tag "InToolStageAreaTools"
		// Second, the name of a tool should be meaningful, TODO however, here I use the tool name block2 for the tool to stable sills (fix it later)
		// In the end, we find the tools in the tag pool
		while (toolname != "") {
			List<GameObject> tools = findExactToolFromType (toolname, "InToolStageAreaTools");
				
			if (tools.Count != 0) {
				Tool = tools.First ();
				yield return TaskStatus.Succeeded;
			}
		}
		toolname = "";
		yield return TaskStatus.Failed;
	}

	/// <summary>
	/// This method is used for simply play the pick up animation and attach the object to the hand.
	/// Moreover, the game logic should be considered, e.g. how to pick, one hand, or two hands, how much to pick etc. 
	/// </summary>
	/// <returns>The up.</returns>
	/// <param name="obj">Object.</param>
	public IEnumerator PickUp ([ScriptParameter] GameObject obj)
	{

		while (obj != null) {
//			// Pick up animation.
//			int IsPickUp_id = Animator.StringToHash ("IsPickUp");
//			m_animator.SetBool (IsPickUp_id, true);
//			// while animation is playing
//			obj.transform.parent = gameObject.transform.Find ("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand");
//			obj.transform.localRotation = Quaternion.identity;
//			obj.transform.localPosition = Vector3.zero;

			InteractableTool tool =  obj.GetComponent<InteractableTool>();
			tool.Initialize(gameObject,m_animator,new Vector3(0,0,0));
			tool.ToolPickUp();
			int IsPickUp_id = Animator.StringToHash ("IsPickUp");
			// TODO: do I need to create some blackboard variable with e.g. Toolinrighthand or Objinlefthand...
			if (m_animator.GetCurrentAnimatorStateInfo (0).IsName ("PickUp")) {
				if (this.m_animator.GetCurrentAnimatorStateInfo (0).normalizedTime < 0.7) {
					yield return TaskStatus.Running;
				} else {
					m_animator.SetBool (IsPickUp_id, false);
					//hasTool = true;
                    tool.tag = "UsedTools";
					yield return TaskStatus.Succeeded;
				}
			} else {
				yield return TaskStatus.Running;
			}
		}
		obj = null;
		yield return TaskStatus.Failed;
		/////////////////////
//		if (obj.name == "StableTool_2") {
//			while (obj != null) {
//				if (obj.transform.renderer.enabled == false)
//					yield return TaskStatus.Succeeded;
//				// Pick up animation.
//				int IsPickUp_id = Animator.StringToHash ("IsPickUp");
//				m_animator.SetBool (IsPickUp_id, true);
//				// while animation is playing
//				obj.transform.parent = gameObject.transform.Find ("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand");
//				obj.transform.localRotation = Quaternion.identity;
//				obj.transform.localPosition = Vector3.zero;
//				// TODO: do I need to create some blackboard variable with e.g. Toolinrighthand or Objinlefthand...
//				if (m_animator.GetCurrentAnimatorStateInfo (0).IsName ("PickUp")) {
//					if (this.m_animator.GetCurrentAnimatorStateInfo (0).normalizedTime < 0.7) {
//						yield return TaskStatus.Running;
//					} else {
//						m_animator.SetBool (IsPickUp_id, false);
//						//hasTool = true;
//						yield return TaskStatus.Succeeded;
//					}
//				} else {
//					yield return TaskStatus.Running;
//				}
//			}
//		} else if (obj.name == "StableTool_1") {
//			while (obj != null) {
//				if (obj.transform.renderer.enabled == false)
//					yield return TaskStatus.Succeeded;
//				// Pick up animation.
//				int IsPickUp_id = Animator.StringToHash ("IsPickUp");
//				m_animator.SetBool (IsPickUp_id, true);
//				// while animation is playing
//				obj.transform.parent = gameObject.transform.Find ("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand");
//				obj.transform.localRotation = Quaternion.identity;
//				obj.transform.localPosition = Vector3.zero;
//				// TODO: do I need to create some blackboard variable with e.g. Toolinrighthand or Objinlefthand...
//				if (m_animator.GetCurrentAnimatorStateInfo (0).IsName ("PickUp")) {
//					if (this.m_animator.GetCurrentAnimatorStateInfo (0).normalizedTime < 0.7) {
//						yield return TaskStatus.Running;
//					} else {
//						m_animator.SetBool (IsPickUp_id, false);
//						//hasTool = true;
//						yield return TaskStatus.Succeeded;
//					}
//				} else {
//					yield return TaskStatus.Running;
//				}
//			}
//		}

		
	}

	/// <summary>
	/// This methods is to implement a way of using all kinds of tools for stabilizing vehicle, manage glasses, space creation and rescue casualty.
	/// It has been defined as a enum Tool. Different tools should play different animations. 
	/// Question: How details I need to implement to operate a tool? 
	/// </summary>
	/// <returns>The tool.</returns>
	public IEnumerator UseTool ([ScriptParameter] GameObject obj)
	{
//        while (obj != null) {
//            InteractableTool tool = obj.GetComponent<InteractableTool> ();
//            tool.Initialize (gameObject, m_animator, new Vector3 (0, 0, 0));
//            tool.ToolUse ();
//            
//            int IsStableSills_id = Animator.StringToHash ("IsStableSills");
//            int IsStableWheels_id = Animator.StringToHash ("IsStableWheels");
//            if (m_animator.GetCurrentAnimatorStateInfo (0).IsName ("Operate")) {
//                if (this.m_animator.GetCurrentAnimatorStateInfo (0).normalizedTime < 0.7) {
//                    yield return TaskStatus.Running;
//                } else {
//                    m_animator.SetBool (IsStableSills_id, false);
//                    CurrentStablePointSill.renderer.enabled = true;
//                    //obj.renderer.enabled = true;
//                    Car1.GetComponent<CrashedCar> ().StablePointSills.Remove (CurrentStablePointSill);
//                    yield return TaskStatus.Succeeded;
//                }
//            } else {
//                yield return TaskStatus.Running;
//            }
//        }
        // TODO: how to manage the tool more efficiently ...
        if (obj.name == "StableTool_2_1") {

            while (obj != null) {
				//GameObject obj=CurrentStablePointSill;
				//Car1.GetComponent<CrashedCar> ().StablePointSills.Remove (CurrentStablePointSill);
				// Using stable sills tool animation.
				int IsStableSills_id = Animator.StringToHash ("IsStableSills");
				m_animator.SetBool (IsStableSills_id, true);
				// After tool used... 
                obj.renderer.enabled = false;

			
				if (m_animator.GetCurrentAnimatorStateInfo (0).IsName ("Operate")) {
					if (this.m_animator.GetCurrentAnimatorStateInfo (0).normalizedTime < 0.7) {
						yield return TaskStatus.Running;
					} else {
						m_animator.SetBool (IsStableSills_id, false);
						CurrentStablePointSill.renderer.enabled = true;
						//obj.renderer.enabled = true;
						Car1.GetComponent<CrashedCar> ().StablePointSills.Remove (CurrentStablePointSill);
						yield return TaskStatus.Succeeded;
					}
				} else {
					yield return TaskStatus.Running;
				}
			}
        } else if (obj.name == "StableTool_1_1") {
            while (obj != null) {
				//GameObject obj=CurrentStablePointWheel;
				//Car1.GetComponent<CrashedCar> ().StablePointWheels.Remove (CurrentStablePointWheel);

				// Using stable sills tool animation.
				int IsStableWheels_id = Animator.StringToHash ("IsStableWheels");
				m_animator.SetBool (IsStableWheels_id, true);
				// After tool used...
                obj.renderer.enabled = false;


				if (m_animator.GetCurrentAnimatorStateInfo (0).IsName ("Operate")) {
					if (this.m_animator.GetCurrentAnimatorStateInfo (0).normalizedTime < 0.7) {
						yield return TaskStatus.Running;
					} else {
						m_animator.SetBool (IsStableWheels_id, false);
						foreach (Transform child in CurrentStablePointWheel.transform) {
							child.renderer.enabled = true;
						}
//						foreach (Transform child in obj.transform) {
//							child.renderer.enabled = true;
//						}
						Car1.GetComponent<CrashedCar> ().StablePointWheels.Remove (CurrentStablePointWheel);
						yield return TaskStatus.Succeeded;
					}
				} else {
					yield return TaskStatus.Running;
				}
			}
		}
		obj = null;
		yield return TaskStatus.Failed;
		
	}

	public TaskStatus AskBackUpStaff ([ScriptParameter] MonoBehaviour tool)
	{
		
		return TaskStatus.Running;
		
	}


	public void taskSetter ()
	{
		if (StablePointSillsCounter > 0 && StablePointWheelsCounter == 0)
			runningTask = Tasks.StableSills;
		else if (StablePointWheelsCounter > 0 && StablePointSillsCounter == 0)
			runningTask = Tasks.StableWheels;
	}


#endregion

#region ToolHandling
	/// <summary>
	/// According to the type of the tool, and the tag information, this function can return an array of an object list that the firefighter will use.
	/// </summary>
	/// <returns>The exact tool from type.</returns>
	/// <param name="toolType">Tool type.</param>
	/// <param name="tag">Tag.</param>
	public List<GameObject> findExactToolFromType (string toolType, string tag)
	{
		// Init.
		string toolItem = "";
		List<GameObject> toolItemObjects = new List<GameObject> ();
		GameObject[] tools = GameObject.FindGameObjectsWithTag (tag);

		// Find the tool object
		GameObject toolobj = null;
		foreach (GameObject tool in tools) {
			if (tool.name == toolType)
				toolobj = tool;
		}

        // test if there is a child
        Transform child =null;
        for (int i = 0; i<6; i++) {
            toolItem = toolType + "_" + (i+1);
            child = toolobj.transform.FindChild (toolItem);
            if(child==null)
                continue;
            else
                break;
        }

		// Calculate the name of the tool
		for (int i = 0; i < 6; i++) {
			toolItem = toolType + "_" + (i + 1);
			Transform children = toolobj.transform.FindChild (toolItem);
			if (children != null && (children.gameObject.tag == tag))
				toolItemObjects.Add (children.gameObject);
		}

		if (child == null) {
			toolItemObjects.Add (toolobj);
			return toolItemObjects;
		}
		return toolItemObjects;
	}
#endregion

#region Glass Management
	public IEnumerator ManageToughGlass ([ScriptParameter] GameObject obj){
		yield return TaskStatus.Succeeded;
	}

	public IEnumerator TapeGlass ([ScriptParameter] GameObject obj){
		yield return TaskStatus.Succeeded;
	}

	public IEnumerator ManageLaminateGlass ([ScriptParameter] GameObject obj){
		yield return TaskStatus.Succeeded;
	}
#endregion




}
