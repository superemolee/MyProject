// C# Unity3D HTN-Planner (CUHP), a simple HTN-Planner written in C# for use in Unity3D
// Copyright (C) 2013  Pieterjan van Gastel
// This software is under a GPLv3 license. For details, see the "HTNPlanner.cs"-file.
// If you do not have a copy of the "HTNPlanner.cs"-file, your version of the C# Unity 3D HTN-Planner is incomplete.

using System.Collections.Generic;
using System.Reflection;

namespace CsharpHTNplanner
{
    /// <summary>
    /// The planning domain for swapping items with each other.
    /// </summary>
    public class Domain_Swap
    {
        /// <summary>
        /// Adds a subtask to the return value of an HTN method (which is a list of subtasks)
        /// </summary>
        /// <param name="returnVal">The return value of an HTN method (a list of subtasks)</param>
        /// <param name="values">An array of strings consisting of a subtask and any number of parameters for the subtask</param>
        private static void AddTask(List<List<string>> returnVal, params string[] values)
        {
            returnVal.Add(new List<string>(values));
        }

        // METHODS (NON-PRIMITIVE TASKS)
        // An HTN method decomposes a non-primitive task into subtasks

        /// <summary>
        /// HTN method for swapping item x for item y
        /// </summary>
        /// <param name="state">The current internal state of the planner</param>
        /// <param name="x">The first item</param>
        /// <param name="y">The second item</param>
        /// <returns>A list of subtasks, contains Drop and Pickup tasks if all goes well</returns>
        public static List<List<string>> Swap_m(State state, string x, string y)
        {
            List<List<string>> returnVal = new List<List<string>>();
            if (state.CheckVar("have", x) && !state.CheckVar("holding", y))
            {
                // An alternative way of adding subtasks:
                // returnVal.Add(new List<string>(new string[] { "Drop", x }));

                // Decomposes the Swap task into Drop and Pickup subtasks
                AddTask(returnVal, "Drop", x);
                AddTask(returnVal, "Pickup", y);
            }
            else if (state.CheckVar("have", y) && !state.CheckVar("holding", x))
            {
                AddTask(returnVal, "Drop", y);
                AddTask(returnVal, "Pickup", x);
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
            MethodInfo[] swapInfos = new MethodInfo[] { this.GetType().GetMethod("Swap_m") };
            myDict.Add("Swap", swapInfos);
            return myDict;
        }


        // OPERATORS (PRIMITIVE TASKS)
        // A primitive task affects the internal state of the planner directly and cannot be decomposed into other subtasks

        /// <summary>
        /// Drop action for removing an item from inventory
        /// </summary>
        /// <param name="state">The current internal state</param>
        /// <param name="item">The item to be dropped</param>
        /// <returns>A new internal state where the given item is dropped</returns>
        public static State Drop(State state, string item)
        {
            State newState = state;
            // can only drop if agent actually has item
            if (state.CheckVar("have", item))
            {
                newState.Remove("have", item);
            }
            return newState;
        }

        /// <summary>
        /// Pickup action for adding an item to inventory
        /// </summary>
        /// <param name="state">The current internal state</param>
        /// <param name="item">The item to be picked up</param>
        /// <returns>A new internal state where the given item is picked up</returns>
        public static State Pickup(State state, string item)
        {
            State newState = state;
            // can only pickup if agent does not already have item
            if (!state.CheckVar("have", item))
            {
                newState.Add("have", item);
            }
            return newState;
        }
    }
}
