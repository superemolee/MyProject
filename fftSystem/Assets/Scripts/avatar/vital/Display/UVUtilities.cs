using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class provides functions for Scaling UV coordinates of a texture. It is primary used for pupil dilation.
/// </summary>
[AddComponentMenu("Avatar/Display/UVUtilities")]
public class UVUtilities : MonoBehaviour
{
	/// <summary>
	/// This enum represents the Eyeside for the pupil dilation
	/// </summary>
	public enum EyeSide{
		Both, 
		Left, 
		Right
	};
	
	/// <summary>
	/// This textasset is a normal textfile exported from max for pupil dilation left eye
	/// </summary>
	public TextAsset m_importedLeftEyeUVs;
	
	/// <summary>
	/// This textasset is a normal textfile exported from max for pupil dilation right eye
	/// </summary>
	public TextAsset m_importedRightEyeUVs;
	
	/// <summary>
	/// Indicator if a default makehuman avatar is used.
	/// </summary>
	public bool m_isDefaultMakehuman = true;
	
	/// <summary>
	/// The mesh of the avatar with the uvs. It will be set to default values if m_isDefaultMakehuman == true.
	/// </summary>
	public Mesh m_mesh;	
	
	/// <summary>
	/// The actual scaling factor.
	/// </summary>
	public float m_scalingFactor =1.0f;

	
	
	
	//--- private fields--------------------------------------------------------
	
	/// <summary>
	/// The original UVs.
	/// </summary>
	private Vector2 [] m_allOrgUVS;
	
	/// <summary>
	/// The left eye center.
	/// </summary>
	private Vector2 m_leftEyeCenter;
	
	/// <summary>
	/// The right eye center.
	/// </summary>
	private Vector2 m_rightEyeCenter;
	
	/// <summary>
	/// The left eye UVs.
	/// </summary>
	private IList<Vector2> m_leftEyeUVs;
	
	/// <summary>
	/// The right eye UVs.
	/// </summary>
	private IList<Vector2> m_rightEyeUVs;

	
	/// <summary>
	/// the imported indices from the textfile <see cref="importedLeftEyeUVs"/>
	/// </summary>
	private IList<int> m_leftEyeUVIndices;
	
	/// <summary>
	/// the readed indices from the textfile <see cref="importedRightEyeUVs"/>
	/// </summary>
	private IList<int> m_rightEyeUVIndices;
	
	/// <summary>
	/// The old scaling factor so we see in the update method, that there was a change.
	/// </summary>
	private float m_lastScalingFactor;
	
	
	//----public functions -----------------------------------------
	
	#region publicFunctions
	/// <summary>
	/// Scales both pupils.
	/// </summary>
	/// <param name='factor'>
	/// Factor.
	/// </param>
	public void ScaleBothPupils(float factor)
	{
		ScalePupils(factor, EyeSide.Both);
		Debug.Log(factor);
	}
	
	/// <summary>
	/// Scales the pupils depending on parameter eyeside
	/// </summary>
	/// <param name='factor'>
	/// Factor.
	/// </param>
	/// <param name='eyeside'>
	/// Eyeside.
	/// </param>
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
	#endregion
	
	//----protected functions -----------------------------------------
	
	
	/// <summary>
	/// Internal function to import the Textfile from 3ds max
	/// </summary>
	/// <param name="ta">
	/// A <see cref="TextAsset"/>
	/// </param>
	/// <param name="listToFill">
	/// A <see cref="IList<Vector2>"/>
	/// </param>
	/// <returns>
	/// A <see cref="System.Boolean"/>
	/// </returns>
	bool ReadTextAsset(TextAsset ta, ref IList<Vector2> listToFill)
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
	/// This function looks for the uvs provided in the textfiles which where exported 
	/// by 3ds max in the original Texture UVs
	/// </summary>
	/// <param name="orguvs">
	/// A <see cref="Vector2[]"/>
	/// </param>
	/// <param name="readUVs">
	/// A <see cref="IList<Vector2>"/>
	/// </param>
	/// <param name="indices">
	/// A <see cref="IList<System.Int32>"/> of indices of the new mesh
	/// </param>
	void FindUVIndexes(Vector2[] orguvs, IList<Vector2> readUVs, ref IList<int> indices)
	{
		for(int i=0; i<orguvs.Length;++i)
		{
			foreach(Vector2 v in readUVs)
			{
				
				if(Mathf.Abs(orguvs[i][0]-v[0])<0.0001f && Mathf.Abs(orguvs[i][1]-v[1])<0.0001f)
				{
					//Debug.Log("found uv" + i + "position = " + orguvs[i]);
					indices.Add(i);
					break;
				}
			}
		}	
	}
	
	/// <summary>
	/// Calculates the center of a given list of uvs
	/// </summary>
	/// <param name="list">
	/// A <see cref="IList<Vector2>"/>
	/// </param>
	/// <returns>
	/// A <see cref="Vector2"/> which is the center of all points provided by parameter list
	/// </returns>
	Vector2 CalculateCenter(IList<Vector2> list)
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
	
	void Awake () {
		m_scalingFactor=1.0f;
		m_lastScalingFactor = 1.0f;
		
		//initialize uv for pupil dilation
		m_leftEyeUVIndices = new List<int>();
		m_rightEyeUVIndices = new List<int>();
		
		m_leftEyeUVs = new List<Vector2>();
		m_rightEyeUVs= new List<Vector2>();
		
		//read exported uvcoordinates from textasset
		ReadTextAsset(m_importedLeftEyeUVs,ref m_leftEyeUVs);
		ReadTextAsset(m_importedRightEyeUVs,ref m_rightEyeUVs);
		
		//calculate the center for scaling uvs around center
		m_leftEyeCenter = CalculateCenter(m_leftEyeUVs);
		m_rightEyeCenter = CalculateCenter(m_rightEyeUVs);
	
		if(m_isDefaultMakehuman)
		{
			
			SkinnedMeshRenderer [] renderers  = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach(SkinnedMeshRenderer smr in renderers)
			{
				if(smr.name.ToLower().Contains(name.ToLower()))
				{
					m_mesh = Instantiate(smr.sharedMesh) as Mesh;
					smr.sharedMesh = m_mesh;
					//send a message that we changed the mesh. Is necessary for morpher
					smr.gameObject.SendMessage("MeshUpdated",SendMessageOptions.DontRequireReceiver);
					break;
				}
			}
		
			
		}
		m_allOrgUVS=m_mesh.uv;
		
		
		//find the left and right uvs
		FindUVIndexes(m_allOrgUVS, m_leftEyeUVs,ref m_leftEyeUVIndices);
		FindUVIndexes(m_allOrgUVS, m_rightEyeUVs, ref m_rightEyeUVIndices);
		
	}
	
	
	
	/// <summary>
	/// The internal function to scale the pupils.
	/// </summary>
	/// <param name='factor'>
	/// Factor.
	/// </param>
	/// <param name='manipUVs'>
	/// Manip U vs.
	/// </param>
	/// <param name='indices'>
	/// Indices.
	/// </param>
	/// <param name='center'>
	/// Center.
	/// </param>
	void ScalePupils(float factor,ref Vector2[] manipUVs, ref IList<int> indices, ref Vector2 center)
	{
		//scale between 0.5f and 1.5f
		float scaleFactor = Mathf.Max(0.5f,Mathf.Min(1.5f, factor));
				
		for(int i = 0; i <indices.Count;++i)
		{
			Vector2 v2 = manipUVs[indices[i]]-center;
			Vector2 v2scaled = Vector2.Scale(v2, new Vector2(scaleFactor,scaleFactor));
			manipUVs[indices[i]]= v2scaled+center;
		}	
		m_mesh.uv = manipUVs;
		
	}
	
	void Update()
	{
		if(m_lastScalingFactor != m_scalingFactor)
		{
			m_lastScalingFactor = m_scalingFactor;
			ScaleBothPupils(m_scalingFactor);
			
		}
	}
	
}


