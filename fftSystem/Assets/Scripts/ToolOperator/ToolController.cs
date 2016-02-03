using UnityEngine;
using System.Collections;

public class ToolController : MonoBehaviour
{
    // all tools that the firefighter have
    public GameObject[] tools;

    // the selected tool to use
    public GameObject selectedTool;

    // tool script can get the type, states etc. infor for the tools
    public BaseToolScript toolScript;


    // Use this for initialization
    void Start()
    {
	
    }
	
    // Update is called once per frame
    void Update()
    {
	
    }

    //find the tool for the current task
    //
    public virtual GameObject[] FindTool(GameObject tool)
    {
        return null;

    }


//    public virtual bool PickUp()
//    {
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
//    }
//    
//    public virtual bool Use()
//    {
//        int IsStableSills_id = Animator.StringToHash("IsStableSills");
//        toolUserAnimator.SetBool(IsStableSills_id, true);
//        return true;
//    }

}
