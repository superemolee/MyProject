/* Simuliert Feuerausbreitung indem das Feuerobjekt skaliert wird.
 * 
 * OnParticleCollision stellt Kollision zwischen Wasser-Partikeln fest. Indem Fall wird das
 * Feuer und der Feuerradius runter skaliert, bis sie einen minimalen Wert erreichen. Danach wird es abgeschaltet.
 * Wenn Feuerobjekte geloescht wurden, wird der rauch abgescgwaecht.
 * 
 * 
 * Folgende Komponenten muessen dem Script uebergeben werden:
 * 
 * ParticleEmitter smoke - Rauch
 * float maxScaleX - Max Skalierungswert X-Achse
 * float maxScaleY - Max Skalierungswert Y-Achse 
 * float maxScaleZ - Max Skalierungswert Z-Achse
 * bool dynamicFire - Breites sich das Feuer aus
 * float fireSpreadingSpeed - Ausbreitungsgeschwindigkeit
 * FireRangeSpreading fireRange - Feuerradius
 * */
using UnityEngine;
using System.Collections;

public class FireSpreading : MonoBehaviour
{
	

	public ParticleEmitter smoke;
	public float maxScaleX = 0.2f;
	public float maxScaleY = 0.2f;
	public float maxScaleZ = 0.2f;
	public bool dynamicFire;
	public float fireSpreadingSpeed = 0.0001f;
	public float x ;
	public float z ;
	public FireRangeSpreading fireRange;
	private float rangeX ;
	private float rangeY ;
	private float rangeZ ;
	private float fireSize = -1;
	private bool colli = false;
	private float y ;
	private ParticleEmitter innerParticle;
	private float fireSizeSmall = 0f;
	private float fireSizeMedium = 1f;
	private float fireSizeBig = 2f;
	private bool maxReached = false;
	
	void Start ()
	{
		innerParticle = this.GetComponent ("ParticleEmitter") as ParticleEmitter;
		x = innerParticle.transform.localScale.x;
		y = innerParticle.transform.localScale.y;
		z = innerParticle.transform.localScale.z;  
		
		if (fireRange != null) {
			rangeX = fireRange.transform.localScale.x;
			rangeY = fireRange.transform.localScale.y;
			rangeZ = fireRange.transform.localScale.z;
		}
 
	}
	
	void OnParticleCollision (GameObject other)
	{
		//print ("außen" + other.name);
		if (other.name.Equals ("WaterFontain")) {
			//print ("innen" + other.name);
			//runter skalieren
			if (x > 0.0001) {
				x -= 0.00035f;
				rangeX -= 0.00035f;
			}
			
			if (z > 0.0001) {
				z -= 0.00035f;
				rangeZ -= 0.00035f;
			}
			
			//Radius wird deaktiviert wenn Feuer gewisse Groesse erreicht
			if (fireRange != null && rangeX < 0.8f) {
				fireRange.gameObject.SetActive (false);
			}
			
			//Neu ausrichten
			innerParticle.transform.localScale = new Vector3 (x, y, z);
			
			if (fireRange != null) {
				fireRange.transform.localScale = new Vector3 (rangeX, y, rangeZ);
			}
			

		} 
	}

	void Update ()
	{
		//Ist der Max-Skalierungswet erreicht?
		if (x >= maxScaleX && z >= maxScaleZ) {
			maxReached = true;
			return;	
		}
		
		//Wenn das Feuer sich ausbreiten kann und Max-Wert noch nicht erreicht wurde
		//Skaliere Feuer
		if (dynamicFire && !maxReached) {
		
			if (x < maxScaleX) {
				x += fireSpreadingSpeed;
			}
			
			if (z < maxScaleZ) {
				z += fireSpreadingSpeed;			
			}
			
			innerParticle.transform.localScale = new Vector3 (x, y, z);
		} else if (!dynamicFire) {
			innerParticle.transform.localScale = new Vector3 (x, y, z);	
		}
		
		//Feuerobjekt deaktivieren
		if (x < 0.0001f && z < 0.0001f) {
				
			//About smoke, edited by Li
			
			innerParticle.maxEmission = 0;
			gameObject.transform.parent.audio.Stop();
			
			//Rauch bleit nachdem Feuer geloescht wurde aktiv
			if (smoke != null) {
				smoke.maxEmission =0;
				//smoke.minEmission = 1f;
				//smoke.maxEmission = 1f;
				//smoke.maxEnergy = 1f;
				//smoke.minEnergy = 11f;
			}
			
			//Rauch wird ausgeschaltet
			if (smoke.maxEmission == 0) {	
				//smoke.transform.gameObject.SetActive (false);
				//transform.parent.gameObject.SetActive (false);
				
			}
			//end edited.
				
			//Deaktivieren
				
			//transform.parent.gameObject.SetActive (false);	
			//gameObject.SetActive (false);	
				
		}

	}
}
