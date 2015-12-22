using UnityEngine;
using System.Collections;

/// <summary>
/// This class triggers a blink animation. Could be morphtarget or bone based animation type. 
/// The class should be placed on the object that holds the morphtarget Blink or the Animation
/// </summary>
public class Blink : MonoBehaviour {
	
	/// <summary>
	/// The blink source channel. This uses morph targets provided by megafiers
	/// </summary>
	public string	m_blinkSrcChannel = "Blink";
	
	/// <summary>
	/// The blink percent of the source channel. This uses morph targets provided by megafiers
	/// </summary>
	public float	m_blinkPercent = 0.0f;
	
	/// <summary>
	/// The blinkchannel of the morphtarget blink.
	/// </summary>
	MegaMorphChan	m_blinkChannel;
	
	/// <summary>
	/// Blink animation type.
	/// </summary>
	public enum BlinkAnimationType{
		/// <summary>
		/// Constant morph target animation.
		/// </summary>
		MorphTargetAnimation,
		/// <summary>
		/// Constant bone based animation.
		/// </summary>
		BoneBasedAnimation,
	}
	
	
	
	/// <summary>
	/// The type of the blink animation.
	/// </summary>
	BlinkAnimationType m_blinkAnimType = BlinkAnimationType.BoneBasedAnimation;	
	
	
	AnimationClip m_boneBasedAnimClip;
	
	
	// Use this for initialization
	void Start () {
			
		if(m_blinkAnimType == BlinkAnimationType.BoneBasedAnimation)
		{
			if(m_boneBasedAnimClip)
			{
				if(!animation.GetClip(m_boneBasedAnimClip.name))
				{
					animation.AddClip(m_boneBasedAnimClip, m_boneBasedAnimClip.name);
					animation[m_boneBasedAnimClip.name].wrapMode = WrapMode.Loop;
					animation[m_boneBasedAnimClip.name].layer = 6;
					
				}
			}
		}
		else{
			
			MegaMorph mr = GetComponentInChildren<MegaMorph>();
	
			if ( mr != null )
			{
				m_blinkChannel = mr.GetChannel(m_blinkSrcChannel);
				AnimationCurve ac = new AnimationCurve();
				ac.AddKey(0.0f,0.0f);
				ac.AddKey(0.175f,100.0f);
				ac.AddKey(0.35f, 0.0f);
				ac.AddKey(4.35f, 0.0f);
				
				AnimationClip aniClip = new AnimationClip();
				aniClip.SetCurve("",typeof(Blink),"m_blinkPercent",ac);
				aniClip.wrapMode= WrapMode.Loop;
				
				
				animation.AddClip(aniClip, "blink");
				animation["blink"].layer = 6;
				animation.CrossFade("blink");
				
				
			}
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if(m_blinkAnimType == BlinkAnimationType.MorphTargetAnimation)
		{
			if (m_blinkChannel!= null )  m_blinkChannel.Percent =  m_blinkPercent;
		}
	}
}
