﻿using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class BaseCrashedCarScript : MonoBehaviour {

    public List <GameObject> StablePointSills;
    
    public List <GameObject> StablePointWheels;
    
    public List <GameObject> ToughGlass;
    
    
    // This name is used for organize object using tags. 
    // Naming in tags are <object name> + <variable name>
    // e.g. Car1StablePointSills.
    public string objName;
    
    // Use this for initialization
    void Start () {
        objName = gameObject.name;
        
        // Find all the object that are with this tag.
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag(name + "StablePointSills")){
            StablePointSills.Add(obj);
        }
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag(name + "StablePointWheels")){
            StablePointWheels.Add(obj);
        }
        
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag(name + "ToughGlass")){
            ToughGlass.Add(obj);
        }
        
        // Init Check
        string str = "";
        str += (StablePointSills.Count==0?" 'StablePointSills' ":"");
        str += StablePointWheels.Count==0?" 'StablePointWheels' ":"";
        str += ToughGlass.Count==0?" 'ToughGlass' ":"";
        if(str != "")
            Debug.LogError("Car Init Error!" + str + "should be initialized");
    }
    
    // Update is called once per frame
    void Update () {

    }

    /// <summary>
    /// Thif function calculate the tough glass that needs to be taped.
    /// TODO: atm the taped glass is predefined.
    /// </summary>
    public void needTapedToughtGlass(){
    }
}
