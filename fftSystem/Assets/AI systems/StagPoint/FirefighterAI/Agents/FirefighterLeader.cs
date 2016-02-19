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


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
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
