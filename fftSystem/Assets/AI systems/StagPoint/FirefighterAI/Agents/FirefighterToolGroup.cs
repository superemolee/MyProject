using UnityEngine;
using System.Collections;


using StagPoint.Core;
using StagPoint.Planning;
using StagPoint.Planning.Components;

using Firefighter.Utilities;

[RequireComponent( typeof(TaskNetworkPlanner) )]
public class FirefighterToolGroup : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    
    public TaskStatus StableSills()
    {
        
        return TaskStatus.Succeeded;
        
    }

    
    public TaskStatus StableWheels()
    {
        
        return TaskStatus.Succeeded;
        
    }
}
