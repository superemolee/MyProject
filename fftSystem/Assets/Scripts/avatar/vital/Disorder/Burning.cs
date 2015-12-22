using System;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
/// <summary>
/// This class represents the Disorder Burning. It calculates the influence on the different burning degrees. 
/// All burnings have to setup in this class.
/// </summary>
public class Burning : Disorder<float>
{
	/// <summary>
	/// The severity degree. It indicates that burnings of different degrees are weighted 
	/// </summary>
	public Vector3 severityDegree = new Vector3 (0.2f, 0.7f, 1.0f);
	
	/// <summary>
	/// The maximum burning area which will used for calculation
	/// </summary>
	public float maxBurningArea = 35.0f;
			
	/// <summary>
	/// Burning degree.
	/// </summary>
	[System.Serializable]
	public enum BurningDegree
	{
		none,
		degree1,
		degree2,
		degree3
	};
		
	/// <summary>
	/// The burning Body region.
	/// </summary>
	[System.SerializableAttribute]
	public class BurningBodyRegion
	{
			
		/// <summary>
		/// The burning degree: none, 1,2,3
		/// </summary>
		public BurningDegree burningDegree;
		/// <summary>
		/// The color of region from the Template.
		/// </summary>
		public Color32 colorOfRegion;
		/// <summary>
		/// The rules of nines from Wallace.
		/// </summary>
		public float rulesOfNines;
	}
	
	/// <summary>
	/// The left upper arm.
	/// </summary>
	public BurningBodyRegion lUpperArm;
	
	/// <summary>
	/// The left lower arm.
	/// </summary>
	public BurningBodyRegion lLowerArm;
	
	/// <summary>
	/// The left hand.
	/// </summary>
	public BurningBodyRegion lHand;
	
	/// <summary>
	/// The right upper arm.
	/// </summary>
	public BurningBodyRegion rUpperArm;
	
	/// <summary>
	/// The right lower arm.
	/// </summary>
	public BurningBodyRegion rLowerArm;
	
	/// <summary>
	/// The right hand.
	/// </summary>
	public BurningBodyRegion rHand;
	
	/// <summary>
	/// The lower front body.
	/// </summary>
	public BurningBodyRegion lowerFrontBody;
	
	/// <summary>
	/// The upper front body.
	/// </summary>
	public BurningBodyRegion upperFrontBody;
	
	/// <summary>
	/// The lower back body.
	/// </summary>
	public BurningBodyRegion lowerBackBody;
	
	/// <summary>
	/// The upper back body.
	/// </summary>
	public BurningBodyRegion upperBackBody;
	
	/// <summary>
	/// The head.
	/// </summary>
	public BurningBodyRegion head;
	
	/// <summary>
	/// The neck.
	/// </summary>
	public BurningBodyRegion neck;
	
	/// <summary>
	/// The left thigh.
	/// </summary>
	public BurningBodyRegion lThigh;
	
	/// <summary>
	/// The left shin.
	/// </summary>
	public BurningBodyRegion lShin;
	
	/// <summary>
	/// The right thight.
	/// </summary>
	public BurningBodyRegion rThigh;
	
	/// <summary>
	/// The right shin.
	/// </summary>
	public BurningBodyRegion rShin;
		
	/// <summary>
	/// The over all burning. Overrides all Burnings with a burning of the degree. 
	/// </summary>
	public BurningDegree overAllBurning;
	
	/// <summary>
	/// The burning 1st degree offset.
	/// </summary>
	public float burning1stDegreeOffset = 0.1f;
	
	/// <summary>
	/// The burning 2nd degree texture.
	/// </summary>
	public Texture2D burning2ndDegreeTex;
	
	/// <summary>
	/// The burning 3rd degree texture.
	/// </summary>
	public Texture2D burning3rdDegreeTex;
	
	
	/// <summary>
	/// The internal list of all burnings.
	/// </summary>
	private List<BurningBodyRegion> m_burnings = new List<BurningBodyRegion>();
	
		
		
	/// <summary>
	/// Gets the indices of the burning regions.
	/// </summary>
	/// <returns>
	/// The burning regions.
	/// </returns>
	/// <param name='degree'>
	/// Degree.
	/// </param>
	public int[] GetBurningRegions(BurningDegree degree)
	{
		//Avatar.Util.TextureUtilities.DebugTextureColors(bodyRegions);
		List<Color32> regionColors = new List<Color32> ();
			
		foreach (BurningBodyRegion br in m_burnings) {
				
			switch (br.burningDegree) {
			case BurningDegree.none:
					//nothing to do
				break;
			case BurningDegree.degree3:
				if(degree == BurningDegree.degree3)
				{
					regionColors.Add (br.colorOfRegion);
				}
				break;
			case BurningDegree.degree2:
				if(degree == BurningDegree.degree2 || degree == BurningDegree.degree1)
				{
					regionColors.Add (br.colorOfRegion);
				}
				
				break;
			case BurningDegree.degree1:
				if(degree == BurningDegree.degree1)
				{
					regionColors.Add (br.colorOfRegion);
				}
				break;
			default:
					//nothing to do
				break;
			}
				
		}
		if(regionColors.Count == 0)
		{
			return null;
		}
		
		int[] regionsDegree = m_bodyRegions.GetCombinedRegions (regionColors.ToArray ());
		return regionsDegree;	
	}
	
	/// <summary>
	/// Initializes the disorder.
	/// </summary>
	/// <param name='avatar'>
	/// the avatar with the disorder.
	/// </param>
	public override void initializeDisorder (UnityEngine.GameObject avatar)
	{
		base.initializeDisorder (avatar);
			
		// we have a bodyregion
		if (m_bodyRegions) {
			lUpperArm.colorOfRegion = m_bodyRegions.lUpperArm;
			lLowerArm.colorOfRegion = m_bodyRegions.lLowerArm;
			lHand.colorOfRegion = m_bodyRegions.lHand;
			rUpperArm.colorOfRegion = m_bodyRegions.rUpperArm;
			rLowerArm.colorOfRegion = m_bodyRegions.rLowerArm;
			rHand.colorOfRegion = m_bodyRegions.rHand;		
			lowerFrontBody.colorOfRegion = m_bodyRegions.lowerFrontBody;						
			upperFrontBody.colorOfRegion = m_bodyRegions.upperFrontBody;	
			lowerBackBody.colorOfRegion = m_bodyRegions.lowerBackBody;	
			upperBackBody.colorOfRegion = m_bodyRegions.upperBackBody;
			head.colorOfRegion = m_bodyRegions.head;
			lThigh.colorOfRegion = m_bodyRegions.lThigh;
			lShin.colorOfRegion = m_bodyRegions.lShin;
			rShin.colorOfRegion = m_bodyRegions.rShin;
			rThigh.colorOfRegion = m_bodyRegions.rThigh;
		}
		
				
		//setup rules of nines	
		lUpperArm.rulesOfNines = 4f;
		lLowerArm.rulesOfNines = 4f;
		lHand.rulesOfNines = 1f;
		rUpperArm.rulesOfNines = 4f;
		rLowerArm.rulesOfNines = 4f;
		rHand.rulesOfNines = 1f;
		lowerFrontBody.rulesOfNines = 9f;				
		upperFrontBody.rulesOfNines = 9f;
		lowerBackBody.rulesOfNines = 9f;
		upperBackBody.rulesOfNines = 9f;
		head.rulesOfNines = 1f;
		lThigh.rulesOfNines = 9f;
		lShin.rulesOfNines = 9f;
		rShin.rulesOfNines = 9f;
		rThigh.rulesOfNines = 9f;
			
		//burning regions in one array
		m_burnings.Add (lUpperArm);
		m_burnings.Add (lLowerArm);
		m_burnings.Add (lHand);
		m_burnings.Add (rUpperArm);
		m_burnings.Add (rLowerArm);
		m_burnings.Add (rHand);
		m_burnings.Add (head);
		m_burnings.Add (lowerFrontBody);
		m_burnings.Add (upperFrontBody);
		m_burnings.Add (lowerBackBody);
		m_burnings.Add (upperBackBody);
		m_burnings.Add (lThigh);
		m_burnings.Add (lShin);
		m_burnings.Add (rThigh);
		m_burnings.Add (rShin);
			
			
		if (overAllBurning != Burning.BurningDegree.none) {
			for (int i=0; i< m_burnings.Count; ++i) {
				m_burnings [i].burningDegree = overAllBurning;
			}
		}
			
				
			
	}
		
	
	/// <summary>
	/// Gets or sets the disorder value. It's the dot of the area and severityDegree.
	/// </summary>
	/// <value>
	/// The disorder value (bu).
	/// </value>
	public override float DisorderValue {
		get {
			Vector3 burningArea = Vector3.zero;
			foreach (BurningBodyRegion br in m_burnings) {
				switch (br.burningDegree) {
				case BurningDegree.none:
						//nothing to do
					break;
				case BurningDegree.degree3:
					burningArea.z += br.rulesOfNines;
					break;
				case BurningDegree.degree2:
					burningArea.y += br.rulesOfNines;
					break;
				case BurningDegree.degree1:
					burningArea.x += br.rulesOfNines;
					break;
				}
					
			}
				
			return Vector3.Dot (burningArea/100.0f, severityDegree);
		}
		set {
			base.DisorderValue = value;
		}
	}
	
	/// <summary>
	/// The disorder update Function.
	/// </summary>
	public override void DOUpdate ()
	{
		//nothing to do
	}

}
