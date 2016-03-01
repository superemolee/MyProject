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

    private bool isLookAt;
    private Vector3 lookAtPos;
	
	
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

    public void SetDirection(Vector3 tar){
        //transform.rotation = Quaternion.LookRotation (tar - transform.position);
        tar.y= 0;
        transform.LookAt(tar);
    }

    // IK for look at the target
    public void LookAtTarget(Vector3 tar){
        isLookAt = true;
        lookAtPos = tar;
    }

    public void StopLooking(){
        isLookAt = false;
    }

    void OnAnimatorIK()
    {
        // lookat IK system
        if(animator) {
            
            //if the IK is active, set the position and rotation directly to the goal. 
            if(isLookAt) {
                
                // Set the look target position, if one has been assigned
                if(lookAtPos != null) {
                    animator.SetLookAtWeight(1,0.3f,0.6f,1.0f,0.5f);
                    animator.SetLookAtPosition(lookAtPos);
                }    
            }
            
            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else {          
                animator.SetLookAtWeight(0);
            }
        }
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
