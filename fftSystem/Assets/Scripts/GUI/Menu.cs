using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour {

    public GameObject debugModeObj;
    public GameObject returnModeObj;

    public GameObject gameSpeedObj;
    private Slider gameSpeedSlider;
    private Text gameSpeedValue;

    public GameObject firefighterSetUp;
    public GameObject firefighterMember;


    private AsyncOperation async = null;

	// Use this for initialization
	void Start () {
        gameSpeedSlider = gameSpeedObj.transform.FindChild("Slider").GetComponent<Slider>();
        gameSpeedValue = gameSpeedObj.transform.FindChild("GameSpeedValue").GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        // used for stop time

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
            if(returnModeObj.activeSelf){
                SetDeactive(returnModeObj);
            }
            else{
                SetActive(returnModeObj);
            }
        }
        Time.timeScale = gameSpeedSlider.normalizedValue*10;
        gameSpeedValue.text = ((int)(gameSpeedSlider.normalizedValue*10)).ToString();
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
