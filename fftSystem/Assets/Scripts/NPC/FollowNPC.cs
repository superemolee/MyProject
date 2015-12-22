using UnityEngine;
using System.Collections;

public class FollowNPC : MonoBehaviour {

	public GameObject NPC1;
	private NavMeshAgent _nav;
	
	void Start () 
	{
		_nav = GetComponent<NavMeshAgent>();
		NPC1 = GameObject.Find("NPC1");	
	}
	
	void Update () 
	{
		//_nav.destination = NPC1.transform.position;	
	}
}
