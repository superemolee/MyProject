using UnityEngine;
using System.Collections;

public class Vision : MonoBehaviour 
{
	public Transform target;
	
	public float fieldOfViewAngle = 300f;
	public float visionLength = 100000f;
	
	public Camera characterCamera;
	public Knowledge charKnowledge;
	public GameObject vehicle;
	public GameObject house;
	public GameObject trashcan;
	public GameObject policeman;
	public GameObject injured;
	public GameObject ambulance;
	public GameObject stretcher;
	public GameObject bleeding;
	public GameObject boneBreak;
	public GameObject burning;
	
	void Awake()
	{		
		//characterCamera = GameObject.Find("firefighter_nonControlable/CameraHelper/CameraBack").camera;
		characterCamera = gameObject.transform.FindChild("CameraHelper").transform.FindChild("CameraBack").camera;
		//characterCamera.fieldOfView = fieldOfViewAngle;
		
		charKnowledge = new Knowledge();
	}
	
	void Start () 
	{	
		vehicle = GameObject.Find("FahrzeugPrefabs");
		house = GameObject.Find("haus_optimized");
		trashcan = GameObject.Find("Muelltonne");
		policeman = GameObject.Find("Polizist");
		//injured = GameObject.Find("NachbarinIdle");
		ambulance = GameObject.Find("Ambulance");
		stretcher = GameObject.Find("Stretcher");
		bleeding = GameObject.Find("bleeding");
		boneBreak = GameObject.Find("bonebreak");
		burning = GameObject.Find("burning");
	}
	
	
	void Update () 
	{
		//GetComponent<NavMeshAgent>().destination = target.position;
		UpdateVision();
		
		//print("IN MY KNOWLEDGE:");
		foreach(tasks task in charKnowledge.tasksList)
		{
			if(task.id != 0)
			{
				//print("task found named: " + task.name + "\n");
			}
		}
	
	}
	
	void UpdateVision()
	{
		
		
		if(IsVisible(vehicle))
			Debug.Log("Vehicle can be seen");
		else
			Debug.Log("Vehicle can't be seen");
		if(IsVisible(house))
			print("House can be seen");
		if(IsVisible(trashcan))
			print("Trashcan can be seen");
		if(IsVisible(policeman))
			print("Policeman can be seen");
	//	if(IsVisible(injured))
	//		print("Injured person can be seen");
		if(IsVisible(ambulance))
			print("Ambulance can be seen");
		if(IsVisible(stretcher))
			print("Stretcher can be seen");
		if(IsVisible(burning))
			print("Burning person can be seen");
		if(IsVisible(boneBreak))
			print("BoneBreak person can be seen");
		if(IsVisible(bleeding))
			print("Bleeding person can be seen");
		
		
		
		if(IsVisible(vehicle))
		{
			if(!charKnowledge.Exists(vehicle))
			{
				charKnowledge.AddToPhysicalObjectsList(vehicle);
			}
			else
			{
				charKnowledge.UpdateLocation(vehicle);
			}
		}
		
		if(IsVisible(house))
		{
			if(!charKnowledge.Exists(house))
			{
				charKnowledge.AddToPhysicalObjectsList(house);
			}
			else
			{
				charKnowledge.UpdateLocation(house);
			}
		}
		
		if(IsVisible(trashcan))
		{
			if(!charKnowledge.Exists(trashcan))
			{
				charKnowledge.AddToPhysicalObjectsList(trashcan);
			}
			else
			{
				charKnowledge.UpdateLocation(trashcan);
			}
		}
		
		if(IsVisible(policeman))
		{
			if(!charKnowledge.Exists(policeman))
			{
				charKnowledge.AddToPhysicalObjectsList(policeman);
			}
			else
			{
				charKnowledge.UpdateLocation(policeman);
			}
		}
		
	/*	if(IsVisible(injured))
		{
			if(!charKnowledge.Exists(injured))
			{
				charKnowledge.AddToPhysicalObjectsList(injured);
			}
			else
			{
				charKnowledge.UpdateLocation(injured);
			}
		}
	*/
		
		if(IsVisible(ambulance))
		{
			if(!charKnowledge.Exists(ambulance))
			{
				charKnowledge.AddToPhysicalObjectsList(ambulance);
			}
			else
			{
				charKnowledge.UpdateLocation(ambulance);
			}			
		}
		
		if(IsVisible(stretcher))
		{
			if(!charKnowledge.Exists(stretcher))
			{
				charKnowledge.AddToPhysicalObjectsList(stretcher);
			}
			else
			{
				charKnowledge.UpdateLocation(stretcher);
			}		
		}
		
		if(IsVisible(bleeding))
		{
			if(!charKnowledge.Exists(bleeding))
			{
				charKnowledge.AddToPhysicalObjectsList(bleeding);
			}
			else
			{
				charKnowledge.UpdateLocation(bleeding);
			}		
		}
		
		if(IsVisible(boneBreak))
		{
			if(!charKnowledge.Exists(boneBreak))
			{
				charKnowledge.AddToPhysicalObjectsList(boneBreak);
			}
			else
			{
				charKnowledge.UpdateLocation(boneBreak);
			}		
		}
		
		if(IsVisible(burning))
		{
			if(!charKnowledge.Exists(burning))
			{
				charKnowledge.AddToPhysicalObjectsList(burning);
			}
			else
			{
				charKnowledge.UpdateLocation(burning);
			}		
		}
	}
	
	public bool IsVisible(GameObject obj)
	{
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(characterCamera);
		Vector3 direction = obj.transform.position - transform.position;
		Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
		Vector3 NPCPosition = transform.position;
		float angle = Vector3.Angle(direction, transform.forward);
		RaycastHit hit;
		RaycastHit hitUp;
		
		if(GeometryUtility.TestPlanesAABB(planes, obj.renderer.bounds))
		{
			if(angle < fieldOfViewAngle * 0.5f)
			{				
				if(Physics.Raycast(NPCPosition, direction.normalized, out hit, visionLength))
				{
					Debug.DrawLine(transform.position, hit.point, Color.red);
					if(hit.collider.gameObject == obj)
					{
						return true;
					}
				}				
				if(Physics.Raycast(NPCPosition + up, direction.normalized, out hitUp, visionLength))
				{
					Debug.DrawLine(transform.position + up, hitUp.point, Color.blue);
					if(hit.collider.gameObject == obj)
					{
						return true;
					}
				}
			}
		}
		return false;
	}
	
	/*
	
	public bool CanSeeObject(GameObject obj)
	{
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(characterCamera);
		Vector3 upHigh = new Vector3(0.0f, 1.0f, 0.0f);
		Vector3 upLow = new Vector3(0.0f, 1.4f, 0.0f);
		Vector3 direction = obj.transform.position - transform.position;
		Vector3 NPCPosition = transform.position;
		float angle = Vector3.Angle(direction, transform.forward);
		RaycastHit hitHigh;
		RaycastHit hitLow;
		
		if(GeometryUtility.TestPlanesAABB(planes, obj.renderer.bounds))
		{
			if(angle < fieldOfViewAngle * 0.5f)
			{
				if(Physics.Raycast(NPCPosition + upHigh, direction.normalized, out hitHigh, visionLength))
				{
					if(hitHigh.collider.gameObject != obj)
					{
						// Do nothing
					}
					else
					{
						return true;
					}
				}
				else
				{
					if(Physics.Raycast(NPCPosition, direction.normalized, out hitLow, visionLength))
					{
						if(hitLow.collider.gameObject == obj)
						{
							return true;
						}
					}
					
				}
			}
		}
		return false;
		
	}
	
	*/
	/*
	public bool CanSeeVehicle(GameObject vehicleObject)
	{
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(characterCamera);
		Vector3 upHigh = new Vector3(0.0f, 1.0f, 0.0f);
		Vector3 upLow = new Vector3(0.0f, 0.5f, 0.0f);
		Vector3 direction = vehicleObject.transform.position - transform.position;
		Vector3 playerPosition = transform.position;
		float angle = Vector3.Angle(direction, transform.forward);
		RaycastHit hitHigh;
		RaycastHit hitLow;
				
		
		if(GeometryUtility.TestPlanesAABB(planes, vehicleObject.renderer.bounds))
		{
			//if(ColliderVerixWithinFieldOfView(vehicleObject))
			{				
				if(Physics.Raycast(playerPosition + upHigh, direction.normalized, out hitHigh, visionLength))
				{
					if(hitHigh.collider.gameObject != vehicleObject)
					{
						//Do nothing					 
					}
					else
					{
						return true;
					}
				}	
				else
				{
					Vector3 angle1 = new Vector3(1.0f, 0.0f, 1.0f); 
					Vector3 angle2 = new Vector3(-1.0f, 0.0f, 1.0f);
					Vector3 dir1 = transform.TransformDirection(Vector3.forward);					
					Vector3 dir2 = transform.TransformDirection(angle1);
					Vector3 dir3 = transform.TransformDirection(angle2);				
					
					if(Physics.Raycast(playerPosition + upLow, dir1.normalized, out hitLow, visionLength))
					{
						Debug.DrawLine(transform.position + upLow, hitLow.point, Color.green);						
						if(hitLow.collider.gameObject == vehicleObject)
						{
							return true;
						}
					}
					if(Physics.Raycast(playerPosition + upLow, dir2.normalized, out hitLow, visionLength))
					{
						Debug.DrawLine(transform.position + upLow, hitLow.point, Color.red);						
						if(hitLow.collider.gameObject == vehicleObject)
						{
							return true;
						}
					}
					if(Physics.Raycast(playerPosition + upLow, dir3.normalized, out hitLow, visionLength))
					{
						Debug.DrawLine(transform.position + upLow, hitLow.point, Color.grey);						
						if(hitLow.collider.gameObject == vehicleObject)
						{
							return true;
						}
					}
					
				}
			}
		}
		return false;
		
	}
	*/
}
