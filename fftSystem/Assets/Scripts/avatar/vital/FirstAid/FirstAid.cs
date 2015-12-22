using System;
using UnityEngine;

/// <summary>
/// This abstract class is the base class for a first aid.
/// </summary>
[Serializable]
public abstract class FirstAid
{
	/// <summary>
	/// The avatar.
	/// </summary>
	protected GameObject m_avatar;
	
	/// <summary>
	/// Identicator if the First Aid is added.
	/// </summary>
	protected bool m_isAdded;
}
