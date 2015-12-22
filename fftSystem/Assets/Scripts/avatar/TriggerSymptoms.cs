using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class TriggerSymptoms : MonoBehaviour {
	
	//--- public enums --------------------------------------------------------
	
	/// <summary>
	/// This enum represents the Eyeside for the pupil dilation
	/// </summary>
	public enum EyeSide{
		Both, 
		Left, 
		Right
	};
	
	
	/// <summary>
	/// This enum provides a priority for building a new 
	/// </summary>
	public enum DisorderVisibility{
		Brightness,
		Redness,
		Hematoms,
		Burnings,
		Wounds,
		OpenFractures,
	}
	
	//--- public inner classes-------------------------------------------------
	
	//--- public fields Unity--------------------------------------------------
	
	public int FireParticleCollided1Degree = 200;
	public int FireParticleCollided2Degree = 400;
	public int FireParticleCollided3Degree = 500;
	
	
	
	/// <summary>
	/// This textasset is a normal textfile exported from max for pupil dilation left eye
	/// </summary>
	public TextAsset importedLeftEyeUVs;
	
	
	/// <summary>
	/// This textasset is a normal textfile exported from max for pupil dilation right eye
	/// </summary>
	public TextAsset importedRightEyeUVs;
	
	/// <summary>
	/// The basic animation for shaking
	/// </summary>
	public AnimationClip shakeAnimation;
	
	
	/// <summary>
	/// the basic animation for open and close eyes
	/// </summary>
	public AnimationClip closeEyes;
	
	/// <summary>
	/// the body template which indicates the regions of the body where a disorder can be placed
	/// </summary>
	public Texture2D bodyTemplate;
	
	/// <summary>
	/// A particle System for generating a nose blood
	/// </summary>
	public GameObject noseBleedSystem;	
	
	//--- protected fields--------------------------------------------------------
	
	
	protected Vector2 [] m_allOrgUVS;
	protected Dictionary<GameObject,int> m_fireCollidedWithBodyRegion = new Dictionary<GameObject, int>();
	protected Vector2 m_leftEyeCenter;
	protected Vector2 m_rightEyeCenter;
	protected List<Vector2> leftEyeUVs;
	protected List<Vector2> rightEyeUVs;
	protected Texture2D m_mainTexture;

	protected Mesh m_mesh;

	
	/// <summary>
	/// the imported indices from the textfile <see cref="importedLeftEyeUVs"/>
	/// </summary>
	private List<int> m_leftEyeUVIndices;
	
	/// <summary>
	/// the readed indices from the textfile <see cref="importedRightEyeUVs"/>
	/// </summary>
	private List<int> m_rightEyeUVIndices;
	
	
	private List<KeyValuePair<Texture2D, Color32> > m_haematoms= new List<KeyValuePair<Texture2D, Color32>>();
	private List<KeyValuePair<Texture2D, Color32> > m_burnings= new List<KeyValuePair<Texture2D, Color32>>();
	private List<KeyValuePair<Texture2D, Color32> > m_wounds= new List<KeyValuePair<Texture2D, Color32>>();
	private List<KeyValuePair<Texture2D, Color32> > m_fractureOpen= new List<KeyValuePair<Texture2D, Color32>>();
	
	private Animation m_avatarAnim;
	
	/// <summary>
	/// Texture for calculation
	/// </summary>
	private Texture2D m_mainOrgTexture;


	//--- functions unity ------------------------------------------------------------
	
	void OnDestroy()
	{
		if(m_allOrgUVS != null)
		{
			
			if(m_mesh!=null)
			{
				m_mesh.uv= m_allOrgUVS;
			}
		}
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	/// <param name="collided">
	/// A <see cref="GameObject"/>
	/// </param>	
	void fireCollided(GameObject collided)
	{
		
		if(!m_fireCollidedWithBodyRegion.ContainsKey(collided))
		{
			m_fireCollidedWithBodyRegion.Add(collided,1);
		}
		else
		{
			m_fireCollidedWithBodyRegion[collided]+=1;
		}
		
		if(m_fireCollidedWithBodyRegion[collided]>FireParticleCollided1Degree)
		{
			//Debug.Log(collided.name +" collided with " + m_fireCollidedWithBodyRegion[collided]);
		}
		
	}
	
	
	
	
	
	/// <summary>
	/// Internal function to import the Textfile from 3ds max
	/// </summary>
	/// <param name="ta">
	/// A <see cref="TextAsset"/>
	/// </param>
	/// <param name="listToFill">
	/// A <see cref="List<Vector2>"/>
	/// </param>
	/// <returns>
	/// A <see cref="System.Boolean"/>
	/// </returns>
	bool ReadTextAsset(TextAsset ta, ref List<Vector2> listToFill)
	{
		if(!ta)
		{
			return false;
		}
		bool retval = true;
		string allText = ta.text;
		string [] lines = allText.Split('\n');
		foreach(string line in lines)
		{
			string[] factors = line.Split(';');
			if(factors.Length==2)
			{
				listToFill.Add(new Vector2((float)Convert.ToDouble(factors[0]), (float)Convert.ToDouble(factors[1])));
			}
			else{
				retval=false;
			}
		}
		
		
		return retval;
	}
	
	
	/// <summary>
	/// Calculates the center of a given list of uvs
	/// </summary>
	/// <param name="list">
	/// A <see cref="List<Vector2>"/>
	/// </param>
	/// <returns>
	/// A <see cref="Vector2"/> which is the center of all points provided by parameter list
	/// </returns>
	Vector2 CalculateCenter(List<Vector2> list)
	{
		Vector2 center = new Vector2();
		if(list!=null && list.Count>0)
		{
			foreach(Vector2 v in list)
			{
				center+=v;
			}
			center/=(float)list.Count;
		}
		return center;
	}
	
	
	
	/// <summary>
	/// This function looks for the uvs provided in the textfiles which where exported 
	/// by 3ds max in the original Texture UVs
	/// </summary>
	/// <param name="orguvs">
	/// A <see cref="Vector2[]"/>
	/// </param>
	/// <param name="readUVs">
	/// A <see cref="List<Vector2>"/>
	/// </param>
	/// <param name="indices">
	/// A <see cref="List<System.Int32>"/>
	/// </param>
	void FindUVIndexes(Vector2[] orguvs, List<Vector2> readUVs, ref List<int> indices)
	{
		for(int i=0; i<orguvs.Length;++i)
		{
			foreach(Vector2 v in readUVs)
			{
				
				if(Mathf.Abs(orguvs[i][0]-v[0])<0.000001f && Mathf.Abs(orguvs[i][1]-v[1])<0.000001f)
				{
					//Debug.Log("found uv" + i + "position = " + orguvs[i]);
					indices.Add(i);
					break;
				}
			}
		}	
	}
	
	
	void Awake () {
		
		m_avatarAnim = animation;
		//initialize uv for pupil dilation
		m_leftEyeUVIndices = new List<int>();
		m_rightEyeUVIndices = new List<int>();
		
		leftEyeUVs = new List<Vector2>();
		rightEyeUVs= new List<Vector2>();
		
		//read exported uvcoordinates from textasset
		ReadTextAsset(importedLeftEyeUVs,ref leftEyeUVs);
		ReadTextAsset(importedRightEyeUVs,ref rightEyeUVs);
		
		//calculate the center for scaling uvs around center
		m_leftEyeCenter = CalculateCenter(leftEyeUVs);
		m_rightEyeCenter = CalculateCenter(rightEyeUVs);
				
		//copy mesh, so changes are not in original
//		Mesh mesh = new Mesh();
//		Mesh toCopy=gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
//		mesh.vertices=toCopy.vertices;
//		mesh.triangles = toCopy.triangles;
//		mesh.tangents = toCopy.tangents;
//		mesh.uv = toCopy.uv;
//		mesh.bindposes = toCopy.bindposes;
//		mesh.boneWeights=toCopy.boneWeights;
//		mesh.RecalculateNormals();
//	 	gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh=mesh;
//		
		m_mesh = gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
		m_allOrgUVS=(Vector2[])m_mesh.uv.Clone();
		
		
		
		//find the left and right uvs
		FindUVIndexes(m_allOrgUVS, leftEyeUVs,ref m_leftEyeUVIndices);
		FindUVIndexes(m_allOrgUVS, rightEyeUVs, ref m_rightEyeUVIndices);
		
		Renderer mainRenderer = GetComponentInChildren<Renderer>();
		if(mainRenderer)
		{
			//copy maintexture
			Texture2D orgTex = (Texture2D)mainRenderer.material.mainTexture;
			m_mainTexture= new Texture2D(orgTex.width, orgTex.height);
			m_mainOrgTexture= new Texture2D(orgTex.width, orgTex.height);
			
			m_mainTexture.SetPixels32(orgTex.GetPixels32());
			m_mainTexture.Apply();
			
			m_mainOrgTexture.SetPixels32(orgTex.GetPixels32());
			m_mainOrgTexture.Apply();
			
			mainRenderer.material.mainTexture= m_mainTexture;
		}
		
		
//		Collider[] cols = GetComponentsInChildren<Collider>();
//		
//		for(int i= 0; i < cols.Length;++i)
//		{
//			BodyRegionCollider brc =cols[i].gameObject.AddComponent<BodyRegionCollider>();
//		}
		
	}
	

	/// <summary>
	/// Debug function for setting brightness via gui
	/// </summary>
	/// <param name="factor">
	/// A <see cref="System.String"/>
	/// </param>
	public void SetBrightness(string factor)
	{
		byte b=Convert.ToByte(factor);
		SetBrightness(b);
	}
	
	

	
	public void SetBrightness(byte factor)
	{
		Color32 [] pixels = m_mainOrgTexture.GetPixels32();
		
		Color32 [] bodyTmpPixels = null;
		if(bodyTemplate)
		{
			bodyTmpPixels = bodyTemplate.GetPixels32();
		}
		
		for(int i=0;i< pixels.Length;++i)
		{
			if(bodyTmpPixels!=null)
			{
				if(bodyTmpPixels[i].r!=0)
				{
					pixels[i].r=(byte)(byte)Mathf.Max((byte)0,Mathf.Min((byte)255, (pixels[i].r+(byte)factor)));
					pixels[i].g=(byte)(byte)Mathf.Max((byte)0,Mathf.Min((byte)255, (pixels[i].g+(byte)factor)));
					pixels[i].b=(byte)(byte)Mathf.Max((byte)0,Mathf.Min((byte)255, (pixels[i].b+(byte)factor)));
				}
			}
			else
			{

				pixels[i].r=(byte)(byte)Mathf.Max((byte)0,Mathf.Min((byte)255, (pixels[i].r+(byte)factor)));
				pixels[i].g=(byte)(byte)Mathf.Max((byte)0,Mathf.Min((byte)255, (pixels[i].g+(byte)factor)));
				pixels[i].b=(byte)(byte)Mathf.Max((byte)0,Mathf.Min((byte)255, (pixels[i].b+(byte)factor)));
			}
		}
		m_mainTexture.SetPixels32(pixels);
		m_mainTexture.Apply();	
	}
	
	
	
	public void ScaleBothPupils(float factor)
	{
		ScalePupils(factor, EyeSide.Both);
		Debug.Log(factor);
	}
	
	public void ScalePupils(float factor, EyeSide eyeside)
	{
		Vector2 [] uvs = (Vector2[]) m_allOrgUVS.Clone();
		switch(eyeside){
		case EyeSide.Left:
			ScalePupils(factor,ref uvs, ref m_leftEyeUVIndices, ref m_leftEyeCenter);
			
			break;
		case EyeSide.Right:
			ScalePupils(factor,ref uvs, ref m_rightEyeUVIndices, ref m_rightEyeCenter);
			
			break;
		case EyeSide.Both:
			
				
			ScalePupils(factor,ref uvs, ref m_leftEyeUVIndices, ref m_leftEyeCenter);
			ScalePupils(factor,ref uvs, ref m_rightEyeUVIndices, ref m_rightEyeCenter);
			
			break;
		}
	}
	
	
	
	void ScalePupils(float factor,ref Vector2[] manipUVs, ref List<int> indices, ref Vector2 center)
	{
		//scale between 0.5f and 1.5f
		float scaleFactor = Mathf.Max(0.5f,Mathf.Min(1.5f, factor));
		
		//clone original uvs first
		
		
		for(int i = 0; i <indices.Count;++i)
		{
			Vector2 v2 = manipUVs[indices[i]]-center;
			Vector2 v2scaled = Vector2.Scale(v2, new Vector2(scaleFactor,scaleFactor));
			manipUVs[indices[i]]= v2scaled+center;
			
		}
		
		gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh.uv = manipUVs;
		
		
	}
	
	
	public void Shake()
	{
		shakeAnimation.wrapMode = WrapMode.Loop;
		if(animation)
			animation.CrossFade(shakeAnimation.name);
	}
	
	public void Nosebleed(bool isActive, float strength)
	{
		if(noseBleedSystem)
		{
			noseBleedSystem.active = isActive;
			if(isActive)
			{
				noseBleedSystem.SendMessage("SetStrength", (object) strength);
			}
			
		}
	}
	public void CloseEyes(string shouldClose)
	{
			CloseEyes(Convert.ToBoolean(shouldClose));
	}
	public void CloseEyes(bool shouldClose)
	{
		if(m_avatarAnim)
		{
			if(shouldClose && m_avatarAnim[closeEyes.name].time == 0)
			{
				m_avatarAnim[closeEyes.name].speed=Mathf.Abs(m_avatarAnim[closeEyes.name].speed);
				m_avatarAnim[closeEyes.name].time = 0;
				m_avatarAnim[closeEyes.name].layer=9;
				m_avatarAnim.CrossFade(closeEyes.name);
			}
			else{
				m_avatarAnim[closeEyes.name].speed=-Mathf.Abs(m_avatarAnim[closeEyes.name].speed);
				m_avatarAnim[closeEyes.name].time = closeEyes.length;
				m_avatarAnim.CrossFade(closeEyes.name);
			}
		}
	}
	
	public void AddTexture(Texture2D texture, Color32 bodypart, DisorderVisibility category)
	{
		switch(category)
		{
		case DisorderVisibility.Brightness:
		case DisorderVisibility.Burnings:
			m_burnings.Add(new KeyValuePair<Texture2D,Color32>(texture, bodypart));
			break;
		}
		
	}
	
	
	public void UpdateBodyTexture()
	{
		
			Color32 [] orgColors = m_mainTexture.GetPixels32();
			
		 	Color32 [] bodyRegionColors = bodyTemplate.GetPixels32();

		
		//priority 1: brightness
		
		//priority 2: redness
		//priority 3: hematoms
		//priority 4: burnings
		for(int i =0; i< orgColors.Length;++i)
		{
			if(bodyRegionColors[i].a == 0)
			{
				continue;
			}
				
			foreach(KeyValuePair<Texture2D,Color32> kvp in m_burnings)
			{
	
				Color32 [] burningColors = kvp.Key.GetPixels32();
				
				
				
			}		
				
		}
		
		//priority 5: wounds
		//priority 6: open fractures
		//

	}
	
	
	
	public void TriggerPulse(float intensity)
	{
	
		Debug.Log("Pulse with intensity "+ intensity+ " at Time: " +DateTime.Now.TimeOfDay); 
	}
	
	
	public void SetOverlayTexture(Texture2D tex)
	{
		//Renderer mainRenderer = GetComponentInChildren<Renderer>();
		m_mainTexture.SetPixels32(TextureUtilities.CombineTextures32(m_mainTexture,tex));
		m_mainTexture.Apply();
		//mainRenderer.material.mainTexture = m_mainTexture;
	}
	
}
