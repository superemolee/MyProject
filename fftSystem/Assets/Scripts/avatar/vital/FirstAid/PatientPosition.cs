using System;
using UnityEngine;

/// <summary>
/// this class activates the Patient position.
/// </summary>
[Serializable]
public class PatientPosition : FirstAid
{
	/// <summary>
	/// Indicator of the Patient position first aid (recovery or shock position).
	/// </summary>
	[Serializable]
	public enum PatientPositionType {
		RecoveryPosition,
		ShockPosition,
		OverstretchHead
	};
	
	/// <summary>
	/// The type of the patient position.
	/// </summary>
	public PatientPositionType m_patientPositionType;
	
	/// <summary>
	/// The patient position animation clip.
	/// </summary>
	public AnimationClip m_patientPositionAnimationClip;	
	
}

