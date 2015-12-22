// C# Unity3D HTN-Planner (CUHP), a simple HTN-Planner written in C# for use in Unity3D
// Copyright (C) 2013  Pieterjan van Gastel
// This software is under a GPLv3 license. For details, see the "HTNPlanner.cs"-file.
// If you do not have a copy of the "HTNPlanner.cs"-file, your version of the C# Unity 3D HTN-Planner is incomplete.

using System.Collections.Generic;
using CsharpHTNplanner;
using UnityEngine;

public class PlannerInterface : MonoBehaviour
{
    // FIELDS

    /// <summary>
    /// The robot controller
    /// </summary>
    public RobotController robotController;

    /// <summary>
    /// The HTN-planner
    /// </summary>
    private HTNPlanner planner;


    // METHODS

	/// <summary>
	/// Initializes the interface to the HTN-planner
	/// </summary>
	void Start () {
        planner = new HTNPlanner(typeof(Domain_CleaningRobot), new Domain_CleaningRobot().GetMethodsDict(), typeof(Domain_CleaningRobot));
	}

    /// <summary>
    /// Generates a plan to clean rooms and sends it to the robot
    /// </summary>
    /// <returns>True if everything went well, false if something went wrong</returns>
    public bool CleanRooms ()
    {
        List<string> plan = GetPlan();
        if (plan != null)
        {
            return SendPlanToRobot(plan);
        }
        Debug.Log("no plan found");
        return false;
    }

    /// <summary>
    /// Gets a plan for cleaning the rooms of the building
    /// </summary>
    /// <returns>The plan made by the HTN-planner</returns>
    private List<string> GetPlan ()
    {
        if (GetComponent<WorldModelManager>())
        {
            State initialState = GetComponent<WorldModelManager>().GetWorldStateCopy();
            if (initialState.ContainsVar("at"))
            {
                initialState.Add("checked", initialState.GetStateOfVar("at")[0]);
            }
            List<List<string>> goalTasks = new List<List<string>>();
            goalTasks.Add(new List<string>(new string[1] { "CleanRooms" }));

            return planner.SolvePlanningProblem(initialState, goalTasks);
        }
        return null;
    }

    /// <summary>
    /// Sends the plan to the robot
    /// </summary>
    /// <param name="plan">The plan</param>
    /// <returns>True if a robot is connected, false otherwise</returns>
    private bool SendPlanToRobot(List<string> plan)
    {
        if (robotController)
        {
            robotController.ExecutePlan(plan);
            return true;
        }
        return false;
    }
}
