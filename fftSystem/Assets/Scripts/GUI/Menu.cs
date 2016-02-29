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

        // game speed
        Time.timeScale = gameSpeedSlider.normalizedValue*10;
        gameSpeedValue.text = ((int)(gameSpeedSlider.normalizedValue*10)).ToString();
	}

    // group matching   
    public void ToggleGroup(){
        if (firefighterSetUp.transform.FindChild("Gruppe").GetComponent<Toggle>().isOn)
        {
            Toggle[] toggles = firefighterMember.GetComponentsInChildren<Toggle>();
            foreach (Toggle toggle in toggles)
            {
                toggle.isOn = true;
                toggle.interactable = false;
            }
        }
    }

    public void ToggleStaffel(){
        if (firefighterSetUp.transform.FindChild("Staffel").GetComponent<Toggle>().isOn)
        {
            firefighterMember.transform.GetChild(0).GetComponent<Toggle>().isOn = true;
            firefighterMember.transform.GetChild(1).GetComponent<Toggle>().isOn = false;
            firefighterMember.transform.GetChild(2).GetComponent<Toggle>().isOn = true;
            firefighterMember.transform.GetChild(3).GetComponent<Toggle>().isOn = true;
            firefighterMember.transform.GetChild(4).GetComponent<Toggle>().isOn = true;
            firefighterMember.transform.GetChild(5).GetComponent<Toggle>().isOn = true;
            firefighterMember.transform.GetChild(6).GetComponent<Toggle>().isOn = true;
            firefighterMember.transform.GetChild(7).GetComponent<Toggle>().isOn = false;
            firefighterMember.transform.GetChild(8).GetComponent<Toggle>().isOn = false;
            Toggle[] toggles = firefighterMember.GetComponentsInChildren<Toggle>();
            foreach (Toggle toggle in toggles)
            {
                toggle.interactable = false;
            }
        }
    }

    public void ToggleCustomize(){
        if (firefighterSetUp.transform.FindChild("Customize").GetComponent<Toggle>().isOn)
        {
            Toggle[] toggles = firefighterMember.GetComponentsInChildren<Toggle>();
            foreach(Toggle toggle in toggles){
                toggle.isOn = false;
                toggle.interactable = true;
            }
        }
    }

    public void SetActive(GameObject obj){
        obj.SetActive(true);
    }

    public void SetDeactive(GameObject obj){
        obj.SetActive(false);
    }

    public void SetToggleActive(GameObject obj){
        if (obj.activeSelf)
            obj.SetActive(false);
        else
            obj.SetActive(true);
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
