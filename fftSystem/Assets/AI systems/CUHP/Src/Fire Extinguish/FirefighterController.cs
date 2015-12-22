using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using CsharpHTNplanner;
using Calculation;

public class FirefighterController : MonoBehaviour
{

		// FIELDS

		/// <summary>
		/// The world model manager
		/// </summary>
		public WorldStateManager worldStateManager;
		State state;
		private NavMeshAgent nav;
		private FirefighterAgent agent;
		private Animator animator;
		private Locomotion locomotion;


		// Li: defined for the picking up animation;
		private bool handWithsomething;


		/// <summary>
		/// Denotes if the robot is busy doing an action
		/// </summary>
		private bool busy;

		/// <summary>
		/// The queue of actions the robot needs to do
		/// </summary>
		private Queue<List<string>> actionQueue;

		/// <summary>
		/// The current action of the robot
		/// </summary>
		private List<string> currentAction;
		private GameObject ext;
		// METHODS

		/// <summary>
		/// Initializes the controller of the robot
		/// </summary>
		void Start ()
		{
//        transform.renderer.material.color = Color.cyan;
				busy = false;
				actionQueue = new Queue<List<string>> ();
				nav = GetComponent<NavMeshAgent> ();
				state = worldStateManager.GetWorldStateCopy ();
				agent = GetComponent<FirefighterAgent> ();
				animator = GetComponent<Animator> ();
				locomotion = new Locomotion (animator);

				handWithsomething = false;
				ext = GameObject.Find ("Extinguisher");
//        if (worldModelManager)
//        {
//            worldModelManager.UpdateKnowledge("at", "room0", true);
//        }
		}
	
		/// <summary>
		/// Makes the robot do actions if it is busy or has actions in queue
		/// </summary>
		/// 
		/// TODO: the AI should be interrupted by dynamic event
		/// TODO: runtime plan (replan) should be implemented
		void Update ()
		{
				/// test case
				//	cube.transform = ext.transform.Find ("Object02_001/Armature/Bone/Bone_001/Bone_002/Bone_003/Bone_004/Bone_005/Bone_006/Bone_007").parent;
				/// end of tese case
				// current implementation: no one could stop until the action has been done.
				// there is no dynamic event recognition
//		if(!busy && actionQueue.Count>0)
//		{
//			currentAction = actionQueue.Dequeue();
//			DoNextAction(currentAction);
//		}
//		print(gameObject.transform.position);
				if (busy) {
						ContinueAction ();
				} else if (actionQueue.Count > 0) {
						currentAction = actionQueue.Dequeue ();
						DoNextAction (currentAction);
				}
		}

		/// <summary>
		/// Start the next action
		/// </summary>
		/// <param name="action">The next action to start doing</param>
		private void DoNextAction (List<string> action)
		{
				switch (action [0]) {
				case "GoTo":
						busy = true;
						GoTo (action [1]);
						break;
				case "PickUp":
						busy = true;
						PickUp (action [1]);
						break;
				case "Extinguish":
						busy = true;
						Extinguish ();
						break;
				}
		}

		/// <summary>
		/// Continue doing the current action
		/// </summary>
		private void ContinueAction ()
		{
				switch (currentAction [0]) {
				case "GoTo":
						GoTo (currentAction [1]);
//				busy = false;
						break;
				case "PickUp":
//			currentAction.Clear ();
						PickUp (currentAction [1]);
//                busy = false;
						break;
				case "Extinguish":
						currentAction.Clear ();
//                busy = false;
						break;
				}
		}

		/// <summary>
		/// Place a new plan in the action queue for execution
		/// </summary>
		/// <param name="plan">The plan to be executed by the robot</param>
		public void ExecutePlan (List<string> plan)
		{
				actionQueue.Clear ();
				foreach (string step in plan) {
						List<string> action = new List<string> ();
						if (step.Contains (",")) {
								action.Add (step.Substring (1, step.IndexOf (',') - 1));

								string stepRemainder = step.Substring (step.IndexOf (',') + 2);
								while (stepRemainder.Contains(",")) {
										action.Add (stepRemainder.Substring (0, stepRemainder.IndexOf (',')));
										stepRemainder = step.Substring (step.IndexOf (',') + 2);
								}
								action.Add (stepRemainder.Substring (0, stepRemainder.IndexOf (')')));
						} else {
								action.Add (step.Substring (1, step.IndexOf (')') - 1));
						}
						actionQueue.Enqueue (action);
				}
		}

		/// <summary>
		/// Move the robot to a target location
		/// </summary>
		/// <param name="target">The target location</param>
//    private void MoveTo(Vector3 target)
//    {
//        float distCovered = (Time.time - startTime) * speed;
//        float fracJourney = distCovered / journeyLength;
//        transform.position = Vector3.Lerp(startMarker, target, fracJourney);
//
//        if (transform.position == target)
//        {
//            currentAction.Clear();
//            busy = false;
//        }
//    }
	
	
		private void GoTo (string position)
		{
				// use different animation layer when hand with something e.g. fire extinguisher

				agent.SetDestination (Calculator.StringToPosition (position));
				
				print ("GoTo -- " + position);
				Vector3 disVec = Calculator.StringToPosition (position) - gameObject.transform.position;
//		print (disVec.magnitude);
				if (disVec.magnitude < 2.5f) {
						busy = false;
				}
				if (handWithsomething) {
						animator.SetBool ("HandwithExtinguisher", true);
				}
		}
	
		private void PickUp (string position)
		{
				print ("PickUp -- at" + position);
				// TODO: reconstruct the state representation
				locomotion.PickUp (true);
				AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo (0);
				if (info.IsName ("PickUp")) {
						// when hand reach the object
						if (info.normalizedTime % 1 > 0.60) {
								// find the object
								//GameObject ext = GameObject.Find ("Extinguisher");
								// attach to the body
//								print (ext.transform.Find ("Fire_extin/sub01"));
//				print (gameObject.transform.Find ("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand"));
								ext.transform.parent = gameObject.transform.Find ("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand").transform;
								
								ext.transform.localRotation = Quaternion.Euler (300, 180, 90);
								ext.transform.localPosition = new Vector3 (-0.25f, 0, 0);
								handWithsomething = true;			
						}
						// when animation ends
						if (info.normalizedTime % 1 > 0.90) {
								locomotion.PickUp (false);
								busy = false;
						}
				}
		
	
		}
	
		private void Extinguish ()
		{
				print ("Extighuish!");
//				GameObject ext = GameObject.Find ("Extinguisher");
//		ext.transform.localRotation = Quaternion.Euler (0, 326, 0);
				ext.transform.FindChild ("Dynamic").gameObject.SetActive (true);
				ext.transform.FindChild ("Static").gameObject.SetActive (false);
				Transform handle = ext.transform.Find ("Dynamic/Handle");

		handle.parent = gameObject.transform.Find ("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand").transform;
		handle.localPosition = new Vector3(-0.1f,0.07f,-0.05f);
		handle.localRotation = Quaternion.Euler (70,83,295);
				animator.SetBool ("IsExtinguishing", true);
//				GameObject.Find ("Fire").SetActive (false);
				busy = false;
		}
}
