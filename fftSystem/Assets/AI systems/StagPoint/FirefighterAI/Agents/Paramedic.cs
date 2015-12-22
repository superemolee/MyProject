using UnityEngine;
using System.Collections;
using StagPoint.Core;

public class Paramedic : RescuerGeneral {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

#region General Action

	public TaskStatus look()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus listen()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus feel()
	{
		
		return TaskStatus.Succeeded;
		
	}

#endregion

#region Airway

	public TaskStatus traumaJawThrust()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus traumaChinLift()
	{
		
		return TaskStatus.Succeeded;
		
	}
	
	public TaskStatus headTiltOrChinLift()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus airwayAdjuncts()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus cervicalSpineImmobilisation()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus suction()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus orpharyngealAirway()
	{
		
		return TaskStatus.Succeeded;
		
	}
#endregion

#region Breathing

	public TaskStatus assemblingOxygenCylinderAndRegulator()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus oxygenDelivery()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus mouthToMouth()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus faceShield()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus faceMask()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus bagValueMask()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus mechanicalVentilator()
	{
		
		return TaskStatus.Succeeded;
		
	}

#endregion

#region Circulation

	public TaskStatus haemorrgageControl()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus casualtyPositioning()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus casualtyPositioningInPregnancy()
	{
		
		return TaskStatus.Succeeded;
		
	}

#endregion

#region Expose

	public TaskStatus immediateLifeThreateningWoundManagement()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus openOrSuckingChestOrUpperAbdominalWoundManagement()
	{
		
		return TaskStatus.Succeeded;
		
	}

#endregion

#region CPR

	public TaskStatus shoutForHelp()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus ensureAmbulanceEnRoute()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus thirtyChestCompressions()
	{
		
		return TaskStatus.Succeeded;
		
	}

	public TaskStatus twoBreaths()
	{
		
		return TaskStatus.Succeeded;
		
	}
#endregion







}
