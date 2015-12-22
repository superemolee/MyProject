// C# Unity3D HTN-Planner (CUHP), a simple HTN-Planner written in C# for use in Unity3D
// Copyright (C) 2013  Pieterjan van Gastel
// This software is under a GPLv3 license. For details, see the "HTNPlanner.cs"-file.
// If you do not have a copy of the "HTNPlanner.cs"-file, your version of the C# Unity 3D HTN-Planner is incomplete.

using System.Collections.Generic;
using CsharpHTNplanner;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    // FIELDS

    /// <summary>
    /// The world model manager
    /// </summary>
    public WorldModelManager worldModelManager;

    /// <summary>
    /// The room generator
    /// </summary>
    public RoomGenerator roomGenerator;

    /// <summary>
    /// Start marker for moving between rooms
    /// </summary>
    private Vector3 startMarker;

    /// <summary>
    /// End marker for moving between rooms
    /// </summary>
    private Vector3 endMarker;

    /// <summary>
    /// The moving speed of the robot
    /// </summary>
    private float speed = 3.0f;

    /// <summary>
    /// Time at which the robot started moving
    /// </summary>
    private float startTime;

    /// <summary>
    /// The length of the journey between two rooms
    /// </summary>
    private float journeyLength;

    /// <summary>
    /// Denotes if the robot is busy doing an action
    /// </summary>
    private bool busy;

    /// <summary>
    /// The queue of actions the robot needs to do
    /// </summary>
    private Queue<List<string>> actionQueue;

    /// <summary>
    /// The current action of the robot
    /// </summary>
    private List<string> currentAction;


    // METHODS

	/// <summary>
	/// Initializes the controller of the robot
	/// </summary>
	void Start () {
        transform.renderer.material.color = Color.cyan;
        busy = false;
        actionQueue = new Queue<List<string>>();

        if (worldModelManager)
        {
            worldModelManager.UpdateKnowledge("at", "room0", true);
        }
	}
	
	/// <summary>
	/// Makes the robot do actions if it is busy or has actions in queue
	/// </summary>
	void Update ()
    {
        if (busy)
        {
            ContinueAction();
        }
        else if (actionQueue.Count > 0)
        {
            currentAction = actionQueue.Dequeue();
            DoNextAction(currentAction);
        }
	}

    /// <summary>
    /// Start the next action
    /// </summary>
    /// <param name="action">The next action to start doing</param>
    private void DoNextAction(List<string> action)
    {
        switch (action[0])
        {
            case "MoveTo":
                startMarker = transform.position;
                Vector3 targetPos = roomGenerator.Rooms.Find(
                    delegate(Transform room)
                    {
                        return room.name == action[1];
                    }
                    ).position;
                endMarker = new Vector3(targetPos.x, 0, targetPos.z);
                startTime = Time.time;
                journeyLength = Vector3.Distance(startMarker, endMarker);

                MoveTo(endMarker);

                busy = true;

                // Update the position of the robot in the knowledge base so the planner does not get confused
                // The robot always finishes moving before continuing with a new plan, which makes this update legit
                if (worldModelManager)
                {
                    State state = worldModelManager.GetWorldStateCopy();
                    worldModelManager.UpdateKnowledge("at", state.GetStateOfVar("at")[0], false);
                    worldModelManager.UpdateKnowledge("at", action[1], true);
                }
                break;
            case "Clean":
                RoomHandler roomHandler = roomGenerator.Rooms.Find(
                    delegate(Transform room)
                    {
                        return room.name == action[1];
                    }
                    ).GetComponent<RoomHandler>();
                roomHandler.CleanRoom();
                busy = true;
                break;
            case "Finish":
                busy = true;
                break;
        }
    }

    /// <summary>
    /// Continue doing the current action
    /// </summary>
    private void ContinueAction()
    {
        switch (currentAction[0])
        {
            case "MoveTo":
                MoveTo(endMarker);
                break;
            case "Clean":
                currentAction.Clear();
                busy = false;
                break;
            case "Finish":
                currentAction.Clear();
                busy = false;
                break;
        }
    }

    /// <summary>
    /// Place a new plan in the action queue for execution
    /// </summary>
    /// <param name="plan">The plan to be executed by the robot</param>
    public void ExecutePlan(List<string> plan)
    {
        actionQueue.Clear();
        foreach (string step in plan)
        {
            List<string> action = new List<string>();
            if (step.Contains(","))
            {
                action.Add(step.Substring(1, step.IndexOf(',') - 1));

                string stepRemainder = step.Substring(step.IndexOf(',') + 2);
                while (stepRemainder.Contains(","))
                {
                    action.Add(stepRemainder.Substring(0, stepRemainder.IndexOf(',')));
                    stepRemainder = step.Substring(step.IndexOf(',') + 2);
                }
                action.Add(stepRemainder.Substring(0, stepRemainder.IndexOf(')')));
            }
            else
            {
                action.Add(step.Substring(1, step.IndexOf(')') - 1));
            }

            actionQueue.Enqueue(action);
        }
    }

    /// <summary>
    /// Move the robot to a target location
    /// </summary>
    /// <param name="target">The target location</param>
    private void MoveTo(Vector3 target)
    {
        float distCovered = (Time.time - startTime) * speed;
        float fracJourney = distCovered / journeyLength;
        transform.position = Vector3.Lerp(startMarker, target, fracJourney);

        if (transform.position == target)
        {
            currentAction.Clear();
            busy = false;
        }
    }
}