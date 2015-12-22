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

[CustomEditor( typeof( TaskNetworkGraph ), true )]
public class GraphInspector : Editor
{

	public override void OnInspectorGUI()
	{

		var graph = target as TaskNetworkGraph;
		if( graph == null )
		{
			EditorGUILayout.HelpBox( "Inspector target is NULL", MessageType.Error );
			return;
		}

		DesignUtil.Header( "Task Network Definition" );

		if( DesignUtil.SectionFoldout( "htn-agent", "Agent Script") )
		{

			var configuredScript = MonoScriptHelper.GetMonoScriptFromType( graph.DomainType );
			var selectedScript = EditorGUILayout.ObjectField( "Domain Type", configuredScript, typeof( MonoScript ), false ) as MonoScript;
			if( selectedScript != configuredScript )
			{
				graph.DomainType = configuredScript == null ? null : configuredScript.GetClass();
				markUndo( graph, "Assign Domain Type" );
				graph.DomainType = selectedScript.GetClass();
			}

			GUI.enabled = ( selectedScript != null );
			if( GUILayout.Button( "Edit Domain Script" ) )
			{
				DesignUtil.Defer( () =>
				{
					AssetDatabase.OpenAsset( selectedScript );
				} );
			}

			GUI.enabled = true;

		}

		if( DesignUtil.SectionFoldout( "htn-blackboard", "Blackboard Definition" ) )
		{

			var selectedBlackboard = EditorGUILayout.ObjectField( "Blackboard", graph.BlackboardDefinition, typeof( Blackboard ), false ) as Blackboard;
			if( selectedBlackboard != graph.BlackboardDefinition )
			{
				DesignUtil.MarkUndo( graph, "Assign Blackboard Definition" );
				graph.BlackboardDefinition = selectedBlackboard;
			}

			if( selectedBlackboard == null )
			{
				if( GUILayout.Button( "Create New Blackboard" ) )
				{
					var blackboard = BlackboardInspector.CreateNewBlackboard( string.Format( "{0} Blackboard", graph.name ) );
					if( blackboard != null )
					{

						graph.BlackboardDefinition = blackboard;

						DesignUtil.Defer( () =>
						{
							Selection.activeObject = graph.BlackboardDefinition;
						} );

					}
				}
			}
			else if( GUILayout.Button( "Edit Blackboard" ) )
			{
				DesignUtil.Defer( () =>
				{
					Selection.activeObject = graph.BlackboardDefinition;
				} );
			}

		}

		GUILayout.Space( 15 );
		DesignUtil.DrawHorzSeparator();
		GUILayout.Space( 15 );

		if( GUILayout.Button( "Open Graph Editor" ) )
		{
			HTN_EditorWindow.EditGraph( graph );
		}

	}

	private void markUndo( TaskNetworkGraph graph, string undoMessage )
	{
		Undo.RegisterCompleteObjectUndo( graph, undoMessage );
		EditorUtility.SetDirty( graph );
	}

}