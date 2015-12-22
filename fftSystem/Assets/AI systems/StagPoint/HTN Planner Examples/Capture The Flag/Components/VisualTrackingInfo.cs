// Copyright (c) 2014 StagPoint Consulting
		
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace StagPoint.Examples
{

	/// <summary>
	/// Provides information about VisualAspect instances that have been detected by 
	/// a VisualSensor component.
	/// </summary>
	public class VisualTrackingInfo
	{

		#region Public fields and properties

		/// <summary>
		/// Indicates whether the object is still visible. If the object has been 
		/// destroyed or its VisualAspect component has been disabled, or the object
		/// has gone outside of the viewable area, this value will be set to FALSE.
		/// </summary>
		public bool IsVisible
		{
			get { return this.isObjectVisible && this.Aspect != null && this.Aspect.enabled; }
			set { this.isObjectVisible = value; }
		}

		/// <summary>
		/// If still visible, this value will contain a reference to the detected 
		/// VisualAspect component.
		/// </summary>
		public VisualAspect Aspect { get; private set; }

		/// <summary>
		/// If still visible, this value will return the GameObject that is being tracked
		/// </summary>
		public GameObject GameObject { get; private set; }

		/// <summary>
		/// Indicates the signed angle to the object relative to the forward direction
		/// of the VisualSensor.
		/// </summary>
		public float Angle;

		/// <summary>
		/// Returns the distance to the object
		/// </summary>
		public float Distance;

		/// <summary>
		/// Indicates the current (if still visible) or last known position (if no longer
		/// visible) of the VisualAspect.
		/// </summary>
		public Vector3 Position;

		/// <summary>
		/// Indicates the last-known velocity of the object
		/// </summary>
		public Vector3 Velocity;

		/// <summary>
		/// Will retain the last time the object was visible (from Time.realTimeSinceStartup)
		/// </summary>
		public float LastSeen;

		/// <summary>
		/// Indicates which direction the detected VisualAspect is facing
		/// </summary>
		public Vector3 Facing;

		#endregion

		#region Private fields

		private bool isObjectVisible = true;

		#endregion

		#region Constructor

		public VisualTrackingInfo( VisualAspect detectedAspect )
		{
			this.Aspect = detectedAspect;
			this.GameObject = detectedAspect.gameObject;
		}

		#endregion

	}

}
