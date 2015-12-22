/* Initialisiert die Feuerobjekte eines jeweiligen Stockwerks
 * 
 * Je nachdem was in der Start GUI ausgewaehlt wurde (Size?, Dynmaic?, SpreadingSpeed?), werden den Fuerobjekten entsprechende Eigenschaften zugewiesen
 * 
 * Wenn der Maximale Skalierungswert eines Feuers erreicht wurde, wird das naechste Feuerobjekt aktiviert.
 * 
 * Folgende Komponenten muessen dem Script uebergeben werden:
 * 
 * FireSpreading innerParticle - Feuer Innerer Kern
 * FireSpreading outerParticle - Feuer Aeußerer Kern
 * float size - Groesse der Feuers
 * bool dynamicFire - Breitet sich das Feuer aus
 * Fire nextFire - Feuerobjekt das als naechstes aktiviert wird, wenn max Skalierungswert erreicht ist
 * float fireSpreadingSpeed - Ausbreitungsgeschwindigkeit
 * FireRangeSpreading fireRange - Radius dem man sic hdem Feuer naehern darf
 * 
 * */

using UnityEngine;
using System.Collections;

public class Fire : MonoBehaviour {
	
	
	public FireSpreading innerParticle;
	public FireSpreading outerParticle;
	public float size;
	public bool dynamicFire;
	public Fire nextFire;
	public float fireSpreadingSpeed;
	public FireRangeSpreading fireRange;
	
	private bool nextFireActive = false;
	
	
	// Use this for initialization
	void Start () {
		
		if(size == 0) {
			innerParticle.x = 0.01f;
			innerParticle.z = 0.01f;
			outerParticle.x = 0.01f;
			outerParticle.z = 0.01f;
			innerParticle.dynamicFire = dynamicFire;
			outerParticle.dynamicFire = dynamicFire;
			innerParticle.fireSpreadingSpeed = fireSpreadingSpeed;
			outerParticle.fireSpreadingSpeed = fireSpreadingSpeed;
			fireRange.dynamicFire = dynamicFire;
			fireRange.fireSpreadingSpeed = fireSpreadingSpeed;
			
			
		} else if(size == 1) {
			innerParticle.x = 0.1f;
			innerParticle.z = 0.1f;
			outerParticle.x = 0.1f;
			outerParticle.z = 0.1f;	
			innerParticle.dynamicFire = dynamicFire;
			outerParticle.dynamicFire = dynamicFire;
			innerParticle.fireSpreadingSpeed = fireSpreadingSpeed;
			outerParticle.fireSpreadingSpeed = fireSpreadingSpeed;
			fireRange.dynamicFire = dynamicFire;
			fireRange.fireSpreadingSpeed = fireSpreadingSpeed;

		} else if(size == 2) {
			innerParticle.x = 0.15f;
			innerParticle.z = 0.15f;
			outerParticle.x = 0.15f;
			outerParticle.z = 0.15f;	
			innerParticle.dynamicFire = dynamicFire;
			outerParticle.dynamicFire = dynamicFire;
			innerParticle.fireSpreadingSpeed = fireSpreadingSpeed;
			outerParticle.fireSpreadingSpeed = fireSpreadingSpeed;
			fireRange.dynamicFire = dynamicFire;
			fireRange.fireSpreadingSpeed = fireSpreadingSpeed;

		} 
			
		
	}
	
	// Update is called once per frame
	void Update () {
		checkFireSize();
	}
	

	
	// Wenn maximalgroesse erreicht wird, wird naechstes Feuerobjekt aktiviert mit entspechenden Eigenschaften wie der Vorgaenger
	//Diese Script wird anschließend deaktiviert.
	private void checkFireSize() {
		if(innerParticle != null && outerParticle !=null && nextFire != null){
		if(!nextFireActive && innerParticle.x >= innerParticle.maxScaleX && innerParticle.z >= innerParticle.maxScaleZ 
			&& outerParticle.x >= outerParticle.maxScaleX && outerParticle.z >= outerParticle.maxScaleZ) {
				nextFireActive = true;
				nextFire.enabled = true;
				nextFire.size = size;
				nextFire.dynamicFire = dynamicFire;
				nextFire.fireSpreadingSpeed = fireSpreadingSpeed;
				nextFire.innerParticle.dynamicFire = innerParticle.dynamicFire;
				nextFire.outerParticle.dynamicFire = outerParticle.dynamicFire;
				nextFire.innerParticle.fireSpreadingSpeed = innerParticle.fireSpreadingSpeed;
				nextFire.outerParticle.fireSpreadingSpeed = outerParticle.fireSpreadingSpeed;
				nextFire.gameObject.SetActiveRecursively(true);
				nextFire.fireRange.dynamicFire = dynamicFire;
				nextFire.fireRange.fireSpreadingSpeed = fireSpreadingSpeed;
			
				this.enabled = false;
				
			
		}
	}
	}
}
