/* Copyright 2014-2015 StagPoint */
using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using StagPoint.Core;
using StagPoint.Planning;
using StagPoint.Planning.Components;

[CustomEditor( typeof( TaskNetworkPlanner ), true )]
public class TaskNetworkInspector : Editor
{

	#region Private variables 

	private BlackboardInspector blackboardInspector = null;

	#endregion 

	#region Editor events

	public override void OnInspectorGUI()
	{
		
		base.OnInspectorGUI();

		var htn = target as TaskNetworkPlanner;

		if( htn.Graph != null )
		{

			if( GUILayout.Button( "Edit HTN Graph" ) )
			{
				HTN_EditorWindow.EditGraph( htn.Graph );
			}

			if( EditorApplication.isPlaying && GUILayout.Button( "Force Replan" ) )
			{

				htn.ClearPlan();
				var plan = htn.GeneratePlan();
				if( plan != null )
				{
					htn.ExecutePlan( plan );
				}

			}

		}
		else if( GUILayout.Button( "Create New HTN Graph" ) )
		{
			htn.Graph = HTN_EditorWindow.CreateAndEditHTN();
		}

		initProxyInspector();
		if( blackboardInspector != null )
		{
			EditorGUILayout.Space();
			blackboardInspector.OnInspectorGUI();
			EditorGUILayout.Space();
		}

	}

	public void OnEnable()
	{
		initProxyInspector();
	}

	public void OnDisable()
	{
		Editor.DestroyImmediate( blackboardInspector );
		blackboardInspector = null;
	}

	#endregion 

	#region Private utility methods 

	private void initProxyInspector()
	{

		if( blackboardInspector != null )
			return;

		var component = (TaskNetworkPlanner)target;

		var blackboard = component.BlackboardDefinition;
		if( blackboard == null )
		{
			return;
		}

		var activeBlackboard = Application.isPlaying ? component.RuntimeBlackboard : component.BlackboardDefinition;

		blackboardInspector = Editor.CreateEditor( activeBlackboard ) as BlackboardInspector;
		if( blackboardInspector == null )
		{
			Debug.LogError( "Failed to create inspector for blackboard" );
			return;
		}

		blackboardInspector.IsProxied = true;

	}

	#endregion 

}
