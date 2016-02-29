using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

    public GameObject debugModeObj;
    public GameObject returnModeObj;

    private AsyncOperation async = null;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        // call menu
        if(Input.GetKeyDown(KeyCode.Tab)&&debugModeObj != null)
        {
            if(debugModeObj.activeSelf)
                SetDeactive(debugModeObj);
            else
                SetActive(debugModeObj);
        }

        // exit
        if(Input.GetKeyDown(KeyCode.Escape)&&returnModeObj != null)
        {
            if(returnModeObj.activeSelf)
                SetDeactive(returnModeObj);
            else
                SetActive(returnModeObj);
        }

	}

    public void SetActive(GameObject obj){
        obj.SetActive(true);
    }

    public void SetDeactive(GameObject obj){
        obj.SetActive(false);
    }

    public void GoToMainMenu(){
        StartCoroutine(LoadALevel("Menu 3D"));
    }

    public void Stay(){
        SetDeactive(returnModeObj);
    }


    private IEnumerator LoadALevel(string levelName)
    {
        async = Application.LoadLevelAsync(levelName);
        yield return async;
    }
}
