// C# Unity3D HTN-Planner (CUHP), a simple HTN-Planner written in C# for use in Unity3D
// Copyright (C) 2013  Pieterjan van Gastel
// This software is under a GPLv3 license. For details, see the "HTNPlanner.cs"-file.
// If you do not have a copy of the "HTNPlanner.cs"-file, your version of the C# Unity 3D HTN-Planner is incomplete.

using UnityEngine;

public class CameraInit : MonoBehaviour
{
    // METHODS

	/// <summary>
    /// Initializes the camera to look at the center of the world
	/// </summary>
	void Start () {
        this.transform.rotation = new Quaternion(0.2798481f, 0.3647052f, -0.1159169f, 0.8804762f);
	}
}
