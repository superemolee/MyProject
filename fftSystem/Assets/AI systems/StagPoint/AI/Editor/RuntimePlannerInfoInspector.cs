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

[CustomEditor( typeof( RuntimePlannerInfo ), true )]
public class RuntimePlannerInfoInspector : Editor
{

	public override void OnInspectorGUI()
	{

		var plannerInfo = target as RuntimePlannerInfo;

		using( DesignUtil.BeginSection( "Visualization" ) )
		{

			plannerInfo.showDebugGUI = EditorGUILayout.Toggle( "Draw Debug Info", plannerInfo.showDebugGUI );
			
			if( plannerInfo.showDebugGUI )
			{
				plannerInfo.showWhenSelected = EditorGUILayout.Toggle( "Only When Selected", plannerInfo.showWhenSelected );
				plannerInfo.debugGUIPosition = (TextAnchor)EditorGUILayout.EnumPopup( "Debug Area", plannerInfo.debugGUIPosition );
				plannerInfo.numLinesToShow = EditorGUILayout.IntField( "# Log Lines", plannerInfo.numLinesToShow );
			}

		}

		DesignUtil.DrawHorzSeparator();

		using( DesignUtil.BeginSection( "Refresh" ) )
		{

			var autoRefreshConfig = EditorPrefs.GetBool( "auto-refresh-htn-stats", false );
			var autoRefresh = EditorGUILayout.Toggle( "Auto-refresh", autoRefreshConfig );
			if( autoRefreshConfig != autoRefresh )
			{
				EditorPrefs.SetBool( "auto-refresh-htn-stats", autoRefresh );
			}

			if( Application.isPlaying )
			{

				if( autoRefresh || GUILayout.Button( "Refresh" ) )
				{
					Repaint();
				}

				if( autoRefresh )
				{
					EditorGUILayout.HelpBox( "Auto-refreshing the planner stats will have an adverse effect on your game's frame time", MessageType.Info );
				}

			}

		}

		if( !Application.isPlaying )
		{
			EditorGUILayout.HelpBox( "This component will render profiling information about the planner during runtime", MessageType.Info );
			return;
		}

		using( DesignUtil.BeginSection( "General" ) )
		{
			EditorGUILayout.FloatField( "Run Time", plannerInfo.TotalRunTime );
		}

		using( DesignUtil.BeginSection( "Planning" ) )
		{
			EditorGUILayout.IntField( "Total", (int)plannerInfo.numPlanningAttempts.Total );
			EditorGUILayout.IntField( "Discarded", (int)plannerInfo.numPlansDiscarded.Total );
			EditorGUILayout.IntField( "Failed", (int)plannerInfo.numPlanningAttemptsFailed.Total );
			EditorGUILayout.IntField( "Plans / Sec", (int)plannerInfo.numPlanningAttempts.PerSecond.Average );
		}

		using( DesignUtil.BeginSection( "Plan Execution" ) )
		{
			EditorGUILayout.IntField( "Executed", (int)plannerInfo.numExecutedPlans.Total );
			EditorGUILayout.IntField( "Aborted", (int)plannerInfo.numAbortedPlans.Total );
			EditorGUILayout.IntField( "Failed", (int)plannerInfo.numFailedPlans.Total );
		}

		using( DesignUtil.BeginSection( "Planning Time" ) )
		{
			EditorGUILayout.TextField( "Total", format( plannerInfo.planningTime.Total ) );
			EditorGUILayout.TextField( "Max", format( plannerInfo.planningTime.Max ) );
			EditorGUILayout.TextField( "Min", format( plannerInfo.planningTime.Min ) );
			EditorGUILayout.TextField( "Avg", format( plannerInfo.planningTime.Average ) );
		}

		using( DesignUtil.BeginSection( "Plan Length" ) )
		{
			EditorGUILayout.IntField( "Max", (int)plannerInfo.planLength.Max );
			EditorGUILayout.IntField( "Min", (int)plannerInfo.planLength.Min );
			EditorGUILayout.FloatField( "Avg", plannerInfo.planLength.Average );
		}

		using( DesignUtil.BeginSection( "Search Depth" ) )
		{
			EditorGUILayout.IntField( "Max", (int)plannerInfo.searchDepth.Max );
			EditorGUILayout.IntField( "Min", (int)plannerInfo.searchDepth.Min );
			EditorGUILayout.FloatField( "Avg", plannerInfo.searchDepth.Average );
		}

	}

	private string format( float value )
	{
		return string.Format( "{0:F3}ms", value * 1000f );
	}

}
