using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Zeigt die Verletztenanhängekarte an und gibt die Möglichkeit Einträge vorzunehmen
/// </summary>
public class InjuredDocumentation : MonoBehaviour {
	
	/// <summary>
	/// Die Vorderseite der VAK
	/// </summary>
	public GameObject frontside;
	/// <summary>
	/// Die Rückseite der VAK
	/// </summary>
	public GameObject backside;
	/// <summary>
	/// Die Kamera die angeschaltet werden kann um direkt auf die VAK schauen zu können
	/// </summary>
	public Camera cam;
	/// <summary>
	/// Die Textur der VAK-Vorderseite
	/// </summary>
	public Texture vakFrontPrefab;
	/// <summary>
	/// Die Textur der VAK-Rückseite
	/// </summary>
	public Texture vakBackPrefab;
	/// <summary>
	/// Die Breite der VAK in Pixeln
	/// </summary>
	private int minSize = 1024;
	/// <summary>
	/// Welche Seite soll gerade dargestellt werden
	/// </summary>
	private int page = 1;
	/// <summary>
	/// Befinden wir uns im Editierungsmodus für die VAK
	/// </summary>
	public bool editMode = false;
	/// <summary>
	/// Die Position an der sich das Scrolling befindet
	/// </summary>
	private Vector2 scrollPosition = Vector2.zero;
	
	private Texture2D bg;
	
	//Seite 1
	private string nachname = "";
	private string vorname = "";
	private string patientenNr = "";
	private string geburtsdatum = "";
	private string nationalitaet = "";
	private string datum = "";
	
	private string ersteSichtung1 = "";
	private string ersteSichtung2 = "";
	private string ersteSichtung3 = "";
	private string ersteSichtung4 = "";
	
	private string zweiteSichtung1 = "";
	private string zweiteSichtung2 = "";
	private string zweiteSichtung3 = "";
	private string zweiteSichtung4 = "";
	
	private string dritteSichtung1 = "";
	private string dritteSichtung2 = "";
	private string dritteSichtung3 = "";
	private string dritteSichtung4 = "";
	
	private string vierteSichtung1 = "";
	private string vierteSichtung2 = "";
	private string vierteSichtung3 = "";
	private string vierteSichtung4 = "";

	private string transportziel = "";
	private string weiblich = "";
	private string maennlich = "";
	
	private string liegend = "";
	private string sitzend = "";
	private string unterAufsicht = "";
	private string isoliert = "";
	//Seite 1 Ende
	
	//Seite 2
	private string verletzung = "";
	private string verbrennung = "";
	private string erkrankung = "";
	private string vergiftung = "";
	private string verstrahlung = "";
	private string psyche = "";
	
	private string kurzDiagnose = "";
	private string bewusstsein1 = "";
	private string bewusstsein2 = "";
	private string atmung1 = "";
	private string atmung2 = "";
	private string kreislauf1 = "";
	private string kreislauf2 = "";
	
	private string medikamente1 = "";
	private string spritze1 = "";
	private string infusion1 = "";
	private string uhrzeit1 = "";
	private string medikamente2 = "";
	private string spritze2 = "";
	private string infusion2 = "";
	private string uhrzeit2 = "";
	
	private string arztvermerke = "";
	private string religion = "";
	private string wohnort = "";
	private string strasse = "";
	private string fundort = "";
	private string uhrzeit = "";
	private string verbleib = "";
	private string helikopter = "";
	
	private int normalFontSize = 30;
	//Seite 2 Ende
	
	private GameObject m_editor = null;
	
	void Start(){
		bg = new Texture2D(1,1);
		bg.SetPixel(0,0,new Color(1,0,0,0.5f));
	}
	
	public void SetEditModeOn(GameObject editor)
	{
		m_editor = editor;
		editMode=true;
		if(m_editor) m_editor.SendMessage("EnableMovement",false, SendMessageOptions.DontRequireReceiver);
	}
	
	public void SetEditModeOff()
	{
		
		editMode=false;
		if(m_editor){
			m_editor.SendMessage("EnableMovement",true, SendMessageOptions.DontRequireReceiver);
			m_editor.SendMessage("EnableRaycast", true, SendMessageOptions.DontRequireReceiver);
		}
	}
	
	void Update()
	{
		if(editMode)
		{
			//must du in repeat mode because of the relationship of gameitem and handler
			if(m_editor) m_editor.SendMessage("EnableMovement",false, SendMessageOptions.DontRequireReceiver);
		}
	}
	
	
	void OnGUI()
	{
		if(editMode)
		{
			GUI.skin.textField.fontSize = normalFontSize;
			GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
			GUI.skin.textArea.fontSize = normalFontSize;
			if(page == 1)
			{							
				scrollPosition = GUI.BeginScrollView(new Rect(Screen.width/2-minSize/2, 0, Screen.width, Screen.height), scrollPosition, new Rect(0, 0, minSize, minSize/0.6f));
				GUI.DrawTexture(new Rect(0,0,1024,minSize/0.6f),vakFrontPrefab);
				nachname = GUI.TextField(new Rect(90,265,475,75),nachname);
				vorname = GUI.TextField(new Rect(660,262,350,75),vorname);
				geburtsdatum = GUI.TextField(new Rect(181,342,384,82),geburtsdatum);
				GUI.skin.textField.fontSize = 60;
				maennlich = GUI.TextField(new Rect(566,341,100,82),maennlich);
				weiblich = GUI.TextField(new Rect(666,341,100,82),weiblich);
				GUI.skin.textField.fontSize = normalFontSize;
				religion = GUI.TextField(new Rect(870,341,130,83),religion);
				wohnort = GUI.TextField(new Rect(122,425,443,80),wohnort);
				nationalitaet = GUI.TextField(new Rect(684,424,320,82),nationalitaet);
				strasse = GUI.TextField(new Rect(92,509,473,80),strasse);
				patientenNr = GUI.TextField(new Rect(862,508,140,82),patientenNr);
				fundort = GUI.TextField(new Rect(187,591,378,82),fundort);
				datum = GUI.TextField(new Rect(635,591,140,83),datum);
				uhrzeit = GUI.TextField(new Rect(851,591,150,83),uhrzeit);
				transportziel = GUI.TextField(new Rect(151,674,415,82),transportziel);
				verbleib = GUI.TextField(new Rect(696,674,308,82),verbleib);
				
				GUI.skin.textField.fontSize = normalFontSize;
				int x = 148;
				int y = 1084;
				int h = 115;
				int w = 210;
				ersteSichtung1 = GUI.TextField(new Rect(x,y,w,h),ersteSichtung1);
				ersteSichtung2 = GUI.TextField(new Rect(x,y+h-1,w,h),ersteSichtung2);
				ersteSichtung3 = GUI.TextField(new Rect(x,y+2*(h-1),w,h),ersteSichtung3);
				ersteSichtung4 = GUI.TextField(new Rect(x,y+3*(h-1),w,h),ersteSichtung4);
				x = 356;
				zweiteSichtung1 = GUI.TextField(new Rect(x,y,w,h),zweiteSichtung1);
				zweiteSichtung2 = GUI.TextField(new Rect(x,y+h-1,w,h),zweiteSichtung2);
				zweiteSichtung3 = GUI.TextField(new Rect(x,y+2*(h-1),w,h),zweiteSichtung3);
				zweiteSichtung4 = GUI.TextField(new Rect(x,y+3*(h-1),w,h),zweiteSichtung4);
				x = 564;
				dritteSichtung1 = GUI.TextField(new Rect(x,y,w,h),dritteSichtung1);
				dritteSichtung2 = GUI.TextField(new Rect(x,y+h-1,w,h),dritteSichtung2);
				dritteSichtung3 = GUI.TextField(new Rect(x,y+2*(h-1),w,h),dritteSichtung3);
				dritteSichtung4 = GUI.TextField(new Rect(x,y+3*(h-1),w,h),dritteSichtung4);
				x = 772;
				vierteSichtung1 = GUI.TextField(new Rect(x,y,w,h),vierteSichtung1);
				vierteSichtung2 = GUI.TextField(new Rect(x,y+h-1,w,h),vierteSichtung2);
				vierteSichtung3 = GUI.TextField(new Rect(x,y+2*(h-1),w,h),vierteSichtung3);
				vierteSichtung4 = GUI.TextField(new Rect(x,y+3*(h-1),w,h),vierteSichtung4);				
				
				liegend = GUI.TextField(new Rect(148,1555,162,136),liegend);
				sitzend = GUI.TextField(new Rect(320,1555,162,136),sitzend);
				unterAufsicht = GUI.TextField(new Rect(485,1555,162,136),unterAufsicht);
				helikopter = GUI.TextField(new Rect(652,1555,162,136),helikopter);
				isoliert = GUI.TextField(new Rect(820,1553,162,136),isoliert);
				GUI.EndScrollView();
			}else if(page == 2)
			{			
				scrollPosition = GUI.BeginScrollView(new Rect(Screen.width/2-minSize/2, 0, Screen.width, Screen.height), scrollPosition, new Rect(0, 0, minSize, minSize/0.6f));
				GUI.DrawTexture(new Rect(0,0,1024,minSize/0.6f),vakBackPrefab);				
				kurzDiagnose = GUI.TextField(new Rect(40,255,555,500),kurzDiagnose);
				verletzung = GUI.TextField(new Rect(705,255,300,85),verletzung);
				verbrennung = GUI.TextField(new Rect(725,338,280,85),verbrennung);
				erkrankung = GUI.TextField(new Rect(712,422,295,85),erkrankung);
				vergiftung = GUI.TextField(new Rect(705,505,300,85),vergiftung);
				verstrahlung = GUI.TextField(new Rect(770,587,237,85),verstrahlung);
				psyche = GUI.TextField(new Rect(750,672,258,85),psyche);
				
				bewusstsein1 = GUI.TextField(new Rect(410,760,290,105),bewusstsein1);
				bewusstsein2 = GUI.TextField(new Rect(725,760,285,105),bewusstsein2);
				atmung1 = GUI.TextField(new Rect(410,867,290,105),atmung1);
				atmung2 = GUI.TextField(new Rect(725,867,285,105),atmung2);
				kreislauf1 = GUI.TextField(new Rect(410,972,290,105),kreislauf1);
				kreislauf2 = GUI.TextField(new Rect(725,972,285,105),kreislauf2);
				
				medikamente1 = GUI.TextField(new Rect(38,1195,556,85),medikamente1);
				medikamente2 = GUI.TextField(new Rect(38,1282,556,85),medikamente2);
				spritze1 = GUI.TextField(new Rect(594,1195,140,85),spritze1);
				spritze2 = GUI.TextField(new Rect(594,1282,140,85),spritze2);
				infusion1 = GUI.TextField(new Rect(733,1195,137,85),infusion1);
				infusion2 = GUI.TextField(new Rect(733,1282,137,85),infusion2);
				uhrzeit1 = GUI.TextField(new Rect(870,1195,137,85),uhrzeit1);
				uhrzeit2 = GUI.TextField(new Rect(870,1282,137,85),uhrzeit2);
				arztvermerke = GUI.TextField(new Rect(38,1400,970,287),arztvermerke);
				
				GUI.EndScrollView();
			}
			GUI.skin.button.alignment = TextAnchor.MiddleCenter;
			GUI.skin.button.fontSize = 20;
			if(page == 2 && GUI.Button(new Rect(Screen.width/2+300,0,200,50),"Vorderseite"))
			{
				page = 1;	
			}else if( page == 1 && GUI.Button(new Rect(Screen.width/2+300,0,200,50),"Rueckseite")){
				page = 2;	
			}
			if(GUI.Button(new Rect(Screen.width/2-500,0,200,50),"Eingabe Beenden"))
			{
				SetEditModeOff();
			}
		}
	}
	
	/// <summary>
	/// Schaltet zwischen Editierung und in der Hand halten hin und her
	/// </summary>
	/// <param name="edit">
	/// A <see cref="System.Boolean"/>
	/// Editierung aktivieren(edit=true) oder deaktivieren(edit=false)
	/// </param>
	public void EditVAK(bool edit)
	{

		if(editMode){
			GUI.skin.textField.active.textColor = Color.black;
			GUI.skin.textField.normal.textColor = Color.black;
			GUI.skin.textField.hover.textColor = Color.black;
			GUI.skin.textField.onActive.textColor = Color.black;
			GUI.skin.textField.onNormal.textColor = Color.black;
			GUI.skin.textField.onHover.textColor = Color.black;
			GUI.skin.textField.focused.textColor = Color.black;
			GUI.skin.textField.onFocused.textColor = Color.black;
			
			//GUI.skin.textArea.font = objects.fontHandWrite;
			//GUI.skin.textField.font = objects.fontHandWrite;
			
		}else{
			//GUI.skin.textArea.font = objects.fontNormal;
			//GUI.skin.textField.font = objects.fontNormal;
		}
	}
}
