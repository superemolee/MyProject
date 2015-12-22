using System;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]

/// <summary>
/// This class represents a Fracture. All extrimities and the hip can have a fracture of a different type.
/// </summary>
public class Fracture : Disorder<float>
{
	/// <summary>
	/// Fracture Type.
	/// </summary>
	public enum FractureType { 
		
		None = 0, //no fracture	
		NormalFracture = 1, //simple fracture closed
		MalPositionFracture = 2, // a fracture with a malposition
		CompoundFracture = 3 //complex fracture open for future implementation
	};
	
	/// <summary>
	/// Fracture kind.
	/// </summary>
	[System.Serializable]
	public class FractureKind
	{
		/// <summary>
		/// The type of the fracture.
		/// </summary>
		public FractureType fractureType;
		/// <summary>
		/// the morphtarget for the malposition.
		/// </summary>
		public MegaMorphChan regionMorph;
		
		/// <summary>
		/// The Color of the specific region.
		/// </summary>
		public Color32 colorOfRegion;
		
		/// <summary>
		/// The blood loss per minute in ml. 0 means no arteria or veins is damaged
		/// </summary>
		public float bloodLossPerMinute;
	};
	
	/// <summary>
	/// The haematom texture template.
	/// </summary>
	public Texture2D m_haematomTextureTemplate;
	
	/// <summary>
	/// The fracture on the left upper arm.
	/// </summary>
	public FractureKind m_lUpperArm;
	
	/// <summary>
	/// The fracture on the left lower arm.
	/// </summary>
	public FractureKind m_lLowerArm;
	
	/// <summary>
	/// The fracture on the right upper arm.
	/// </summary>
	public FractureKind m_rUpperArm;
	
	/// <summary>
	/// The fracture on the right lower arm.
	/// </summary>
	public FractureKind m_rLowerArm;
	
	/// <summary>
	/// The fracture on the pelvis.
	/// </summary>
	public FractureKind m_pelvis;

	/// <summary>
	/// The fracture on the left thigh.
	/// </summary>
	public FractureKind m_lThigh;
	
	/// <summary>
	/// The fracture on the left shin.
	/// </summary>
	public FractureKind m_lShin;
	
	/// <summary>
	/// The fracture on the right thigh.
	/// </summary>
	public FractureKind m_rThigh;
	
	/// <summary>
	/// The fracture on the right shin.
	/// </summary>
	public FractureKind m_rShin;
	
	
	//for future implementation of compound fractures
	//public Texture2D m_woundTexture;
	//public Transform m_boneObject;
	
	/// <summary>
	/// The Morphtarget pain.
	/// </summary>
	protected MegaMorphChan m_pain;
	
	/// <summary>
	/// The internal list of all fractures.
	/// </summary>
	protected List<FractureKind> m_fractures = new List<FractureKind>();

	

	/// <summary>
	/// Initializes the disorder.
	/// </summary>
	/// <param name='avatar'>
	/// The Avatar.
	/// </param>
	public override void initializeDisorder (GameObject avatar)
	{
		base.initializeDisorder (avatar);
		m_fractures.Add(m_lUpperArm);
		m_fractures.Add(m_rUpperArm);
		m_fractures.Add(m_lLowerArm);
		m_fractures.Add(m_rLowerArm);
		m_fractures.Add(m_pelvis);
		m_fractures.Add(m_lThigh);
		m_fractures.Add(m_lShin);
		m_fractures.Add(m_rThigh);
		m_fractures.Add(m_rShin);
		DisorderValue = -1.0f;
		m_lastDisorderValue = 0.0f;
		
		MegaMorph mm = avatar.gameObject.GetComponentInChildren<MegaMorph>();
		if(mm!= null)
		{
			m_pain = mm.GetChannel("pain");
			//seems there is a mirror problem during import of the character. so switched the sides
			m_lUpperArm.regionMorph = mm.GetChannel("lUpArmBroke");
			m_lLowerArm.regionMorph = mm.GetChannel("lLowArmBroke");
			m_rUpperArm.regionMorph = mm.GetChannel("rUpArmBroke");
			m_rLowerArm.regionMorph = mm.GetChannel("rLowArmBroke");
			m_lThigh.regionMorph = mm.GetChannel("lUpLegBroke");
			m_lShin.regionMorph = mm.GetChannel("lLowLegBroke"); 
			m_rThigh.regionMorph = mm.GetChannel("rUpLegBroke");
			m_rShin.regionMorph = mm.GetChannel("rLowLegBroke");
			
		}
		
		if(m_bodyRegions!= null)
		{
			m_lUpperArm.colorOfRegion = m_bodyRegions.lUpperArm;
			m_lLowerArm.colorOfRegion = m_bodyRegions.lLowerArm;
			m_rUpperArm.colorOfRegion = m_bodyRegions.rUpperArm;
			m_rLowerArm.colorOfRegion = m_bodyRegions.rLowerArm;
			m_lThigh.colorOfRegion = m_bodyRegions.lThigh;
			m_lShin.colorOfRegion = m_bodyRegions.lShin;
			m_rThigh.colorOfRegion = m_bodyRegions.rThigh;
			m_rShin.colorOfRegion = m_bodyRegions.rShin;
			m_pelvis.colorOfRegion = m_bodyRegions.lowerFrontBody;
		}
		
		
		//lUpperArm.regionMorph.SetTarget(100.0f,1.0f);
		List<Color32> haematoms = new List<Color32>();
		float painValue = 0.0f;
		for(int i=0; i < m_fractures.Count;++i)
		{
			FractureType ft = m_fractures[i].fractureType;
			switch(ft)
			{
			case FractureType.None:
				//nothing todo
				continue;
				
			case FractureType.NormalFracture:
				//haematom on Region
				haematoms.Add(m_fractures[i].colorOfRegion);
				//set pain target to higher level
				//set to 25 if no higher level set
				painValue = Mathf.Max(25.0f, painValue);
				break;
			case FractureType.MalPositionFracture:
				//haematom on Region
				haematoms.Add(m_fractures[i].colorOfRegion);
				//set malposition morph
				m_fractures[i].regionMorph.SetTarget(100.0f,100.0f);
				painValue = Mathf.Max(75.0f, painValue);
				
				break;
			case FractureType.CompoundFracture:
				//tbd
				break;
			default:
				//nothing todo
				break;
			}
		}
		if(m_pain!= null)
		{
			m_pain.SetTarget(painValue,100.0f);
		}
		
		
	}
	
	/// <summary>
	/// Gets the hematom regions of interest.
	/// </summary>
	/// <returns>
	/// The hematom regions.
	/// </returns>
	public int[] GetHematomRegions()
	{
		//lUpperArm.regionMorph.SetTarget(100.0f,1.0f);
		List<Color32> haematoms = new List<Color32>();
		for(int i=0; i < m_fractures.Count;++i)
		{
			FractureType ft = m_fractures[i].fractureType;
			switch(ft)
			{
			case FractureType.None:
				//nothing todo
				continue;
			case FractureType.NormalFracture:
				//haematom on Region
				haematoms.Add(m_fractures[i].colorOfRegion);
				break;
			case FractureType.MalPositionFracture:
				//haematom on Region
				haematoms.Add(m_fractures[i].colorOfRegion);
				break;
			default:
				//nothing todo
				break;
			}
		}
		if(haematoms.Count== 0)
		{
			return null;
		}
		int [] combinedRegions = m_bodyRegions.GetCombinedRegions(haematoms.ToArray());
		return combinedRegions;
	}	
	
	/// <summary>
	/// Gets the total blood loss caused by the fractures that were setup.
	/// </summary>
	/// <value>
	/// The total blood loss.
	/// </value>
	public float TotalBloodLoss{
		get{ 
			float totalBloodlossPerMinute =0.0f;
			foreach(FractureKind fk in m_fractures)
			{
				if(fk.fractureType != FractureType.None)
				{
					totalBloodlossPerMinute += fk.bloodLossPerMinute;
				}
					
			}
			return  totalBloodlossPerMinute;
		}
	}
	
	/// <summary>
	/// The disorder update.
	/// </summary>
	public override void DOUpdate ()
	{
		base.DOUpdate ();
		if(DisorderValue != m_lastDisorderValue)
		{
			
		}
		
	}
	
	
	
	
	
}

