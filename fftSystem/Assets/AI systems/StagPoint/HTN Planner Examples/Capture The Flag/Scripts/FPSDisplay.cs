// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace StagPoint.Examples
{

	[AddComponentMenu( "StagPoint/Examples/Capture the Flag/FPS Display" )]
	public class FPSDisplay : MonoBehaviour
	{

		public bool editorOnly = true;

		private const float UPDATE_INTERVAL = 0.5f;

		private string labelText = "FPS";
		private Color labelColor = Color.white;
		private GUIStyle labelStyle;

		private float timeleft;
		private float lastFrameTime;

		private Func<float, float> movingAVG = generateMovingAverageCalculator( 15 );

		void Start()
		{
			timeleft = UPDATE_INTERVAL;
		}

		void OnGUI()
		{

			if( editorOnly && !Application.isEditor )
				return;

			if( labelStyle == null )
			{
				labelStyle = new GUIStyle( "label" );
				labelStyle.alignment = TextAnchor.MiddleCenter;
			}

			var savedColor = GUI.color;

			var rect = new Rect( 0, 0, 100, 25 );

			GUI.color = Color.black;
			GUI.Box( rect, GUIContent.none );

			GUI.color = labelColor;
			labelStyle.normal.textColor = labelColor;
			GUI.Label( rect, labelText, labelStyle );

			GUI.color = savedColor;

		}

		void Update()
		{

			var realDeltaTime = Time.realtimeSinceStartup - lastFrameTime;
			lastFrameTime = Time.realtimeSinceStartup;

			var fps = Mathf.CeilToInt( 1f / movingAVG( realDeltaTime ) );

			if( Time.timeScale <= 0.01f )
			{

				labelColor = Color.yellow;
				labelText = "Pause";

				return;

			}

			timeleft -= realDeltaTime;

			if( timeleft <= 0.0 )
			{

				labelText = System.String.Format( "{0:F0} FPS", fps );

				if( fps < 30 )
					labelColor = Color.yellow;
				else if( fps < 10 )
					labelColor = Color.red;
				else
					labelColor = Color.green;

				timeleft = UPDATE_INTERVAL;

			}

		}

		private static Func<float, float> generateMovingAverageCalculator( int sampleCount )
		{

			if( sampleCount < 1 )
				throw new ArgumentOutOfRangeException( "sampleCount", sampleCount, "Sample count must be greater than zero." );

			float[] values = new float[ sampleCount ];
			int idx = 0;
			int count = 0;
			float sum = 0f;

			return ( value ) =>
			{

				if( float.IsNaN( value ) )
					return sum / count;

				sum += value;

				if( count < sampleCount )
					count++;
				else
					sum -= values[ idx ];

				values[ idx++ ] = value;

				if( idx == sampleCount )
					idx = 0;

				return sum / count;

			};

		}

	}

}
