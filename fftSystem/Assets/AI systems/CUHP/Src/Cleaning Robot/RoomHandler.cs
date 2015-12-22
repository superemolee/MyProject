// C# Unity3D HTN-Planner (CUHP), a simple HTN-Planner written in C# for use in Unity3D
// Copyright (C) 2013  Pieterjan van Gastel
// This software is under a GPLv3 license. For details, see the "HTNPlanner.cs"-file.
// If you do not have a copy of the "HTNPlanner.cs"-file, your version of the C# Unity 3D HTN-Planner is incomplete.

using UnityEngine;

public class RoomHandler : MonoBehaviour
{
    // FIELDS

    /// <summary>
    /// The world model manager
    /// </summary>
    public WorldModelManager worldModelManager;

    /// <summary>
    /// Denotes if this room is dirty
    /// </summary>
    private bool dirty;

    /// <summary>
    /// The default (or clean) color of the room
    /// </summary>
    private Color defaultColor = Color.white;

    /// <summary>
    /// The dirty color of the room
    /// </summary>
    private Color dirtyColor = Color.green;


    // PROPERTIES

    /// <summary>
    /// Denotes if this room is dirty
    /// </summary>
    public bool Dirty
    {
        get { return dirty; }
    }


    // METHODS

	/// <summary>
	/// Initializes this room handler
	/// </summary>
	void Start () {
        CleanRoom();
	}

    /// <summary>
    /// Changes the dirty/clean state of the room
    /// </summary>
    void OnMouseDown()
    {
        if (!dirty)
        {
            MakeDirty();
        }
        else
        {
            CleanRoom();
        }
    }

    /// <summary>
    /// Makes the room clean
    /// </summary>
    public void CleanRoom()
    {
        dirty = false;
        renderer.material.color = defaultColor;
        if (worldModelManager)
        {
            worldModelManager.UpdateKnowledge("dirty", name, false);
        }
    }

    /// <summary>
    /// Makes the room dirty
    /// </summary>
    private void MakeDirty()
    {
        dirty = true;
        renderer.material.color = dirtyColor;
        if (worldModelManager)
        {
            worldModelManager.UpdateKnowledge("dirty", name, true);
        }
    }
}
