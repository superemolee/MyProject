using UnityEngine;
using System.Collections.Generic;

public class RaycastTable : MonoBehaviour {
	
	public int xRes = 11, yRes = 11;
	
	public float hAng = 300.0f;
	
	public float vAng = 150.0f;
	
	public int viewDistance = 100;
	
	int lastXRes, lastYRes;
	float lastVAng, lastHAng;
	
	public List<Transform> seenObjects;
	
	Vector4 [,] cosine;
	
	// Use this for initialization
	void Start () {
		InitCosineTable();
	}
	
	// Update is called once per frame
	void Update () {
		if(xRes != lastXRes || yRes != lastYRes || hAng != lastHAng || vAng != lastVAng){
			InitCosineTable();
		}
		
		UpdateRaycasting();
		
	}
	
	
	void UpdateRaycasting(){
		for(int i = 0; i < yRes; i++){
			for(int j = 0; j < xRes; j++){	
				cosine[i, j].w = 0;
			}
		}
		
		
		int hitCount = 0;
		
		int depth = 0;
		
		int totalPixels = xRes * yRes;
		
		RaycastHit hit;
		Vector3 origin = transform.position;
		Vector3 target;
		
		foreach(Transform t in seenObjects){
			t.renderer.material.color = Color.white;
		}
		
		seenObjects.Clear();
		
		while(hitCount < totalPixels && depth < viewDistance){
			depth++;
			
			for(int i = 0; i < yRes; i++){
				for(int j = 0; j < xRes; j++){	
					
					target.x = cosine[i, j].x;
					target.y = cosine[i, j].y;
					target.z = cosine[i, j].z;
					target = transform.rotation * target;
					target = target * depth;
					target += origin;
					
					
					
					if(cosine[i, j].w == 0){
						Debug.DrawLine(origin, target);
						if(Physics.Raycast(origin, target - origin, out hit, depth)){
							cosine[i, j].w = 1;
							if(!seenObjects.Contains(hit.transform)){
								seenObjects.Add(hit.transform);
							}
							hitCount++;
						}
					}
				}
			}
		}
		
		foreach(Transform t in seenObjects){
			t.renderer.material.color = Color.red;
		}
		
	}
	
	void InitCosineTable(){
		
		if(xRes % 2 == 0 || yRes % 2 == 0){
			throw new System.Exception("Resolutions must be odd");
		}
		
		cosine = new Vector4[yRes, xRes];
		
		int midX, midY;
		midX = (xRes / 2) + 1;
		midY = (yRes / 2) + 1;
		
		float xStep, yStep;
		
		xStep = (hAng * 0.5f) / (midX - 1);
		
		
		yStep = (vAng * 0.5f) / (midY - 1);
		
		
		for(int i = 0; i < yRes; i++){
			float yAng = (i + 1 - midY) * yStep;
			for(int j = 0; j < xRes; j++){
				float xAng = (j + 1 - midX) * xStep;	
				cosine[i, j].x = Mathf.Sin(xAng * Mathf.Deg2Rad);
				cosine[i, j].y = Mathf.Sin(yAng * Mathf.Deg2Rad);
				cosine[i, j].z = Mathf.Cos(xAng * Mathf.Deg2Rad);
				cosine[i, j].w = 0;
			}
		}
		
		lastXRes = xRes;
		lastYRes = yRes;
		lastVAng = vAng;
		lastHAng = hAng;
		
	}
}
