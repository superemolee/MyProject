using UnityEngine;
using CsharpHTNplanner;
using System.Collections.Generic;
using System.Collections;

public class Interface : MonoBehaviour {

 // FIELDS

    /// <summary>
    /// The robot controller
    /// </summary>
    public FirefighterController firefighterController;

    /// <summary>
    /// The HTN-planner
    /// </summary>
    private HTNPlanner planner;


    // METHODS

	/// <summary>
	/// Initializes the interface to the HTN-planner
	/// </summary>
	void Start () {
        planner = new HTNPlanner(typeof(Domain_Firefighter), new Domain_Firefighter().GetMethodsDict(), typeof(Domain_Firefighter));
	}

    /// <summary>
    /// Generates a plan to clean rooms and sends it to the robot
    /// </summary>
    /// <returns>True if everything went well, false if something went wrong</returns>
    public bool Extinguishfire ()
    {
        List<string> plan = GetPlan();
        if (plan != null)
        {
            return SendPlanToFirefighter(plan);
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
        if (GetComponent<WorldStateManager>())
        {
			
            State initialState = GetComponent<WorldStateManager>().GetWorldStateCopy();
            List<List<string>> goalTasks = new List<List<string>>();
            goalTasks.Add(new List<string>(new string[1] { "ExtinguishFire" }));

            return planner.SolvePlanningProblem(initialState, goalTasks);
        }
        return null;
    }

    /// <summary>
    /// Sends the plan to the robot
    /// </summary>
    /// <param name="plan">The plan</param>
    /// <returns>True if a robot is connected, false otherwise</returns>
    private bool SendPlanToFirefighter(List<string> plan)
    {
        if (firefighterController)
        {
            firefighterController.ExecutePlan(plan);
            return true;
        }
        return false;
    }
}
