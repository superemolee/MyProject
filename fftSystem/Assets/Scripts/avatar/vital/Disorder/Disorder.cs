using System;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
/// <summary>
/// The base class for all Disorders.
/// </summary>
public abstract class Disorder
{
	/// <summary>
	/// The avatar of the disorder.
	/// </summary>
	protected GameObject m_avatar;
	/// <summary>
	/// The body regions.
	/// </summary>
	protected BodyRegions m_bodyRegions;
	 
	/// <summary>
	/// Initializes the disorder.
	/// </summary>
	/// <param name='avatar'>
	/// The avatar with the disorders.
	/// </param>
	public virtual void initializeDisorder(GameObject avatar)
	{
		m_avatar = avatar;
		m_bodyRegions = avatar.GetComponent<BodyRegions>();
	}
	
	/// <summary>
	/// The update of the disorders.
	/// </summary>
	public virtual void DOUpdate(){
	}
	
}
[System.Serializable]
/// <summary>
/// The generic Disorder to use different disordervalues.
/// </summary>
public abstract class Disorder<T> : Disorder
{
	/// <summary>
	/// The disorder value.
	/// </summary>
	protected T m_disorderValue;
	/// <summary>
	/// The last disorder value.
	/// </summary>
	protected T m_lastDisorderValue;
	
	/// <summary>
	/// Gets or sets the disorder value.
	/// </summary>
	/// <value>
	/// The disorder value.
	/// </value>
	public virtual T DisorderValue
	{
		get{ return m_disorderValue;}
		set {
			m_lastDisorderValue = m_disorderValue;
			m_disorderValue = value;
		} 
	}
	

	
}



