using UnityEngine;
using System.Collections;


using System.Collections.Generic;
using CsharpHTNplanner;
using UnityEngine;

public class WorldStateManager : MonoBehaviour
{
    
	
	// FIELDS

	/// <summary>
	/// The state of the world (a knowledge base)
	/// </summary>
	private State worldState;
	

	// METHODS

	/// <summary>
	/// Creates the world state at start of scene
	/// </summary>
	void Awake ()
	{
		worldState = new State ("statefirefighter");
//		InitScene();
	}

	/// <summary>
	/// Gets a copy of the world state
	/// </summary>
	/// <returns>A copy of the world state</returns>
	public State GetWorldStateCopy ()
	{
		if (worldState != null) {
			return new State (worldState);
		}
		return null;
	}

	/// <summary>
	/// Updates the knowledge in the world state (knowledge base)
	/// </summary>
	/// <param name="variable">The variable to update</param>
	/// <param name="value">The value for the variable to update</param>
	/// <param name="truthValue">Whether the variable needs to be set to true or false</param>
	public void UpdateKnowledge (string variable, string value, bool truthValue)
	{
		if (truthValue) {
			worldState.Add (variable, value);
		} else {
			worldState.Remove (variable, value);
		}
	}

	/// <summary>
	/// Updates the knowledge in the world state (knowledge base)
	/// </summary>
	/// <param name="relation">The relation to update</param>
	/// <param name="firstElement">The first element of the relation to update</param>
	/// <param name="secondElement">The second element of the relation to update</param>
	/// <param name="truthValue">Whether the relation needs to be set to true or false</param>
	public void UpdateKnowledge (string relation, string firstElement, string secondElement, bool truthValue)
	{
		if (truthValue) {
			worldState.Add (relation, firstElement, secondElement);
		} else {
			worldState.Remove (relation, firstElement, secondElement);
		}
	}
	
	//init scene state
	// TODO: add and delete should use the update knowledge
//	public void InitScene ()
//	{
//		worldState.Add ("at", "fire", "A");
//		worldState.Add ("at", "extinguisher", "B");
//		worldState.Add ("at", "firefighter", "C");
//		worldState.Add ("handempty", "firefighter");
//
//	}
//    /// <summary>
//    /// Connects all rooms by marking them adjecent to each other in the world state (knowledge base)
//    /// </summary>
//    /// <param name="rooms">The list of rooms to connect with each other</param>
//    /// <param name="roomDistance">The distance between rooms</param>
//    public void ConnectRooms(List<Transform> rooms, float roomDistance)
//    {
//        foreach (Transform room in rooms)
//        {
//            foreach (Transform otherRoom in rooms)
//            {
//                if (room != otherRoom)
//                {
//                    if (room.position.x == otherRoom.position.x)
//                    {
//                        if (Mathf.Abs(room.position.z - otherRoom.position.z) == roomDistance)
//                        {
//                            worldState.Add("adjecent", room.name, otherRoom.name);
//                        }
//                    }
//                    else if (room.position.z == otherRoom.position.z)
//                    {
//                        if (Mathf.Abs(room.position.x - otherRoom.position.x) == roomDistance)
//                        {
//                            worldState.Add("adjecent", room.name, otherRoom.name);
//                        }
//                    }
//                }
//            }
//        }
//    }
}
