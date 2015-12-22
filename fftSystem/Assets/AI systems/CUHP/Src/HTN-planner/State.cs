// C# Unity3D HTN-Planner (CUHP), a simple HTN-Planner written in C# for use in Unity3D
// Copyright (C) 2013  Pieterjan van Gastel
// This software is under a GPLv3 license. For details, see the "HTNPlanner.cs"-file.
// If you do not have a copy of the "HTNPlanner.cs"-file, your version of the C# Unity 3D HTN-Planner is incomplete.

using System.Collections.Generic;

namespace CsharpHTNplanner
{
    public class State
    {
        // FIELDS

        /// <summary>
        /// Dictionary containing variables. A variable can be represented as: "have(money)", "name(John)", or "likes(Mary)".
        /// </summary>
        private Dictionary<string, List<string>> stateVariables = new Dictionary<string, List<string>>();

        /// <summary>
        /// Dictionary containing variables in the form of relations. This allows for more complex planning behavior.
        /// A relation can be represented as: "parentOf(Glóin, Gimli)", "sees(ammo, weapon)" (i.e. agent sees "ammo" of type "weapon"), 
        /// or "adjecent(roomOne, roomTwo)".
        /// </summary>
        private Dictionary<string, Dictionary<string, List<string>>> stateRelations = new Dictionary<string, Dictionary<string, List<string>>>();


        // PROPERTIES

        /// <summary>
        /// The name of the state.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        // CONSTRUCTORS

        /// <summary>
        /// Constructor for a new empty state.
        /// </summary>
        /// <param name="name">The name of the state</param>
        public State(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Constructor for a copy of another state.
        /// </summary>
        /// <param name="state">The state which needs to be copied</param>
        public State(State state)
        {
            this.Name = state.Name;
            this.stateVariables = state.GetVariablesCopy();
            this.stateRelations = state.GetRelationsCopy();
        }


        // METHODS

        /// <summary>
        /// Adds a variable to the internal state of the planner.
        /// </summary>
        /// <param name="variable">The variable name</param>
        /// <param name="innerState">The variable value</param>
        public void Add(string variable, string innerState)
        {
            if (stateVariables.ContainsKey(variable))
            {
                stateVariables[variable].Add(innerState);
            }
            else
            {
                stateVariables.Add(variable, new List<string>(new string[]{innerState}));
            }
        }

        /// <summary>
        /// Removes a variable from the internal state of the planner.
        /// </summary>
        /// <param name="variable">The variable name</param>
        /// <param name="innerState">The variable value</param>
        public void Remove(string variable, string innerState)
        {
            if (stateVariables.ContainsKey(variable))
            {
                stateVariables[variable].Remove(innerState);
                if (stateVariables[variable].Count == 0)
                {
                    stateVariables.Remove(variable);
                }
            }
        }

        /// <summary>
        /// Gets all the values of a variable.
        /// </summary>
        /// <param name="variable">The variable name</param>
        /// <returns>A list containing all values of the given variable</returns>
        public List<string> GetStateOfVar(string variable)
        {
            if (ContainsVar(variable))
                return stateVariables[variable];
            return null;
        }

        /// <summary>
        /// Checks if variable is true for any value
        /// </summary>
        /// <param name="variable">The variable name</param>
        /// <returns>True if variable is true for any value, false if variable is true for no value</returns>
        public bool ContainsVar(string variable)
        {
            return stateVariables.ContainsKey(variable);
        }

        /// <summary>
        /// Checks if variable is true for given value, return true if a match is found.
        /// </summary>
        /// <param name="variable">The variable name</param>
        /// <param name="innerState">The value to check</param>
        /// <returns>True if a match between the variable and value is found, else false</returns>
        public bool CheckVar(string variable, string innerState)
        {
            if (stateVariables.ContainsKey(variable) && stateVariables[variable].Contains(innerState))
                return true;
            return false;
        }

        /// <summary>
        /// Checks if variable is true for given value, return true if a match is found.
        /// </summary>
        /// <param name="variable">The variable name</param>
        /// <param name="innerState">The value to check</param>
        /// <returns>True if a match between the variable and value is found, else false</returns>
        public bool Holds(string variable, string innerState)
        {
            return CheckVar(variable, innerState);
        }

        /// <summary>
        /// Gets a copy of the variables.
        /// </summary>
        /// <returns>Dictionary containing variables</returns>
        public Dictionary<string, List<string>> GetVariablesCopy()
        {
            Dictionary<string, List<string>> copy = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, List<string>> pair in stateVariables)
            {
                copy.Add(pair.Key, new List<string>(pair.Value));
            }
            return copy;
        }

        /// <summary>
        /// Adds a relation to the internal state of the planner.
        /// </summary>
        /// <param name="variable">The relation name</param>
        /// <param name="elementOne">The first element of the relation</param>
        /// <param name="elementTwo">The second element of the relation</param>
        public void Add(string variable, string elementOne, string elementTwo)
        {
            if (stateRelations.ContainsKey(variable))
            {
                if (stateRelations[variable].ContainsKey(elementOne))
                {
                    stateRelations[variable][elementOne].Add(elementTwo);
                }
                else
                {
                    stateRelations[variable].Add(elementOne, new List<string>(new string[] { elementTwo }));
                }
            }
            else
            {
                stateRelations.Add(variable, new Dictionary<string, List<string>>());
                stateRelations[variable].Add(elementOne, new List<string>(new string[] { elementTwo }));
            }
        }

        /// <summary>
        /// Removes a relation from the internal state of the planner.
        /// </summary>
        /// <param name="variable">The relation name</param>
        /// <param name="elementOne">The first element of the relation</param>
        /// <param name="elementTwo">The second element of the relation</param>
        public void Remove(string variable, string elementOne, string elementTwo)
        {
            if (stateRelations.ContainsKey(variable) && stateRelations[variable].ContainsKey(elementOne))
            {
                stateRelations[variable][elementOne].Remove(elementTwo);
                if (stateRelations[variable][elementOne].Count == 0)
                {
                    stateRelations[variable].Remove(elementOne);
                    if (stateRelations[variable].Count == 0)
                    {
                        stateRelations.Remove(variable);
                    }
                }
            }
        }

        /// <summary>
        /// Gets all the values of a relation.
        /// </summary>
        /// <param name="variable">The relation name</param>
        /// <returns>A dictionary containing all values of a relation</returns>
        public Dictionary<string, List<string>> GetStateOfRelation(string variable)
        {
            return stateRelations[variable];
        }

        /// <summary>
        /// Gets all matches of a relation with the first element filled in.
        /// </summary>
        /// <param name="variable">The relation name</param>
        /// <param name="elementOne">The first element of the relation</param>
        /// <returns>A list containing all "second elements" that match with the first element for the given relation name</returns>
        public List<string> GetStateOfRelation(string variable, string elementOne)
        {
            return stateRelations[variable][elementOne];
        }

        /// <summary>
        /// Checks if relation is true for any value.
        /// </summary>
        /// <param name="variable">The relation name</param>
        /// <returns>True if relation is true for any value, false if relation is true for no value</returns>
        public bool ContainsRelation(string variable)
        {
            return stateRelations.ContainsKey(variable);
        }

        /// <summary>
        /// Checks if there is a relation between one given element and any other.
        /// </summary>
        /// <param name="variable">The relation name</param>
        /// <param name="elementOne">The first element of the relation</param>
        /// <returns>True if any second element can be found for this relation, false if no second element can be found</returns>
        public bool ContainsRelation(string variable, string elementOne)
        {
            return (stateRelations.ContainsKey(variable) && stateRelations[variable].ContainsKey(elementOne));
        }

        /// <summary>
        /// Checks if there is a relation between any element and a given "second element".
        /// </summary>
        /// <param name="variable">The relation name</param>
        /// <param name="elementTwo">The second element of the relation</param>
        /// <returns>True if any match is found, else false</returns>
        public bool ContainsRelationMatch(string variable, string elementTwo)
        {
            if (!stateRelations.ContainsKey(variable))
                return false;

            foreach (KeyValuePair<string, List<string>> pair in stateRelations[variable])
            {
                if (pair.Value.Contains(elementTwo))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a list of elements that fit as "first element" in a relation given a "second element".
        /// </summary>
        /// <param name="variable">The relation name</param>
        /// <param name="elementTwo">The second element of the relation</param>
        /// <returns>A list of elements that fit as "first element" in a relation given a "second element", 
        /// returns an empty list of no elements match the given format</returns>
        public List<string> UnifyRelation(string variable, string elementTwo)
        {
            List<string> unificationList = new List<string>();

            if (!stateRelations.ContainsKey(variable))
                return unificationList;

            foreach (KeyValuePair<string, List<string>> pair in stateRelations[variable])
            {
                if (pair.Value.Contains(elementTwo))
                    unificationList.Add(pair.Key);
            }

            return unificationList;
        }

        /// <summary>
        /// Checks if a relation is true between two given elements.
        /// </summary>
        /// <param name="variable">The relation name</param>
        /// <param name="elementOne">The first element of the relation</param>
        /// <param name="elementTwo">The second element of the relation</param>
        /// <returns>True if a match is found, else false</returns>
        public bool CheckRelation(string variable, string elementOne, string elementTwo)
        {
            if (stateRelations.ContainsKey(variable) && stateRelations[variable].ContainsKey(elementOne) 
                && stateRelations[variable][elementOne].Contains(elementTwo))
                return true;
            return false;
        }

        /// <summary>
        /// Checks if a relation is true between two given elements.
        /// </summary>
        /// <param name="variable">The relation name</param>
        /// <param name="elementOne">The first element of the relation</param>
        /// <param name="elementTwo">The second element of the relation</param>
        /// <returns>True if a match is found, else false</returns>
        public bool Holds(string variable, string elementOne, string elementTwo)
        {
            return CheckRelation(variable, elementOne, elementTwo);
        }

        /// <summary>
        /// Gets a copy of the relations.
        /// </summary>
        /// <returns>Dictionary containing the relations</returns>
        public Dictionary<string, Dictionary<string, List<string>>> GetRelationsCopy()
        {
            Dictionary<string, Dictionary<string, List<string>>> copy = new Dictionary<string, Dictionary<string, List<string>>>();
            foreach (KeyValuePair<string, Dictionary<string, List<string>>> pair in stateRelations)
            {
                copy.Add(pair.Key, new Dictionary<string, List<string>>());
                foreach (KeyValuePair<string, List<string>> otherPair in pair.Value)
                {
                    copy[pair.Key].Add(otherPair.Key, new List<string>(otherPair.Value));
                }
            }
            return copy;
        }
    }
}
