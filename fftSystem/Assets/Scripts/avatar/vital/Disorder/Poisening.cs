using System;
using UnityEngine;



/// <summary>
/// This class represents the Disorder Poisening, which indicates if the avatar is in a region of carbonmonoxid
/// </summary>
[System.Serializable]
public class Poisening : Disorder<float>
{
	/// <summary>
	/// The head bone is used as reference to indicate in which breathingAir-Area we are. If no head bone will be found, the root bone will be used.
	/// </summary>
	public Transform m_head;
	
	/// <summary>
	/// The breathing air regions in the scene.
	/// </summary>
	BreathingAir [] m_breathingAirs;
	/// <summary>
	/// The colliders of the breathing air regions in the scene.
	/// </summary>
	BoxCollider [] m_breathingAirColliders;
	
	/// <summary>
	/// The actual carbon monoxid in the air.
	/// </summary>
	double m_carbonMonoxid=0.0;
	
	
	/// <summary>
	/// Initializes the disorder.
	/// </summary>
	/// <param name='avatar'>
	/// The avatar with the disorder.
	/// </param>
	public override void initializeDisorder (UnityEngine.GameObject avatar)
	{
		base.initializeDisorder (avatar);
		
		// search head in the bone hierarchy because this is nearly the position of the nose
		Transform [] alltrans = avatar.GetComponentsInChildren<Transform>();
		
		
		//if bone not provided search for head bone
		if(m_head == null)
		{
			foreach(Transform t in alltrans)
			{
				if(t.name.ToLower().Contains("head"))
				{
					m_head = t;
					break;
				}
			}
		}
		//use avatars parent if head not found
		if(m_head==null)
		{
			m_head = avatar.transform;
		}
		
		m_breathingAirs = GameObject.FindSceneObjectsOfType(typeof(BreathingAir)) as BreathingAir[];
		m_breathingAirColliders = new BoxCollider[m_breathingAirs.Length];
		for(int i=0; i < m_breathingAirs.Length;++i)
		{
			m_breathingAirColliders[i] = m_breathingAirs[i].GetComponent<BoxCollider>();
			if(m_head)
			{
				if(m_breathingAirColliders[i].bounds.Contains(m_head.position))
				{
					//convert to ppm
					m_carbonMonoxid=(m_breathingAirs[i].CarbonMonoxide*10000.0);
				}
			}
			
		}
		DisorderValue = 0.0f;
		
	}
	
	/// <summary>
	/// Gets the carbon monoxid in PPM.
	/// </summary>
	/// <value>
	/// The carbon monoxid in PPM.
	/// </value>
	public double CarbonMonoxidPPM
	{
		get{return m_carbonMonoxid;}
	}
	
	/// <summary>
	/// The disorder update function.
	/// </summary>
	public override void DOUpdate ()
	{
		base.DOUpdate ();
		for(int i=0; i < m_breathingAirs.Length;++i)
		{
			if(m_head)
			{
				if(m_breathingAirColliders[i].bounds.Contains(m_head.position))
				{
					//convert to ppm
					m_carbonMonoxid=(m_breathingAirs[i].CarbonMonoxide*10000.0);
					break;
				}
			}
		}		
	}
}
