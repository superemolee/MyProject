using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using StagPoint.Core;
using StagPoint.Planning;
using StagPoint.Planning.Components;

using Firefighter.Utilities;

[RequireComponent( typeof(TaskNetworkPlanner) )]
public class FirefighterToolGroup : MonoBehaviour {

    private TaskNetworkPlanner planner;
    
    [NonSerialized]
    public Blackboard
        Blackboard;

    [BlackboardVariable]
    [NonSerialized]
    public ToolGroupTasks
        currentToolGroupTask;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        // Obtain a reference to the runtime Blackboard instance
        this.Blackboard = planner.RuntimeBlackboard;
        
        // Data bind blackboard variables
        Blackboard.DataBind(this, BlackboardBindingMode.AttributeControlled);
	
	}

    void OnEnable()
    {
        planner = GetComponent<TaskNetworkPlanner>();
        
    }

    
    public TaskStatus StableSills()
    {
        
        return TaskStatus.Succeeded;
        
    }

    
    public TaskStatus StableWheels()
    {
        
        return TaskStatus.Succeeded;
        
    }
}
