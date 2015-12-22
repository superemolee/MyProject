using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class InteractableTool : MonoBehaviour {

	[NonSerialized]
	public GameObject toolUser;
	[NonSerialized]
	public Animator anim;
	[NonSerialized]
	public Vector3 usedPos;

	public GameObject replacedObj;

	[NonSerialized]
	public bool is_init;

	// Use this for initialization
	void Start () {
		is_init = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void Initialize(GameObject tUser, Animator a, Vector3 uPos){
		toolUser = tUser;
		anim = a;
		usedPos = uPos;
		is_init = true;
	}

	/// <summary>
	/// Define how to pick up this tool, and this will be defined together with animations.
	/// </summary>
	void ToolPickUp(){

		if(is_init){

		}
	}

	/// <summary>
	/// Define how to use the tool, and this will be defined together with animations. 
	/// </summary>
	void ToolUse(){

		if(is_init){
			
		}
	}


}
