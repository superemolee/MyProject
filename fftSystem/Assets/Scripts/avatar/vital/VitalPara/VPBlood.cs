using UnityEngine;

[System.Serializable]
/// <summary>
/// This class represents the vital parameter blood. It initializes the bloodamount (based on "Notfallrettung und Krankentransport") and the hemoglobin (based on th.
/// </summary>
public class VPBlood : VitalPara<float>
{
	/// <summary>
	/// The hemoglobin value of the blood.
	/// </summary>
	public float m_hemoglobin;
	
	/// <summary>
	/// The max blood loss in percent.
	/// </summary>
	public float m_maxBloodLoss = 0.5f;
	
	#region implemented abstract members of Avatar.VitalSign[System.Single]
	/// <summary>
	/// Sets the default blood amount of the avatar. Amount is randomm between 6-8% of mass. Source "Notfallrettung und qualifizierter Krankentransport". 
	/// </summary>
	public override bool InitializeVitalPara (UnityEngine.GameObject target, int age, float mass, Gender gender)
	{
		base.InitializeVitalPara (target, age, mass, gender);
			
		MAXVITALPARA = mass * Random.Range (0.06f, 0.08f) * 1000.0f;
			
		if (gender == VitalPara.Gender.male) {
			m_hemoglobin = Random.Range (8.694f, 11.178f);
		} else {
			m_hemoglobin = Random.Range (7.452f, 9.936f);
		}
			
		VitalParaVal = MAXVITALPARA;
		initialVitalPara = VitalParaVal;
		MINVITALPARA = VitalParaVal* m_maxBloodLoss;
			
		return true;
	}
	
	public override bool VPUpdate ()
	{
		return true;
	}
		
	#endregion
			
}

