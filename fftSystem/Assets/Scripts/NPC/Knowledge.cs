using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*****************************************************/
/*
 * Struct of the physical objects in the scene. 
 */
/*****************************************************/
public struct physicalObjects
{	
	//public int id;
	public string name;
	public Transform transf;
}
/*****************************************************/
/*
 * Struct for the tasks. 
 */
/*****************************************************/
public struct tasks
{
	public int id;
	public string name;
	public int priority;
	public int numberOfNPCs;
	public List<physicalObjects> objectsList;
	public bool completed;
}
/*****************************************************/
/*
 * Class Knowledge.
 */
/*****************************************************/
public class Knowledge
{
	public bool isEmpty = true;
	public List<physicalObjects> physicalObjectsList = new List<physicalObjects>();
	public List<tasks> tasksList = new List<tasks>();
	public tasks emptyTask = new tasks();
	public physicalObjects emptyObject = new physicalObjects();
	
	
	
	/*****************************************************/
	// Constructor for the helper class.
	public Knowledge()
	{
		isEmpty = true;
		LoadTasks();
		emptyTask.id = 0;
		emptyObject.name = "empty";
	}
	
	public void setCompleted(bool isCompleted){
		emptyTask.completed = isCompleted;
	}
	
	public void setTaskCompleted(int id, bool isCompleted)
	{		
		for(int i = 0; i < tasksList.Count; i++)
		{
			if(tasksList[i].id == id)
			{
				tasks tempTask = new tasks();
				tempTask.id = id;
				tempTask.completed = isCompleted;
				tempTask.name = tasksList[i].name;
				tempTask.numberOfNPCs = tasksList[i].numberOfNPCs;
				tempTask.priority = tasksList[i].priority;
				tempTask.objectsList = tasksList[i].objectsList;
				tasksList.RemoveAt(i);
				tasksList.Add(tempTask);
			}
			
		}
		
	}
	
	/*****************************************************/
	/*
	 * Adds a new physical object to the list.
	 */
	public void AddToPhysicalObjectsList(GameObject newObj)
	{
		if(!Exists(newObj))
		{
			physicalObjects newPhysicalObject;
			newPhysicalObject.name = newObj.name;
			newPhysicalObject.transf = newObj.transform;
			physicalObjectsList.Add(newPhysicalObject);
			isEmpty = false;
		}
	}
	/*****************************************************/
	/*
	 * Returns true if the GameObject has been already seen.
	 */
	public bool Exists(GameObject testObj)
	{
		foreach(physicalObjects obj in physicalObjectsList)
		{
			if(obj.name == testObj.name)
				return true;
		}
		return false;
	}
	/*****************************************************/
	/*
	 * Updates the location of the object in case it changed.
	 */
	public void UpdateLocation(GameObject testObj)
	{
		for(int i = 0; i < physicalObjectsList.Count; i++)
		{
			if((physicalObjectsList[i].name == testObj.name) && (physicalObjectsList[i].transf != testObj.transform))
			{
				physicalObjects temp;
				temp.name = testObj.name;
				temp.transf = testObj.transform;
				physicalObjectsList.RemoveAt(i);
				physicalObjectsList.Add(temp);
			}
		}
	}
	/*****************************************************/
	/*
	 * Load list of tasks for the NPC.
	 */
	public void LoadTasks()
	{
		tasks task1 = new tasks();
		task1.id = 1;
		task1.name = "RescueBurningPerson";
		task1.priority = 1;
		task1.numberOfNPCs = 2;
		task1.completed = false;
		physicalObjects burningPerson = new physicalObjects();
		burningPerson.name = "burning";
		task1.objectsList = new List<physicalObjects>();
		task1.objectsList.Add(burningPerson);
		tasksList.Add(task1);
		
		tasks task2 = new tasks();
		task2.id = 2;
		task2.name = "RescueBleedingPerson";
		task2.priority = 2;
		task2.completed = false;
		task2.numberOfNPCs = 2;
		physicalObjects bleedingPerson = new physicalObjects();
		bleedingPerson.name = "bleeding";
		task2.objectsList = new List<physicalObjects>();
		task2.objectsList.Add(bleedingPerson);
		tasksList.Add(task2);
		
		tasks task3 = new tasks();
		task3.id = 3;
		task3.name = "RescueBonebreakPerson";
		task3.priority = 3;
		task3.completed = false;
		task3.numberOfNPCs = 2;
		physicalObjects injuredPerson = new physicalObjects();
		injuredPerson.name = "bonebreak";
		task3.objectsList = new List<physicalObjects>();
		task3.objectsList.Add(injuredPerson);
		tasksList.Add(task3);
		
		tasks task4 = new tasks();
		task4.id = 4;
		task4.name = "fightfire";
		task4.priority = 4;
		task4.completed = false;
		task4.numberOfNPCs = 2;
		physicalObjects trushcanFire = new physicalObjects();
		trushcanFire.name = "Muelltonne";
		task4.objectsList = new List<physicalObjects>();
		task4.objectsList.Add(trushcanFire);
		tasksList.Add(task4);
		
	}
	/*****************************************************/
	/*
	 * Search for a given physical object by name, returns true if the object has been already seen, false otherwise.
	 */
	public bool ExistsByName(string name)
	{
		foreach(physicalObjects obj in physicalObjectsList)
		{
			if(obj.name == name)
				return true;
		}
		return false;
	}	
	/*****************************************************/
	/*
	 * Returns the highest priority task from the NPC's environment.
	 */
	public tasks SearchForHighestPriorityTask()
	{
		bool found = false;
		bool exitWithBreak = false;
		bool exitWithContinue = false;
		for(int i = 0; i < tasksList.Count; i++)
		{
			if(!tasksList[i].completed)
			{	
				for(int j = 0; j < tasksList[i].objectsList.Count; j++)
				{
					if(ExistsByName(tasksList[i].objectsList[j].name))
					{
						found = true;					
					}
					else
					{
						found = false;					
					}
				
					if(!found)
					{
						break;
					}
				}
				if(found)
				{
					return tasksList[i];
				}
			}
		}
		return emptyTask;
	}
	/*****************************************************/
	public physicalObjects FindObjectWithName(string name)
	{
		foreach(physicalObjects obj in physicalObjectsList)
		{
			if(obj.name == name)
				return obj;
		}
		return emptyObject;
	}
}