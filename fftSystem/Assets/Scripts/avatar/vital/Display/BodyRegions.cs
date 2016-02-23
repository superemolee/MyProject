using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class represents the Body regions of the Avatar. They are provided through colors and a template. A base template for 
/// makehuman is provided.
/// </summary>
[System.Serializable]
[AddComponentMenu("Avatar/Display/BodyRegions")]
public class BodyRegions : MonoBehaviour
{
	/// <summary>
	/// Indicator if this avatar default makehuman. If yes it overrides the selected colors for the regions
	/// </summary>
	public bool m_isDefaultMakehuman = true;
	
	/// <summary>
	/// The body region template which contains the corresponding colors.
	/// </summary>
	public Texture2D BodyRegionTemplate;
	
	
	/// <summary>
	/// The indices of the regions in the texture. This is based on the fact, that unity uses an array of height*width to represent the pixels.
	/// </summary>
	Dictionary<Color32, List<int> > m_colorIdx = new  Dictionary<Color32, List<int>>();
		
	/// <summary>
	/// The left upper arm.
	/// </summary>
	public Color32 lUpperArm;
	
	/// <summary>
	/// The left lower arm.
	/// </summary>
	public Color32 lLowerArm;
	
	/// <summary>
	/// The left hand.
	/// </summary>
	public Color32 lHand;
	
	/// <summary>
	/// The right upper arm.
	/// </summary>
	public Color32 rUpperArm;
	
	/// <summary>
	/// The right lower arm.
	/// </summary>
	public Color32 rLowerArm;
	
	/// <summary>
	/// The right hand.
	/// </summary>
	public Color32 rHand;
	
	/// <summary>
	/// The lower front body.
	/// </summary>
	public Color32 lowerFrontBody;
	
	/// <summary>
	/// The upper front body.
	/// </summary>
	public Color32 upperFrontBody;
	
	/// <summary>
	/// The lower back body.
	/// </summary>
	public Color32 lowerBackBody;
	
	/// <summary>
	/// The upper back body.
	/// </summary>
	public Color32 upperBackBody;
	
	/// <summary>
	/// The neck.
	/// </summary>
	public Color32 neck;
	
	/// <summary>
	/// The head.
	/// </summary>
	public Color32 head;
	
	/// <summary>
	/// The left thigh.
	/// </summary>
	public Color32 lThigh;
	
	/// <summary>
	/// The left shin.
	/// </summary>
	public Color32 lShin;
	
	/// <summary>
	/// The right thigh.
	/// </summary>
	public Color32 rThigh;
	
	/// <summary>
	/// The right shin.
	/// </summary>
	public Color32 rShin;
	
	/// <summary>
	/// The nailbed of both hands.
	/// </summary>
	public Color32 nailbed;
	
	/// <summary>
	/// The lips.
	/// </summary>
	public Color32 lips;
	
	

	void Awake()
	{
		// initialize in awake method to provide the regions in start for the vital functions
		
		
		//use default values if makehuman avatar
		if(m_isDefaultMakehuman)
		{
			lUpperArm = new Color32(0x7f,0,0,0);
			lLowerArm = new Color32(0x8f,0,0,0);
			lHand = new Color32(0x9f,0,0,0);
				
			rUpperArm = new Color32(0x5f,0,0,0);
			rLowerArm = new Color32(0x6f,0,0,0);
			rHand = new Color32(0x4f,0,0,0);

			lowerFrontBody = new Color32(0xbf,0,0,0);
			upperFrontBody = new Color32(0xcf,0,0,0);

			lowerBackBody = new Color32(0xdf,0,0,0);
			upperBackBody = new Color32(0xef,0,0,0);
			
			
			neck = new Color32(0xb2,0,0,0);
			head = new Color32(0xaf,0,0,0);
			
			lThigh = new Color32(0x1f,0,0,0);
			lShin = new Color32(0x0f,0,0,0);
			
			rShin = new Color32(0x3f,0,0,0);
			rThigh = new Color32(0x2f,0,0,0);
			
			lips = new Color32(0x0,0x10, 0 ,0);
			nailbed = new Color32(0x0, 0x20, 20 ,0);
			

		}
		
		//if body template is provided
		if(BodyRegionTemplate!=null)
		{
			Color32 [] pixels = BodyRegionTemplate.GetPixels32();
			Color32 [] regions = {
				lUpperArm,
				lLowerArm,
				lHand,
				rUpperArm,
				rLowerArm,
				rHand,
				lowerFrontBody,
				upperFrontBody,
				lowerBackBody,
				upperBackBody,
				neck,
				head,			
				lThigh,
				lShin,
				rShin,
				rThigh,
				lips,
				nailbed
				
			};
			
			for(int j = 0; j < regions.Length;++j)
			{
				m_colorIdx.Add(regions[j], new List<int>());
			}
//			Debug.Log("time: " + Time.timeSinceLevelLoad);
			//fill internal array for each region to get the region of interest
			for(int i=0; i< pixels.Length;++i)
			{
				Color32 pixel = pixels[i];
	
				//if pixel alpha is empty continue
				if(pixel.a== 0)
				{
					continue;
				}
				for(int j = 0; j < regions.Length;++j)
				{
					byte red = regions[j].r;
					byte green = regions[j].g;
					bool found = false;
					//seems like unity3d has an import bug. pixels are somehow changed
					if ( (pixel.r)>=(red-3)&&(pixel.r) <=(red+3))  
					{
	
						m_colorIdx[regions[j]].Add(i);
						found = true;
					}
					// check green channels for additional stuff
					if(pixel.g != 0 && (pixel.g)>=(green-3)&&(pixel.g) <=(green+3)) 
					{
						m_colorIdx[regions[j]].Add(i);
						found = true;
					}
					if(found)
					{
						break;
					}
				}
			}
			Debug.Log("time: " + Time.timeSinceLevelLoad);
		}
	}
	
	
	/// <summary>
	/// Gets the combined regions.
	/// </summary>
	/// <returns>
	/// The combined regions.
	/// </returns>
	/// <param name='colors'>
	/// Colors.
	/// </param>
	public  int [] GetCombinedRegions(Color32 [] colors)
	{
		List<int> idx = new List<int>();
		for(int i = 0; i < colors.Length;++i)
		{
			if(m_colorIdx.ContainsKey(colors[i]))
			{
				idx.AddRange(m_colorIdx[colors[i]]);
			}
		}
		return idx.ToArray();
	}
	
	
}

