using UnityEngine;
using System.Collections;

public class FirefighterAnimatorControl : MonoBehaviour {
	
	//public Vector3	target;
	protected NavMeshAgent	agent;
	protected Animator	animator;
	protected Locomotion locomotion;
	//public bool rayNav;
	private float SpeedDampTime = .25f;
	private float DirectionDampTime = .25f;	
	
	
	// Use this for initialization
	void Start ()
	{
		agent = GetComponent<NavMeshAgent> ();
		agent.updateRotation = false;
		agent.stoppingDistance = 1f;
		
		animator = GetComponent<Animator> ();
		locomotion = new Locomotion (animator);
	}
	
	public void SetDestination (Vector3 tar)
	{
		agent.destination = tar;
	}
	
	protected void SetupAgentLocomotion ()
	{
		if (AgentDone ()) {			
			//			locomotion.Do (0, 0);
			//			if (particleClone != null) {
			//				GameObject.Destroy (particleClone);
			//				particleClone = null;
			//			}
			//SetDirection();
			
		} else {
			float speed = agent.desiredVelocity.magnitude;
			
			Vector3 velocity = Quaternion.Inverse (transform.rotation) * agent.desiredVelocity;
			
			float angle = Mathf.Atan2 (velocity.x, velocity.z) * 180.0f / 3.14159f;
//			Debug.Log("transform.rotation   " + transform.rotation);
//			Debug.Log("agent.desiredVelocity   " + agent.desiredVelocity);
//			Debug.Log("velocity   " + velocity);

			//Debug.Log("angle   " + speed);

			locomotion.Do (speed, angle);
		}
	}
	
	void OnAnimatorMove ()
	{
		agent.velocity = animator.deltaPosition / Time.deltaTime;
		transform.rotation = animator.rootRotation;
	}
	
	public bool AgentDone ()
	{
		return !agent.pathPending && AgentStopping ();
	}
	
	public bool AgentStopping ()
	{
		
		return (agent.remainingDistance <= agent.stoppingDistance);
	}
	

	
	// Li: set up the direction after reach the destination
	// Direction changed slowly, can be made more flunt and faster.
//	protected void SetDirection ()
//	{
//		// hard setting the face target
//		locomotion.Do (0,0);
//		Vector3 dir = target - transform.position;
//		animator.rootRotation= new Quaternion(0, Quaternion.LookRotation(dir).y, 0, Quaternion.LookRotation(dir).w);
//	}
	
	// Update is called once per frame
	void Update ()
	{
		SetupAgentLocomotion ();
	}
}
