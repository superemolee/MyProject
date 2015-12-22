/* Skaliert den Feuerradius dem sich der Spieler nähern kann
 * Wird bis zum einem maxWert skaliert.
 * 
 * Folgende Komponenten muessen dem Script uebergeben werden:
 * 
 * float fireSpreadingSpeed - Ausbreitungsgeschwindigkeit des Feuerradius
 * bool dynamicFire - Breitet sich der Radius aus
 * */

using UnityEngine;
using System.Collections;

public class FireRangeSpreading : MonoBehaviour {
	
	public float fireSpreadingSpeed = 0.0001f;
	public bool dynamicFire;
	
	private float x ;
	private float z ; 
	private float maxScaleX = 0.8f;
	private float maxScaleY = 0.2f;
	private float maxScaleZ = 0.8f;
	private bool maxReached = false;
	private float y ;
	
	// Use this for initialization
	void Start () {
		x = this.transform.localScale.x;
		y = this.transform.localScale.y;
		z = this.transform.localScale.z;  
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if(x >= maxScaleX && z >= maxScaleZ) {
			maxReached = true;
				
		}
		
		if(dynamicFire && !maxReached) {
		
			if(x < maxScaleX) {
				x+=fireSpreadingSpeed;
			}
			
			if(z < maxScaleZ) {
				z+=fireSpreadingSpeed;			
			}
			
			 this.transform.localScale = new Vector3(x,y,z);
		 } else if(!dynamicFire) {
			this.transform.localScale = new Vector3(x,y,z);	
		}
	}
}
