// Copyright (c) 2014 StagPoint Consulting
		
using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Reflection;

namespace StagPoint.Examples
{

	[AddComponentMenu( "StagPoint/Examples/Capture the Flag/CTF Camera" )]
	public class CTF_Camera : MonoBehaviour
	{

		void OnEnable()
		{
			DebugMessages.LogImportant( "Use the mouse wheel to zoom, hold the middle mouse button to pan" );
		}

		void OnGUI()
		{

			var evt = Event.current;
			if( evt.type == EventType.scrollWheel )
			{
				camera.transform.position += camera.transform.forward * -evt.delta.y;
			}
			else if( evt.type == EventType.mouseDrag && evt.button == 2 )
			{

				var delta = evt.delta * 0.1f;
				delta.x *= -1;

				camera.transform.Translate( delta );

			}
#if UNITY_EDITOR
			else if( evt.type == EventType.keyDown && evt.keyCode == KeyCode.P )
			{
				UnityEditor.EditorApplication.isPaused = true;
			}
#endif

		}

	}

}
