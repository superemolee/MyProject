//
// C# Unity3D HTN-Planner (CUHP), version 1.0.1
//
// A simple HTN-Planner written in C# for use in Unity3D
// Author: Pieterjan van Gastel
// Date created: 2013-08-19
// Date last modified: 2013-10-23
//
// This work of software is inspired by:
// Dana Nau's pyhop implementation; and
// the book Automated Planning: Theory and Practice by Malik Ghallab et al.
//
//
// LICENSE:
// C# Unity3D HTN-Planner (CUHP), a simple HTN-Planner written in C# for use in Unity3D
// Copyright (C) 2013  Pieterjan van Gastel
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
//
// VERBOSE PARAMETER:
// The verbose parameter (debugging) explained:
// if verbose = 0 (the default), no debug information is provided;
// if verbose >= 1, debug information includes the initial parameters and the answer;
// if verbose >= 2, debug information also includes a message on each recursive call;
// if verbose = 3, debug information also includes info about what it is computing.
//
//
// CONTACT DETAILS:
// Full name:           P.J.G. van Gastel, B-ICT
// Email address:       pjvgastel@hotmail.com
// Programming blog:    http://pjvgastel-programming.blogspot.com/
// Facebook page:       https://www.facebook.com/PJsAIandOtherDev
// Linkedin profile:    http://www.linkedin.com/in/pjgvangastel
// Twitter account:     https://twitter.com/PJvG90
//

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CsharpHTNplanner
{
    public class HTNPlanner
    {
        // FIELDS

        /// <summary>
        /// A list containing the names of all tasks of the given planning domain for which there are primitive actions
        /// </summary>
        private List<string> operators = new List<string>();

        /// <summary>
        /// A dictionary containing the names of all non-primitive tasks of the given planning domain as dictionary keys, 
        /// and a list containing the names of all HTN-methods, which can decompose the non-primitive tasks into subtasks, as dictionary values
        /// </summary>
        private Dictionary<string, List<string>> methods = new Dictionary<string, List<string>>();

        /// <summary>
        /// The type of the class containing the HTN-methods of the given planning domain
        /// </summary>
        private Type methodsType;

        /// <summary>
        /// The type of the class containing the operators of the given planning domain
        /// </summary>
        private Type operatorsType;

        /// <summary>
        /// The depth at which the planner should stop looking for a plan (prevents the planner from looking too deeply)
        /// </summary>
        private int searchDepth = 30;


        // CONSTRUCTORS

        /// <summary>
        /// Creates a new HTN planner
        /// </summary>
        /// <param name="methodsType">The type of the class containing the HTN-methods of the planning domain</param>
        /// <param name="methodsDict">Dictionary containing task names as keys, and arrays containing all HTN-methods (to decompose the given task into subtasks) as values</param>
        /// <param name="operatorsType">The type of the class containing the operators of the planning domain</param>
        public HTNPlanner(Type methodsType, Dictionary<string, MethodInfo[]> methodsDict, Type operatorsType)
        {
            this.methodsType = methodsType;
            this.operatorsType = operatorsType;

            InitializePlanner(methodsDict);
        }


        // METHODS

        /// <summary>
        /// Initializes the planner by declaring operators and methods.
        /// </summary>
        /// <param name="methodsDict">Dictionary containing task names as keys, and arrays containing all HTN-methods (to decompose the given task into subtasks) as values</param>
        private void InitializePlanner(Dictionary<string, MethodInfo[]> methodsDict)
        {
            DeclareOperators();
            foreach (KeyValuePair<string, MethodInfo[]> method in methodsDict)
            {
                DeclareMethods(method.Key, method.Value);
            }
        }

        /// <summary>
        /// Finds all operators in the given domain and adds their names to the list "operators" for later use.
        /// </summary>
        /// <returns>Returns the list "operators" (for debugging purposes)</returns>
        public List<string> DeclareOperators()
        {
            MethodInfo[] methodInfos = operatorsType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            operators = new List<string>();

            foreach (MethodInfo info in methodInfos)
            {
                if (info.ReturnType.Name.Equals("State"))
                {
                    string methodName = info.Name;
                    if (!operators.Contains(methodName))
                        operators.Add(methodName);
                }
            }

            return operators;
        }

        /// <summary>
        /// Finds the non-primitive action matching the task name and the HTN-methods in the given domain which can be used to decompose 
        /// the given task into subtasks, and adds their names to a list.
        /// </summary>
        /// <param name="taskName">The task name</param>
        /// <param name="methodInfos">An array containing the info of all HTN-methods that can decompose the given task into subtasks</param>
        /// <returns>Returns the list with the names of the HTN-methods which can be used for the given task (for debugging purposes)</returns>
        public List<string> DeclareMethods(string taskName, MethodInfo[] methodInfos)
        {
            List<string> methodList = new List<string>();

            foreach (MethodInfo info in methodInfos)
            {
                if (info != null && info.ReturnType.Name.Equals("List`1"))
                {
                    methodList.Add(info.Name);
                }
            }

            if (methods.ContainsKey(taskName))
                methods.Remove(taskName);
            methods.Add(taskName, methodList);

            return methods[taskName];
        }

        /// <summary>
        /// Solves a given planning problem.
        /// </summary>
        /// <param name="state">The initial state of the planning problem</param>
        /// <param name="tasks">The goal tasks of the planning problem</param>
        /// <param name="verbose">The level of debugging information</param>
        /// <returns>A plan in the form of a list of strings, in which each item is a primitive action</returns>
        public List<string> SolvePlanningProblem(State state, List<List<string>> tasks, int verbose = 0)
        {
            if (verbose > 0)
            {
                Debug.Log("CUHP, verbose = " + verbose);
                Debug.Log("State = " + state.Name);
                Debug.Log("Tasks = " + tasks.ToString());
            }
            List<string> result = SeekPlan(state, tasks, new List<string>(), 0, verbose);
            if (verbose > 0)
                Debug.Log("Result = " + result.ToString());
            return result;
        }

        /// <summary>
        /// The actual HTN-planner.
        /// </summary>
        /// <param name="state">The current state of the planner</param>
        /// <param name="tasks">The current (goal) tasks of the planner</param>
        /// <param name="plan">The (partial) plan</param>
        /// <param name="depth">The depth of the plan (for debugging purposes)</param>
        /// <param name="verbose">The level of debugging information</param>
        /// <returns></returns>
        public List<string> SeekPlan(State state, List<List<string>> tasks, List<string> plan, int depth, int verbose = 0)
        {
            // safety
            if (searchDepth > 0)
            {
                if (depth >= searchDepth)
                    return null;
            }

            if (verbose > 1)
                Debug.Log("Depth: " + depth + ", tasks: " + tasks.ToString());
            if (tasks.Count == 0)
            {
                if (verbose > 2)
                    Debug.Log("Depth " + depth + " returns plan: " + plan.ToString());
                return plan;
            }
            List<string> task = tasks[0];
            if (operators.Contains(task[0]))
            {
                if (verbose > 2)
                    Debug.Log("Depth: " + depth + ", action: " + task.ToString());

                MethodInfo info = operatorsType.GetMethod(task[0]);
                object[] parameters = new object[task.Count];
                parameters[0] = new State(state);
                if (task.Count > 1)
                {
                    int x = 1;
                    List<string> paramets = task.GetRange(1, (task.Count - 1));
                    foreach (string param in paramets)
                    {
                        parameters[x] = param;
                        x++;
                    }
                }
                State newState = (State)info.Invoke(null, parameters);

                if (verbose > 2)
                    Debug.Log("Depth: " + depth + ", new state: " + newState.ToString());

                if (newState != null)
                {
                    string toAddToPlan = "(" + task[0];
                    if (task.Count > 1)
                    {
                        List<string> paramets = task.GetRange(1, (task.Count - 1));
                        foreach (string param in paramets)
                        {
                            toAddToPlan += (", " + param);
                        }
                    }
                    toAddToPlan += ")";
                    plan.Add(toAddToPlan);
                    List<string> solution = SeekPlan(newState, tasks.GetRange(1, (tasks.Count - 1)), plan, (depth + 1), verbose);
                    if (solution != null)
                        return solution;
                }
            }
            if (methods.ContainsKey(task[0]))
            {
                if (verbose > 2)
                    Debug.Log("Depth: " + depth + ", method instance: " + task.ToString());

                List<string> relevant = methods[task[0]];
                foreach (string method in relevant)
                {
                    // Decompose non-primitive task into subtasks by use of a HTN method (invoke C# method)
                    MethodInfo info = methodsType.GetMethod(method);
                    object[] parameters = new object[task.Count];
                    parameters[0] = new State(state);
                    if (task.Count > 1)
                    {
                        int x = 1;
                        List<string> paramets = task.GetRange(1, (task.Count - 1));
                        foreach (string param in paramets)
                        {
                            parameters[x] = param;
                            x++;
                        }
                    }
                    List<List<string>> subtasks = null;
                    try
                    {
                        subtasks = (List<List<string>>)info.Invoke(null, parameters);
                    }
                    catch (Exception)
                    {
                    }

                    if (verbose > 2)
                        Debug.Log("Depth: " + depth + ", new tasks: " + subtasks.ToString());
                    if (subtasks != null)
                    {
                        List<List<string>> newTasks = new List<List<string>>(subtasks);
                        newTasks.AddRange(tasks.GetRange(1, (tasks.Count - 1)));
                        try
                        {
                            List<string> solution = SeekPlan(state, newTasks, plan, (depth + 1), verbose);
                            if (solution != null)
                                return solution;
                        }
                        catch (StackOverflowException)
                        {
                        }
                    }
                }
            }
            if (verbose > 2)
                Debug.Log("Depth " + depth + " returns failure!");
            return null;
        }
    }
}