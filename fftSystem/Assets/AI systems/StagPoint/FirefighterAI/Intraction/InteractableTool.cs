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

	public void Initialize(GameObject tUser, Animator a, Vector3 uPos){
		toolUser = tUser;
		anim = a;
		usedPos = uPos;
		is_init = true;
	}

	/// <summary>
	/// Define how to pick up this tool, and this will be defined together with animations.
	/// </summary>
	public void ToolPickUp(){
		// TODO: to solve the one hand picking/two hands picking or multiperson picking here.
		// default pick up is using one hand(right hand) pick up
		if(is_init){
			// Pick up animation.
			int IsPickUp_id = Animator.StringToHash ("IsPickUp");
			anim.SetBool (IsPickUp_id, true);
			// while animation is playing
			gameObject.transform.parent = toolUser.transform.Find ("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand");
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localPosition = Vector3.zero;
		}
	}

	/// <summary>
	/// Define how to use the tool, and this will be defined together with animations. 
	/// </summary>
	public void ToolUse(){

		if(is_init){
            //GameObject obj=CurrentStablePointSill;
            //Car1.GetComponent<CrashedCar> ().StablePointSills.Remove (CurrentStablePointSill);
            // Using stable sills tool animation.
            int IsStableSills_id = Animator.StringToHash ("IsStableSills");
            anim.SetBool (IsStableSills_id, true);
            // After tool used... 
            //tool.renderer.enabled = false;
        }
    }


}
