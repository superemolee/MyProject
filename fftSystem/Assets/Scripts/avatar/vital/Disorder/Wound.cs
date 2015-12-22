using System;
using UnityEngine;
using System.Collections.Generic;
[System.Serializable]

/// <summary>
/// This class represents all wounds on the body. Each wound has to setup on the avatar in a specific region
/// </summary>
public class Wound : Disorder<float>
{
	/// <summary>
	/// Wound size.
	/// </summary>
	public enum WoundSize{
		none,
		normal,
		big
	}
	
	[System.Serializable]
	/// <summary>
	/// A class to hold the values for Wounds in a specific region.
	/// </summary>
	public class WoundKind
	{
		/// <summary>
		/// The size of the wound.
		/// </summary>
		public WoundSize woundSize;
		/// <summary>
		/// The color of bodyregion from the Template.
		/// </summary>
		public Color32 colorOfRegion;
		
		/// <summary>
		/// The blood loss per minute in ml.
		/// </summary>
		public float bloodLossPerMinute;
	};
	
	/// <summary>
	/// The wound texture template. Will be used if vitalmodell wants to create the texture during runtime
	/// </summary>
	public Texture2D m_woundTextureTemplate;
	
	/// <summary>
	/// Assume default values for bloodloss.
	/// </summary>
	public bool m_assumeDefaultValues = false;
	
	/// <summary>
	/// The wound on the left upper arm.
	/// </summary>
	public WoundKind m_lUpperArm;
	
	/// <summary>
	/// The wound on the left lower arm.
	/// </summary>
	public WoundKind m_lLowerArm;
	/// <summary>
	/// The wound on the right upper arm.
	/// </summary>
	public WoundKind m_rUpperArm;
	
	/// <summary>
	/// The wound on the right lower arm.
	/// </summary>	
	public WoundKind m_rLowerArm;

	/// <summary>
	/// The wound on the head.
	/// </summary>
	public WoundKind m_head;
	
	/// <summary>
	/// The wound on the left thigh.
	/// </summary>
	public WoundKind m_lThigh;
	
	/// <summary>
	/// The wound on the left shin.
	/// </summary>
	public WoundKind m_lShin;
	
	/// <summary>
	/// The wound on the right thigh.
	/// </summary>
	public WoundKind m_rThigh;
	
	/// <summary>
	/// The wound on the right shin.
	/// </summary>
	public WoundKind m_rShin;

	// Uncomment the following lines to activate this if more wounds are necessary	
//	public WoundKind m_lHand;
//	public WoundKind m_rHand;	
//	public WoundKind m_lowerFrontBody;
//	public WoundKind m_upperFrontBody;
//	public WoundKind m_lowerBackBody;
//	public WoundKind m_upperBackBody;
	
	
	/// <summary>
	/// The internal list that holds all wounds. This is used for internal calculation
	/// </summary>
	protected List<WoundKind> m_wounds = new List<WoundKind>();
	

	/// <summary>
	/// Gets the total blood loss of all wounds.
	/// </summary>
	/// <value>
	/// The total blood loss.
	/// </value>
	public float TotalBloodLoss{
		get{ 
			float totalBloodlossPerMinute =0.0f;
			foreach(WoundKind wk in m_wounds)
			{
				if(wk.woundSize != WoundSize.none)
				{
						totalBloodlossPerMinute += wk.bloodLossPerMinute;
				}
					
			}
			return  totalBloodlossPerMinute;
		}
	}
	
	/// <summary>
	/// Initializes the disorder.
	/// </summary>
	/// <param name='avatar'>
	/// Avatar.
	/// </param>
	public override void initializeDisorder(GameObject avatar)
	{
		base.initializeDisorder(avatar);
		DisorderValue = -1.0f;

		// bodyregions are on the avatar
		if(m_bodyRegions)
		{
			m_lUpperArm.colorOfRegion = m_bodyRegions.lUpperArm;
			m_lLowerArm.colorOfRegion = m_bodyRegions.lLowerArm;
			m_rUpperArm.colorOfRegion = m_bodyRegions.rUpperArm;
			m_rLowerArm.colorOfRegion = m_bodyRegions.rLowerArm;
			m_head.colorOfRegion = m_bodyRegions.head;
			m_lThigh.colorOfRegion = m_bodyRegions.lThigh;
			m_lShin.colorOfRegion = m_bodyRegions.lShin;
			m_rShin.colorOfRegion = m_bodyRegions.rShin;
			m_rThigh.colorOfRegion = m_bodyRegions.rThigh;
		}
		
		if(m_assumeDefaultValues)
		{		
			m_lUpperArm.bloodLossPerMinute=15.0f;
			m_lLowerArm.bloodLossPerMinute=10.0f;
			m_rUpperArm.bloodLossPerMinute=15.0f;
			m_rLowerArm.bloodLossPerMinute=10.0f;
			m_head.bloodLossPerMinute=10.0f;
			m_lThigh.bloodLossPerMinute=20.0f;
			m_lShin.bloodLossPerMinute=10.0f;
			m_rThigh.bloodLossPerMinute=20.0f;
			m_rShin.bloodLossPerMinute=10.0f;
		}
		
		//add all wounds to the internal list
		m_wounds.Add(m_lUpperArm);
		m_wounds.Add(m_lLowerArm);
		m_wounds.Add(m_rUpperArm);
		m_wounds.Add(m_rLowerArm);
		m_wounds.Add(m_head);
		m_wounds.Add(m_lThigh);
		m_wounds.Add(m_lShin);
		m_wounds.Add(m_rThigh);
		m_wounds.Add(m_rShin);
	}
	
	/// <summary>
	/// Gets the wound regions. Calculates every index of the wounds 
	/// </summary>
	/// <returns>
	/// The wound regions.
	/// </returns>
	public int[] GetWoundRegions()
	{
		List<Color32> bodyRegionColors = new List<Color32>();
		//create texture for wounds
		foreach(WoundKind wk in m_wounds)
		{
			if(wk.woundSize != WoundSize.none)
			{
					bodyRegionColors.Add(wk.colorOfRegion);
			}
				
		}
		//calculate bloodloss perMinute of hole body
		if(bodyRegionColors.Count == 0)
		{
			return null;
		}
		int[] woundbodyRegions = m_bodyRegions.GetCombinedRegions(bodyRegionColors.ToArray());
		return woundbodyRegions;
	}
	

	
	/// <summary>
	/// Sets the blood loss of a region. Is used for setup a tourniquet
	/// </summary>
	/// <param name='region'>
	/// Region.
	/// </param>
	public void SetBloodLossOfRegion(Color32 region)
	{
		
		foreach(WoundKind wk in m_wounds)
		{
			if(wk.woundSize != WoundSize.none)
			{
				if(region.r == wk.colorOfRegion.r)
				{
					wk.bloodLossPerMinute*=0.1f;
					break;
				}
			}
				
		}
	}
	
	public override void DOUpdate()
	{
		//nothing to do here		
	}

}
