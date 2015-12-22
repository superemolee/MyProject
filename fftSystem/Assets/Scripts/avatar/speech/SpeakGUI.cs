using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Speak))]
[AddComponentMenu("Avatar/Speak/SpeakGUI")]
/// <summary>
/// This class can used to show a speak gui field. It will use the Speak class <see cref="Speak"/> and send the user input to the speak class.
/// </summary>
///
public class SpeakGUI : MonoBehaviour
{
	/// <summary>
	/// Show the GUI or not.
	/// </summary>
	public bool m_showGUI;
	/// <summary>
	/// The key to enable GUI.
	/// </summary>
	public string m_keyToEnableGUI = "t";
	/// <summary>
	/// Should the GUI always be activated.
	/// </summary>
	public bool m_alwaysActive = false;
	
	/// <summary>
	/// The guistyle for different styles.
	/// </summary>
	private GUIStyle m_guistyle;
	
	/// <summary>
	/// The speak component will be searched during start.
	/// </summary>
	private Speak m_speak;
	
	/// <summary>
	/// The speaktext what the user typed in.
	/// </summary>
	private string m_speaktext="";
	
	// Use this for initialization
	void Start ()
	{
		//showGUI = false;
		m_speak = GetComponent<Speak>();
		m_guistyle = new GUIStyle ();
		m_guistyle.alignment = TextAnchor.MiddleLeft;	
		m_guistyle.normal.textColor = Color.white;
			
		
	}
	
	
	
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="SpeakGUI"/> show GUI.
	/// </summary>
	/// <value>
	/// <c>true</c> if show GU; otherwise, <c>false</c>.
	/// </value>
	public bool ShowGUI {
		get {
			return this.m_showGUI;
		}
		set {
			if(m_alwaysActive)
			{
				m_showGUI = true;
			}
			else{
				m_showGUI = value;
			}
		}
	}

	// Update is called once per frame
	void Update ()
	{

		if (m_showGUI && Input.GetKey (KeyCode.Escape)) 
		{
			//InputSwitcher.ReleaseInput(gameObject);
			ShowGUI = false;
			m_speaktext = "";
			
		}
		if(Input.GetKeyUp(""+m_keyToEnableGUI.Substring(0,1))&& !m_showGUI)
		{
			ShowGUI = true;
			m_speaktext = "";
		}
		
	
	
		
	}
	
	/// <summary>
	/// Override the GUI event.
	/// </summary>
	void OnGUI ()
	{
		if(m_showGUI)
		{
			GUI.Box (new Rect (0, Screen.height - 30, Screen.width, 30), "");
			//GUILayout.FlexibleSpace();
			
			GUI.Label (new Rect (5, Screen.height - 25, 75, 20), "Speaktext: ", m_guistyle);
			GUI.SetNextControlName ("SpeakTextField");
			
			m_speaktext = GUI.TextField (new Rect (80, Screen.height - 25, Screen.width - 160, 20), m_speaktext);
			GUI.FocusControl ("SpeakTextField");	
			if (GUI.Button (new Rect (Screen.width - 75, Screen.height - 25, 70, 20), "speak")) {
				m_speak.sayText(m_speaktext);
				
				m_speaktext = "";
				ShowGUI = false;// this.enabled = false;
				
				
			}
			else if(m_speaktext !="" &&Event.current.keyCode==KeyCode.Return)
			{
				m_speak.sayText (m_speaktext);
				
				m_speaktext = "";
				ShowGUI= false;
			}
		}
		
	}
	
}