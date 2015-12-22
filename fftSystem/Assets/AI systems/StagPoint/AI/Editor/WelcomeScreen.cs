// Copyright (c) 2014 StagPoint Consulting
		
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace StagPoint.Editor
{

	public class WelcomeScreen : EditorWindow
	{

		private Texture2D m_KeyImage;
		private TextAsset m_Readme;

		private Vector2 m_ReadmeScrollPosition;
		private Rect m_KeyImageRect = new Rect( 0, 0, 512, 100 );
		private Rect m_MainAreaRect = new Rect( 4, 72, 512, 324 );
		private bool m_ViewingReadme;

		[MenuItem( "Tools/StagPoint/HTN Planner/Help/View the Changelog" )]
		internal static void ShowWelcomeMessage()
		{

			var window = GetWindow<WelcomeScreen>();
			window.title = "Welcome";
			window.minSize = window.maxSize = new Vector2( 520, 400 );

			window.ShowUtility();

		}

		[MenuItem( "Tools/StagPoint/HTN Planner/Help/Documentation", false, 98 )]
		public static void ShowDocumentation( MenuCommand command )
		{
			Help.BrowseURL( "http://stagpoint.com/docs/planner-guide.html" );
		}

		[MenuItem( "Tools/StagPoint/HTN Planner/Help/Main Website" )]
		public static void ShowHelp( MenuCommand command )
		{
			Help.BrowseURL( "http://stagpoint.com" );
		}

		[MenuItem( "Tools/StagPoint/HTN Planner/Help/Support Forums" )]
		public static void ShowSupportForums( MenuCommand command )
		{
			Help.BrowseURL( "http://stagpoint.com/forums/" );
		}

		void OnEnable()
		{

			m_KeyImage = Resources.Load( "stagpoint-logo", typeof( Texture2D ) ) as Texture2D;
			m_KeyImageRect.width = m_KeyImage.width;
			m_KeyImageRect.height = m_KeyImage.height;

			m_Readme = Resources.Load( "ChangeLog", typeof( TextAsset ) ) as TextAsset;
			
			this.minSize = this.maxSize = new Vector2( 520, 400 );
			this.position = new Rect( position.x, position.y, minSize.x, minSize.y );

		}

		public void OnGUI()
		{

			try
			{

				if( m_KeyImage == null || m_Readme == null )
				{
					EditorGUILayout.HelpBox( "This installation appears to be broken. Cannot find the 'ChangeLog.txt' resource", MessageType.Error );
					return;
				}

				drawGrid();

				GUI.DrawTexture( new Rect( 0, 0, this.position.width - 1, m_KeyImage.height ), EditorGUIUtility.whiteTexture );

				m_KeyImageRect = new Rect( ( this.position.width - m_KeyImage.width ) * 0.5f, 0, m_KeyImage.width, m_KeyImage.height );
				GUI.DrawTexture( m_KeyImageRect, m_KeyImage );

				m_MainAreaRect = new Rect( 4, m_KeyImage.height + 8, this.position.width - 8, this.position.height - m_KeyImage.height - 8 );
				GUILayout.BeginArea( m_MainAreaRect );
				{

					m_ReadmeScrollPosition = GUILayout.BeginScrollView( m_ReadmeScrollPosition, false, false, GUILayout.Width( m_MainAreaRect.width - 2 ), GUILayout.Height( m_MainAreaRect.height - 38 ) );
					{
						GUILayout.Label( m_Readme.text, EditorStyles.whiteLabel );
					}
					GUILayout.EndScrollView();

					GUILayout.BeginVertical();
					{

						GUILayout.FlexibleSpace();

						var buttonStyle = new GUIStyle( (GUIStyle)"flow node 5" );
						buttonStyle.fontStyle = FontStyle.Bold;

						if( GUILayout.Button( "Done", buttonStyle, GUILayout.Height( 30 ), GUILayout.ExpandWidth( true ) ) )
							this.Close();

						GUILayout.FlexibleSpace();

					}
					GUILayout.EndVertical();

				}
				GUILayout.EndArea();

			}
			catch { }

		}

		private void drawGrid()
		{

			var backStyle = new GUIStyle( (GUIStyle)"GameViewBackground" );
			backStyle.padding = new RectOffset();
			GUI.Box( new Rect( 0, 0, position.width, position.height ), GUIContent.none, backStyle );

			if( Event.current.type != EventType.repaint )
				return;

			const int MAJOR_GRIDLINES = 100;
			const int MINOR_GRIDLINES = 10;

			int size = Mathf.CeilToInt( Mathf.Max( position.width, position.height ) );

			for( int i = 0; i < size; i++ )
			{

				if( i % MAJOR_GRIDLINES == 0 )
				{
					Handles.color = new Color( 0f, 0f, 0f, 0.33f );
					Handles.DrawLine( new Vector3( 0, i, 0 ), new Vector3( size, i, 0 ) );
					Handles.DrawLine( new Vector3( i, 0, 0 ), new Vector3( i, size, 0 ) );
				}
				else if( i % MINOR_GRIDLINES == 0 )
				{
					Handles.color = new Color( 0.5f, 0.5f, 0.5f, 0.1f );
					Handles.DrawLine( new Vector3( i, 0, 0 ), new Vector3( i, size, 0 ) );
					Handles.DrawLine( new Vector3( 0, i, 0 ), new Vector3( size, i, 0 ) );
				}

			}

			Handles.color = Color.white;

		}

	}

	public class InstallPostProcessor : AssetPostprocessor
	{

		private const string VERSION_KEY = "StagPoint.Planning.Version";

		static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
		{

			var changelog = Resources.Load( "ChangeLog", typeof( TextAsset ) ) as TextAsset;
			if( changelog == null )
			{
				Debug.LogError( "This installation of StagPoint.Planning appears to be broken. Cannot find the 'ChangeLog.txt' resource" );
				return;
			}

			var path = Path.Combine( Application.dataPath.Replace( "Assets", "" ), AssetDatabase.GetAssetPath( changelog ) );
			if( !File.Exists( path ) )
			{
				Debug.LogError( "Failed to find path to StagPoint.Planning changelog" );
				return;
			}

			var currentVersion = File.GetLastWriteTimeUtc( path ).Second;

			var lastVersion = EditorPrefs.GetInt( VERSION_KEY, int.MinValue );
			if( lastVersion == int.MinValue || lastVersion != currentVersion )
			{

				EditorApplication.delayCall += ShowWelcomeDialog;

				Debug.Log( "New version of HTN Planner for Unity installed" );
				EditorPrefs.SetInt( VERSION_KEY, currentVersion );

			}

		}

		private static void ShowWelcomeDialog()
		{
			EditorApplication.delayCall -= ShowWelcomeDialog;
			WelcomeScreen.ShowWelcomeMessage();
		}

	}

}
