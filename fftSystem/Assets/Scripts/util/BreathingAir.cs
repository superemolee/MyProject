using System;
using UnityEngine;

/// <summary>
/// This class represents the beathing air. It contains all the components of the Air in percentage values. This is used for calculating i.e. poisening.
/// </summary>
[Serializable]
[RequireComponent(typeof(BoxCollider))]
[AddComponentMenu("Avatar/Util/BreathingAir")]
public class BreathingAir : MonoBehaviour
{
	/// <summary>
	/// The nitrogen.
	/// </summary>
	public float Nitrogen = 0.79f;
	/// <summary>
	/// The oxygen.
	/// </summary>
	public float Oxygen = 0.21f;
	/// <summary>
	/// The carbon dioxide.
	/// </summary>
	public float CarbonDioxide = 0.0f;
	/// <summary>
	/// The carbon monoxide.
	/// </summary>
	public double CarbonMonoxide = 0.0;
	/// <summary>
	/// The air temperature.
	/// </summary>
	public float AirTemperature = 25.0f;
	/// <summary>
	/// The other gases, which are maybe usefull.
	/// </summary>
	public float [] otherGases;
	

}

