/* Prüft ob sich der Spieler zu nahm am Feuer befindet.
 * Im Falle wird eine entsprechende GUI eingeblendet.
 * 
 * Wenn innerer und aeußerer Kern geloescht wurden, wird dieses Objekt deaktiviert
 * 
 * Folgende Komponenten muessen dem Script uebergeben werden:
 * 
 * GUITexture redRec - GUI Einblendung zu nah am feuer
 * bool playerHits = false
 * ParticleEmitter innerCore - Emitter Innerer Kern
 * ParticleEmitter outerCore - Emitter aeußerer Kern
 * GUIText labelZuNahAmFeuer - GUI-Label Einblendung zu nah am feuer
 * GUI_MENU gui - GUI des Hauptmenues
 * 
 * */

using UnityEngine;
using System.Collections;

public class Fire_Hit_Range : MonoBehaviour {
	
	public GUITexture redRec;
	public ParticleEmitter innerCore;
	public ParticleEmitter outerCore;


	private bool playerHits = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		//Wenn der innere und äußere Kern des Feuers deaktiviert sind, wird dieses Objekt deaktiviert
		if(innerCore != null && outerCore != null){
		if(innerCore.enabled == false && outerCore.enabled == false)
		/*	this.transform.gameObject.SetActiveRecursively(false)*/;
		}
		//Wenn der Spieler zu nah am Feuer ist, werden entsprechende GUI_Infos angezeigt
/*		if(playerHits) {
			redRec.enabled = true;
			labelZuNahAmFeuer.enabled = true;
			redRec.pixelInset = new Rect (-Screen.width/2, -Screen.height/2, Screen.width,  Screen.height);
			
		} else {
			redRec.enabled = false;
			labelZuNahAmFeuer.enabled = false;
		}*/
		
		
	}
	
	 /*   void OnTriggerStay(Collider other)  {
			playerHits = true;
		
		}
	
		 void OnTriggerExit(Collider other) {
			playerHits = false;
		
		}	*/
}
