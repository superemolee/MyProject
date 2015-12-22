using UnityEngine;
using System.Collections;

public class Agent : MonoBehaviour
{

	public GameObject			particle;
	public Vector3			target;
	protected NavMeshAgent		agent;
	protected Animator			animator;
	protected Locomotion locomotion;
	protected Object particleClone;
	public bool rayNav;
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

		particleClone = null;
	}

	protected void SetDestination ()
	{
		// Construct a ray from the current mouse coordinates
		var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit = new RaycastHit ();
		if (Physics.Raycast (ray, out hit)) {
			if (particleClone != null) {
				GameObject.Destroy (particleClone);
				particleClone = null;
			}

			// Create a particle if hit
			Quaternion q = new Quaternion ();
			q.SetLookRotation (hit.normal, Vector3.forward);
			particleClone = Instantiate (particle, hit.point, q);

			agent.destination = hit.point;
		}
	}
	
	public void SetDestination (Vector3 tar)
	{
		agent.destination = tar;
		target = tar;
	}
	
	protected void SetupAgentLocomotion ()
	{
		if (AgentDone ()) {			
//			locomotion.Do (0, 0);
//			if (particleClone != null) {
//				GameObject.Destroy (particleClone);
//				particleClone = null;
//			}
			SetDirection();
			
		} else {
			float speed = agent.desiredVelocity.magnitude;

			Vector3 velocity = Quaternion.Inverse (transform.rotation) * agent.desiredVelocity;

			float angle = Mathf.Atan2 (velocity.x, velocity.z) * 180.0f / 3.14159f;

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
	protected void SetDirection ()
	{
		// hard setting the face target
		locomotion.Do (0,0);
		Vector3 dir = target - transform.position;
		animator.rootRotation= new Quaternion(0, Quaternion.LookRotation(dir).y, 0, Quaternion.LookRotation(dir).w);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (rayNav) {
			if (Input.GetButtonDown ("Fire1")) 
				SetDestination ();
		} else {
//			if (target)
//				SetDestination (target);
		}
		SetupAgentLocomotion ();
	}
}
