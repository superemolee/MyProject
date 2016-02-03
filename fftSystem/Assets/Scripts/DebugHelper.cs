using UnityEngine;
using System.Collections;

public class DebugHelper : MonoBehaviour {

    public float gamespeed = 2.0f;
	// Use this for initialization
	void Start () {
        Time.timeScale = gamespeed;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
