using UnityEngine;
using System.Collections;


[System.Serializable]
/// <summary>
/// This class represents the vital parameter temperature.
/// </summary>
public class VPTemperature : VitalPara<float>
{
	/// <summary>
	/// The breathing air objects in the scene <seealso cref="BreathingAir"/>.
	/// </summary>
	BreathingAir [] m_breathingAirs;
	/// <summary>
	/// The breathing air colliders. They are used to check whether we are in a specifc region
	/// </summary>
	BoxCollider [] m_breathingAirColliders;
	
	/// <summary>
	/// The current transform object. Is the root object of the avatar.
	/// </summary>
	Transform m_currentPosition;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="VPTemperature"/> class.
	/// </summary>
	public VPTemperature ()
	{
		MINVITALPARA = 27.0f;
		MAXVITALPARA = 42.5f;
	}
		
	#region implemented abstract members of VitalSign[System.Single]
	/// <summary>
	/// Initializes the vital para.
	/// </summary>
	/// <returns>
	/// The vital para.
	/// </returns>
	/// <param name='target'>
	/// The avatar
	/// </param>
	/// <param name='age'>
	/// The age of the avatar
	/// </param>
	/// <param name='mass'>
	/// The mass of the avatar.
	/// </param>
	/// <param name='gender'>
	/// the gender of the avatar.
	/// </param>
	public override bool InitializeVitalPara (GameObject target, int age, float mass, Gender gender)
	{
		base.InitializeVitalPara (target, age, mass, gender);
		VitalParaVal = Random.Range (36.4f, 37.3f);
		m_currentPosition = target.transform;
		m_breathingAirs = GameObject.FindSceneObjectsOfType(typeof(BreathingAir)) as BreathingAir[];
		m_breathingAirColliders = new BoxCollider[m_breathingAirs.Length];
		for(int i=0; i < m_breathingAirs.Length;++i)
		{
			m_breathingAirColliders[i] = m_breathingAirs[i].GetComponent<BoxCollider>();
			
		}
		return true;
	}
	
	/// <summary>
	/// Provide the Surroundings the air temperature.
	/// </summary>
	/// <returns>
	/// The air temperature.
	/// </returns>
	public float SurroundingAirTemperature()
	{
		float temp = 0.0f;
		for(int i=0; i < m_breathingAirs.Length;++i)
		{
			if(m_breathingAirColliders[i].bounds.Contains(m_currentPosition.position))
			{
				temp= m_breathingAirs[i].AirTemperature;
				break;
			}
		}
		return temp;
	}
	
	/// <summary>
	/// VPs the update.
	/// </summary>
	/// <returns>
	/// The update.
	/// </returns>
	public override bool VPUpdate ()
	{
		return true;
	}
	
	#endregion
}

