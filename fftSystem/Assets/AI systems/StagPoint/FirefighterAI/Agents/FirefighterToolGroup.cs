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
        CurrentToolGroupTask;

    // Note need to be assigned
    // Atf
    public GameObject
        toolOperator1;
    
    // Note need to be assigned
    // Atm
    public GameObject
        toolOperator2;
    
    [NonSerialized]
    public FirefighterToolOperator
        toolOperatorScript1;
    
    [NonSerialized]
    public FirefighterToolOperator
        toolOperatorScript2;


	// Use this for initialization
	void Start () {
        if(toolOperator1 != null)
            toolOperatorScript1 = toolOperator1.GetComponent<FirefighterToolOperator>();
        else
            Debug.LogError("Init Error: Please assign ToolOperator 1 object to the firefighter tool group.");
        
        if(toolOperator2 != null)
            toolOperatorScript2 = toolOperator2.GetComponent<FirefighterToolOperator>();
        else
            Debug.LogError("Init Error: Please assign ToolOperator 2 object to the firefighter tool group.");
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
        // this is the logically assigning tasks from tool group (Angriffstrupp) to tool operator
        // for reality, we need animations like giveing order.. 
        if (toolOperatorScript1 != null)
        {
            toolOperatorScript1.CurrentToolOperatorTask = ToolOperatorTasks.StableSills;
            return TaskStatus.Succeeded;
        } else 
            return TaskStatus.Failed;
        
    }
    
    
    public TaskStatus StableWheels()
    {
        // this is the logically assigning tasks from tool group (Angriffstrupp) to tool operator
        // for reality, we need animations like giveing order.. 
        if (toolOperatorScript2 != null)
        {
            toolOperatorScript2.CurrentToolOperatorTask = ToolOperatorTasks.StableWheels;
            return TaskStatus.Succeeded;
        } else 
            return TaskStatus.Failed;
        
    }
}
