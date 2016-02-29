

using UnityEngine;
using System.Collections;

public class DebugHelper : MonoBehaviour
{

    public float gamespeed = 1f;

    public bool masterButton = true;

    public GameObject firefighterLeader;

    // Use this for initialization
    void Start()
    {
       // Time.timeScale = gamespeed;
    }
	
    // Update is called once per frame
    void Update()
    {
	
    }

    void OnGUI()
    {
        if (masterButton)
        {
            if (GUI.Button(new Rect(20, 20, 100, 30), "Master Button"))
            {
                if(firefighterLeader != null){
                    FirefighterLeader ffl = firefighterLeader.GetComponent<FirefighterLeader>();

                    ffl.Rescue = true;
                }
            }
        
        }
    }


}
