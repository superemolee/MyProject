﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ToolController : MonoBehaviour
{
    // all tools that the firefighter have
    public List<GameObject> toolsInHand;

    // the selected tool to use
    public GameObject selectedTool;

    // tool script can get the type, states etc. infor for the tools
    public BaseToolScript selectedToolScript;

    // animator controller deals with animations etc.
    public Animator toolUserAnim;


    public FirefighterToolOperator toolUser;


    // Use this for initialization
    void Start()
    {
        // there is no tool in firefighters hand when game starts
        toolsInHand = new List<GameObject>();
        selectedTool = null;

        selectedToolScript = null;

        toolUserAnim = GetComponent<Animator>();

        // init with the current firefighter role
        toolUser = GetComponent<FirefighterToolOperator>();

    }
	
    // Update is called once per frame
    void Update()
    {
        // connect the tasks with tools
    }

    /// <summary>
    /// Find the tool game object within the tag. This function is only used to get to know
    /// where is the tool and where to get the tool object.
    /// </summary>
    /// <returns>The tool with the correct name.</returns>
    /// <param name="toolName">Tool name: this is the name of the tool with out postfix, 
    /// and this method can find the coorect tool with the correct name. 
    /// </param>
    /// <param name="tag">Tag: is telling where to find the tool. There are two tags: 
    /// InToolStageAreaTools and NotInToolStageAreaTools. This tag can also be a 
    /// parameter from primitive task.
    /// </param>
    public GameObject FindToolByName(string toolName, string tag)
    {

        // Init.
        string toolFullName = "";
        List<GameObject> toolItemObjects = new List<GameObject>();

        // Get all the tools from the defined tag
        GameObject[] tools = GameObject.FindGameObjectsWithTag(tag);

        // Find the tool object in the tag
        foreach (GameObject tool in tools)
        {
            for (int i = 0; i<GlobalValues.MAXTOOLMUNBERINONETYPE; i++)
            {
                toolFullName = toolName + "_" + (i + 1);
                if (tool.name == toolFullName)
                {
                    if (tool.GetComponent<BaseToolScript>().ownerState == Firefighter.ToolStates.ToolOwnerStates.noOwner)
                    {
                        // mark the tool as "it is waiting for me, other people please don't touch it"
                        tool.GetComponent<BaseToolScript>().ownerState = Firefighter.ToolStates.ToolOwnerStates.waitingForOwner;
                        return tool;
                    } else if (tool.GetComponent<BaseToolScript>().ownerState == Firefighter.ToolStates.ToolOwnerStates.waitingForOwner)
                    {
                        Debug.LogError("Someone is going to use the tool, you have to wait!");
                        return null;
                    } else
                    {
                        Debug.LogError("Someone already has the tool, you cannot use it!");
                        return null;
                    }
                }
            }
        }
        return null;
    }


    /// <summary>
    /// This function will play the pick up animation, attach the tool to the body/hand
    /// And then the tool will have a owner.
    /// </summary>
    /// <returns><c>true</c>, if up was picked, <c>false</c> otherwise.</returns>
    public virtual bool PickUp(GameObject tool)
    {
        bool isToolPick = false;
        if (tool != null)
        {
            // init
            toolsInHand.Add(tool);
            selectedTool = tool;
            selectedToolScript = tool.GetComponent<BaseToolScript>();

            if (selectedToolScript.pickUpState == Firefighter.ToolStates.ToolPickUpStates.rightHand)
            {

                // control the animator controller
                int IsPickUp_id = Animator.StringToHash("IsPickUp");
                toolUserAnim.SetBool(IsPickUp_id, true);

//                // while animation is playing
//                tool.transform.parent = toolUserAnim.transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand");
//                tool.transform.localRotation = Quaternion.identity;
//                tool.transform.localPosition = Vector3.zero;

//                StartCoroutine(PlayOneShot("IsPickUp"));
//                return true;
//                //
                if (toolUserAnim.GetCurrentAnimatorStateInfo(0).IsName("PickUp"))
                {
                    if (toolUserAnim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f)
                    {
                        // while animation is playing
                        tool.transform.parent = toolUserAnim.transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand");
                        tool.transform.localRotation = Quaternion.identity;
                        tool.transform.localPosition = Vector3.zero;
                        isToolPick = true;
                    }
//                    // if the animation is played once continue
//                    if (toolUserAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.7f)
//                    {
//                        // running
//                        return false;
//                    } else
//                    {
//                        toolUserAnim.SetBool(IsPickUp_id, false);
//                        selectedToolScript.ownerState = ToolStates.ToolOwnerStates.hasOwner;
//                        return true;
//                    }
                    if (isToolPick)
                    {
                        toolUserAnim.SetBool(IsPickUp_id, false);
                        selectedToolScript.ownerState = Firefighter.ToolStates.ToolOwnerStates.hasOwner;
                        return true;
                    } else 
                        return false;
                } else
                {
                    return false;
                }
            } else if (selectedToolScript.pickUpState == Firefighter.ToolStates.ToolPickUpStates.doubleHands)
            {
                // TODO: deal with this later
            } else if (selectedToolScript.pickUpState == Firefighter.ToolStates.ToolPickUpStates.doublePerson)
            {
                // TODO: deal with this later
            }
            return true;
        } else
        {
            Debug.LogError("Tool is empty, cannot pick up!");
            return false;
        }
    }

    /// <summary>
    /// Use this instance.
    /// </summary>
    public virtual bool Use(Firefighter.ToolStates.ToolType toolType)
    {
        ChooseCorrectToolToUse(toolType);
       
        // if use tool to stable sill
        if (toolType == Firefighter.ToolStates.ToolType.StableSill)
        {
            // animate
            int IsStableSills_id = Animator.StringToHash("IsStableSills");
            toolUserAnim.SetBool(IsStableSills_id, true);

            // after using the tool
            selectedTool.renderer.enabled = false;

            if (toolUserAnim.GetCurrentAnimatorStateInfo(0).IsName("Operate"))
            {
                if (toolUserAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.7f)
                {
                    // still running
                    return false;
                } else
                {
                    toolUserAnim.SetBool(IsStableSills_id, false);
                    ToolActivate(toolUser.CurrentStablePointSill);
//                    toolUser.CurrentStablePointSill.renderer.enabled = true;
                    toolUser.crashedCarScript.StablePointSills.Remove(toolUser.CurrentStablePointSill);
                    return true;
                }
            }

        }
        // if use tool to stalble wheel
        else if (toolType == Firefighter.ToolStates.ToolType.StableWheel)
        {
            // animate
            int IsStableWheels_id = Animator.StringToHash("IsStableWheels");
            toolUserAnim.SetBool(IsStableWheels_id, true);

            // after using the tool
            selectedTool.renderer.enabled = false;

            if (toolUserAnim.GetCurrentAnimatorStateInfo(0).IsName("Operate"))
            {
                if (toolUserAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.7f)
                {
                    // still running
                    return false;
                } else
                {
                    toolUserAnim.SetBool(IsStableWheels_id, false);
                    ToolActivate(toolUser.CurrentStablePointWheel);
                    //toolUser.CurrentStablePointWheel.renderer.enabled = true;
                    toolUser.crashedCarScript.StablePointWheels.Remove(toolUser.CurrentStablePointWheel);
                    return true;
                }
            }
        } else if (toolType == Firefighter.ToolStates.ToolType.TapeDispenser)
        {

            // animate
            int IsCenterPunch_id = Animator.StringToHash("IsCenterPunch");
            toolUserAnim.SetBool(IsCenterPunch_id, true);

            if (toolUserAnim.GetCurrentAnimatorStateInfo(0).IsName("CenterPunch"))
            {
                if (toolUserAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.7f)
                {
                    // still running
                    return false;
                } else
                {
                    toolUserAnim.SetBool(IsCenterPunch_id, false);

                    // TODO .manage glass
                    
                   
                    return true;
                }
            }

        } else if (toolType == Firefighter.ToolStates.ToolType.SpringCenterPunch)
        {
            // animate
            int IsTape_id = Animator.StringToHash("IsTape");
            toolUserAnim.SetBool(IsTape_id, true);
            
            if (toolUserAnim.GetCurrentAnimatorStateInfo(0).IsName("Tape"))
            {
                if (toolUserAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.7f)
                {
                    // still running
                    return false;
                } else
                {
                    toolUserAnim.SetBool(IsTape_id, false);
                    
                    // TODO .manage glass
                    // currently delete
                    //ToolDeactivate(toolUser.CurrentToughGlass);
                    toolUser.CurrentToughGlass.GetComponent<CrashedCarGlassController>().assignTexture();
                    toolUser.crashedCarScript.ToughGlass.Remove(toolUser.CurrentToughGlass);
                    return true;
                }
            }

        }
        //Debug.LogError("The tool is set as general, please set the tool as a specific purpose tool, e.g. stableSill.");
        return false;
    }

    
    public void ChooseCorrectToolToUse(Firefighter.ToolStates.ToolType toolT)
    {
        foreach (GameObject tool in toolsInHand)
        {
            if (tool.GetComponent<BaseToolScript>().type == toolT)
            {
                selectedTool = tool;
                selectedToolScript = tool.GetComponent<BaseToolScript>();
            }
        }
    }

    /// <summary>
    /// To deactivate the given tool.
    /// </summary>
    /// <param name="dTool">D tool.</param>
    public void ToolDeactivate(GameObject dTool)
    {
        Renderer rend = dTool.GetComponent<Renderer>();
        if (rend != null)
            rend.enabled = false;
        else
        {
            Renderer [] renderers = toolUser.CurrentStablePointWheel.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.enabled = false;
            }
        }
    }


    /// <summary>
    /// To activate the given tool.
    /// </summary>
    /// <param name="aTool">A tool.</param>
    public void ToolActivate(GameObject aTool)
    {
        Renderer rend = aTool.GetComponent<Renderer>();
        if (rend != null)
            rend.enabled = true;
        else
        {
            Renderer [] renderers = toolUser.CurrentStablePointWheel.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.enabled = true;
            }
        }
    }

    /// <summary>
    /// used for play an animation once.
    /// </summary>
    /// <returns>The one shot.</returns>
    /// <param name="paramName">Parameter name.</param>
    public IEnumerator PlayOneShot(string paramName)
    {
        toolUserAnim.SetBool(paramName, true);
        yield return null;
        toolUserAnim.SetBool(paramName, false);
    }


}
