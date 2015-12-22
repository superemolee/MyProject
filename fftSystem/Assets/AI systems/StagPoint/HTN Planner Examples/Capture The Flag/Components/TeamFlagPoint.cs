// Copyright (c) 2014 StagPoint Consulting
		
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace StagPoint.Examples
{

	[AddComponentMenu( "StagPoint/Examples/Capture the Flag/Flag" )]
	public class TeamFlagPoint : MonoBehaviour
	{

		#region Public serialized fields

		public CTF_Commander Commander;

		public TeamType Team = TeamType.RedTeam;

		#endregion

		#region Public properties

		public CTF_Agent FlagBearer { get { return follow == null ? null : follow.GetComponent<CTF_Agent>(); } }

		public bool IsCaptured { get { return ( follow != null ); } }

		public Vector3 Position
		{
			get { return transform.position; }
		}

		#endregion

		#region Private runtime variables

		private GameObject follow;
		private Vector3 startPosition;

		private bool hasFlag = true;
		private bool waitingToReturn = false;
		private float returnTimer;
		private GUIStyle labelStyle;

		#endregion

		#region Monobehaviour events

		void OnDrawGizmos()
		{

			if( hasFlag )
			{
				Gizmos.color = ( Team == TeamType.RedTeam ) ? Color.red : Color.blue;
				Gizmos.DrawSphere( transform.position + Vector3.up * 0.5f, 0.5f );
			}

		}

		void OnGUI()
		{

			if( !waitingToReturn )
			{
				return;
			}

			if( labelStyle == null )
			{

				var background = new Texture2D( 2, 2, TextureFormat.ARGB32, false );
				var color = this.Team == TeamType.RedTeam ? Color.red : Color.blue;
				background.SetPixels( new Color[] { color, color, color, color } );
				background.Apply( false, false );

				labelStyle = new GUIStyle( GUI.skin.label );
				labelStyle.padding = new RectOffset( 5, 5, 2, 2 );
				labelStyle.normal.textColor = Color.white;
				labelStyle.normal.background = background;
				labelStyle.fontSize = 9;

			}

			var screenPosition = (Vector2)Camera.main.WorldToScreenPoint( transform.position );
			screenPosition.y = Screen.height - screenPosition.y;

			var text = ( Mathf.FloorToInt( returnTimer ) + 1 ).ToString();
			DebugRender.DrawLabel( text, screenPosition, TextAnchor.MiddleCenter, labelStyle );

		}

		void OnEnable()
		{
			this.startPosition = transform.position;
		}

		void Update()
		{

			if( !hasFlag && follow == null )
			{

				this.renderer.enabled = true;
				hasFlag = true;

				waitingToReturn = true;
				returnTimer = 30f;

				DebugMessages.LogImportant( this.Team + " flag has been dropped and will reset in 30 seconds" );

			}

			if( waitingToReturn )
			{

				returnTimer -= Time.deltaTime;

				if( returnTimer <= 0 )
				{
					DebugMessages.LogImportant( this.Team + " flag returning" );
					ResetFlag();
					return;
				}

			}

			if( hasFlag )
				return;

			var followPosition = follow.transform.position;
			followPosition.y = 0.1f;

			this.transform.position = followPosition;

		}

		void OnTriggerEnter( Collider other )
		{

			if( !hasFlag )
				return;

			var agent = other.GetComponent<CTF_Agent>();
			if( agent != null && agent.Team != this.Team )
			{

				agent.SendMessage( "OnGotFlag", null, SendMessageOptions.RequireReceiver );

				this.follow = agent.gameObject;
				this.hasFlag = false;
				this.renderer.enabled = false;
				this.waitingToReturn = false;
				this.returnTimer = 20f;

			}

		}

		#endregion

		#region Public methods

		public void ResetFlag()
		{

			this.transform.position = startPosition;
			this.hasFlag = true;
			this.follow = null;
			this.renderer.enabled = true;
			this.waitingToReturn = false;

			if( Commander != null )
			{
				Commander.TeamFlagReset();
			}

		}

		#endregion

	}

}
