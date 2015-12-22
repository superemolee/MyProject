using UnityEngine;
using System.Collections;
		
[System.Serializable]
/// <summary>
/// The base class for all vital parameters. it holds init functions and base information about the avatar
/// </summary>
public abstract class VitalPara
{
	/// <summary>
	/// The avatar.
	/// </summary>
	protected GameObject m_avatar;
	/// <summary>
	/// An enum of Gender (m,f).
	/// </summary>
	public enum Gender
	{
		male,
		female
	}
	/// <summary>
	/// The age of the avatar.
	/// </summary>
	protected int m_age;
	
	/// <summary>
	/// The gender of the avatar.
	/// </summary>
	protected Gender m_gender;
	
	/// <summary>
	/// The mass of the avatar.
	/// </summary>
	protected float m_mass;
	
	/// <summary>
	/// The update function of the vital parameter.
	/// </summary>
	/// <returns>
	/// true if the update successes, false otherwise.
	/// </returns>
	public abstract bool VPUpdate ();
		
	/// <summary>
	/// Initializes the vital para.
	/// </summary>
	/// <returns>
	/// True if the vital parameter could initialize successfully.
	/// </returns>
	/// <param name='target'>
	/// the target gameObject, typically the avatar.
	/// </param>
	/// <param name='age'>
	/// The age of the avatar.
	/// </param>
	/// <param name='mass'>
	/// The mass of the avatar.
	/// </param>
	/// <param name='gender'>
	/// The gender of the avatar.
	/// </param>
	public virtual bool InitializeVitalPara (GameObject target, int age, float mass, Gender gender)
	{
		m_avatar = target;
		m_age = age;
		m_gender = gender;
		m_mass = mass;
		return true;
	}
}
	
[System.Serializable]
/// <summary>
/// The generic abstract class of the Vital para.
/// </summary>
public abstract class VitalPara<T> : VitalPara
{
	/// <summary>
	/// The minimum vital para.
	/// </summary>
	public T MINVITALPARA;
	/// <summary>
	/// The maximum vital para.
	/// </summary>
	public T MAXVITALPARA;
	/// <summary>
	/// The initial vital para.
	/// </summary>
	public T initialVitalPara;
	/// <summary>
	/// The actual vital para.
	/// </summary>
	public T vitalPara;
	
	/// <summary>
	/// Gets or sets the vital para value.
	/// </summary>
	/// <value>
	/// The vital para value.
	/// </value>
	public virtual T VitalParaVal {
		get { return vitalPara; }
		set { vitalPara = value; }
	}
			
		
		
}
	
