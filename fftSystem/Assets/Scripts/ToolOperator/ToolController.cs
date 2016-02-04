using UnityEngine;
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


    public RescuerGeneral toolUser;


    // Use this for initialization
    void Start()
    {
        // there is no tool in firefighters hand when game starts
        toolsInHand = new List<GameObject>();
        selectedTool = null;

        selectedToolScript = null;

        toolUserAnim = GetComponent<Animator>();

        // init with the specific firefighter role
        // TODO to create the others
        // TODO WARNING this might not necessary
        // Use the tool name in HTN parameters
        if (toolUser == null)
            toolUser = GetComponent<FirefighterToolOperator>();
        if (toolUser == null)
            toolUser = GetComponent<FirefighterBackUp>();

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
    public GameObject FindToolFromName(string toolName, string tag)
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
                    return tool;
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
    public bool PickUp(GameObject tool)
    {
        if (tool != null)
        {
            // init
            toolsInHand.Add(tool);
            selectedTool = tool;
            selectedToolScript = tool.GetComponent<BaseToolScript>();

            if (selectedToolScript.pickUpState == ToolStates.ToolPickUpStates.rightHand)
            {

                // control the animator controller
                int IsPickUp_id = Animator.StringToHash("IsPickUp");
                toolUserAnim.SetBool(IsPickUp_id, true);

                // while animation is playing
                tool.transform.parent = toolUserAnim.transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand");
                tool.transform.localRotation = Quaternion.identity;
                tool.transform.localPosition = Vector3.zero;
            }else if(selectedToolScript.pickUpState == ToolStates.ToolPickUpStates.doubleHands){
                // TODO: deal with this later
            }else if(selectedToolScript.pickUpState == ToolStates.ToolPickUpStates.doublePerson){
                // TODO: deal with this later
            }
            return true;
        } else
            return false;

    }
//    
    public virtual bool Use()
    {
        int IsStableSills_id = Animator.StringToHash("IsStableSills");
        toolUserAnim.SetBool(IsStableSills_id, true);
        return true;
    }

}
