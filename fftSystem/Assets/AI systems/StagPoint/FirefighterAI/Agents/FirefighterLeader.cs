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
    public bool Rescue{ get; set; }


    private TaskNetworkPlanner planner;
    
    [NonSerialized]
    public Blackboard
        Blackboard;

    // Note need to be assigned
    public GameObject
        toolGroup;
    // TODO other groups ... 
    
    [NonSerialized]
    public FirefighterToolGroup
        toolGroupScript; 

    [BlackboardVariable]
    [NonSerialized]
    public bool
        IsInnerCircleSurveySucceed;

    [BlackboardVariable]
    [NonSerialized]
    public bool
        IsVehicleStabilizationSucceed;

    [BlackboardVariable]
    [NonSerialized]
    public bool
        IsGlassManagementSucceed;


	// Use this for initialization
	void Start () {
        if (toolGroup != null)
        {
            toolGroupScript = toolGroup.GetComponent<FirefighterToolGroup>();
        } else
        {
            Debug.LogError("Init Error: Please assign ToolGroup object to the firefighter leader.");
        }
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
        // this is the logically assigning tasks from firefighter leader to tool group (Angriffstrupp)
        // for reality, we need animations like giveing order.. 
        if (toolGroupScript != null)
        {
            toolGroupScript.CurrentToolGroupTask = ToolGroupTasks.InnerCircleSurvey;
            return TaskStatus.Succeeded;
        } else 
            return TaskStatus.Failed;
		
	}

	public TaskStatus OuterCircleSurvey()
	{
		
		return TaskStatus.Succeeded;
		
	}

    public TaskStatus VehicleStabilization()
    {
        // this is the logically assigning tasks from firefighter leader to tool group (Angriffstrupp)
        // for reality, we need animations like giveing order.. 
        if (toolGroupScript != null)
        {
            toolGroupScript.CurrentToolGroupTask = ToolGroupTasks.StableVehicle;
            return TaskStatus.Succeeded;
        } else 
            return TaskStatus.Failed;
        
    }


    public TaskStatus GlassManagement()
    {
        // this is the logically assigning tasks from firefighter leader to tool group (Angriffstrupp)
        // for reality, we need animations like giveing order.. 
        if(toolGroupScript != null){
            toolGroupScript.CurrentToolGroupTask = ToolGroupTasks.GlassManagement;
            return TaskStatus.Succeeded;
        } else 
            return TaskStatus.Failed;
      
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
