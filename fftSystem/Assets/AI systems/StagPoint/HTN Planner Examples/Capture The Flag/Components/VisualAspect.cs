// Copyright (c) 2014 StagPoint Consulting
		
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace StagPoint.Examples
{

	[AddComponentMenu( "StagPoint/Examples/Capture the Flag/Visual Aspect" )]
	public class VisualAspect : MonoBehaviour
	{

		#region Static variables

		public static List<VisualAspect> ActiveItems = new List<VisualAspect>();

		#endregion

		#region Public fields

		public bool UpdateVelocity = true;

		public Vector3 Velocity = Vector3.zero;

		public Vector3 Position = Vector3.zero;

		#endregion

		#region Private serialized fields

		[SerializeField]
		protected List<string> tags = new List<string>();

		#endregion

		#region Private non-serialized fields

		private Vector3 lastPosition;
		private Rigidbody physicsBody;

		#endregion

		#region Monobehaviour events

		public void OnEnable()
		{

			lock( ActiveItems )
			{
				ActiveItems.Add( this );
			}

			// Cache this - Unity will generate a memory allocation every time you access Monobehaviour.rigidbody
			this.physicsBody = this.rigidbody;

			lastPosition = this.transform.position;
			Update();

		}

		public void OnDisable()
		{
			lock( ActiveItems )
			{
				ActiveItems.Remove( this );
			}
		}

		public void Update()
		{

			if( UpdateVelocity )
			{

				if( this.physicsBody != null && !this.physicsBody.isKinematic )
				{
					this.Velocity = this.physicsBody.velocity;
				}
				else
				{
					this.Position = this.transform.position;
					this.Velocity = ( Position - this.lastPosition ) / Mathf.Max( Time.deltaTime, float.Epsilon );
					this.lastPosition = Position;
				}

			}

		}

		#endregion

		#region Public methods

		public void AddTag( string tag )
		{

			if( !tags.Contains( tag ) )
			{
				tags.Add( tag );
			}

		}

		public void RemoveTag( string tag )
		{
			tags.Remove( tag );
		}

		public bool HasTag( string tag )
		{
			return tags.Contains( tag );
		}

		#endregion

	}

}
