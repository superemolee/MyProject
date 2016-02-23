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

    [BlackboardVariable]
    [NonSerialized]
    public bool
        isSillStabled;

    [BlackboardVariable]
    [NonSerialized]
    public bool
        isWheelStabled;


#region Vars for Order
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
#endregion

#region Vars for Report
    // used for report
    public GameObject
        firefighterLeader;
    
    [NonSerialized]
    public FirefighterLeader
        leaderScript;
#endregion

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

        if(firefighterLeader != null)
            leaderScript = firefighterLeader.GetComponent<FirefighterLeader>();
        else
            Debug.LogError("Init Error: Please assign firefighter leader object to the firefighter leader.");
	}
	
	// Update is called once per frame
	void Update () {
        if(InnerCircleReport())
            leaderScript.IsInnerCircleSurveySucceed = true;

        if(isSillStabled&&isWheelStabled)
            leaderScript.IsVehicleStabilizationSucceed = true;
        
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

    public TaskStatus InnerCircle()
    {
        bool canPerformInnerCircleSurvey = false;

        // this is the logically assigning tasks from tool group (Angriffstrupp) to tool operator
        // for reality, we need animations like giveing order.. 
        if (toolOperatorScript1 != null)
        {
            toolOperatorScript1.CurrentToolOperatorTask = ToolOperatorTasks.InnerCircle;
            canPerformInnerCircleSurvey = true;
        } 
        if(toolOperatorScript2 != null)
        {
            toolOperatorScript2.CurrentToolOperatorTask = ToolOperatorTasks.InnerCircle;
            canPerformInnerCircleSurvey = true;
        }

        if(canPerformInnerCircleSurvey)
        {
            return TaskStatus.Succeeded;
        }
        else 
            return TaskStatus.Failed;
        
    }

    /// <summary>
    /// Report to the firefighter leader whether the inner circle succeed by the report from tool operators
    /// </summary>
    /// <returns><c>true</c>, if InnerCircleSurvey is performed successfully, <c>false</c> otherwise.</returns>
    public bool InnerCircleReport(){
        bool isPerformInnerCircleSurvey = false;
        if (toolOperatorScript1 != null)
        {
            if(toolOperatorScript1.IsInnerCircleSurveySucceed == true)
                isPerformInnerCircleSurvey = true;
        }
        if(toolOperatorScript2 != null){
            if(toolOperatorScript2.IsInnerCircleSurveySucceed == true)
                isPerformInnerCircleSurvey = true;
        }
        return isPerformInnerCircleSurvey;
    }
}
