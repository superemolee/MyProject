// C# Unity3D HTN-Planner (CUHP), a simple HTN-Planner written in C# for use in Unity3D
// Copyright (C) 2013  Pieterjan van Gastel
// This software is under a GPLv3 license. For details, see the "HTNPlanner.cs"-file.
// If you do not have a copy of the "HTNPlanner.cs"-file, your version of the C# Unity 3D HTN-Planner is incomplete.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace CsharpHTNplanner
{
    /// <summary>
    /// The planning domain for cleaning rooms with a vacuum cleaning robot.
    /// </summary>
    public class Domain_CleaningRobot
    {
        /// <summary>
        /// Adds a subtask to the return value of an HTN method (which is a list of subtasks)
        /// </summary>
        /// <param name="returnVal">The return value of an HTN method (a list of subtasks)</param>
        /// <param name="values">An array of strings consisting of a subtask and any number of parameters for the subtask</param>
        private static void AddTask(List<List<string>> returnVal, params string[] values)
        {
            try
            {
                returnVal.Add(new List<string>(values));
            }
            catch (StackOverflowException)
            {
            }
        }

        // METHODS (NON-PRIMITIVE TASKS)
        // An HTN method decomposes a non-primitive task into subtasks

        /// <summary>
        /// HTN method for cleaning all dirty rooms
        /// </summary>
        /// <param name="state">The current internal state</param>
        /// <returns>A list of subtasks which ensure all dirty rooms are cleaned</returns>
        public static List<List<string>> CleanRooms_m(State state)
        {
            List<List<string>> returnVal = new List<List<string>>();

            if (state.ContainsVar("dirty"))
            {
                string room = state.GetStateOfVar("at")[0];
                if (state.CheckVar("dirty", room))
                {
                    AddTask(returnVal, "Clean", room);
                    // make sure all dirty rooms will be cleaned
                    AddTask(returnVal, "CleanRooms");
                }
                else
                {
                    string dirtyRoom = state.GetStateOfVar("dirty")[0];
                    AddTask(returnVal, "MoveTo", dirtyRoom);
                    // AddTask(returnVal, "Clean", dirtyRoom);
                    // make sure all dirty rooms will be cleaned
                    AddTask(returnVal, "CleanRooms");
                }
            }
            else
            {
                AddTask(returnVal, "Finish");
            }

            return returnVal;
        }

        /// <summary>
        /// HTN method for finding a way to a room
        /// </summary>
        /// <param name="state">The current internal state</param>
        /// <param name="room">To room to which the robot needs to travel to</param>
        /// <returns>A list of subtasks to get from the current room to the room given in the parameters if a path can be found, else it returns null</returns>
        public static List<List<string>> MoveTo_m(State state, string room)
        {
            List<List<string>> returnVal = new List<List<string>>();

            if (state.ContainsVar("at") && !state.CheckVar("at", room))
            {
                string location = state.GetStateOfVar("at")[0];
                List<string> adjecentRooms = state.GetStateOfRelation("adjecent", location);
                bool foundLink = false;
                foreach (string nextRoom in adjecentRooms)
                {
                    if (state.CheckRelation("adjecent", nextRoom, room))
                    {
                        foundLink = true;
                        AddTask(returnVal, "MoveTo", nextRoom);
                        break;
                    }
                }
                if (!foundLink)
                {
                    foreach (string nextRoom in adjecentRooms)
                    {
                        List<string> moreRooms = state.GetStateOfRelation("adjecent", nextRoom);
                        foreach (string otherRoom in moreRooms)
                        {
                            if (otherRoom != location && state.CheckRelation("adjecent", otherRoom, room))
                            {
                                foundLink = true;
                                AddTask(returnVal, "MoveTo", nextRoom);
                                break;
                            }
                        }
                    }
                }
                if (!foundLink)
                {
                    bool allRoomsChecked = true;
                    foreach (string adjRoom in adjecentRooms)
                    {
                        if (!state.CheckVar("checked", adjRoom))
                        {
                            allRoomsChecked = false;
                            AddTask(returnVal, "MoveTo", adjRoom);
                            break;
                        }
                    }
                    if (allRoomsChecked)
                        return null;
                }
            }
            else
            {
                return null;
            }
                
            return returnVal;
        }

        /// <summary>
        /// Function for getting all methods of this domain, so the HTN planner can use it. This is mandatory for the HTN planner to work.
        /// </summary>
        /// <returns>Dictionary containing task names as keys, and arrays containing all HTN methods (to decompose the given task into subtasks) as values</returns>
        public Dictionary<string, MethodInfo[]> GetMethodsDict()
        {
            Dictionary<string, MethodInfo[]> myDict = new Dictionary<string, MethodInfo[]>();
            MethodInfo[] cleanInfos = new MethodInfo[] { this.GetType().GetMethod("CleanRooms_m") };
            myDict.Add("CleanRooms", cleanInfos);
            MethodInfo[] moveInfos = new MethodInfo[] { this.GetType().GetMethod("MoveTo_m") };
            myDict.Add("MoveTo", moveInfos);
            return myDict;
        }


        // OPERATORS (PRIMITIVE TASKS)
        // A primitive task affects the internal state of the planner directly and cannot be decomposed into other subtasks

        /// <summary>
        /// Action to move from the current room to an adjecent room.
        /// </summary>
        /// <param name="state">The current internal state</param>
        /// <param name="room">The room to which the robot needs to travel</param>
        /// <returns>A new internal state in which the robot has moved to another room (but only if it is an adjecent room)</returns>
        public static State MoveTo(State state, string room)
        {
            State newState = state;
            if (state.ContainsVar("at") && !state.CheckVar("at", room))
            {
                // there should only be one match for the "at"-variable and that is the robot's current location
                string location = state.GetStateOfVar("at")[0];
                // the robot can only move to the given room if it is adjecent to its current location
                if (state.CheckRelation("adjecent", location, room))
                {
                    newState.Remove("at", location);
                    newState.Add("at", room);
                    newState.Add("checked", room);
                }
                else
                    return null;
            }
            return newState;
        }

        /// <summary>
        /// Action to clean a room.
        /// </summary>
        /// <param name="state">The current internal state</param>
        /// <param name="room">The room which needs to be cleaned</param>
        /// <returns>A new internal state in which room is cleaned if it was dirty</returns>
        public static State Clean(State state, string room)
        {
            State newState = state;
            // can only clean if robot is at room and room is dirty
            if (state.CheckVar("at", room) && state.CheckVar("dirty", room))
            {
                newState.Remove("dirty", room);
                newState.Add("clean", room);

                // removing checked rooms so robot does not get stuck
                List<string> checkedList = new List<string>(newState.GetStateOfVar("checked"));
                foreach (string checkedRoom in checkedList)
                {
                    if (checkedRoom != room)
                        newState.Remove("checked", checkedRoom);
                }
            }
            return newState;
        }

        /// <summary>
        /// (Meaningless) action to indicate the robot is finished cleaning all rooms.
        /// </summary>
        /// <param name="state">The current internal state</param>
        /// <returns>A new internal state which states the robot is finished</returns>
        public static State Finish(State state)
        {
            State newState = state;

            newState.Add("finished", "true");

            return newState;
        }
    }
}
