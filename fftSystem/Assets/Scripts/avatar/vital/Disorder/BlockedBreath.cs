using UnityEngine;
using System.Collections;



/// <summary>
/// This class represents the Blocked breath.
/// </summary>
[System.Serializable]
public class BlockedBreath : Disorder<int>
{
	/// <summary>
	/// The blocked object.
	/// </summary>
	public Transform m_blockedObject;
	
	/// <summary>
	/// Indicator if the breathways are blocked.
	/// </summary>
	public bool m_isBlocked;
	
	/// <summary>
	/// Initializes the disorder.
	/// </summary>
	/// <param name='avatar'>
	/// The Avatar.
	/// </param>
	public override void initializeDisorder (GameObject avatar)
	{
		base.initializeDisorder (avatar);
		DisorderValue = m_isBlocked ? 1 : 0;
	}
	
	public override int DisorderValue {
		get {
			return m_isBlocked ? 1 : 0;
		}
		set {
			base.DisorderValue = value;
		}
	}

}
