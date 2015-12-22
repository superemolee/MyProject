// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace StagPoint.Planning
{

	using StagPoint.Core;

#if UNITY_EDITOR
	using UnityEditor;
#endif
	
	/// <summary>
	/// Describes the minimum functionality implemented by every type of node in the hierarchy
	/// </summary>
	[Serializable]
	public abstract partial class GraphNodeBase : ISerializationCallbackReceiver
	{

		#region Public fields 

		/// <summary>
		/// Used internally to ensure that each graph node has a unique identifier
		/// </summary>
		public string UID = "NO ID SET";

		/// <summary>
		/// This is the human-readable name of the node
		/// </summary>
		public string Name = "Node";

		/// <summary>
		/// Holds any notes entered by the designer
		/// </summary>
		public string Notes = string.Empty;

		/// <summary>
		/// Indicates whether the node is currently enabled or not. When a node is not enabled, it will
		/// never be included in the planning phase (nor will any child nodes)
		/// </summary>
		public bool IsEnabled = true;

		/// <summary>
		/// Used at design time for hit testing, rendering, etc.
		/// </summary>
		[NonSerialized]
		public Rect Bounds = new Rect();

		/// <summary>
		/// Used during planning to implement Method Traversal Record tracking for calculating plan priorities
		/// </summary>
		[NonSerialized]
		public int Index = 0;

		/// <summary>
		/// Used in the editor for tracking max/avg/min planner search depth
		/// </summary>
		[NonSerialized]
		public int Depth = 0;

		/// <summary>
		/// Holds the collection of child nodes for this node
		/// </summary>
		public List<GraphNodeBase> ChildNodes = new List<GraphNodeBase>();

		/// <summary>
		/// Holds the collection of required preconditions for this node
		/// </summary>
		public List<NodePreconditionBase> Conditions = new List<NodePreconditionBase>();

		/// <summary>
		/// Holds the collection of effects that will be applied by this node
		/// </summary>
		public List<NodeEffectBase> Effects = new List<NodeEffectBase>();

		#endregion 

		#region Public properties 

		/// <summary>
		/// Gets or sets whether the node is selected in the HTN Graph Editor window
		/// </summary>
		public bool IsSelected { get; set; }

		/// <summary>
		/// Returns whether the node is currently expanded in the HTN Graph Editor window
		/// </summary>
		public bool IsExpanded { get; protected set; }

		/// <summary>
		/// Returns the parent of this node. This value is maintained by the HTN Planner library
		/// and should never be modified.
		/// </summary>
		public GraphNodeBase Parent { get; set; }

		#endregion 

		#region Constructor 

		public GraphNodeBase()
		{

			this.UID = System.Guid.NewGuid().ToString();
			this.IsExpanded = true;

#if UNITY_EDITOR
			this.Name = this.getNodeType();
#endif

		}

		#endregion 

		#region Public methods 

		internal virtual void Initialize( object agent, Blackboard state )
		{

			for( int i = 0; i < Conditions.Count; i++ )
			{
				Conditions[ i ].Initialize( agent, state );
			}

			for( int i = 0; i < Effects.Count; i++ )
			{
				Effects[ i ].Initialize( agent, state );
			}

			for( int i = 0; i < ChildNodes.Count; i++ )
			{
				ChildNodes[ i ].Initialize( agent, state );
			}

		}

		public virtual TaskStatus Execute( object agent, Blackboard state )
		{
			return TaskStatus.Succeeded;
		}

		/// <summary>
		/// Returns TRUE if all defined preconditions can be satisfied by the current
		/// planner state, returns FALSE if any condition cannot be satisfied.
		/// </summary>
		/// <param name="agent">The agent for which the plan is being generated.</param>
		/// <param name="state">A Blackboard representing the complete world state at the 
		/// point in the plan for which this node is being evaluated.</param>
		public virtual bool ArePreconditionsSatisfied( object agent, Blackboard state )
		{

			for( int i = 0; i < this.Conditions.Count; i++ )
			{

				var condition = this.Conditions[ i ];

				if( !condition.IsConditionSatisfied( agent, state ) )
					return false;

			}

			return true;

		}

		/// <summary>
		/// Apply any effects defined for this node.
		/// </summary>
		/// <param name="agent">The agent for which the plan is being generated or upon which the effects should be applied.</param>
		/// <param name="state">The Blackboard representing the agent's current world state</param>
		/// <param name="isExecuting">Set this value to TRUE when the plan is being executed, and to FALSE when the plan is being generated</param>
		/// <returns></returns>
		public virtual TaskStatus ApplyEffects( object agent, Blackboard state, bool isExecuting )
		{

			try
			{

				for( int i = 0; i < this.Effects.Count; i++ )
				{

					var effect = this.Effects[ i ];

					if( isExecuting && !effect.ApplyAtRuntime )
						continue;

					if( effect.Apply( agent, state ) != TaskStatus.Succeeded )
						return TaskStatus.Failed;

				}


			}
			catch( Exception err )
			{
				var errorMessage = string.Format( "Exception thrown while applying effects on task '{0}': {1}", this.Name, err.Message );
				Debug.LogError( errorMessage );
				return TaskStatus.Failed;
			}

			return TaskStatus.Succeeded;

		}

		/// <summary>
		/// Adds a new child node to this node's ChildNodes list.
		/// </summary>
		/// <param name="child">The child node to be added.</param>
		public virtual void AddChild( GraphNodeBase child )
		{

			if( child == null )
			{
				throw new NullReferenceException();
			}

			if( ChildNodes.Contains( child ) )
			{
				throw new InvalidOperationException( string.Format( "Node '{0}' is already a child of '{1}'", child.Name, this.Name ) );
			}

			if( !CanAttach( child ) )
			{
				throw new Exception( string.Format( "Cannot add a {0} node as a child of a {1} node", child.GetType().Name, this.GetType().Name ) );
			}

			this.ChildNodes.Add( child );
			child.Index = this.ChildNodes.Count - 1;
			child.Parent = this;

		}

		public virtual bool RemoveChild( GraphNodeBase child )
		{

			child.Parent = null;

			var removed = ChildNodes.Remove( child );
			if( removed )
			{
				for( int i = 0; i < ChildNodes.Count; i++ )
				{
					ChildNodes[ i ].Index = i;
				}
			}

			return removed;

		}

		public abstract bool CanAttach( GraphNodeBase other );

		public abstract bool CanAttach<T>() where T : GraphNodeBase;

		public virtual void Validate()
		{
			// To be overridden by subclasses
		}

		public string GetPath()
		{

			var buffer = new StringBuilder();

			var loop = this;
			while( loop != null && !( loop is RootNode ) )
			{
				
				if( buffer.Length > 0 )
					buffer.Insert( 0, "/" );

				buffer.Insert( 0, loop.Name );

				loop = loop.Parent;

			}

			return buffer.ToString();

		}

		public override string ToString()
		{
			return string.Format( "{0} ({1})", this.Name, this.GetType().Name );
		}

		#endregion 

		#region ISerializationCallbackReceiver Members

		public virtual void OnAfterDeserialize()
		{
			for( int i = 0; i < ChildNodes.Count; i++ )
			{
				ChildNodes[ i ].Parent = this;
			}
		}

		public virtual void OnBeforeSerialize()
		{
			// No action needed in the base class
		}

		#endregion

	}

#if UNITY_EDITOR

	/*****************************
	 * Design-time support only *
	 ****************************/

	public enum NodeHitType
	{
		None,
		Node,
		Link,
		Decorator
	}

	public abstract partial class GraphNodeBase
	{

		#region Static fields and constants 

		private const float CONNECTION_SIZE = 15f;

		#endregion 

		#region Public design-time methods

		/// <summary>
		/// Called by the graph editor to request that the object render any 
		/// configuration UI as a custom Inspector
		/// </summary>
		public virtual void OnInspectorGUI( TaskNetworkGraph graph )
		{

			DesignUtil.Header( getNodeType() );

			GUIUtility.GetControlID( this.UID.GetHashCode() ^ this.GetHashCode(), FocusType.Native );

			var labelStyle = DesignUtil.GetStyle( "node-type-description", (GUIStyle)"AboutWIndowLicenseLabel" );
			labelStyle.margin = new RectOffset( 5, 5, 3, 3 );

			GUILayout.Label( getHelpDescription(), labelStyle );
			GUILayout.Space( 10 );

			if( DesignUtil.SectionFoldout( "node-general", "General" ) )
			{

				this.Name = EditorGUILayout.TextField( "Name", this.Name );
				this.Notes = DesignUtil.MemoField( "Notes", this.Notes );

				if( !( this is RootNode ) )
				{

					var enabled = EditorGUILayout.Toggle( "Enabled", this.IsEnabled );
					if( enabled != this.IsEnabled )
					{
						setEnabled( enabled );
					}

				}

			}

			GUILayout.Space( 5 );

		}

		/// <summary>
		/// Called by the editor in order to render the node during drag-and-drop operations
		/// </summary>
		public virtual void OnDragGUI( Vector2 position )
		{

			var label = new GUIContent( getLabel() );
			var boxStyle = getBoxStyle();
			var styleSize = boxStyle.CalcSize( label );
			var rect = new Rect( position.x, position.y, styleSize.x + 35, 30 );

			var guiColor = GUI.color;
			GUI.color = new Color( guiColor.r, guiColor.g, guiColor.b, guiColor.a * 0.65f );
			
			GUI.Box( rect, label, boxStyle );

			GUI.color = guiColor;

		}

		/// <summary>
		/// Calculate the dimensions needed to render or layout the node, based on the node's 
		/// style and label.
		/// </summary>
		public virtual Vector2 CalculateSize()
		{

			var label = new GUIContent( getLabel() );
			var boxStyle = getBoxStyle();

			// Calculate size based on node style and label
			var size = boxStyle.CalcSize( label );

			// Inflate for visual padding and room for decorators
			size.x += 35;
			size.y = 30;

			return size;

		}

		/// <summary>
		/// Called by the graph editor to request that the node draw itself 
		/// </summary>
		public virtual void OnCanvasGUI()
		{

			var label = new GUIContent( getLabel() );
			var boxStyle = getBoxStyle();

			var size = boxStyle.CalcSize( label );
			Bounds.width = size.x + 40;
			Bounds.height = 30;

			var shadowStyle = DesignUtil.GetStyle( "node-shadow", (GUIStyle)"IN ThumbnailShadow" );
			GUI.Box( this.Bounds, GUIContent.none, shadowStyle );
			
			GUI.Label( this.Bounds, label, boxStyle );

			if( IsSelected )
			{
				var selectionStyle = DesignUtil.GetStyle( "node-highlight", (GUIStyle)"LightmapEditorSelectedHighlight" );
				GUI.Box( this.Bounds, GUIContent.none, selectionStyle );
			}

			drawNotesIcon();
			drawExpandCollapseButton();
			drawBreakpoint();

		}

		public virtual void DrawFailDecorator()
		{

			if( Event.current.type != EventType.repaint )
				return;

			var icon = EditorGUIUtility.FindTexture( "d_console.erroricon" );
			var rect = new Rect( Bounds.xMin - icon.width * 0.5f, Bounds.yMin + ( Bounds.height - icon.height ) * 0.5f + 2, icon.width, icon.height );
			GUI.DrawTexture( rect, icon );

		}

		public void Expand()
		{
			this.IsExpanded = true;
		}

		public void Collapse()
		{
			if( this is CompositeNode )
			{
				this.IsExpanded = false;
			}
		}

		public bool Intersects( Rect rect )
		{

			var outside =
				Bounds.xMax < rect.xMin ||
				Bounds.yMax < rect.yMin ||
				Bounds.xMin > rect.xMax ||
				Bounds.yMin > rect.yMax;

			return !outside;

		}

		public virtual NodeHitType HitTest( Vector2 pos )
		{

			if( !Bounds.Contains( pos ) )
				return NodeHitType.None;

			if( !( this is RootNode ) && this.ChildNodes.Count > 0 )
			{
				var indicatorStyle = DesignUtil.GetStyle( "annotations-indicator-expand", (GUIStyle)"ShurikenPlus" );
				var size = indicatorStyle.CalcSize( GUIContent.none );
				var rect = new Rect( Bounds.x + 6, Bounds.center.y - size.y * 0.5f, size.x, size.y );
				if( rect.Contains( pos ) )
					return NodeHitType.Decorator;
			}

			return NodeHitType.Node;

		}

		#endregion

		#region Protected design-time utility methods

		protected void setEnabled( bool enabled )
		{

			this.IsEnabled = enabled;

			for( int i = 0; i < ChildNodes.Count; i++ )
			{
				ChildNodes[ i ].setEnabled( enabled );
			}

		}

		protected virtual string getNodeType()
		{
			return "**ERROR**";
		}

		protected virtual string getHelpDescription()
		{
			return "**ERROR**";
		}

		protected virtual string getLabel()
		{
			return "<size=14><b><color=red>ERROR</color></b></size>";
		}

		protected virtual GUIStyle getBoxStyle()
		{

			var result = (GUIStyle)null;

			result = DesignUtil.GetStyle( "flow node 0" );

			result.fontStyle = FontStyle.Bold;
			result.fontSize = 12;
			result.richText = true;

			return result;

		}

		private void drawBreakpoint()
		{

			if( Event.current.type != EventType.repaint )
				return;

			if( !( this is OperatorNode ) )
				return;

			var task = this as OperatorNode;
			if( !task.PauseOnRun )
				return;

			var icon = EditorGUIUtility.FindTexture( "d_PauseButton" );

			if( Application.isPlaying )
			{
				icon = this.IsSelected && UnityEditor.EditorApplication.isPaused
					? EditorGUIUtility.FindTexture( "d_PauseButton Anim" )
					: EditorGUIUtility.FindTexture( "d_PauseButton On" );
			}

			var iconRect = new Rect( Bounds.x, Bounds.y + Mathf.FloorToInt( ( Bounds.height - icon.height ) * 0.5f ) - 1, icon.width, icon.height );
			GUI.DrawTexture( iconRect, icon );

		}

		private void drawNotesIcon()
		{

			if( Event.current.type != EventType.repaint )
				return;

			if( string.IsNullOrEmpty( this.Notes.Trim() ) )
				return;

			var icon = EditorGUIUtility.FindTexture( "d_UnityEditor.ConsoleWindow" );

			var iconRect = new Rect( Bounds.xMax - icon.width - 2, Bounds.y + Mathf.FloorToInt( ( Bounds.height - icon.height ) * 0.5f ) + 1, icon.width, icon.height );
			GUI.DrawTexture( iconRect, icon );

		}

		private void drawExpandCollapseButton()
		{

			if( Event.current.type != EventType.repaint )
				return;

			if( this.ChildNodes.Count == 0 )
				return;

			var indicatorStyle = DesignUtil.GetStyle( "annotations-indicator-expand", (GUIStyle)"ShurikenPlus" );
			if( this.IsExpanded )
			{
				indicatorStyle = DesignUtil.GetStyle( "annotations-indicator-collapse", (GUIStyle)"ShurikenMinus" );
			}

			var size = indicatorStyle.CalcSize( GUIContent.none );
			var rect = new Rect( Bounds.x + 6, Bounds.center.y - size.y * 0.5f, size.x, size.y );

			indicatorStyle.Draw( rect, GUIContent.none, -1 );

		}

		#endregion

	}

#endif

}
