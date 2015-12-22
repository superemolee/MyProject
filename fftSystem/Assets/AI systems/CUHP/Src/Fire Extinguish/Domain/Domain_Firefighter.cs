using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

namespace CsharpHTNplanner
{
	public class Domain_Firefighter
	{
		
		// METHODS (NON-PRIMITIVE TASKS)
        // An HTN method decomposes a non-primitive task into subtasks

        /// <summary>
        /// HTN method for cleaning all dirty rooms
        /// </summary>
        /// <param name="state">The current internal state</param>
        /// <returns>A list of subtasks which ensure all dirty rooms are cleaned</returns>
        public static List<List<string>> ExtinguishFire_m(State state)
        {
            List<List<string>> returnVal = new List<List<string>>();

            if (state.ContainsRelation("at","fire"))
            {
                string fireposition = state.GetStateOfRelation("at","fire")[0];
                if (state.CheckRelation("have","firefighter","extinguisher"))
				{
                	if(state.CheckRelation("at","firefighter",fireposition))
					{
                    	AddTask(returnVal, "Extinguish");
						AddTask(returnVal, "ExtinguishFire");
               	 	}
                	else
                	{
                    	AddTask(returnVal, "GoTo", fireposition);
						
						AddTask(returnVal, "Extinguish");
						
						AddTask(returnVal, "ExtinguishFire");
                	}
				}
				else
				{
					string extinguisherposition = state.GetStateOfRelation("at","extinguisher")[0];
					AddTask(returnVal, "GoTo", extinguisherposition);
					AddTask(returnVal, "PickUp",extinguisherposition);
					AddTask(returnVal, "GoTo",fireposition);
					AddTask(returnVal,"Extinguish");
					AddTask(returnVal, "ExtinguishFire");
				}
            }

            return returnVal;
        }

		// OPERATORS (PRIMITIVE TASKS)
		// A primitive task affects the internal state of the planner directly and cannot be decomposed into other subtasks


		public static State GoTo (State state, string position)
		{
			State newState = state;
			if (state.ContainsRelation ("at", "firefighter") && !state.CheckRelation("at", "firefighter", position)) {
				// there should only be one match for the "at"-variable and that is the robot's current location
				string currentposition = state.GetStateOfRelation ("at","firefighter") [0];
				// the robot can only move to the given room if it is adjecent to its current location
				newState.Remove("at","firefighter",currentposition);
				newState.Add("at","firefighter",position);
			}
			return newState;
		}
		
		// pick up the fire extinguisher
		// could be a method??? when not at , goto
		public static State PickUp (State state, string extinguisher)
		{
			State newState = state;
			if(state.ContainsVar("handempty")&&state.ContainsRelation("at","firefighter"))
			{
				if((state.GetStateOfVar("handempty")[0] == "firefighter")&&(state.GetStateOfRelation("at","firefighter")[0] == "A"))
				{
					newState.Remove("handempty", "firefighter");
					newState.Remove("at","firefighter","A");
					newState.Add("have", "firefighter", extinguisher);
				}
			}
			return newState;
		}
		
		public static State Extinguish(State state)
		{
			State newState = state;
			if(state.ContainsRelation("at","fire"))
			{
				string fireposition = state.GetStateOfRelation("at","fire")[0];
				newState.Remove("at","fire",fireposition);
			}
			return newState;
		}
		
		        /// <summary>
        /// Function for getting all methods of this domain, so the HTN planner can use it. This is mandatory for the HTN planner to work.
        /// </summary>
        /// <returns>Dictionary containing task names as keys, and arrays containing all HTN methods (to decompose the given task into subtasks) as values</returns>
        public Dictionary<string, MethodInfo[]> GetMethodsDict()
        {
            Dictionary<string, MethodInfo[]> myDict = new Dictionary<string, MethodInfo[]>();
            MethodInfo[] extinguishfireInfos = new MethodInfo[] { this.GetType().GetMethod("ExtinguishFire_m") };
            myDict.Add("ExtinguishFire", extinguishfireInfos);
            return myDict;
        }
		
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
				Debug.Log("addtask went wrong!");
            }
        }
	}
}
