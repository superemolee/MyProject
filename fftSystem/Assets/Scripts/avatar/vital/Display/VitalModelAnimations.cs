using UnityEngine;
using System.Collections;



/// <summary>
/// This class holds all necessary animations for the Vital model. For mixing Animations it is recommonded to use this class. 
/// Otherwise you should change the layer manually
/// </summary>
[AddComponentMenu("Avatar/Display/Animation")]
public class VitalModelAnimations : MonoBehaviour
{
	[System.Serializable]
	/// <summary>
	/// Enum to mix different animation types.
	/// </summary>
	public enum VitalAnimationLayer
	{
		BoneBasedAnimation,
		DefaultAnimation,
		InverseKinematicAnimation,
		FractureAnimation,
		BreathAnimation,
		SpeakAnimation,
		MimikAnimation
	}
	
	[System.Serializable]
	/// <summary>
	/// Combination of animationclip and layer.
	/// </summary>
	public class VitalAnimation
	{
		/// <summary>
		/// The layer.
		/// </summary>
		public VitalAnimationLayer layer;
		/// <summary>
		/// The clip.
		/// </summary>
		public AnimationClip clip;
	}

	/// <summary>
	/// The array of animations.
	/// </summary>
	public VitalAnimation [] m_animations;
	
	
	// Use this for initialization
	void Start ()
	{
		foreach(VitalAnimation va in m_animations)
		{
			animation.AddClip(va.clip, va.clip.name);
			animation[va.clip.name].layer = (int)va.layer;
		}
	
	}
}

