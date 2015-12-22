// Copyright (c) 2014 StagPoint Consulting
		
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace StagPoint.Examples
{

	[AddComponentMenu( "StagPoint/Examples/Capture the Flag/Bot Laser" )]
	public class BotLaser : MonoBehaviour
	{

		#region Public fields

		public Material material;
		public Vector3 offset;
		public float maxDistance = 99f;
		public int damage = 10;
		public Color laserColor = Color.red;

		#endregion

		#region Private runtime variables

		private float timeout = 0f;
		private bool isFiring = false;

		#endregion

		#region Monobehaviour events

		void OnGUI()
		{

			if( !isFiring || !enabled )
				return;

			timeout -= Time.deltaTime;
			if( timeout <= 0f )
				isFiring = false;

			drawLaser();

		}

		#endregion

		#region Public methods

		public GameObject FireLaser( float time )
		{

			if( !enabled )
				return null;

			this.timeout = time;
			this.isFiring = true;

			var startPoint = transform.position + transform.TransformDirection( offset );
			var target = raycastTarget( startPoint );

			if( target != null )
			{

				target.TakeDamage( this.damage, this );
				if( target.CurrentHealth <= 0 )
				{
					SendMessage( "OnTargetDestroyed", target, SendMessageOptions.DontRequireReceiver );
				}

				return target.gameObject;

			}

			return null;

		}

		#endregion

		#region Private utility methods

		private void drawLaser()
		{

			if( material == null )
				return;

			var startPoint = transform.position + transform.TransformDirection( offset );
			var endPoint = raycastFromPoint( startPoint );

			material.SetPass( 0 );

			GL.Begin( GL.LINES );
			{
				GL.Color( laserColor );
				GL.Vertex( startPoint );
				GL.Vertex( endPoint );
			}
			GL.End();

		}

		private BotHealthComponent raycastTarget( Vector3 startPoint )
		{

			// Only raycast for objects on the same layer as this bot, plus the Default layer
			var mask = ( 1 << gameObject.layer ) | 1;

			RaycastHit hitInfo;
			if( !Physics.Raycast( transform.position, transform.forward, out hitInfo, maxDistance, mask ) )
			{
				return null;
			}

			// Return the BotHealthComponent of the target, if one is defined
			return hitInfo.collider.gameObject.GetComponent<BotHealthComponent>();

		}

		private Vector3 raycastFromPoint( Vector3 startPoint )
		{

			// Only raycast for objects on the same layer as this bot, plus the Default layer
			var mask = ( 1 << gameObject.layer ) | 1;

			// Raycast to see what the laser's endpoint will be. If the raycast does not
			// find something to hit, then return a point at the maximum range of the laser
			RaycastHit hitInfo;
			if( !Physics.Raycast( transform.position, transform.forward, out hitInfo, maxDistance, mask ) )
			{
				return startPoint + transform.forward * maxDistance;
			}

			return hitInfo.point;

		}

		#endregion

	}

}
