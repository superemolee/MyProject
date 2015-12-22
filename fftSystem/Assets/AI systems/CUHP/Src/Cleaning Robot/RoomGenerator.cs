// C# Unity3D HTN-Planner (CUHP), a simple HTN-Planner written in C# for use in Unity3D
// Copyright (C) 2013  Pieterjan van Gastel
// This software is under a GPLv3 license. For details, see the "HTNPlanner.cs"-file.
// If you do not have a copy of the "HTNPlanner.cs"-file, your version of the C# Unity 3D HTN-Planner is incomplete.

using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    // FIELDS

    /// <summary>
    /// The prefab of a room
    /// </summary>
    public Transform roomPrefab = null;

    /// <summary>
    /// The map of the building (a list containing positions of rooms)
    /// </summary>
    public List<Vector2> map;

    /// <summary>
    /// The list of generated rooms
    /// </summary>
    private List<Transform> rooms = null;

    /// <summary>
    /// The distance between rooms
    /// </summary>
    private float roomDistance = 4.5f;


    // PROPERTIES

    /// <summary>
    /// The list of generated rooms
    /// </summary>
    public List<Transform> Rooms
    {
        get { return rooms; }
    }


    // METHODS

	/// <summary>
	/// Initializes the room generator
	/// </summary>
	void Start () {
        if (roomPrefab != null)
        {
            rooms = new List<Transform>();
            GenerateRooms();
            if (GetComponent<WorldModelManager>())
            {
                GetComponent<WorldModelManager>().ConnectRooms(rooms, roomDistance);
            }
        }
        else
        {
            Debug.Log("No prefab found");
        }
	}

    /// <summary>
    /// Generates all the rooms of the building
    /// </summary>
    private void GenerateRooms()
    {
        Vector3 startPos = new Vector3(0, -1, 0);

        AddRoom(Instantiate(roomPrefab, startPos, Quaternion.identity) as Transform);

        RemoveDuplicatesFromMap();

        foreach (Vector2 roomLoc in map)
        {
            if (roomLoc.x != 0 || roomLoc.y != 0)
            {
                Vector3 position = startPos;
                position.x = roomLoc.x * roomDistance;
                position.z = roomLoc.y * roomDistance;
                AddRoom(Instantiate(roomPrefab, position, Quaternion.identity) as Transform);
            }
        }
    }

    /// <summary>
    /// Adds a room to the list of generated rooms
    /// </summary>
    /// <param name="newRoom">The new room to be added</param>
    private void AddRoom(Transform newRoom)
    {
        newRoom.name = "room" + rooms.Count;
        newRoom.GetComponent<RoomHandler>().worldModelManager = GetComponent<WorldModelManager>();
        rooms.Add(newRoom);
    }

    /// <summary>
    /// Removes all duplicate rooms from the map so no rooms get generated on the same position
    /// </summary>
    private void RemoveDuplicatesFromMap()
    {
        List<Vector2> newMap = new List<Vector2>();

        foreach (Vector2 position in map)
        {
            if (!newMap.Contains(position))
                newMap.Add(position);
        }

        map = newMap;
    }
}
