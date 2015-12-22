using UnityEngine;
using System.Collections;

public class Navigation : MonoBehaviour {
	
	private NavMeshAgent _nav;
	public float navSpeed = 1f;	
	public Vector3 startPosition;
	public bool reachBurning = false;
	public bool reachFireExtinguisher = false;
	public bool reachBurningWithFE = false;
	public bool reachStretcher = false;
	public GameObject fireEx;
	public GameObject door3;
	
	public Vision NPCvision;
	public Knowledge NPCknowledge;
	public tasks priorityTask;
	public tasks inProgressTask;
	
	public Transform target;
	
	public bool navigationInProgress = false;
	
	void Awake()
	{
		_nav = GetComponent<NavMeshAgent>();
		_nav.speed = navSpeed;
		startPosition = transform.position;
		priorityTask = new tasks();
		priorityTask.id = 0;
		inProgressTask = new tasks();
		inProgressTask.id = 0;
		
		fireEx = GameObject.Find("Feuerloescher");
		door3 = GameObject.Find("FFC_G3Door");
	}
	
	void Start()
	{
		NPCvision = (Vision) GetComponent(typeof(Vision));
		NPCknowledge = NPCvision.charKnowledge;
	}
	
	
	void Update ()
	{
		if(NPCknowledge.isEmpty)
		{
			_nav.destination = target.position;
			navigationInProgress = true;
		}
		else
		{
			priorityTask = NPCknowledge.SearchForHighestPriorityTask();
			if(priorityTask.id == NPCknowledge.emptyTask.id && inProgressTask.id == NPCknowledge.emptyTask.id)
			{
				_nav.destination = target.position;
				navigationInProgress = true;			
			}
			else if(priorityTask.id != NPCknowledge.emptyTask.id && inProgressTask.id != NPCknowledge.emptyTask.id)
			{
				if(priorityTask.priority < inProgressTask.priority)
				{
					inProgressTask = priorityTask;
				}								
			}
			else if(priorityTask.id != NPCknowledge.emptyTask.id && inProgressTask.id == NPCknowledge.emptyTask.id)
			{
				inProgressTask = priorityTask;
			}			
			CompleteTask(inProgressTask);
		}
	
	}
	
	public void CompleteTask(tasks inProgTask)
	{
		switch(inProgTask.id)
		{
			case 1:
			{
				RescueBurningPerson();
				break;
			}
			case 2:
			{
				RescueBleedingPerson();
				break;
			}
			case 3:
			{
				RescueBonebreakPerson();
				break;
			}
			case 4:
			{
				Fightfire();
				break;
			}
		}
	}
	
	public void RescueBurningPerson()
	{
		// Go near the burning person.
		print("GOING TO RESCUE BURNING PERSON");
		physicalObjects burningPerson = NPCknowledge.FindObjectWithName("burning");
		physicalObjects stretcher = NPCknowledge.FindObjectWithName("Stretcher");
		physicalObjects ambulance = NPCknowledge.FindObjectWithName("Ambulance");
		if(burningPerson.name != NPCknowledge.emptyObject.name)
		{
			_nav.destination = burningPerson.transf.position;
			float distance = Vector3.Distance(transform.position, burningPerson.transf.position);
			
			if(distance <= 2.0f)
			{						
				reachBurning = true;				
			}
			if(reachBurning)
			{
				print("GOING TO BRING FIRE EXTINGUISHER");
				_nav.destination = fireEx.transform.position;
				float dist = Vector3.Distance(transform.position, _nav.destination);
				if(dist <= 2.0f)
				{
					reachFireExtinguisher = true;				
				}
				if(reachFireExtinguisher)
				{
					// Open the door
					Vector3 newTrans = new Vector3(1.317959f, 4.3f, 1.767436f);
					door3.transform.position = newTrans;		
					
					// Take the fire extinguisher out					
					fireEx.transform.parent = transform;
					
					// Go back to burning person
					_nav.destination = burningPerson.transf.position;
					float dist2 = Vector3.Distance(transform.position, _nav.destination);
					if(dist2 <= 2.0f)
					{
						reachBurningWithFE = true;						
					}
					if(reachBurningWithFE)
					{
						// Fight fire with fire extinguisher
						
						// Leave fire extinguisher on the floor
						fireEx.transform.parent = null;
						
						// Go to stretcher
						_nav.destination = stretcher.transf.position;
						float dist3 = Vector3.Distance(transform.position, _nav.destination);
						if(dist3 <= 2.0f)
						{
							reachStretcher = true;
						}
						if(reachStretcher)
						{
							// Take the stetcher
							stretcher.transf.parent = transform;
							stretcher.transf.position = transform.position;
							
							// Move with stretcher to the burning person
							_nav.destination = burningPerson.transf.position;
							float dist4 = Vector3.Distance(transform.position, _nav.destination);
							if(dist4 <= 2.0f)
							{
								// Put burning person on stretcher
								burningPerson.transf.parent = transform;
								burningPerson.transf.position = transform.position;
								//burningPerson.transf.position = new Vector3(1410.308f, 11.3f, 1636.6f);
								//burningPerson.transf.rotation = new Quaternion(275f, 223f, -5.88f, 0f);
								
								
								_nav.destination = ambulance.transf.position;
								float dist5 = Vector3.Distance(transform.position, _nav.destination);
								if(dist5 <= 2.0f)
								{
									stretcher.transf.parent = null;
									inProgressTask.completed = true;
									NPCknowledge.setTaskCompleted(inProgressTask.id, true);
									inProgressTask.id = 0;
									print("DONE RESCUING BURNING PERSON");
								}
							}
						}					
					}
				}
			}
		}
	}
	
/*	IEnumerator Wait()
	{
		yield return new WaitForSeconds(3);
		
	}
*/
	
	public void RescueBleedingPerson()
	{
		// Go near the bleeding person.
		print("GOING TO RESCUE BLEEDING PERSON");
		physicalObjects bleedingPerson = NPCknowledge.FindObjectWithName("bleeding");
		if(bleedingPerson.name != NPCknowledge.emptyObject.name)
		{
			_nav.destination = bleedingPerson.transf.position;
			float distance = Vector3.Distance(transform.position, bleedingPerson.transf.position);
			if(distance <= 2.0f)
			{
				inProgressTask.completed = true;
				NPCknowledge.setTaskCompleted(inProgressTask.id, true);
				inProgressTask.id = 0;
				print("DONE RESCUING BLEEDING PERSON");
			}
		}
	}
	
	public void RescueBonebreakPerson()
	{
		// Go near the bonebreak person.
		print("GOING TO RESCUE BONEBREAK PERSON");
		physicalObjects bonebreakPerson = NPCknowledge.FindObjectWithName("bonebreak"); 
		if(bonebreakPerson.name != NPCknowledge.emptyObject.name)
		{
			_nav.destination = bonebreakPerson.transf.position;
			float distance = Vector3.Distance(transform.position, bonebreakPerson.transf.position);
			if(distance <= 2.0f)
			{
				inProgressTask.completed = true;				
				NPCknowledge.setTaskCompleted(inProgressTask.id, true);
				inProgressTask.id = 0;
				print("DONE RESCUING BONEBREAK PERSON");
			}
		}
	}
	
	public void Fightfire()
	{
		// Go near the fire.
		print("GOING TO FIGHT FIRE");
		physicalObjects fire = NPCknowledge.FindObjectWithName("Muelltonne");
		if(fire.name != NPCknowledge.emptyObject.name)
		{
			_nav.destination = fire.transf.position;
			float distance = Vector3.Distance(transform.position, fire.transf.position);
			if(distance <= 2.0f)
			{
				inProgressTask.completed = true;
				NPCknowledge.setTaskCompleted(inProgressTask.id, true);
				inProgressTask.id = 0;
				print("DONE FIGHTING FIRE");
			}
		}
	}
}
