using UnityEngine;
using System.Collections;

public class MouseLookCharacterController : MonoBehaviour {
	
	private CharacterMotor motor;
	
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;

	private float rotationX = 0F;
	private float rotationY = 0F;
	
	private Quaternion originalRotation;
	Camera cam = null;
	private Vector3 idleCamPos, desiredCamPos;
	
	float running = -1.0f;
	public float smoothFactor = 6.0f;
	// Use this for initialization
	void Start () {
		motor = GetComponent(typeof(CharacterMotor)) as CharacterMotor;
		if (motor==null) Debug.Log("Motor is null!!");
		
		originalRotation = transform.localRotation;
		cam = GetComponentInChildren<Camera>();
		idleCamPos = cam.transform.localPosition;
		desiredCamPos = idleCamPos;
		desiredCamPos[2]+= 0.4f;
		
	}
	
	// Update is called once per frame
	void Update () {
		

		// Read the mouse input axis
		rotationX += Input.GetAxis("Mouse X") * sensitivityX;
		rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
		rotationX += Input.GetAxis("Horizontal2") * Time.deltaTime * 300;
		rotationY += Input.GetAxis("Vertical2" +
			"") * Time.deltaTime * 300;

		rotationX = Mathf.Repeat(rotationX, 360);
		rotationY = Mathf.Clamp(rotationY, -75, 75);
		
		Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
		Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, -Vector3.right);
		
		motor.desiredFacingDirection = originalRotation * xQuaternion * yQuaternion * Vector3.forward;
	}
	
	void LateUpdate(){
		if(motor.desiredVelocity.magnitude >1.8f)
		{
			
			running +=(Time.deltaTime*smoothFactor);
		 	running = Mathf.Min(1.0f, running);

			
		}
		else
		{
				running -=(Time.deltaTime*smoothFactor);
		
				running = Mathf.Max(0.0f, running);
			
			
				
		}
		cam.transform.localPosition = Vector3.Lerp(idleCamPos, desiredCamPos, running);
	}
}
