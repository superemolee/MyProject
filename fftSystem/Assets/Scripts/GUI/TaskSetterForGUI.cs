using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using StagPoint.Core;
using StagPoint.Planning;
using StagPoint.Planning.Components;

using Firefighter.Utilities;

public class TaskSetterForGUI : MonoBehaviour {

    public GameObject leader;
    public GameObject toolGroup;
    public GameObject toolOp1;
    public GameObject toolOp2;
    public GameObject waterGroup;
    public GameObject waterOp1;
    public GameObject waterOp2;
    public GameObject pipeGroup;
    public GameObject pipeOp1;
    public GameObject pipeOp2;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    // all task 
    public void allTasksSetter  (){
        leader.GetComponent<FirefighterLeader>().Rescue = true;
    }

    // single tasks
    public void innerCircleSetter(){
        toolGroup.GetComponent<FirefighterToolGroup>().CurrentToolGroupTask = ToolGroupTasks.InnerCircleSurvey;
    }

    public void outerCircleSetter(){

    }

    public void vehicleStableSetter(){
        toolGroup.GetComponent<FirefighterToolGroup>().CurrentToolGroupTask = ToolGroupTasks.StableVehicle;
    }

    public void glassManagementSetter(){
        toolGroup.GetComponent<FirefighterToolGroup>().CurrentToolGroupTask = ToolGroupTasks.GlassManagement;
    }


}
