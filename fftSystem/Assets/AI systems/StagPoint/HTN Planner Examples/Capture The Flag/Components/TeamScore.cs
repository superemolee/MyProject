// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using StagPoint.Core;
using StagPoint.Planning.Components;

namespace StagPoint.Examples
{

	[AddComponentMenu( "StagPoint/Examples/Capture the Flag/Team Score Display" )]
	public class TeamScore : MonoBehaviour
	{

		#region Public fields and properties

		public CTF_Commander commander;

		#endregion

		#region Private runtime variables

		private int numDeaths = 0;
		private int numKills = 0;
		private int numCaptures = 0;
		private int numGoals = 0;
		private bool rebuildText = true;

		private Texture2D backgroundTexture;
		private GUIStyle textStyle;
		private GUIContent text;
		private Rect rect;

		#endregion

		#region Monobehaviour events

		public void OnGUI()
		{

			if( commander == null )
			{
				commander = GetComponent<CTF_Commander>();
				if( commander == null )
				{
					Debug.LogError( string.Format( "The {0} component needs a {1} reference", this.GetType().Name, typeof( CTF_Commander ).Name ) );
					this.enabled = false;
					return;
				}
			}

			if( backgroundTexture == null )
			{
				initializeBackgroundTexture();
				initializeStyle();
			}

			if( rebuildText )
			{

				rebuildText = false;

				var textFormat = @"<b>Goals:</b> {0}
<b>Captures:</b> {1}
<b>Kills:</b> {2}
<b>Deaths:</b> {3}";

				this.text = new GUIContent( string.Format( textFormat, numGoals, numCaptures, numKills, numDeaths ) );

				var size = textStyle.CalcSize( text );
				var position = ( commander.Team == TeamType.RedTeam ) ? Screen.width * 0.33f : Screen.width * 1.66f;

				this.rect = new Rect( ( position - size.x ) * 0.5f, 5, size.x, size.y );

			}

			GUI.DrawTexture( rect, backgroundTexture );
			GUI.Label( rect, text, textStyle );

		}

		#endregion

		#region SendMessage targets

		void OnSoldierKilled()
		{
			numDeaths += 1;
			rebuildText = true;
		}

		void OnEnemyKilled()
		{
			numKills += 1;
			rebuildText = true;
		}

		void OnEnemyFlagCaptured()
		{
			numCaptures += 1;
			rebuildText = true;
		}

		void OnGoalScored()
		{
			numGoals += 1;
			rebuildText = true;
		}

		#endregion

		#region Private utility functions

		private void initializeStyle()
		{

			textStyle = new GUIStyle();
			textStyle.richText = true;
			textStyle.padding = new RectOffset( 10, 10, 10, 10 );
			textStyle.fontSize = 12;
			textStyle.normal.textColor = Color.white;

		}

		private void initializeBackgroundTexture()
		{

			backgroundTexture = new Texture2D( 2, 2, TextureFormat.ARGB32, false )
			{
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Repeat
			};

			var backgroundColor = ( commander.Team == TeamType.RedTeam ? Color.red : Color.blue ) * 0.8f;

			backgroundTexture.SetPixels( new Color[] { backgroundColor, backgroundColor, backgroundColor, backgroundColor } );
			backgroundTexture.Apply( false );

		}

		#endregion

	}

}
