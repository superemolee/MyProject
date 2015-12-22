using UnityEngine;
using System.Collections;

public class RandomCloth : MonoBehaviour {
	
	public string matName;
	public Texture2D [] textures;
	// Use this for initialization
	void Awake () {
	
		SkinnedMeshRenderer [] materials = (SkinnedMeshRenderer [])GameObject.FindSceneObjectsOfType(typeof(SkinnedMeshRenderer));
		foreach(SkinnedMeshRenderer smr in materials)
		{
			if(smr.name.Contains(matName))
			{
				smr.material.SetTexture("_MainTex", textures[Random.Range(0,textures.Length-1)]);
			}
		}
		
	}
	

}
