using UnityEngine;
using System.Collections;
using System;
using Calculation;

public class WorldStateInitializer : MonoBehaviour
{
	
	public GameObject[] gameObjects;
	public GameObject controller;

	// Use this for initialization
	void Start ()
	{
		WorldStateInit ();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	
	// TODO: find a smarter way to implement the initializer
	void WorldStateInit ()
	{
		if (controller) {
			foreach (GameObject gameObject in gameObjects) {
				AddState (gameObject);
			}
		}
	}
	
	private void AddState (GameObject obj)
	{
		switch (obj.name) {
		case "Fire":
			controller.GetComponent<WorldStateManager> ().UpdateKnowledge ("at", "fire", Calculator.PositionToString(obj.transform.position), true);
			break;
		case "Player":
			controller.GetComponent<WorldStateManager> ().UpdateKnowledge ("at", "firefighter", Calculator.PositionToString(obj.transform.position), true);
			controller.GetComponent<WorldStateManager> ().UpdateKnowledge ("handempty", "firefighter", true);
			break;
		case "Extinguisher":
			controller.GetComponent<WorldStateManager> ().UpdateKnowledge ("at", "extinguisher", Calculator.PositionToString(obj.transform.position), true);
			break;
		}
	}
	

}
