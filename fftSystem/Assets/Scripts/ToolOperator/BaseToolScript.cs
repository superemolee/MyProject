using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Needed to use Generic lists

using ToolStates; // include the tool states

public class BaseToolScript : MonoBehaviour
{

    private bool isActive;

    public bool isPickUp;

    // check wheather the power/water... supply is ready
    public bool isReadyToUse;
	
    // the user's animator to use the tool
    public Animator[] toolUserAnimator;

    // the user or the use's hand
    // this can be one, can be multiple. e.g. stretcher needs at least two person
    public GameObject[] toolUsers;

    public int userIndex;

    // tool state
    public ToolUseStates useState;
    public ToolPickUpStates pickUpState;
    public ToolType type;



    // Use this for initialization
    void Start()
    {
        isActive = true;
        // the tool is not picked up when the game starts, and it is not ready to use and not in use
        isPickUp = false;
        isReadyToUse = false;

        // tool is not in use when the game starts and the default state for pick up is using one hand
        useState = ToolUseStates.beforeUse;
        pickUpState = ToolPickUpStates.rightHand;

        // defaul type is general... this value should be set if we need the specific function.
        type = ToolType.General;
				
    }
	
    // Update is called once per frame
    void Update()
    {

    }
	
}
