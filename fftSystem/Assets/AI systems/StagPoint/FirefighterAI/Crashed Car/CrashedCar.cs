using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class CrashedCar : MonoBehaviour {

	public List <GameObject> StablePointSills;
	public List <GameObject> StablePointWheels;

	// This name is used for organize object using tags. 
	// Naming in tags are <object name> + <variable name>
	// e.g. Car1StablePointSills.
	public string objName;

	// Use this for initialization
	void Start () {
		objName = gameObject.name;
		
		// Find all the object that are with this tag.
		foreach(GameObject obj in GameObject.FindGameObjectsWithTag(name + "StablePointSills")){
			StablePointSills.Add(obj);
		}
		foreach(GameObject obj in GameObject.FindGameObjectsWithTag(name + "StablePointWheels")){
			StablePointWheels.Add(obj);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
