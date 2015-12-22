// C# Unity3D HTN-Planner (CUHP), a simple HTN-Planner written in C# for use in Unity3D
// Copyright (C) 2013  Pieterjan van Gastel
// This software is under a GPLv3 license. For details, see the "HTNPlanner.cs"-file.
// If you do not have a copy of the "HTNPlanner.cs"-file, your version of the C# Unity 3D HTN-Planner is incomplete.

using System;
using UnityEngine;

public class RobotGUI : MonoBehaviour
{
    // FIELDS

    /// <summary>
    /// Determines if the info-window needs to be shown
    /// </summary>
    private bool showInfoWindow = false;

    /// <summary>
    /// Rectangle for the pop-up window (this is required to make the window dragable)
    /// </summary>
    private Rect windowRect = new Rect(110, 10, 520, 200);

    /// <summary>
    /// Denotes if something went wrong in the planning interface
    /// </summary>
    private bool somethingWrong = false;


    // METHODS

    /// <summary>
    /// Shows the GUI elements
    /// </summary>
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 80, 30), "Info"))
        {
            showInfoWindow = !showInfoWindow;
        }

        if (GUI.Button(new Rect(10, 50, 80, 30), "Clean"))
        {
            if (GetComponent<PlannerInterface>())
            {
                somethingWrong = !(GetComponent<PlannerInterface>().CleanRooms());
            }
        }

        if (somethingWrong)
            GUI.Label(new Rect(10, 90, 300, 30), "Sorry, something went wrong.");

        if (showInfoWindow)
            windowRect = GUI.Window(0, windowRect, DoInfoWindow, "Information");
    }

    /// <summary>
    /// Shows a pop-up window
    /// </summary>
    /// <param name="windowID">The ID of the window</param>
    void DoInfoWindow(int windowID)
    {
        GUI.Label(new Rect(10, 30, 500, 80), "This is the cleaning robot example of the C# Unity3D HTN-Planner (CUHP)." + Environment.NewLine +
            "Click on a room to make it dirty or clean." + Environment.NewLine +
            "If you press the \"Clean\"-button, a plan is made for the robot to execute. On receiving this plan, the robot will move from room to room to clean the building.");

        GUI.Label(new Rect(10, 105, 500, 50), "C# Unity3D HTN-Planner (CUHP)  Copyright (C) 2013  Pieterjan van Gastel" + Environment.NewLine +
            "This program comes with ABSOLUTELY NO WARRANTY. This is free software," + Environment.NewLine +
            "and you are welcome to redistribute it under certain conditions.");

        if (GUI.Button(new Rect(210, 170, 100, 20), "Close"))
            showInfoWindow = false;

        GUI.DragWindow();
    }
}
