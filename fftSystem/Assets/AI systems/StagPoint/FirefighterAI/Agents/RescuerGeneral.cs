using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using StagPoint.Core;
using StagPoint.Planning;
using StagPoint.Planning.Components;

using Firefighter.Utilities;

public class RescuerGeneral : MonoBehaviour {
	
	public Tasks runningTask;

	// Use this for initialization
	void Start () {
		runningTask = Tasks.Free;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setcurrentTask(int id){
		switch (id){
		case 0:
			runningTask = Tasks.Free;
			break;
		case 1:
			runningTask = Tasks.SurveyInnerCircle;
			break;
		case 2:
			runningTask = Tasks.SurveyOuterCircle;
			break;
		case 3:
			runningTask = Tasks.TriageCasualties;
			break;
		case 4:
			runningTask = Tasks.EstablishCasualtyContact;
			break;
		case 5:
			runningTask = Tasks.StabiliseVehicles;
			break;
		case 6:
			runningTask = Tasks.EstablishAToolStagingArea;
			break;
		case 7:
			runningTask = Tasks.StableSills;
			break;
		case 8:
			runningTask = Tasks.StableWheels;
			break;
		case 9:
			runningTask = Tasks.ManageGlass;
			break;
		default:
			runningTask = Tasks.Free;
			break;
			
		}
	}
}
