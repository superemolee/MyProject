using UnityEngine;
using System.Collections;

using StagPoint.Core;
using StagPoint.Planning;
using StagPoint.Planning.Components;

public class Sphereagent : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public TaskStatus dosomething()
	{
		Debug.Log ("dosomething");
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus dosomething2()
	{
		Debug.LogWarning ("dosomething2");
		return TaskStatus.Succeeded;
		
	}

    public TaskStatus dosomething3()
    {
        Debug.LogWarning ("dosomething3");
        return TaskStatus.Succeeded;
        
    }

    public TaskStatus dosomething4()
    {
        Debug.LogWarning ("dosomething3");
        return TaskStatus.Succeeded;
        
    }

    public TaskStatus dosomething5()
    {
        Debug.LogWarning ("dosomething3");
        return TaskStatus.Succeeded;
        
    }
}
