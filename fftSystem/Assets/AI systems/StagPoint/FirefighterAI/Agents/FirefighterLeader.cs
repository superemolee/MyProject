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
public class FirefighterLeader : RescuerGeneral {

    [BlackboardVariable]
    public bool Rescue;


    private TaskNetworkPlanner planner;
    
    [NonSerialized]
    public Blackboard
        Blackboard;

    [NonSerialized]
    public GameObject toolGroup;
    // TODO other groups ... 

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

	
	#region Action/Operator methods

	//Scene assessment and safety

	public TaskStatus InnerCircleSurvey()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus OuterCircleSurvey()
	{
		
		return TaskStatus.Succeeded;
		
	}

    public TaskStatus VehicleStabilization()
    {
        toolGroup.GetComponent<FirefighterToolGroup>().currentToolGroupTask = ToolGroupTasks.StableVehicle;
        return TaskStatus.Succeeded;
        
    }


    public TaskStatus GlassManagement()
    {
        
        return TaskStatus.Succeeded;
        
    }

    public TaskStatus FullAccess()
    {
        
        return TaskStatus.Succeeded;
        
    }

    public TaskStatus CasualtyRelease()
    {
        
        return TaskStatus.Succeeded;
        
    }

    public TaskStatus ResetAllMemberPosition()
    {
        
        return TaskStatus.Succeeded;
        
    }


	#endregion
}
