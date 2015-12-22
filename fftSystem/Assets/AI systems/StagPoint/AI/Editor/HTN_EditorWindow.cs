using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using StagPoint.Core;
using StagPoint.Planning;
using StagPoint.Planning.Components;
using UnityEditor;
using UnityEngine;

public class HTN_EditorWindow : EditorWindow
{

	#region Static properties

	public static HTN_EditorWindow Instance;

	#endregion Static properties

	#region Static methods

	[MenuItem( "Assets/Edit HTN Graph", false, 100 )]
	public static void EditGraph()
	{
		var graph = Selection.activeObject as TaskNetworkGraph;
		if( graph != null )
		{
			EditGraph( graph );
		}
	}

	[MenuItem( "Window/Task Network Graph Editor", false, 100 )]
	public static void OpenGraphEditor()
	{
		var window = Instance = GetWindow( typeof( HTN_EditorWindow ), false, "HTN Graph", true ) as HTN_EditorWindow;

		var graph = Selection.activeObject as TaskNetworkGraph;
		if( graph != null )
		{
			EditGraph( graph );
		}

		window.Focus();
	}

	[MenuItem( "Assets/Edit HTN Graph", true, 100 )]
	public static bool EditGraphValidate()
	{
		var graph = Selection.activeObject as TaskNetworkGraph;
		return !( graph == null );
	}

	[MenuItem( "Assets/Create/StagPoint/Hierarchical Task Network", false )]
	[MenuItem( "Tools/StagPoint/HTN Planner/Create Hierarchical Task Network" )]
	public static TaskNetworkGraph CreateAndEditHTN()
	{
		var graph = CreateNewGraph();
		if( graph == null )
			return null;

		Selection.activeObject = graph;

		var window = HTN_EditorWindow.EditGraph( graph );
		window.selectNode( graph.RootNode );

		return graph;
	}

	public static TaskNetworkGraph CreateNewGraph()
	{
		var path = EditorUtility.SaveFilePanel( "New Task Network", "Assets", "HierarchicalTaskNetwork", "asset" );
		if( string.IsNullOrEmpty( path ) )
			return null;

		var graph = ScriptableObject.CreateInstance<TaskNetworkGraph>();
		var root = new RootNode()
		{
			Bounds = new Rect( ( Screen.width - 100 ) * 0.5f, 100, 100, 30 ),
			IsSelected = true
		};

		graph.Nodes.Add( root );

		Path.ChangeExtension( path, ".asset" );

		AssetDatabase.CreateAsset( graph, makeRelativePath( path ) );
		AssetDatabase.Refresh();
		AssetDatabase.ImportAsset( path, ImportAssetOptions.ForceSynchronousImport );

		return graph;
	}

	public static HTN_EditorWindow EditGraph( TaskNetworkGraph graph )
	{
		var window = OpenWindow();

		window.selected.Clear();
		window.deferred.Clear();

		window.graph = graph;
		graph.Validate();

		window.mode = EditorMode.Default;
		window.wantsMouseMove = true;
		window.scrollPos = Vector2.zero;

		var nodes = graph.GetAllNodes();
		foreach( var node in nodes )
		{
			node.IsSelected = false;
		}

		window.selectLastSelectedNode();

		return window;
	}

	private static HTN_EditorWindow OpenWindow()
	{
		var window = Instance = GetWindow( typeof( HTN_EditorWindow ) ) as HTN_EditorWindow;
		window.title = "HTN Graph";

		return window;
	}

	#endregion Static methods

	#region Private variables and constants

	private const int NODE_VERTICAL_SPACING = 12;
	private const int GRAPH_BOTTOM_PADDING = 8;

	private TaskNetworkGraph graph = null;

	[NonSerialized]
	private int inspectorWidth = 300;

	[NonSerialized]
	private int toolbarHeight = 35;

	[NonSerialized]
	private List<GraphNodeBase> selected = new List<GraphNodeBase>();

	[NonSerialized]
	private Queue<Action> deferred = new Queue<Action>();

	[NonSerialized]
	private EditorMode mode = EditorMode.Default;

	[NonSerialized]
	private GraphNodeBase debugNode = null;

	[NonSerialized]
	private GraphNodeBase dragNode = null;

	[NonSerialized]
	private GraphNodeBase nodeToScroll = null;

	private Vector2 dragNodeOffset = Vector2.zero;
	private Vector2 dragStartPosition = Vector2.zero;
	private Vector2 dragEndPosition = Vector2.zero;

	private Vector2 scrollPos = Vector2.zero;
	private Vector2 mouseScrollAdjust = Vector2.zero;

	private Rect canvasRect = new Rect();
	private Rect inspectorRect = new Rect();
	private Rect toolbarRect = new Rect();

	private int canvasControlID = 0;
	private bool showDebugInfo = true;
	private bool showInspector = true;
	private bool showTooltips = true;

	private string searchText = string.Empty;
	private string tooltip = string.Empty;

	#endregion Private variables and constants

	#region EditorWindow message handlers

	public void OnEnable()
	{
		Instance = this;

		this.title = "HTN Graph";
		this.mode = EditorMode.Default;

		this.inspectorWidth = EditorPrefs.GetInt( "htn_editor_inspector_width", 350 );
		this.scrollPos = Vector2.zero;

		clearSelection();
		selectLastSelectedNode();

		OnSelectionChange();
	}

	public void OnFocus()
	{
		this.wantsMouseMove = true;
		OnSelectionChange();
	}

	public void OnDestroy()
	{
		Instance = null;
	}

	public void OnInspectorUpdate()
	{
		Repaint();
	}

	public void OnSelectionChange()
	{
		if( Selection.activeGameObject == null )
		{
			return;
		}
		else
		{
			var selectedPlanner = Selection.activeGameObject.GetComponent<TaskNetworkPlanner>();
			if( selectedPlanner != null )
			{
				this.RemoveNotification();

				if( selectedPlanner.Graph == this.graph )
					return;

				TaskNetworkPlanner.DebugTarget = null;

				selectedPlanner.Graph.Validate();

				this.graph = selectedPlanner.Graph;
				this.selected.Clear();
				this.deferred.Clear();
				this.scrollPos = Vector2.zero;

				var nodes = graph.GetAllNodes();
				foreach( var node in nodes )
				{
					node.IsSelected = false;
				}

				this.selectLastSelectedNode();

				return;
			}
		}

		this.graph = null;
	}

	public void OnGUI()
	{
		this.canvasControlID = GUIUtility.GetControlID( this.GetType().Name.GetHashCode(), FocusType.Native );

		while( deferred.Count > 0 )
		{
			deferred.Dequeue()();
		}

		if( graph == null )
		{
			ShowNotification( new GUIContent( "No Graph is currently selected" ) );
			return;
		}

		if( EditorApplication.isCompiling )
		{
			ShowNotification( new GUIContent( "Compiling Please Wait..." ) );
			return;
		}

		// GAH! Stupid workaround for Unity editor weirdness
		var evt = Event.current;
		if( evt.type == EventType.ValidateCommand && evt.commandName == "UndoRedoPerformed" )
		{
			OnFocus();
			Repaint();
			return;
		}

		// Draw the task network hierarchy
		drawNodes();

		if( nodeToScroll != null )
		{
			scrollIntoView( nodeToScroll );
			nodeToScroll = null;
		}

		if( Application.isPlaying && showDebugInfo )
		{
			drawDebugInfo();
		}
		else
		{
			// Draw breadcrumbs if a node has been selected
			drawBreadCrumbs();
		}

		// Inspect selected node's properties
		if( this.showInspector )
		{
			inspectNode();
		}

		// Handle user input after the graph has been rendered
		handleUserInput( canvasControlID );

		if( this.showTooltips && !string.IsNullOrEmpty( tooltip ) && this.canvasRect.Contains( evt.mousePosition ) )
		{
			showTooltip( evt.mousePosition );
		}

		// Always repaint the editor window when there is a mouse event
		if( evt.isMouse )
		{
			Repaint();
		}
	}

	#endregion EditorWindow message handlers

	private void showTooltip( Vector2 position )
	{
		if( string.IsNullOrEmpty( tooltip ) )
			return;

		var node = graph.HitTest( position + mouseScrollAdjust );
		if( node == null )
			return;

		var tooltipContent = new GUIContent( tooltip.Trim() );

		var style = DesignUtil.GetStyle( "htn_notes_tooltip", (GUIStyle)"tooltip" );
		style.richText = true;
		style.wordWrap = true;
		style.fontStyle = FontStyle.Bold;
		style.alignment = TextAnchor.MiddleLeft;
		style.fontSize = 0;
		style.padding = new RectOffset( 5, 5, 5, 5 );

		var bounds = node.Bounds;
		bounds.y -= mouseScrollAdjust.x;
		bounds.y -= mouseScrollAdjust.y;

		var size = style.CalcSize( tooltipContent );
		if( size.x > 600 )
		{
			size.x = 600;
			size.y = style.CalcHeight( tooltipContent, size.x );
		}

		var rect = new Rect( position.x + 15, bounds.yMax + 5, size.x, size.y );

		if( rect.yMax >= canvasRect.yMax )
		{
			rect.y = bounds.yMin - size.y - 5;
		}

		var shadowStyle = DesignUtil.GetStyle( "htn_notes_shadow", (GUIStyle)"IN ThumbnailShadow" );
		GUI.Box( rect, GUIContent.none, shadowStyle );

		GUI.Label( rect, tooltipContent, style );
	}

	private void selectLastSelectedNode()
	{
		var lastSelectedUID = EditorPrefs.GetString( "last-selected-node", string.Empty );
		if( this.graph != null && !string.IsNullOrEmpty( lastSelectedUID ) )
		{
			var node = graph.FindByUID( lastSelectedUID );
			if( node != null )
			{
				selectNode( node );
				scrollIntoView( node );
			}
		}
	}

	private void defer( Action action )
	{
		deferred.Enqueue( action );
	}

	private void handleToolbar()
	{
		GUILayout.BeginArea( new Rect( 0, 0, Screen.width, Screen.height ) );
		{
			GUILayout.BeginHorizontal( EditorStyles.toolbar );
			{
				this.showInspector = doToolbarButton( "htn_show_inspector", this.showInspector, "Inspector" );
				this.showTooltips = doToolbarButton( "htn_show_tooltips", this.showTooltips, "Tooltips" );

				if( GUILayout.Button( "Expand All", EditorStyles.toolbarButton ) )
				{
					var allNodes = graph.GetAllNodes();
					foreach( var node in allNodes )
					{
						node.Expand();
					}
				}
				else if( GUILayout.Button( "Collapse All", EditorStyles.toolbarButton ) )
				{
					var allNodes = graph.GetAllNodes();
					foreach( var node in allNodes )
					{
						node.Collapse();
					}
				}

				if( Application.isPlaying )
				{
					this.showDebugInfo = doToolbarButton( "htn_show_debug", this.showDebugInfo, "Debug" );
				}

				if( this.showDebugInfo )
				{
					handleToolbarDebug();
				}

				handleSearch();

				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			toolbarRect = GUILayoutUtility.GetLastRect();
		}
		GUILayout.EndArea();
	}

	private void handleSearch()
	{

		if( this.showDebugInfo && getDebugPlan() != null )
			return;

		GUILayout.Space( 75 );

		const string FIELD_NAME = "htn_planner_search";

		// Obtain the control ID and event information before editing the search text
		var controlID = GUIUtility.GetControlID( this.GetHashCode() & FIELD_NAME.GetHashCode(), FocusType.Native );
		var evt = Event.current;
		var eventType = evt.GetTypeForControl( controlID );
		
		GUI.SetNextControlName( FIELD_NAME );
		this.searchText = EditorGUILayout.TextField( this.searchText, (GUIStyle)"ToolbarSeachTextField", GUILayout.Width( 250 ) );

		if( GUI.GetNameOfFocusedControl() == FIELD_NAME )
		{

			if( eventType == EventType.keyDown && evt.keyCode == KeyCode.Return )
			{

				var closestNode = graph.GetAllNodes()
					.OrderBy( x => scoreSearch( searchText, x.Name ) )
					.FirstOrDefault();

				if( closestNode != null )
				{

					clearSelection();
					selectNode( closestNode );

					ensureExpanded( closestNode );
					scrollIntoView( closestNode );

					this.Repaint();

				}

			}

		}

		if( GUILayout.Button( GUIContent.none, (GUIStyle)"ToolbarSeachCancelButton" ) )
		{
			GUI.FocusControl( string.Empty );
			this.searchText = string.Empty;
			this.Repaint();
		}

	}

	private int scoreSearch( string filter, string text )
	{

		if( text == filter )
			return -int.MaxValue;

		if( text.ToLower() == filter.ToLower() )
			return -int.MaxValue / 2;

		if( text.StartsWith( filter ) )
			return -filter.Length * 3;

		if( text.ToLower().StartsWith( filter.ToLower() ) )
			return -filter.Length * 2;

		if( text.ToLower().Contains( filter.ToLower() ) )
			return -filter.Length;

		return calcEditDistance( filter, text, 10 );

	}

	private static void swap<T>( ref T arg1, ref T arg2 )
	{
		T temp = arg1;
		arg1 = arg2;
		arg2 = temp;
	}

	private static int calcEditDistance( string source, string target, int threshold )
	{

		// http://stackoverflow.com/a/9454016/154165

		int length1 = source.Length;
		int length2 = target.Length;

		if( Math.Abs( length1 - length2 ) > threshold ) 
		{ 
			return int.MaxValue; 
		}

		if( length1 > length2 )
		{
			swap( ref target, ref source );
			swap( ref length1, ref length2 );
		}

		int maxi = length1;
		int maxj = length2;

		int[] dCurrent = new int[ maxi + 1 ];
		int[] dMinus1 = new int[ maxi + 1 ];
		int[] dMinus2 = new int[ maxi + 1 ];
		int[] dSwap;

		for( int i = 0; i <= maxi; i++ ) 
		{ 
			dCurrent[ i ] = i; 
		}

		int jm1 = 0, im1 = 0, im2 = -1;

		for( int j = 1; j <= maxj; j++ )
		{

			dSwap = dMinus2;
			dMinus2 = dMinus1;
			dMinus1 = dCurrent;
			dCurrent = dSwap;

			int minDistance = int.MaxValue;
			dCurrent[ 0 ] = j;
			im1 = 0;
			im2 = -1;

			for( int i = 1; i <= maxi; i++ )
			{

				int cost = source[ im1 ] == target[ jm1 ] ? 0 : 1;

				int del = dCurrent[ im1 ] + 1;
				int ins = dMinus1[ i ] + 1;
				int sub = dMinus1[ im1 ] + cost;

				int min = ( del > ins ) ? ( ins > sub ? sub : ins ) : ( del > sub ? sub : del );

				if( i > 1 && j > 1 && source[ im2 ] == target[ jm1 ] && source[ im1 ] == target[ j - 2 ] )
					min = Math.Min( min, dMinus2[ im2 ] + cost );

				dCurrent[ i ] = min;

				if( min < minDistance )
					minDistance = min;

				im1++;
				im2++;

			}

			jm1++;

			if( minDistance > threshold )
				return int.MaxValue;

		}

		int result = dCurrent[ maxi ];

		return ( result > threshold ) ? int.MaxValue : result;

	}

	private void handleToolbarDebug()
	{
		
		var planner = getDebugPlanner();
		if( planner == null )
			return;

		var plan = planner.Plan;
		if( plan == null )
			return;

		GUILayout.Space( 75 );

		var toolbarButtonLeft = DesignUtil.GetStyle( "htn-editor-toolbarButton-left", (GUIStyle)"ButtonLeft" );
		toolbarButtonLeft.fixedHeight = 18;
		toolbarButtonLeft.margin = new RectOffset();

		var playIcon = EditorGUIUtility.FindTexture( "d_PlayButton On" );
		var play = new GUIContent( playIcon, "Stop the program" );
		if( !GUILayout.Toggle( true, play, toolbarButtonLeft ) )
		{
			EditorApplication.isPlaying = false;
		}

		var toolbarButtonMid = DesignUtil.GetStyle( "htn-editor-toolbarButton-mid", (GUIStyle)"ButtonMid" );
		toolbarButtonMid.fixedHeight = 18;
		toolbarButtonMid.margin = new RectOffset();

		var pauseIcon = !EditorApplication.isPaused ? EditorGUIUtility.FindTexture( "d_PauseButton On" ) : EditorGUIUtility.FindTexture( "d_PauseButton On" );
		if( EditorApplication.isPaused && plan.CurrentTask.Node.PauseOnRun )
			pauseIcon = EditorGUIUtility.FindTexture( "PauseButton Anim" );

		var pause = new GUIContent( pauseIcon, "Pause" );
		if( GUILayout.Toggle( EditorApplication.isPaused, pause, toolbarButtonMid ) != EditorApplication.isPaused )
		{
			EditorApplication.isPaused = !EditorApplication.isPaused;
			if( !EditorApplication.isPaused )
			{
				TaskNetworkPlanner.DebugTarget = null;
			}
		}

		var toolbarButtonRight = DesignUtil.GetStyle( "htn-editor-toolbarButton-right", (GUIStyle)"ButtonRight" );
		toolbarButtonRight.fixedHeight = 18;
		toolbarButtonRight.margin = new RectOffset();

		var step = new GUIContent( EditorGUIUtility.FindTexture( "d_StepButton On" ), "Run to next task" );
		if( GUILayout.Button( step, toolbarButtonRight ) )
		{
			TaskNetworkPlanner.DebugTarget = planner.Agent;

			var taskIndex = plan.tasks.IndexOf( plan.CurrentTask );
			if( taskIndex < plan.TaskCount - 1 )
			{
				plan.tasks[ taskIndex + 1 ].PauseOnRun = true;
			}

			EditorApplication.isPaused = false;
		}
	}

	private bool doToolbarButton( string key, bool currentValue, string text )
	{
		var config = EditorPrefs.GetBool( key, currentValue );
		var actual = GUILayout.Toggle( currentValue, text, EditorStyles.toolbarButton );

		if( actual != config )
		{
			EditorPrefs.SetBool( key, actual );
		}

		return actual;
	}

	private void handleUserInput( int controlID )
	{
		handleToolbar();

		var isDebugging = showDebugInfo && Application.isPlaying && getDebugPlan() != null;
		if( isDebugging )
			return;

		switch( mode )
		{
			case EditorMode.Default:
				handleDefaultMode( controlID );
				break;

			case EditorMode.DraggingSelection:
				handleDraggingSelection();
				break;

			case EditorMode.DraggingNode:
				handleDraggingNode();
				break;

			case EditorMode.Inspector:
				handleInspectorInput();
				break;

			case EditorMode.ResizingsInspector:
				handleInspectorResize();
				break;

			default:
				break;
		}
	}

	private void handleInspectorResize()
	{
		var evt = Event.current;

		if( evt.type == EventType.mouseUp || evt.rawType == EventType.mouseUp )
		{
			mode = EditorMode.Default;
			return;
		}

		var mousePos = evt.mousePosition;

		var cursorRect = new Rect( mousePos.x - 15, mousePos.y - 15, 30, 30 );
		EditorGUIUtility.AddCursorRect( cursorRect, MouseCursor.ResizeHorizontal );

		this.inspectorWidth = (int)Mathf.Clamp( Screen.width - mousePos.x, 250, 600 );
		EditorPrefs.SetInt( "htn_editor_inspector_width", this.inspectorWidth );
	}

	private void handleInspectorInput()
	{
		var evt = Event.current;

		if( evt.type == EventType.mouseDown || evt.type == EventType.mouseDrag )
		{
			if( !inspectorRect.Contains( evt.mousePosition ) )
			{
				DesignUtil.Defer( () =>
				{
					EditorGUIUtility.hotControl = EditorGUIUtility.keyboardControl = 0;
					mode = EditorMode.Default;
					base.SendEvent( evt );
				} );

				return;
			}
		}
	}

	private void handleDefaultMode( int controlID )
	{
		var evt = Event.current;
		var eventType = evt.GetTypeForControl( controlID );

		if( evt.isMouse )
		{
			if( eventType == EventType.mouseDown )
			{
				RemoveNotification();

				var hit = graph.HitTest( evt.mousePosition + this.mouseScrollAdjust );

				if( evt.button == 0 )
				{
					handleDefaultLeftButtonDown( controlID, evt, hit );
				}
				else if( evt.button == 1 )
				{
					handleDefaultRightButtonDown( evt, hit );
				}
				else if( evt.button == 2 )
				{
					dragStartPosition = dragEndPosition = evt.mousePosition + this.mouseScrollAdjust;
				}

				Repaint();

				return;
			}
			else if( eventType == EventType.mouseDrag )
			{
				if( ignoreToolWindowEvent( evt ) )
					return;

				handleDefaultMouseDrag( controlID, evt );
			}
			else if( eventType == EventType.scrollWheel )
			{
				this.scrollPos.y += evt.delta.y;
			}
			else if( eventType == EventType.mouseMove )
			{
				var hit = graph.HitTest( evt.mousePosition + this.mouseScrollAdjust );
				if( hit != null )
				{
					tooltip = hit.Notes;
				}
				else
				{
					tooltip = string.Empty;
				}
			}
		}
		else if( eventType == EventType.keyDown )
		{
			if( selected.Count == 1 )
			{
				switch( evt.keyCode )
				{
					case KeyCode.Home:
						{
							clearSelection();
							selectNode( graph.RootNode );
							scrollIntoView( graph.RootNode );
						}
						break;

					case KeyCode.End:
						{
							var lastVisibleNode = graph.GetVisibleNodes().Last();
							clearSelection();
							selectNode( lastVisibleNode );
							scrollIntoView( lastVisibleNode );
						}
						break;

					case KeyCode.PageUp:
						{
							if( evt.control || evt.command )
							{
								moveNodeToTop( selected.First() );
							}
						}
						break;

					case KeyCode.PageDown:
						{
							if( evt.control || evt.command )
							{
								moveNodeToBottom( selected.First() );
							}
						}
						break;

					case KeyCode.LeftArrow:
						{
							var selectedNode = selected.First();
							if( selectedNode.ChildNodes.Count > 0 && selectedNode.IsExpanded )
							{
								selectedNode.Collapse();
							}
							else
							{
								clearSelection();
								var newSelection = graph.GetParentNode( selectedNode );
								selectNode( newSelection );
								scrollIntoView( newSelection );
							}
						}
						break;

					case KeyCode.RightArrow:
						{
							var selectedNode = selected.First();
							if( selectedNode.ChildNodes.Count > 0 )
							{
								if( !selectedNode.IsExpanded )
								{
									selectedNode.Expand();
								}
								else
								{
									clearSelection();
									var newSelection = selectedNode.ChildNodes[ 0 ];
									selectNode( newSelection );
									scrollIntoView( newSelection );
								}
							}
						}
						break;

					case KeyCode.UpArrow:
						{
							if( evt.control || evt.command )
							{
								moveNodeUp( selected.First() );
							}
							else
							{
								var allNodes = graph.GetVisibleNodes();
								var selectedIndex = allNodes.IndexOf( selected.First() );
								if( selectedIndex > 0 )
								{
									clearSelection();

									var newSelection = allNodes[ selectedIndex - 1 ];
									selectNode( newSelection );
									scrollIntoView( newSelection );
								}
							}
						}
						break;

					case KeyCode.DownArrow:
						{
							if( evt.control || evt.command )
							{
								moveNodeDown( selected.First() );
							}
							else
							{
								var allNodes = graph.GetVisibleNodes();
								var selectedIndex = allNodes.IndexOf( selected.First() );
								if( selectedIndex < allNodes.Count - 1 )
								{
									clearSelection();

									var newSelection = allNodes[ selectedIndex + 1 ];
									selectNode( newSelection );
									scrollIntoView( newSelection );
								}
							}
						}
						break;

					case KeyCode.Delete:
						removeSelectedNodes();
						break;
				}
			}
			else
			{
				switch( evt.keyCode )
				{
					case KeyCode.Delete:
						removeSelectedNodes();
						break;

					case KeyCode.Escape:
						// This is just here for those pseudo-random situations where the HTN_EditorWindow instance
						// has focus and won't let you click on other tabs or windows. In those situations, pressing
						// the ESC key will release focus.
						GUIUtility.hotControl = GUIUtility.keyboardControl = 0;
						break;
				}
			}

			Repaint();
		}
	}

	private void handleDefaultMouseDrag( int controlID, Event evt )
	{
		var mousePosition = evt.mousePosition + this.mouseScrollAdjust;
		if( Vector2.Distance( dragStartPosition, mousePosition ) < 3 )
			return;

		if( showInspector && inspectorRect.Contains( dragStartPosition ) )
			return;

		if( evt.button == 0 )
		{
			var hit = graph.HitTest( evt.mousePosition + this.mouseScrollAdjust );
			if( hit != null && !( hit is RootNode ) )
			{
				if( selected.Count == 1 )
				{
					mode = EditorMode.DraggingNode;

					dragNodeOffset = dragStartPosition - new Vector2( hit.Bounds.x, hit.Bounds.y );
					dragNode = hit;

					return;
				}
			}

			mode = EditorMode.DraggingSelection;
			handleDraggingSelection();

			return;
		}
	}

	private void handleDefaultRightButtonDown( Event evt, GraphNodeBase targetNode )
	{
		if( targetNode != null )
		{
			if( !evt.control )
				clearSelection();

			selectNode( targetNode );

			if( selected.Count == 1 )
			{
				defer( () =>
				{
					showNodeContextMenu( targetNode );
				} );
			}
		}
		else
		{
			showCanvasContextMenu();
		}
	}

	private void handleDefaultLeftButtonDown( int controlID, Event evt, GraphNodeBase targetNode )
	{
		dragStartPosition = dragEndPosition = evt.mousePosition + this.mouseScrollAdjust;

		if( targetNode != null )
		{
			scrollIntoView( targetNode );

			// Remove focus from the node inspector text fields (sheesh)
			EditorGUIUtility.hotControl = EditorGUIUtility.keyboardControl = 0;

			if( !targetNode.IsSelected )
			{
				if( !evt.control )
					clearSelection();

				selectNode( targetNode );
			}

			if( evt.clickCount == 1 )
			{
				var hitType = targetNode.HitTest( Event.current.mousePosition + mouseScrollAdjust );
				if( hitType == NodeHitType.Decorator )
				{
					if( targetNode.IsExpanded )
						targetNode.Collapse();
					else
						targetNode.Expand();
				}
			}
			else if( evt.clickCount == 2 )
			{
				if( targetNode is LinkNode && ( (LinkNode)targetNode ).LinkedNode != null )
				{
					//var hitType = targetNode.HitTest( Event.current.mousePosition + mouseScrollAdjust );
					//if( hitType == NodeHitType.Link )
					{
						var linkedNode = ( (LinkNode)targetNode ).LinkedNode;
						clearSelection();
						selectNode( linkedNode );
						scrollIntoView( linkedNode );
					}
				}

				var composite = targetNode as CompositeNode;
				if( composite != null )
				{
					if( composite.IsExpanded )
						composite.Collapse();
					else
						composite.Expand();
				}
			}
		}
		else
		{
			var resizeHandleRect = new Rect( Screen.width - inspectorWidth - 3, inspectorRect.y, 10, inspectorRect.height );
			if( showInspector && resizeHandleRect.Contains( evt.mousePosition ) )
			{
				mode = EditorMode.ResizingsInspector;
			}

			if( !inspectorRect.Contains( evt.mousePosition ) )
			{
				clearSelection();
			}
		}
	}

	private bool ignoreToolWindowEvent( Event evt )
	{
		var ignore = false;

		if( showInspector && inspectorRect.Contains( evt.mousePosition ) )
		{
			ignore = true;
			mode = EditorMode.Inspector;
		}

		if( toolbarRect.Contains( evt.mousePosition ) )
		{
			ignore = true;
		}

		if( ignore )
		{
			evt.Use();
		}

		return ignore;
	}

	private void removeSelectedNodes()
	{
		var message = selected.Count == 1 ? "Are you sure you want to delete this node?" : "Are you sure you want to delete these nodes?";
		if( !EditorUtility.DisplayDialog( "Delete Node(s)", message, "Yes", "No" ) )
		{
			return;
		}

		markUndo( "Delete node(s)" );

		selected.RemoveAll( x => x is RootNode );

		for( int i = 0; i < selected.Count; i++ )
		{
			graph.Nodes.AddRange( selected[ i ].ChildNodes );
			selected[ i ].ChildNodes.Clear();
		}

		graph.Nodes = graph.Nodes.Distinct().ToList();

		var nodes = graph.GetAllNodes();
		foreach( var node in nodes )
		{
			for( int i = 0; i < selected.Count; i++ )
			{
				selected[ i ].ChildNodes.Clear();
				node.RemoveChild( selected[ i ] );
			}
		}

		for( int i = 0; i < selected.Count; i++ )
		{
			graph.Nodes.Remove( selected[ i ] );
		}

		clearSelection();

		this.Focus();
		Repaint();
	}

	private void handleDraggingNode()
	{
		var evt = Event.current;
		var mouseUp = evt.type == EventType.mouseUp || evt.rawType == EventType.mouseUp;

		if( evt.type == EventType.keyDown && evt.keyCode == KeyCode.Escape )
		{
			this.mode = EditorMode.Default;
			return;
		}

		autoScrollGraph( evt );

		var dragPos = evt.mousePosition - dragNodeOffset;
		var dragRect = new Rect( dragPos.x, dragPos.y, dragNode.Bounds.width, dragNode.Bounds.height );

		dragNode.OnDragGUI( dragPos );

		var dropTarget = getDropTarget( dragRect );

		var dropType = NodeHitType.None;

		if( dropTarget != null )
		{
			dropType = ( dragNode is CompositeNode && dropTarget is LinkNode )
				? dropTarget.HitTest( evt.mousePosition + mouseScrollAdjust )
				: NodeHitType.Node;
		}

		if( dropType != NodeHitType.None && canDropNode( dragNode, dropTarget, dropType ) )
		{
			drawDragDropIndicator( dropType, dropTarget );

			if( mouseUp )
			{
				if( dropType == NodeHitType.Link )
					handleLinkingNode( dragNode, dropTarget );
				else
					handleDroppingNode( dragNode, dropTarget );
			}
		}
		else
		{
			setCursor( MouseCursor.MoveArrow );
		}

		if( mouseUp )
		{
			mode = EditorMode.Default;
			return;
		}
	}

	private bool canDropNode( GraphNodeBase dragNode, GraphNodeBase dropTarget, NodeHitType hitType )
	{
		if( hitType == NodeHitType.Link && dragNode is CompositeNode )
			return true;

		if( isNodeDescendentOf( dropTarget, dragNode ) )
			return false;

		if( dropTarget is CompositeNode )
			return true;

		if( dropTarget is RootNode )
			return ( dragNode is CompositeNode );

		return true;
	}

	private bool isNodeDescendentOf( GraphNodeBase potentialDescendent, GraphNodeBase nodeToCheck )
	{
		while( potentialDescendent != null )
		{
			if( potentialDescendent == nodeToCheck )
				return true;
			potentialDescendent = graph.GetParentNode( potentialDescendent );
		}

		return false;
	}

	private void handleLinkingNode( GraphNodeBase dragNode, GraphNodeBase dropTarget )
	{
		var linkNode = dropTarget as LinkNode;
		if( linkNode == null )
			return;

		markUndo( "Link Node" );
		linkNode.LinkedNode = dragNode;
	}

	private void handleDroppingNode( GraphNodeBase dragNode, GraphNodeBase dropTarget )
	{
		markUndo( "Move Node" );

		var dragParent = graph.GetParentNode( dragNode );
		dragParent.ChildNodes.Remove( dragNode );

		if( dropTarget is CompositeNode || dropTarget is RootNode )
		{
			dropTarget.ChildNodes.Insert( 0, dragNode );
		}
		else
		{
			var dropParent = graph.GetParentNode( dropTarget );
			var dropIndex = dropParent.ChildNodes.IndexOf( dropTarget );

			dropParent.ChildNodes.Insert( dropIndex + 1, dragNode );
		}

		ensureExpanded( dropTarget );
		updateTreeLayout();
		scrollIntoView( dropTarget );
	}

	private GraphNodeBase getDropTarget( Rect dragRect )
	{
		var visibleNodes = graph.GetVisibleNodes();

		foreach( var target in visibleNodes )
		{
			var targetRect = getNodeBounds( target );
			if( targetRect.Overlaps( dragRect ) )
			{
				return target;
			}
		}

		return null;
	}

	private void drawDragDropIndicator( NodeHitType hitType, GraphNodeBase target )
	{
		var bounds = getNodeBounds( target );
		GUI.Box( bounds, GUIContent.none, (GUIStyle)"TL SelectionButton PreDropGlow" );

		if( hitType == NodeHitType.Link )
		{
			setCursor( MouseCursor.Link );
		}
		else
		{
			setCursor( MouseCursor.ArrowPlus );

			var insertStyle = DesignUtil.GetStyle( "htn_drag_drop_indicator", (GUIStyle)"PR Insertion" );

			var isComposite = target is RootNode || target is CompositeNode;
			var left = Mathf.Max( 45, isComposite ? bounds.x + 40 : bounds.x - 10 );

			var rect = new Rect(
				left,
				bounds.y + target.Bounds.height - 8,
				Mathf.Max( 250, bounds.width + 50 ),
				20
			);

			GUI.Box( rect, GUIContent.none, insertStyle );
		}
	}

	private void setCursor( MouseCursor cursor )
	{
		var mousePos = Event.current.mousePosition;
		var rect = new Rect( mousePos.x - 10, mousePos.y - 10, 20, 20 );

		EditorGUIUtility.AddCursorRect( rect, cursor, this.canvasControlID );
	}

	private Rect getNodeBounds( GraphNodeBase node )
	{
		var bounds = node.Bounds;
		bounds.position -= mouseScrollAdjust;

		return bounds;
	}

	private void handleDraggingSelection()
	{
		var evt = Event.current;

		var mousePosition = evt.mousePosition + this.mouseScrollAdjust;
		var cursorRect = new Rect( mousePosition.x - 20, mousePosition.y - 20, 40, 40 );
		EditorGUIUtility.AddCursorRect( cursorRect, MouseCursor.SlideArrow );

		dragEndPosition = mousePosition;

		autoScrollGraph( evt );

		if( evt.type == EventType.repaint )
		{
			clearSelection();

			var selectionRect = rectFromPoints( dragStartPosition, mousePosition );

			var allNodes = graph.GetAllNodes();
			for( int i = 0; i < allNodes.Count; i++ )
			{
				var node = allNodes[ i ];
				if( node.Intersects( selectionRect ) )
				{
					node.IsSelected = true;
					selected.Add( node );
				}
			}
		}

		if( evt.type == EventType.mouseUp || evt.rawType == EventType.mouseUp )
		{
			mode = EditorMode.Default;
		}

		Repaint();
	}

	private void autoScrollGraph( Event evt )
	{
		if( evt.type == EventType.repaint )
		{
			const int PANNING_SPEED = 3;
			const int GUTTER_SIZE_X = 25;
			const int GUTTER_SIZE_y = 50;

			var inspectorWidth = this.showInspector ? this.inspectorWidth : 30;

			var delta = Vector2.zero;
			var viewportTop = 30 + GUTTER_SIZE_X;
			var viewportRight = Screen.width - inspectorWidth - GUTTER_SIZE_X;
			var viewportBottom = Screen.height - 20 - GUTTER_SIZE_y;

			if( evt.mousePosition.x < GUTTER_SIZE_y )
			{
				delta.x = PANNING_SPEED;
			}
			else if( evt.mousePosition.x > viewportRight )
			{
				delta.x = -PANNING_SPEED;
			}

			if( evt.mousePosition.y < viewportTop )
			{
				delta.y = PANNING_SPEED;
			}
			else if( evt.mousePosition.y > viewportBottom )
			{
				delta.y = -PANNING_SPEED;
			}

			if( delta != Vector2.zero )
			{
				this.scrollPos -= delta;
				Repaint();
			}
		}
	}

	private void showCanvasContextMenu()
	{
		var options = new List<DesignerMenuItem>();

		var rootNode = graph.RootNode;

		if( rootNode.ChildNodes.Count > 0 )
		{
			options.Add( new DesignerMenuItem( "Select/ROOT", () =>
			{
				clearSelection();
				selectBranch( rootNode );
				scrollIntoView( rootNode );
			} ) );

			for( int i = 0; i < rootNode.ChildNodes.Count; i++ )
			{
				var childNode = rootNode.ChildNodes[ i ];
				fillSelectNodeContextMenu( "Select", childNode, options );
			}
		}

		if( options.Count == 0 )
			return;

		showContextMenu( options );
	}

	private void fillSelectNodeContextMenu( string prefix, GraphNodeBase node, List<DesignerMenuItem> options )
	{
		var menuText = string.Format( "{0}/{1}", prefix, node.Name );

		options.Add( new DesignerMenuItem( menuText + ( node.ChildNodes.Count > 0 ? "/* " + node.Name : "" ), () =>
		{
			clearSelection();
			selectBranch( node );
			scrollIntoView( node );
		} ) );

		for( int i = 0; i < node.ChildNodes.Count; i++ )
		{
			fillSelectNodeContextMenu( menuText, node.ChildNodes[ i ], options );
		}
	}

	private void showNodeContextMenu( GraphNodeBase node )
	{
		var options = new List<DesignerMenuItem>();

		if( node.CanAttach<CompositeNode>() )
		{
			options.Add( new DesignerMenuItem( "Add New/Composite Task/Select All", () =>
			{
				markUndo( "Add Child Node" );

				var newMethod = new CompositeNode()
				{
					Bounds = getNewChildNodePosition( node ),
					Name = "Sequence",
					Mode = DecompositionMode.SelectAll
				};

				addNewChild( node, newMethod );
				node.Expand();

				clearSelection();
				selectNode( newMethod );
				scrollIntoView( newMethod );
			} ) );

			options.Add( new DesignerMenuItem( "Add New/Composite Task/Select One", () =>
			{
				markUndo( "Add Child Node" );

				var newMethod = new CompositeNode()
				{
					Bounds = getNewChildNodePosition( node ),
					Name = "Selector",
					Mode = DecompositionMode.SelectOne
				};

				addNewChild( node, newMethod );
				node.Expand();

				clearSelection();
				selectNode( newMethod );
				scrollIntoView( newMethod );
			} ) );
		}

		if( node.CanAttach<OperatorNode>() )
		{
			fillAddOperatorMenu( options );
		}

		if( node.CanAttach<LinkNode>() )
		{
			options.Add( new DesignerMenuItem( "Add New/-", null ) );

			options.Add( new DesignerMenuItem( "Add New/GOTO", () =>
			{
				markUndo( "Add GOTO Node" );

				var newTask = new LinkNode()
				{
					Bounds = getNewChildNodePosition( node ),
					Name = "GOTO"
				};

				addNewChild( node, newTask );
				node.Expand();

				clearSelection();
				selectNode( newTask );
				scrollIntoView( newTask );
			} ) );
		}

		if( !( node is RootNode ) )
		{
			if( options.Count > 0 )
			{
				options.Add( new DesignerMenuItem( "-", null ) );
			}

			var parentNode = graph.GetParentNode( node );
			var childIndex = parentNode.ChildNodes.IndexOf( node );

			if( childIndex > 0 )
			{
				options.Add( new DesignerMenuItem( "Move/First\t(CTRL-PgUp)", () =>
				{
					moveNodeToTop( node );
				} ) );

				options.Add( new DesignerMenuItem( "Move/Up\t(CTRL-UpArrow)", () =>
				{
					moveNodeUp( node );
				} ) );
			}

			if( childIndex < parentNode.ChildNodes.Count - 1 )
			{
				options.Add( new DesignerMenuItem( "Move/Down\t(CTRL-DownArrow)", () =>
				{
					moveNodeDown( node );
				} ) );

				options.Add( new DesignerMenuItem( "Move/Last\t(CTRL-PgDown)", () =>
				{
					moveNodeToBottom( node );
				} ) );
			}

			options.Add( new DesignerMenuItem( "Delete", () =>
			{
				removeSelectedNodes();
			} ) );
		}

		if( node is OperatorNode )
		{

			var actionNode = node as OperatorNode;

			options.Add( new DesignerMenuItem( "Toggle Breakpoint", () =>
			{
				actionNode.PauseOnRun = !actionNode.PauseOnRun;
			} ) );

		}

		if( options.Count == 0 )
		{
			mode = EditorMode.Default;
			return;
		}

		showContextMenu( options );
	}

	private void addNewChild( GraphNodeBase node, GraphNodeBase child )
	{
		updateTreeLayout();

		if( node is RootNode )
			node.ChildNodes.Insert( 0, child );
		else
			node.AddChild( child );

		nodeToScroll = child;
	}

	private void moveNodeToBottom( GraphNodeBase node )
	{
		var parentNode = graph.GetParentNode( node );
		if( parentNode == null )
			return;

		var childIndex = parentNode.ChildNodes.IndexOf( node );
		if( childIndex >= parentNode.ChildNodes.Count - 1 )
			return;

		markUndo( "Move node" );

		parentNode.ChildNodes.Remove( node );
		parentNode.ChildNodes.Add( node );

		updateTreeLayout();
		scrollIntoView( node );
	}

	private void moveNodeDown( GraphNodeBase node )
	{
		var parentNode = graph.GetParentNode( node );
		if( parentNode == null )
			return;

		var childIndex = parentNode.ChildNodes.IndexOf( node );
		if( childIndex >= parentNode.ChildNodes.Count - 1 )
			return;

		markUndo( "Move node" );

		parentNode.ChildNodes.Remove( node );
		parentNode.ChildNodes.Insert( childIndex + 1, node );

		drawNodes();
		scrollIntoView( node );
	}

	private void moveNodeUp( GraphNodeBase node )
	{
		var parentNode = graph.GetParentNode( node );
		if( parentNode == null )
			return;

		var childIndex = parentNode.ChildNodes.IndexOf( node );
		if( childIndex <= 0 )
			return;

		markUndo( "Move node" );

		parentNode.ChildNodes.Remove( node );
		parentNode.ChildNodes.Insert( childIndex - 1, node );

		updateTreeLayout();
		scrollIntoView( node );
	}

	private void moveNodeToTop( GraphNodeBase node )
	{
		var parentNode = graph.GetParentNode( node );
		if( parentNode == null )
			return;

		var childIndex = parentNode.ChildNodes.IndexOf( node );
		if( childIndex <= 0 )
			return;

		markUndo( "Move node" );

		parentNode.ChildNodes.Remove( node );
		parentNode.ChildNodes.Insert( 0, node );

		updateTreeLayout();
		scrollIntoView( node );
	}

	private void selectBranch( GraphNodeBase node )
	{
		for( int i = 0; i < node.ChildNodes.Count; i++ )
		{
			selectBranch( node.ChildNodes[ i ] );
		}

		selectNode( node );
	}

	private void showContextMenu( List<DesignerMenuItem> options )
	{
		var menu = new GenericMenu();

		for( int i = 0; i < options.Count; i++ )
		{
			var index = i;

			if( options[ i ].Text == "-" )
			{
				menu.AddSeparator( "" );
			}
			else if( options[ i ].Text.EndsWith( "-" ) )
			{
				menu.AddSeparator( options[ i ].Text.Substring( 0, options[ i ].Text.Length - 1 ) );
			}
			else
			{
				menu.AddItem( new GUIContent( options[ i ].Text ), false, () =>
				{
					var callback = options[ index ].Callback;
					if( callback != null )
					{
						defer( callback );
					}
				} );
			}
		}

		menu.ShowAsContext();
	}

	private Rect getNewChildNodePosition( GraphNodeBase node )
	{
		var rect = node.Bounds;

		rect.x += 25;
		rect.y += rect.height + 15;

		for( int i = 0; i < node.ChildNodes.Count; i++ )
		{
			var childRect = getNewChildNodePosition( node.ChildNodes[ i ] );
			rect.y = childRect.yMax + 15;
		}

		return rect;
	}

	private void fillAddOperatorMenu( List<DesignerMenuItem> options )
	{
		var node = selected.FirstOrDefault();

		var type = graph.DomainType;
		if( type == null )
			return;

		var methods = type.GetMethods();
		Array.Sort( methods, ( lhs, rhs ) =>
		{
			return lhs.Name.CompareTo( rhs.Name );
		} );

		for( int i = 0; i < methods.Length; i++ )
		{
			var method = methods[ i ];

			if( !isOperatorMethod( method, false ) )
				continue;

			var attribute = method.GetCustomAttributes( typeof( OperatorAttribute ), true ).FirstOrDefault() as OperatorAttribute;

			var name = ObjectNames.NicifyVariableName( method.Name );
			if( attribute != null && !string.IsNullOrEmpty( attribute.Label ) )
			{
				name = attribute.Label;
			}

			options.Add( new DesignerMenuItem( "Add New/Task/" + name, () =>
			{
				markUndo( "Add Task" );

				var newTask = new OperatorNode()
				{
					Method = method,
					Arguments = buildArgumentList( method ),
					Bounds = getNewChildNodePosition( node ),
					Name = name
				};

				if( attribute != null && !string.IsNullOrEmpty( attribute.Description ) )
				{
					newTask.Notes = attribute.Description;
				}

				addNewChild( node, newTask );
				node.Expand();

				clearSelection();
				selectNode( newTask );
				scrollIntoView( newTask );
			} ) );
		}

		fillLibraryOperatorMenu( options, node );
	}

	private void fillLibraryOperatorMenu( List<DesignerMenuItem> options, GraphNodeBase node )
	{
		bool separatorAdded = false;

		var libraryTypes = DesignUtil.definedTypes
			.Where( x => x.IsDefined( typeof( FunctionLibraryAttribute ), false ) )
			.ToArray();

		foreach( var libraryType in libraryTypes )
		{
			var libraryAttribute = (FunctionLibraryAttribute)libraryType.GetCustomAttributes( typeof( FunctionLibraryAttribute ), false ).FirstOrDefault();
			var libraryName = libraryAttribute.LibraryName;

			var libraryMethods = libraryType
				.GetMethods( BindingFlags.Public | BindingFlags.Static )
				.Where( x =>
					isOperatorMethod( x, true ) &&
					x.DeclaringType == libraryType
				)
				.ToArray();

			Array.Sort( libraryMethods, ( lhs, rhs ) =>
			{
				return lhs.Name.CompareTo( rhs.Name );
			} );

			if( libraryMethods.Length > 0 && !separatorAdded )
			{
				separatorAdded = true;
				options.Add( new DesignerMenuItem( "Add New/Task/-", null ) );
			}

			for( int i = 0; i < libraryMethods.Length; i++ )
			{
				var method = libraryMethods[ i ];
				var attribute = method.GetCustomAttributes( typeof( OperatorAttribute ), true ).FirstOrDefault() as OperatorAttribute;

				var name = ObjectNames.NicifyVariableName( method.Name );
				if( attribute != null && !string.IsNullOrEmpty( attribute.Label ) )
				{
					name = attribute.Label;
				}

				var menuItemName = string.Format( "Add New/Task/{0}/{1}", libraryName, name );

				options.Add( new DesignerMenuItem( menuItemName, () =>
				{
					markUndo( "Add Task" );

					var newTask = new OperatorNode()
					{
						Method = method,
						Arguments = buildArgumentList( method ),
						Bounds = getNewChildNodePosition( node ),
						Name = name
					};

					if( attribute != null && !string.IsNullOrEmpty( attribute.Description ) )
					{
						newTask.Notes = attribute.Description;
					}

					addNewChild( node, newTask );
					node.Expand();

					clearSelection();
					selectNode( newTask );
					scrollIntoView( newTask );
				} ) );
			}
		}
	}

	private List<object> buildArgumentList( MethodInfo method )
	{
		var parameters = method.GetParameters();

		var arguments = new List<object>();

		for( int x = 0; x < parameters.Length; x++ )
		{
			arguments.Add( getDefaultParameterValue( parameters[ x ] ) );
		}

		return arguments;
	}

	private static object getDefaultParameterValue( ParameterInfo parameter )
	{
		if( parameter.IsDefined( typeof( ScriptParameterAttribute ), false ) )
		{
			return "/* Script */";
		}

		var defaultValueAttribute = parameter.GetCustomAttributes( typeof( DefaultValueAttribute ), false ).FirstOrDefault() as DefaultValueAttribute;
		if( defaultValueAttribute != null )
		{
			return defaultValueAttribute.Value;
		}

		if( parameter.IsOptional )
			return parameter.RawDefaultValue;

		var parameterType = parameter.ParameterType;

		if( parameterType.IsValueType )
			return Activator.CreateInstance( parameterType );

		if( parameterType == typeof( string ) )
			return string.Empty;

		return null;
	}

	private bool isEffectMethod( MethodInfo method )
	{
		// Effect methods *must* return a TaskStatus value
		if( method.ReturnType != typeof( StagPoint.Core.TaskStatus ) )
			return false;

		// Need to make sure that, at a minimum, the first parameter is of type Blackboard
		var parameters = method.GetParameters();
		if( parameters.Length == 0 || !typeof( Blackboard ).IsAssignableFrom( parameters[ 0 ].ParameterType ) )
			return false;

		// Assume that the method is valid
		return true;
	}

	private bool isConditionMethod( MethodInfo method )
	{
		// Only public methods are allowed. The designer interface should not
		// allow access to private methods.
		if( !method.IsPublic )
			return false;

		// Precondition methods must return a boolean to indicate whether the
		// condition is satisfied or not.
		if( method.ReturnType != typeof( bool ) )
			return false;

		// Need to make sure that, at a minimum, the first parameter is of type Blackboard
		var parameters = method.GetParameters();
		if( parameters.Length == 0 || !typeof( Blackboard ).IsAssignableFrom( parameters[ 0 ].ParameterType ) )
			return false;

		// Assume that the method is valid
		return true;
	}

	private bool isOperatorMethod( MethodInfo method, bool allowStatic )
	{
		// Only public instance methods allowed. Presumably, instance methods
		// will already have access to any information needed to perform their
		// task, including the active Blackboard instance.
		if( !method.IsPublic || ( method.IsStatic && !allowStatic ) )
			return false;

		// Tasks must always return a status code that can be used to determine
		// the current state of the task (Success or failure, running, paused, etc)
		if( method.ReturnType != typeof( TaskStatus ) )
		{
			// Alternatively, tasks can be defined as an IEnumerator<Status>
			// function. This would be used for long-running tasks rather than
			// instantaneous tasks.
			var iteratorType = typeof( IEnumerator );
			if( !iteratorType.IsAssignableFrom( method.ReturnType ) )
			{
				return false;
			}
		}

		// We don't want to list methods that are defined on base classes
		// such as Monobehaviour.
		if( !allowStatic && method.DeclaringType != graph.DomainType )
			return false;

		if( !string.IsNullOrEmpty( method.DeclaringType.Namespace ) && method.DeclaringType.Namespace.Contains( "StagPoint" ) )
			return false;

		// Ensure that there are no array, list, or dictionary types defined as parameters
		var parameters = method.GetParameters();
		foreach( var parameter in parameters )
		{
			if( typeof( IList ).IsAssignableFrom( parameter.ParameterType ) )
				return false;

			if( typeof( IDictionary ).IsAssignableFrom( parameter.ParameterType ) )
				return false;

			if( !string.IsNullOrEmpty( parameter.ParameterType.Namespace ) && parameter.ParameterType.Namespace.Contains( "StagPoint" ) )
				return false;
		}

		// Assume that the method is valid
		return true;
	}

	private void selectNode( GraphNodeBase node )
	{
		if( node == null )
			return;

		if( !selected.Contains( node ) )
		{
			EditorPrefs.SetString( "last-selected-node", node.UID );

			node.IsSelected = true;
			selected.Add( node );
		}
	}

	private void selectNodes( IEnumerable<GraphNodeBase> nodes )
	{
		foreach( var node in nodes )
		{
			selectNode( node );
		}
	}

	private void clearSelection()
	{
		for( int i = 0; i < selected.Count; i++ )
		{
			selected[ i ].IsSelected = false;
		}

		selected.Clear();

		if( graph != null )
		{
			var nodes = graph.GetAllNodes();

			for( int i = 0; i < nodes.Count; i++ )
			{
				nodes[ i ].IsSelected = false;
			}
		}
	}

	private static void drawSelectionRect( Vector2 from, Vector2 to )
	{
		GUI.Box( rectFromPoints( from, to ), GUIContent.none, (GUIStyle)"SelectionRect" );
	}

	private static Rect rectFromPoints( Vector2 a, Vector2 b )
	{
		var min = Vector2.Min( a, b );
		var max = Vector2.Max( a, b );
		return new Rect( min.x, min.y, max.x - min.x, max.y - min.y );
	}

	private void inspectNode()
	{
		var controlID = EditorGUIUtility.GetControlID( "Task Network Node Inspector".GetHashCode(), FocusType.Native );

		var height = Mathf.Max( this.position.height - 12, 480 );

		this.inspectorRect = new Rect( Screen.width - inspectorWidth, 18, inspectorWidth - 5, height );

		var inspectorCursorRect = new Rect( inspectorRect.x + 5, inspectorRect.y, inspectorRect.width, inspectorRect.height );
		EditorGUIUtility.AddCursorRect( inspectorCursorRect, MouseCursor.Arrow, controlID );

		var resizeHandleRect = new Rect( Screen.width - inspectorWidth, inspectorRect.y, 8, inspectorRect.height );
		EditorGUIUtility.AddCursorRect( resizeHandleRect, MouseCursor.ResizeHorizontal, controlID );

		var backStyle = DesignUtil.GetStyle( "sv_iconselector_back" );
		backStyle.padding = new RectOffset();

		var shadowStyle = DesignUtil.GetStyle( "inspector-shadow", "IN ThumbnailShadow" );
		GUI.Box( inspectorCursorRect, GUIContent.none, shadowStyle );

		EditorGUIUtility.labelWidth = Mathf.Max( 100, inspectorWidth * 0.25f );
		EditorGUIUtility.fieldWidth = inspectorWidth - EditorGUIUtility.labelWidth - 10;

		GUILayout.BeginArea( inspectorRect, backStyle );
		{
			var tabStyle = DesignUtil.GetStyle( "dragtabdropwindow" );
			tabStyle.fontStyle = FontStyle.Bold;

			GUILayout.Label( "Edit Node", tabStyle, GUILayout.ExpandWidth( true ) );
			GUILayout.Space( 10 );

			if( selected.Count == 1 )
			{
				var node = selected.First();

				EditorGUI.BeginChangeCheck();
				{
					node.OnInspectorGUI( graph );
				}

				if( EditorGUI.EndChangeCheck() )
				{
					if( Event.current.isKey )
					{
						Event.current.Use();
					}
				}

				if( !( node is RootNode ) )
				{
					if( DesignUtil.SectionFoldout( "node-conditions", "Conditions and Effects" ) )
					{
						inspectNodeConditions( node.Conditions );

						if( node is OperatorNode )
						{
							GUILayout.Space( 10 );
							inspectNodeEffects( node.Effects );
						}
					}

					DesignUtil.DrawHorzLine();
				}
			}
			else if( selected.Count == 0 )
			{
				EditorGUILayout.HelpBox( "Select a graph node to edit its configuration", MessageType.Info );
			}
			else
			{
				inspectMultipleNodes();
			}
		}
		GUILayout.EndArea();
	}

	private void inspectNodeConditions( List<NodePreconditionBase> conditions )
	{
		DesignUtil.DoGroup( "node-preconditions", "Pre-Conditions",
			() =>
			{
				var agent = (object)null;
				if( Selection.activeGameObject != null )
				{
					agent = Selection.activeGameObject.GetComponent( graph.DomainType );
				}

				var itemToRemove = -1;

				if( conditions.Count == 0 )
				{
					GUILayout.Space( 5 );
					GUILayout.Label( " There are no Pre-Conditions defined ", DesignUtil.elementBackground );
				}
				else
				{
					EditorGUIUtility.fieldWidth -= 65;

					for( int i = 0; i < conditions.Count; i++ )
					{
						var condition = conditions[ i ];

						EditorGUIUtility.GetControlID( condition.GetHashCode(), FocusType.Passive );

						var isExpanded = false;
						GUILayout.BeginHorizontal();
						{
							isExpanded = condition.DoFoldout( inspectorWidth - 65 );
							if( GUILayout.Button( DesignUtil.iconToolbarMinus, DesignUtil.elementBackground, GUILayout.Width( 20 ) ) )
							{
								itemToRemove = i;
							}
						}
						GUILayout.EndHorizontal();

						if( isExpanded )
						{
							GUILayout.Space( 5 );
							GUILayout.BeginHorizontal();
							{
								GUILayout.Space( 25 );
								GUILayout.BeginVertical();
								{
									EditorGUI.BeginChangeCheck();
									{
										condition.OnInspectorGUI( inspectorWidth - 85, agent, graph.BlackboardDefinition );
									}
									if( EditorGUI.EndChangeCheck() )
									{
										EditorUtility.SetDirty( graph );
									}
								}
								GUILayout.EndVertical();
							}
							GUILayout.EndHorizontal();
						}
					}

					EditorGUIUtility.fieldWidth += 65;

					if( itemToRemove != -1 )
					{
						markUndo( "Remove Pre-Condition" );
						conditions.RemoveAt( itemToRemove );
					}
				}

				GUILayout.Space( 5 );
			},
			() =>
			{
				GUILayout.BeginHorizontal( DesignUtil.groupContainer );
				{
					GUILayout.FlexibleSpace();
					if( GUILayout.Button( DesignUtil.iconToolbarPlus, DesignUtil.footerBackground, GUILayout.Width( 30 ) ) )
					{
						showAddConditionContextMenu( conditions );
					}
				}
				GUILayout.EndHorizontal();
			}
		);
	}

	private void showAddConditionContextMenu( List<NodePreconditionBase> conditions )
	{
		var options = new List<DesignerMenuItem>();

		#region Add "Script expression" precondition

		var item = new DesignerMenuItem( "Expression", () =>
		{
			markUndo( "Add Pre-Condition" );
			conditions.Add( new EvalCondition() { Script = "true" } );
		} );

		options.Add( item );

		#endregion Add "Script expression" precondition

		fillBlackboardVariableConditionMenu( options, conditions );
		fillPredefinedConditionMenu( options, conditions );

		showContextMenu( options );
	}

	private void fillBlackboardVariableConditionMenu( List<DesignerMenuItem> options, List<NodePreconditionBase> conditions )
	{
		var blackboard = graph.BlackboardDefinition;
		if( blackboard == null )
			return;

		var keys = blackboard.GetKeys();
		keys.RemoveAll( x => conditions.Any( y => y is VariableCondition && ( (VariableCondition)y ).VariableName == x ) );
		keys.Sort();

		if( keys.Count == 0 )
			return;

		for( int i = 0; i < keys.Count; i++ )
		{
			
			var key = keys[ i ];
			
			var variableType = blackboard.GetVariableType( key );
			if( variableType == null )
				continue;

			var defaultValue = blackboard.GetVariable( key ).GetValue<object>();
			var text = string.Format( "Blackboard Variable/{0}", key );

			var defaultCompareType = ConditionType.EqualTo;
			if( variableType == typeof( bool ) )
				defaultCompareType = ConditionType.IsTrue;
			else if( !variableType.IsValueType ) 
				defaultCompareType = ConditionType.IsNotNull;

			var item = new DesignerMenuItem( text, () =>
			{
				markUndo( "Add Pre-Condition" );
				conditions.Add( new VariableCondition( key, defaultCompareType, defaultValue ) );
			} );

			options.Add( item );
		}
	}

	private void fillPredefinedConditionMenu( List<DesignerMenuItem> options, List<NodePreconditionBase> conditions )
	{
		var type = graph.DomainType;
		if( type == null )
			return;

		var category = ObjectNames.NicifyVariableName( type.Name.Replace( "_", " " ).Trim() );

		var methods = type.GetMethods( BindingFlags.Public | BindingFlags.Instance )
			.Where( x =>
				isConditionMethod( x ) &&
				x.DeclaringType == graph.DomainType &&
				!conditions.Any( y => y is MethodCondition && ( (MethodCondition)y ).Method == x )
			)
			.ToArray();

		if( methods.Length > 0 )
		{
			Array.Sort( methods, ( lhs, rhs ) =>
			{
				return lhs.Name.CompareTo( rhs.Name );
			} );

			if( options.Count > 0 )
			{
				options.Add( new DesignerMenuItem( "-", null ) );
			}

			for( int i = 0; i < methods.Length; i++ )
			{
				var method = methods[ i ];

				var name = ObjectNames.NicifyVariableName( method.Name );

				var conditionAttribute = (ConditionAttribute)method.GetCustomAttributes( typeof( ConditionAttribute ), true ).FirstOrDefault();
				if( conditionAttribute != null && !string.IsNullOrEmpty( conditionAttribute.Label ) )
				{
					name = conditionAttribute.Label;
				}

				var menuItemName = string.Format( "{0}/{1}", category, name );
				options.Add( new DesignerMenuItem( menuItemName, () =>
				{
					markUndo( "Add Pre-Condition" );

					var arguments = buildArgumentList( method );

					conditions.Add( new MethodCondition()
					{
						Method = method,
						Arguments = arguments
					} );
				} ) );
			}
		}

		var libraryTypes = DesignUtil.definedTypes
			.Where( x => x.IsDefined( typeof( FunctionLibraryAttribute ), false ) )
			.ToArray();

		foreach( var libraryType in libraryTypes )
		{
			var libraryAttribute = (FunctionLibraryAttribute)libraryType.GetCustomAttributes( typeof( FunctionLibraryAttribute ), false ).FirstOrDefault();
			var libraryName = libraryAttribute.LibraryName;

			var libraryMethods = libraryType
				.GetMethods( BindingFlags.Public | BindingFlags.Static )
				.Where( x =>
					isConditionMethod( x ) &&
					x.DeclaringType == libraryType
				)
				.ToArray();

			Array.Sort( libraryMethods, ( lhs, rhs ) =>
			{
				return lhs.Name.CompareTo( rhs.Name );
			} );

			for( int i = 0; i < libraryMethods.Length; i++ )
			{
				var method = libraryMethods[ i ];
				var name = ObjectNames.NicifyVariableName( method.Name );

				var conditionAttribute = (ConditionAttribute)method.GetCustomAttributes( typeof( ConditionAttribute ), true ).FirstOrDefault();
				if( conditionAttribute != null && !string.IsNullOrEmpty( conditionAttribute.Label ) )
				{
					name = conditionAttribute.Label;
				}

				var menuItemName = string.Format( "{0}/{1}", libraryName, name );

				options.Add( new DesignerMenuItem( menuItemName, () =>
				{
					markUndo( "Add Pre-Condition" );

					var arguments = buildArgumentList( method );

					conditions.Add( new MethodCondition()
					{
						Method = method,
						Arguments = arguments
					} );
				} ) );
			}
		}
	}

	private void inspectNodeEffects( List<NodeEffectBase> effects )
	{
		DesignUtil.DoGroup( "node-effects", "Effects",
			() =>
			{
				var agent = (object)null;
				if( Selection.activeGameObject != null )
				{
					agent = Selection.activeGameObject.GetComponent( graph.DomainType );
				}

				var itemToRemove = -1;

				if( effects.Count == 0 )
				{
					GUILayout.Space( 5 );
					GUILayout.Label( " There are no Effects defined ", DesignUtil.elementBackground );
				}
				else
				{
					EditorGUIUtility.fieldWidth -= 65;

					for( int i = 0; i < effects.Count; i++ )
					{
						var effect = effects[ i ];

						EditorGUIUtility.GetControlID( effect.GetHashCode(), FocusType.Passive );

						var isExpanded = false;
						GUILayout.BeginHorizontal( GUILayout.Width( inspectorWidth - 55 ) );
						{
							isExpanded = effect.DoFoldout( inspectorWidth - 65 );
							if( GUILayout.Button( DesignUtil.iconToolbarMinus, DesignUtil.elementBackground, GUILayout.Width( 20 ) ) )
							{
								itemToRemove = i;
							}
						}
						GUILayout.EndHorizontal();

						if( isExpanded )
						{
							GUILayout.Space( 5 );
							GUILayout.BeginHorizontal();
							{
								GUILayout.Space( 25 );
								GUILayout.BeginVertical();
								{
									EditorGUI.BeginChangeCheck();
									{
										effect.OnInspectorGUI( inspectorWidth - 85, agent, graph.BlackboardDefinition );
									}
									if( EditorGUI.EndChangeCheck() )
									{
										EditorUtility.SetDirty( graph );
									}
								}
								GUILayout.EndVertical();
							}
							GUILayout.EndHorizontal();
						}
					}

					EditorGUIUtility.fieldWidth += 65;

					if( itemToRemove != -1 )
					{
						markUndo( "Remove Effect" );
						effects.RemoveAt( itemToRemove );
					}
				}

				GUILayout.Space( 5 );
			},
			() =>
			{
				GUILayout.BeginHorizontal( DesignUtil.groupContainer );
				{
					GUILayout.FlexibleSpace();
					if( GUILayout.Button( DesignUtil.iconToolbarPlus, DesignUtil.footerBackground, GUILayout.Width( 30 ) ) )
					{
						showAddEffectContextMenu( effects );
					}
				}
				GUILayout.EndHorizontal();
			}
		);
	}

	private void showAddEffectContextMenu( List<NodeEffectBase> effects )
	{
		var options = new List<DesignerMenuItem>();

		#region Add "Script expression" precondition

		var item = new DesignerMenuItem( "Expression", () =>
		{
			markUndo( "Add Effect" );
			effects.Add( new EvalEffect() );
		} );

		options.Add( item );

		#endregion Add "Script expression" precondition

		fillBlackboardVariableEffectMenu( options, effects );
		fillPredefinedEffectMenu( options, effects );

		showContextMenu( options );
	}

	private void fillBlackboardVariableEffectMenu( List<DesignerMenuItem> options, List<NodeEffectBase> effects )
	{
		var blackboard = graph.BlackboardDefinition;

		var keys = blackboard.GetKeys();
		keys.RemoveAll( x => effects.Any( y => y is VariableEffect && ( (VariableEffect)y ).VariableName == x ) );
		keys.Sort();

		if( keys.Count == 0 )
			return;

		for( int i = 0; i < keys.Count; i++ )
		{
			var key = keys[ i ];
			var defaultValue = blackboard.GetVariable( key ).GetValue<object>();
			var text = string.Format( "Blackboard Variable/{0}", key );

			var item = new DesignerMenuItem( text, () =>
			{
				markUndo( "Add Effect" );

				var newEffect = new VariableEffect()
				{
					VariableName = key,
					ActionType = EffectType.SetValue,
					Argument = defaultValue
				};

				effects.Add( newEffect );
			} );

			options.Add( item );
		}
	}

	private void fillPredefinedEffectMenu( List<DesignerMenuItem> options, List<NodeEffectBase> effects )
	{
		var type = graph.DomainType;
		if( type == null )
			return;

		var category = ObjectNames.NicifyVariableName( type.Name.Replace( "_", " " ).Trim() );

		var methods = type
			.GetMethods( BindingFlags.Public | BindingFlags.Instance )
			.Where( x =>
				isEffectMethod( x ) &&
					// We don't want to list methods that are defined on base classes
					// such as Monobehaviour.
				x.DeclaringType == graph.DomainType
			)
			.ToArray();

		if( methods.Length > 0 )
		{
			Array.Sort( methods, ( lhs, rhs ) =>
			{
				return lhs.Name.CompareTo( rhs.Name );
			} );

			options.Add( new DesignerMenuItem( "-", null ) );

			for( int i = 0; i < methods.Length; i++ )
			{
				var method = methods[ i ];

				var name = ObjectNames.NicifyVariableName( method.Name );

				var conditionAttribute = (EffectAttribute)method.GetCustomAttributes( typeof( EffectAttribute ), true ).FirstOrDefault();
				if( conditionAttribute != null )
				{
					name = conditionAttribute.Label;
				}

				var menuItemName = string.Format( "{0}/{1}", category, name );
				options.Add( new DesignerMenuItem( menuItemName, () =>
				{
					markUndo( "Add Effect" );

					var arguments = buildArgumentList( method );

					var newEffect = new MethodEffect()
					{
						Method = method,
						Arguments = arguments
					};

					effects.Add( newEffect );
				} ) );
			}
		}

		var libraryTypes = DesignUtil.definedTypes
			.Where( x => x.IsDefined( typeof( FunctionLibraryAttribute ), false ) )
			.ToArray();

		foreach( var libraryType in libraryTypes )
		{
			var libraryAttribute = (FunctionLibraryAttribute)libraryType.GetCustomAttributes( typeof( FunctionLibraryAttribute ), false ).FirstOrDefault();
			var libraryName = libraryAttribute.LibraryName;

			var libraryMethods = libraryType
				.GetMethods( BindingFlags.Public | BindingFlags.Static )
				.Where( x =>
					isEffectMethod( x ) &&
					x.DeclaringType == libraryType
				)
				.ToArray();

			Array.Sort( libraryMethods, ( lhs, rhs ) =>
			{
				return lhs.Name.CompareTo( rhs.Name );
			} );

			for( int i = 0; i < libraryMethods.Length; i++ )
			{
				var method = libraryMethods[ i ];
				var name = ObjectNames.NicifyVariableName( method.Name );

				var effectAttribute = (EffectAttribute)method.GetCustomAttributes( typeof( EffectAttribute ), true ).FirstOrDefault();
				if( effectAttribute != null && !string.IsNullOrEmpty( effectAttribute.Label ) )
				{
					name = effectAttribute.Label;
				}

				var menuItemName = string.Format( "{0}/{1}", libraryName, name );

				options.Add( new DesignerMenuItem( menuItemName, () =>
				{
					markUndo( "Add Effect" );

					var arguments = buildArgumentList( method );

					var newEffect = new MethodEffect()
					{
						Method = method,
						Arguments = arguments
					};

					effects.Add( newEffect );
				} ) );
			}
		}
	}

	private void inspectMultipleNodes()
	{
		var headerStyle = DesignUtil.GetStyle( (GUIStyle)"TL Selection H2" );
		GUILayout.Label( "Multiple Selections", headerStyle );

		if( GUILayout.Button( "Delete Selected Nodes" ) )
		{
			removeSelectedNodes();
		}
	}

	private void markUndo( string label )
	{
		Undo.RegisterFullObjectHierarchyUndo( graph, label );

		graph.MarkDirty();
		EditorUtility.SetDirty( graph );
	}

	private void scrollIntoView( GraphNodeBase node )
	{
		var bounds = node.Bounds;

		if( bounds.yMax >= scrollPos.y + canvasRect.yMax - bounds.height - 20 )
		{
			scrollPos.y = bounds.yMax - canvasRect.height + 30;
		}
		else if( bounds.yMin <= scrollPos.y )
		{
			scrollPos.y = bounds.yMin - 20;
		}
	}

	private void drawNodes()
	{
		var inspectorWidth = this.showInspector ? this.inspectorWidth : 5;

		this.canvasRect = new Rect( 0, toolbarHeight, Screen.width - inspectorWidth - 1, Screen.height - toolbarHeight - 21 );

		var visibleNodes = graph.GetVisibleNodes();
		var nodeHeight = graph.RootNode.Bounds.height; // Assumes that all nodes are exact same height
		var viewHeight = Mathf.Max( canvasRect.height, visibleNodes.Count * ( nodeHeight + NODE_VERTICAL_SPACING ) + GRAPH_BOTTOM_PADDING );
		var viewWidth = getViewWidth( visibleNodes );
		var viewRect = new Rect( 0, 0, viewWidth, viewHeight );

		var debugPlan = getDebugPlan();

		var backStyle = DesignUtil.GetStyle( "htn_tree_background", (GUIStyle)"GameViewBackground" );
		backStyle.padding = new RectOffset();
		GUI.Box( canvasRect, GUIContent.none, backStyle );

		this.scrollPos = GUI.BeginScrollView( canvasRect, this.scrollPos, viewRect, false, false );
		{
			drawGrid();

			graph.RootNode.Name = string.Format( "Task Network: {0}", graph.name );

			updateTreeLayout();

			drawConnections( graph.RootNode );

			if( debugPlan != null )
			{
				drawDebugConnectors( debugPlan );
			}

			drawNodeHierarchy( graph.RootNode, debugPlan, true );

			if( debugPlan != null )
			{
				drawDebugTaskStatus( debugPlan );
			}

			if( mode == EditorMode.DraggingSelection )
			{
				drawSelectionRect( dragStartPosition, dragEndPosition );
			}
		}
		GUI.EndScrollView();

		this.mouseScrollAdjust = this.scrollPos - Vector2.up * toolbarHeight;
	}

	private int getViewWidth( List<GraphNodeBase> visibleNodes )
	{
		var max = 0f;

		foreach( var node in visibleNodes )
		{
			max = Mathf.Max( node.Bounds.xMax, max );
		}

		return (int)max + 1024;
	}

	private void updateTreeLayout()
	{
		updateHierarchy( graph.RootNode, 5, 10 );
	}

	private int updateHierarchy( GraphNodeBase node, int indent, int top )
	{
		var size = node.CalculateSize();
		node.Bounds = new Rect( indent, top, size.x, size.y );

		var height = Mathf.CeilToInt( size.y ) + NODE_VERTICAL_SPACING;

		if( !( node is RootNode ) && !node.IsExpanded )
			return height;

		for( int i = 0; i < node.ChildNodes.Count; i++ )
		{
			var child = node.ChildNodes[ i ];

			// Note that these fields are maintained in the graph itself at runtime, but
			// when dragging and dropping nodes in the editor to rearrange the hierarchy,
			// it is helpful to keep these values immediately updated (not doing so here
			// doesn't affect runtime behavior, but would affect editor behavior).
			child.Parent = node;
			child.Depth = node.Depth + 1;
			child.Index = i;

			height += updateHierarchy( child, indent + 50, top + height );
		}

		return height;
	}

	private void drawNodeHierarchy( GraphNodeBase node, TaskNetworkPlan debugPlan, bool enabled )
	{
		if( !( enabled && node.IsEnabled ) )
			GUI.color = Color.white * 0.8f;

		// Draw non-active nodes darker when in debug mode
		if( !( node is RootNode ) && debugPlan != null && !debugPlan.nodes.Contains( node ) )
		{
			GUI.color = Color.white * 0.7f;
		}

		node.OnCanvasGUI();

		if( debugPlan != null && debugPlan.failedNodes.Contains( node ) )
		{
			node.DrawFailDecorator();
		}

		var conditionStyle = DesignUtil.GetStyle( "node-conditions", (GUIStyle)"WhiteLabel" );
		conditionStyle.richText = true;
		conditionStyle.normal.textColor = Color.grey * 1.5f;

		var notesStyle = DesignUtil.GetStyle( "node-annotations", (GUIStyle)"LargeLabel" );
		notesStyle.alignment = TextAnchor.UpperLeft;
		notesStyle.richText = true;
		notesStyle.fontSize = 10;
		notesStyle.normal.textColor = Color.green * 0.9f;

		var annotationRect = new Rect( node.Bounds.xMax + 7, node.Bounds.y, 2400, 30 );

		if( node.Conditions.Count > 0 )
		{
			if( node is LinkNode )
			{
				var link = ( (LinkNode)node ).LinkedNode;
				if( link != null )
				{
					annotationRect.x += 35 + link.Bounds.width;
				}
			}

			var conditionsText = new GUIContent( "Conditions: " + String.Join( ", ", node.Conditions.Select( x => x.ToString() ).ToArray() ) );
			var conditionSize = conditionStyle.CalcSize( conditionsText );

			annotationRect.width = conditionSize.x;
			annotationRect.height = conditionSize.y;

			GUI.Label( annotationRect, conditionsText, conditionStyle );

			annotationRect.y += conditionSize.y;
		}

		if( node.Effects.Count > 0 )
		{
			var effectsText = new GUIContent( "Effects: " + String.Join( ", ", node.Effects.Select( x => x.ToString() ).ToArray() ) );
			var effectsSize = conditionStyle.CalcSize( effectsText );

			annotationRect.width = effectsSize.x;
			annotationRect.height = effectsSize.y;

			GUI.Label( annotationRect, effectsText, conditionStyle );
		}

		//if( !string.IsNullOrEmpty( node.Notes.Trim() ) )
		//{
		//	annotationRect.x = node.Bounds.x;
		//	annotationRect.y = node.Bounds.y - 18;
		//	annotationRect.width = 1024;

		//	GUI.Label( annotationRect, "<b>// " + node.Notes.Trim().Replace( "\n", " " ) + "</b>", notesStyle );

		//}

		GUI.color = Color.white;

		if( !( node is RootNode ) && !node.IsExpanded )
			return;

		for( int i = 0; i < node.ChildNodes.Count; i++ )
		{
			var child = node.ChildNodes[ i ];
			drawNodeHierarchy( child, debugPlan, enabled && node.IsEnabled );
		}
	}

	private void drawDebugTaskStatus( TaskNetworkPlan plan )
	{
		var currentTaskIndex = plan.tasks.IndexOf( plan.CurrentTask );

		for( int i = 0; i < plan.TaskCount; i++ )
		{
			drawTastStatusIndicator( plan.tasks[ i ], i, currentTaskIndex );
		}
	}

	private void drawDebugConnectors( TaskNetworkPlan plan )
	{
		var nodes = plan.nodes;
		for( int i = 0; i < nodes.Count; i++ )
		{
			ensureExpanded( nodes[ i ] );
			drawNodeConnectionToParent( nodes[ i ], true );
		}
	}

	private void drawTastStatusIndicator( PlannerTask task, int taskIndex, int currentTaskIndex )
	{
		if( Event.current.type != EventType.repaint )
			return;

		var style = DesignUtil.GetStyle( "htn-task-pending", (GUIStyle)"sv_label_0" );

		if( taskIndex < currentTaskIndex )
		{
			style = DesignUtil.GetStyle( "htn-task-completed", (GUIStyle)"sv_label_3" );
		}
		else if( taskIndex == currentTaskIndex )
		{
			style = DesignUtil.GetStyle( "htn-task-running", (GUIStyle)"sv_label_4" );
		}

		var bounds = task.Node.Bounds;
		var size = style.CalcSize( GUIContent.none );
		var rect = new Rect( bounds.x - size.x * 0.75f, bounds.center.y - size.y * 0.5f, size.x, size.y );

		style.Draw( rect, GUIContent.none, -1 );
	}

	private void ensureExpanded( GraphNodeBase node )
	{
		while( node != null )
		{
			node.Expand();
			node = graph.GetParentNode( node );
		}
	}

	private void drawConnections( GraphNodeBase node )
	{
		drawNodeConnectionToParent( node, false );

		if( !node.IsExpanded )
			return;

		for( int i = 0; i < node.ChildNodes.Count; i++ )
		{
			drawConnections( node.ChildNodes[ i ] );
		}
	}

	private void drawNodeConnectionToParent( GraphNodeBase node, bool debug )
	{
		if( node is RootNode )
			return;

		var parent = graph.GetParentNode( node );
		if( parent == null )
			return;

		var lineStart = new Vector2( parent.Bounds.xMin + 25, parent.Bounds.yMax );
		var lineEnd = new Vector2( lineStart.x, node.Bounds.center.y );
		var childStart = new Vector2( node.Bounds.xMin, lineEnd.y );

		drawNodeConnection( lineStart, lineEnd, debug );
		drawNodeConnection( lineEnd, childStart, debug );
	}

	private void drawNodeConnection( GraphNodeBase parent, GraphNodeBase child )
	{
		var lineStart = new Vector2( parent.Bounds.xMin + 25, parent.Bounds.yMax );
		var lineEnd = new Vector2( lineStart.x, child.Bounds.center.y );
		var childStart = new Vector2( child.Bounds.xMin, lineEnd.y );

		drawNodeConnection( lineStart, lineEnd, false );
		drawNodeConnection( lineEnd, childStart, false );
	}

	private void drawNodeConnection( Vector2 start, Vector2 end, bool debug )
	{
		var savedColor = Handles.color;

		start += Vector2.one;
		end += Vector2.one;

		Handles.color = debug ? new Color( 1f, 0.33f, 0f ) : Color.grey;

		for( int i = 0; i < 3; i++ )
		{
			Handles.DrawLine( start, end );

			if( start.y == end.y )
			{
				start.y = --end.y;
			}
			else
			{
				start.x = --end.x;
			}
		}

		Handles.color = savedColor;
	}

	private void drawBreadCrumbs()
	{
		var breadCrumbLeft = DesignUtil.GetStyle( "breadcrumb-left", (GUIStyle)"GUIEditor.BreadcrumbLeft" );
		var breadCrumbMid = DesignUtil.GetStyle( "breadcrumb-mid", (GUIStyle)"GUIEditor.BreadcrumbMid" );
		var backgroundStyle = (GUIStyle)"AnimationCurveEditorBackground";

		var rect = new Rect( 0, 18, Screen.width, breadCrumbLeft.fixedHeight );
		GUI.Box( rect, GUIContent.none, backgroundStyle );

		if( selected.Count != 1 )
		{
			var text = new GUIContent( "ROOT" );
			var size = breadCrumbLeft.CalcSize( text );

			if( GUI.Button( new Rect( rect.x, rect.y, size.x, size.y ), text, breadCrumbLeft ) )
			{
				var rootNode = graph.RootNode;
				clearSelection();
				selectNode( rootNode );
				scrollIntoView( rootNode );
			}

			return;
		}

		var node = selected[ 0 ];
		var stack = new Stack<GraphNodeBase>();

		stack.Push( node );

		while( true )
		{
			var parent = graph.GetParentNode( node );
			if( parent == null )
				break;

			stack.Push( parent );
			node = parent;
		}

		var style = breadCrumbLeft;

		while( stack.Count > 0 )
		{
			node = stack.Pop();

			// Need to make sure that the text color and font style are defaults,
			// because these properties may be changed during debug mode
			style.normal.textColor = Color.white;
			style.fontStyle = FontStyle.Normal;

			var text = new GUIContent( node is RootNode ? "ROOT" : node.Name );
			var size = style.CalcSize( text );

			rect.x += 3;
			rect.width = size.x;
			rect.height = size.y;

			if( GUI.Button( rect, text, style ) )
			{
				clearSelection();
				selectNode( node );
				scrollIntoView( node );
			}

			rect.x += rect.width;
			style = breadCrumbMid;
		}
	}

	private TaskNetworkPlanner getDebugPlanner()
	{
		if( !Application.isPlaying )
			return null;

		if( !this.showDebugInfo )
			return null;

		if( Selection.activeGameObject == null )
			return null;

		return Selection.activeGameObject.GetComponent<TaskNetworkPlanner>();
	}

	private TaskNetworkPlan getDebugPlan()
	{
		var planner = getDebugPlanner();
		if( planner == null )
			return null;

		return planner.Plan;
	}

	private void drawDebugInfo()
	{
		var plan = getDebugPlan();
		if( plan == null )
			return;

		if( plan.CurrentTask != null )
		{
			clearSelection();
			selectNode( plan.CurrentTask.Node );

			if( debugNode != plan.CurrentTask.Node )
			{
				scrollIntoView( debugNode = plan.CurrentTask.Node );
			}
		}

		#region Draw the current plan as "breadcrumbs" style

		var breadCrumbLeft = DesignUtil.GetStyle( "breadcrumb-left", (GUIStyle)"GUIEditor.BreadcrumbLeft" );
		var breadCrumbMid = DesignUtil.GetStyle( "breadcrumb-mid", (GUIStyle)"GUIEditor.BreadcrumbMid" );
		var backgroundStyle = (GUIStyle)"AnimationEventBackground";

		var rect = new Rect( 0, 18, Screen.width, breadCrumbLeft.fixedHeight );
		GUI.Box( rect, GUIContent.none, backgroundStyle );

		if( selected.Count != 1 )
		{
			var text = new GUIContent( "ROOT" );
			var size = breadCrumbLeft.CalcSize( text );

			if( GUI.Button( new Rect( rect.x, rect.y, size.x, size.y ), text, breadCrumbLeft ) )
			{
				var rootNode = graph.RootNode;
				clearSelection();
				selectNode( rootNode );
				scrollIntoView( rootNode );
			}

			return;
		}

		var style = breadCrumbLeft;

		var currentTaskIndex = plan.nodes.IndexOf( plan.CurrentTask.Node );

		for( int i = currentTaskIndex; i < plan.nodes.Count; i++ )
		{
			var node = plan.nodes[ i ];
			if( !( node is OperatorNode ) )
				continue;

			var color = ( node == plan.CurrentTask.Node ) ? Color.yellow : Color.white;
			if( !EditorGUIUtility.isProSkin )
			{
				color = ( node == plan.CurrentTask.Node ) ? Color.blue : Color.black;
			}

			style.normal.textColor = color;
			//style.fontStyle = ( node == plan.CurrentTask.Node ) ? FontStyle.Bold : FontStyle.Normal;

			var text = new GUIContent( node is RootNode ? "ROOT" : node.Name );
			var size = style.CalcSize( text );

			rect.x += 3;
			rect.width = size.x;
			rect.height = size.y;

			if( GUI.Button( rect, text, style ) )
			{
				clearSelection();
				selectNode( node );
				scrollIntoView( node );
			}

			rect.x += rect.width;
			style = breadCrumbMid;
		}

		#endregion Draw the current plan as "breadcrumbs" style
	}

	private void drawGrid()
	{

		if( Event.current.type != EventType.repaint )
			return;

		const int MAJOR_GRIDLINES = 100;
		const int MINOR_GRIDLINES = 10;
		const int SIZE = 8192;

		for( int i = 0; i < SIZE; i++ )
		{
			if( i % MAJOR_GRIDLINES == 0 )
			{
				Handles.color = new Color( 0f, 0f, 0f, 0.33f );
				Handles.DrawLine( new Vector3( 0, i, 0 ), new Vector3( SIZE, i, 0 ) );
				Handles.DrawLine( new Vector3( i, 0, 0 ), new Vector3( i, SIZE, 0 ) );
			}
			else if( i % MINOR_GRIDLINES == 0 )
			{
				Handles.color = new Color( 0.5f, 0.5f, 0.5f, 0.1f );
				Handles.DrawLine( new Vector3( i, 0, 0 ), new Vector3( i, SIZE, 0 ) );
				Handles.DrawLine( new Vector3( 0, i, 0 ), new Vector3( SIZE, i, 0 ) );
			}
		}

		Handles.color = Color.white;

		if( Application.isPlaying )
		{

			var debugPlan = getDebugPlan();
			if( debugPlan != null )
			{
				
				var debugLabel = "Running";
				if( EditorApplication.isPaused )
				{
					if( debugPlan.CurrentTask.PauseOnRun )
					{
						debugLabel = "Breakpoint";
					}
					else
					{
						debugLabel = "Debugging";
					}
				}
				else
				{
					if( debugPlan.Agent == TaskNetworkPlanner.DebugTarget )
					{
						debugLabel = "Run to next task";
					}
				}

				drawEditorModeLabel( debugLabel );

			}
			else
			{
				drawEditorModeLabel( "Edit" );
			}

		}

	}

	private void drawEditorModeLabel( string text )
	{

		var label = new GUIContent( text );

		var style = DesignUtil.GetStyle( "htn_editor_mode_label", (GUIStyle)"LODLevelNotifyText" );
		style.normal.textColor = new Color( 1, 1, 0, 0.2f );
		style.fontStyle = FontStyle.Bold;

		var size = style.CalcSize( label );

		var inspectorWidth = this.showInspector ? this.inspectorWidth : 10;
		var viewportTop = 5;
		var viewportRight = Screen.width - inspectorWidth - 30;

		var rect = new Rect( viewportRight - size.x + scrollPos.x, viewportTop + scrollPos.y, size.x, size.y );
		GUI.Label( rect, label, style );

	}

	/// <summary>
	/// Makes a file path relative to the Unity project's path
	/// </summary>
	private static string makeRelativePath( string path )
	{
		if( string.IsNullOrEmpty( path ) )
		{
			return "";
		}

		return path.Substring( path.IndexOf( "Assets/", StringComparison.OrdinalIgnoreCase ) );
	}

	#region Nested types

	private enum EditorMode
	{
		Default,
		DraggingNode,
		DraggingSelection,
		Inspector,
		ResizingsInspector
	}

	#endregion Nested types
}