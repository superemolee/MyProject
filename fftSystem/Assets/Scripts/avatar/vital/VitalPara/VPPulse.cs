using UnityEngine;
using System.Collections;

[System.Serializable]
/// <summary>
/// This class represents the Vitalparameter pulse. It triggers the forcefeedback if controller script is connected <seealso cref="Control"/> 
/// </summary>
public class VPPulse : VitalPara<int>
{
	
		
	/// <summary>
	/// Pulse regularity.
	/// </summary>
	public enum PulseRegularity
	{
		regularis,
		irregulares
	}
		
	/// <summary>
	/// Pulse quality.
	/// </summary>
	public enum PulseQuality
	{
		good = 2500,
		bad = 500
	};
	
	/// <summary>
	/// The pulse regularity of this pulse.
	/// </summary>
	public PulseRegularity pulseRegularity;
	
	/// <summary>
	/// The pulse quality of this pulse. The values are used also for forcefeedback controlling
	/// </summary>
	public PulseQuality pulseQuality;
	
	/// <summary>
	/// The last update time.
	/// </summary>
	float m_lastUpdateTime = 0.0f;
	
	#region implemented abstract members of VitalSign[System.Int32]
	/// <summary>
	/// Initializes a new instance of the <see cref="VPPulse"/> class.
	/// </summary>
	public VPPulse ()
	{
		//default values, will be overridden by the initialize function.
		MINVITALPARA = 50;
		MAXVITALPARA = 200;
		pulseRegularity = VPPulse.PulseRegularity.regularis;
		pulseQuality = VPPulse.PulseQuality.good;
	}
	
	/// <summary>
	/// Initializes the vital parameter pulse. Values based on "Notfallrettung und qualifizierter Krankentransport"
	/// </summary>
	public override bool InitializeVitalPara (GameObject target, int age, float mass, Gender gender)
	{
		base.InitializeVitalPara (target, age, mass, gender);
		if (m_age > 2 && m_age <= 6) {
			VitalParaVal = Random.Range (75, 120);
		} else if (m_age > 6 && m_age <= 12) {
			VitalParaVal = Random.Range (75, 110);
		} else if (m_age > 13) {
			VitalParaVal = Random.Range (60, 100);
		}
		initialVitalPara = VitalParaVal;
		MAXVITALPARA = Mathf.CeilToInt (205.8f - (0.685f * age) + Random.Range (0.0f, 12.8f));
		MINVITALPARA = Mathf.CeilToInt (30 + Random.Range (0.0f, 10.0f));
		return true;
			
	}

	#endregion
	
	#region implemented abstract members of VitalSign[System.Int32]
	public override bool VPUpdate ()
	{
		m_lastUpdateTime += Time.deltaTime;
		if (pulseRegularity == PulseRegularity.regularis) {		
			float finalUpdateTime = 60.0f / VitalParaVal;
			if (m_lastUpdateTime > finalUpdateTime) {
				int tmp = (int)pulseQuality;
				
				object [] ffparams = {(float)tmp,Mathf.Min (0.2f, finalUpdateTime * 0.5f)};
				m_avatar.SendMessage ("TriggerForceFeedback", ffparams, SendMessageOptions.DontRequireReceiver);
				m_lastUpdateTime = 0.0f;
			}
		} else if (pulseRegularity == PulseRegularity.irregulares) {
			if (m_lastUpdateTime > (60.0f / VitalParaVal)) {
				m_avatar.SendMessage ("TriggerPulse", 500.0f, SendMessageOptions.DontRequireReceiver);
					
				VitalParaVal += Random.Range (-2, 2);
				m_lastUpdateTime = 0.0f;
			}
		}
			
		return true;
	}
	#endregion
}
	
