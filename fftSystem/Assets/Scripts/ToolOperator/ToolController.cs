using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ToolController : MonoBehaviour
{
    // all tools that the firefighter have
    public GameObject[] tools;

    // the selected tool to use
    public GameObject selectedTool;

    // tool script can get the type, states etc. infor for the tools
    public BaseToolScript toolScript;

    // animator controller deals with animations etc.
    public Animator toolUserAnim;


    public RescuerGeneral toolUser;


    // Use this for initialization
    void Start()
    {
        // there is no tool in firefighters hand when game starts
        tools = null;
        selectedTool = null;

        toolScript = null;

        toolUserAnim = GetComponent<Animator>();

        // init with the specific firefighter role 
        // TODO to create the others
        // TODO WARNING this might not necessary
        // Use the tool name in HTN parameters
        if (toolUser = null)
            toolUser = GetComponent<FirefighterToolOperator>();
        if (toolUser = null)
            toolUser = GetComponent<FirefighterBackUp>();
    }
	
    // Update is called once per frame
    void Update()
    {
        // connect the tasks with tools
    }

    public GameObject GetToolFromName(string toolName){
        // TODO: this function is similiar as below
        // but need to find the exact object
    }

    /// <summary>
    /// According to the type of the tool, and the tag information, this function can return an array of an object list that the firefighter will use.
    /// </summary>
    /// <returns>The exact tool from type.</returns>
    /// <param name="toolType">Tool type.</param>
    /// <param name="tag">Tag.</param>
    public List<GameObject> findExactToolFromType (string toolType, string tag)
    {
        // Init.
        string toolItem = "";
        List<GameObject> toolItemObjects = new List<GameObject> ();
        GameObject[] tools = GameObject.FindGameObjectsWithTag (tag);
        
        // Find the tool object
        GameObject toolobj = null;
        foreach (GameObject tool in tools) {
            if (tool.name == toolType)
                toolobj = tool;
        }
        
        // test if there is a child
        Transform child =null;
        for (int i = 0; i<6; i++) {
            toolItem = toolType + "_" + (i+1);
            child = toolobj.transform.FindChild (toolItem);
            if(child==null)
                continue;
            else
                break;
        }
        
        // Calculate the name of the tool
        for (int i = 0; i < 6; i++) {
            toolItem = toolType + "_" + (i + 1);
            Transform children = toolobj.transform.FindChild (toolItem);
            if (children != null && (children.gameObject.tag == tag))
                toolItemObjects.Add (children.gameObject);
        }
        
        if (child == null) {
            toolItemObjects.Add (toolobj);
            return toolItemObjects;
        }
        return toolItemObjects;
    }

    //find the tool for the current task
    public virtual GameObject[] FindTool(GameObject tool)
    {
        return null;
    }

    public virtual bool PickUp()
    {
//        // TODO: to solve the one hand picking/two hands picking or multiperson picking here.
//        // default pick up is using one hand(right hand) pick up
//        // Pick up animation.
//        int IsPickUp_id = Animator.StringToHash("IsPickUp");
//        toolUserAnimator.SetBool(IsPickUp_id, true);
//        // while animation is playing
//        gameObject.transform.parent = toolUserAnimator.transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand");
//        gameObject.transform.localRotation = Quaternion.identity;
//        gameObject.transform.localPosition = Vector3.zero;
//        return true;
//    
    }
//    
    public virtual bool Use()
    {
//        int IsStableSills_id = Animator.StringToHash("IsStableSills");
//        toolUserAnimator.SetBool(IsStableSills_id, true);
//        return true;
    }

}
