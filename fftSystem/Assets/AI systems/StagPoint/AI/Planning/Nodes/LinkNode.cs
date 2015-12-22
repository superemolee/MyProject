// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StagPoint.Core;

using UnityEngine;

namespace StagPoint.Planning
{

#if UNITY_EDITOR
	using UnityEditor;
#endif

	public partial class LinkNode : GraphNodeBase
	{

		#region Public properties 

		public GraphNodeBase LinkedNode;

		#endregion 

		#region Base class overrides

		public override void AddChild( GraphNodeBase child )
		{

			if( child == null )
			{
				throw new NullReferenceException();
			}

			if( !CanAttach( child ) )
			{
				throw new Exception( string.Format( "Cannot add a {0} node as a child of a {1} node", child.GetType().Name, this.GetType().Name ) );
			}

			LinkedNode = child;

		}

		public override bool CanAttach( GraphNodeBase other )
		{
			return false;
		}

		public override bool CanAttach<T>()
		{
			return false;
		}

		public override string ToString()
		{
			return string.Format( "Link: {0}", this.LinkedNode != null ? this.LinkedNode.Name : "(Not Linked)" );
		}

		#endregion 

	}

#if UNITY_EDITOR

	/*****************************
	 * Design-time support only *
	 ****************************/

	public partial class LinkNode : GraphNodeBase
	{

		public override void OnCanvasGUI()
		{

			var label = new GUIContent( getLabel() );

			var boxStyle = getBoxStyle();

			var styleSize = boxStyle.CalcSize( label );
			Bounds.width = styleSize.x + 30;
			Bounds.height = 30;

			var shadowStyle = DesignUtil.GetStyle( "node-shadow", (GUIStyle)"IN ThumbnailShadow" );
			GUI.Box( this.Bounds, GUIContent.none, shadowStyle );

			GUI.Box( this.Bounds, label, boxStyle );

			if( IsSelected )
			{
				var selectionStyle = DesignUtil.GetStyle( "node-highlight", (GUIStyle)"LightmapEditorSelectedHighlight" );
				GUI.Box( this.Bounds, GUIContent.none, selectionStyle );
			}

			if( Event.current.type == EventType.repaint )
			{

				var indicatorStyle = DesignUtil.GetStyle( "link-node-indicator", (GUIStyle)"ProjectBrowserSubAssetExpandBtn" );
				var rect = getLinkTargetRect();

				indicatorStyle.Draw( rect, GUIContent.none, -1 );

			}

			if( this.LinkedNode != null )
			{
				this.LinkedNode.OnDragGUI( Bounds.position + Vector2.right * ( Bounds.width + 40 ) );
			}
			else
			{
				drawInstructions();
			}

		}

		public override NodeHitType HitTest( Vector2 pos )
		{

			if( getLinkTargetRect().Contains( pos ) )
				return NodeHitType.Link;

			return base.HitTest( pos );

		}

		public override void OnInspectorGUI( TaskNetworkGraph graph )
		{

			base.OnInspectorGUI( graph );

			if( DesignUtil.SectionFoldout( "link-configuration", "Linked Node" ) )
			{
				var label = ( this.LinkedNode != null ) ? this.LinkedNode.Name : "(Not Linked)";
				EditorGUILayout.LabelField( "Target Node", label, EditorStyles.objectField );
			}

			GUILayout.Space( 5 );

		}

		protected override string getNodeType()
		{
			return "GOTO Node";
		}

		protected override string getHelpDescription()
		{
			return "GOTO nodes allow you to connect to any other compound node in the tree, which can be used to implement recursion or to increase modularity.\n\n<b>Note</b> Any conditions defined on a GOTO node are checked during planning only.";
		}

		protected override string getLabel()
		{
			return "<size=14><b>GOTO</b></size>";
		}

		protected override GUIStyle getBoxStyle()
		{

			var result = DesignUtil.GetStyle( "htn_link_node", (GUIStyle)"flow node 0" );

			result.fontStyle = FontStyle.Bold;
			result.fontSize = 14;
			result.richText = true;

			return result;

		}

		private void drawInstructions()
		{

			var instructions = new GUIContent( "Drag and drop a composite node on the arrow to link" );

			var instructionsStyle = DesignUtil.GetStyle( "link-node-instructions", (GUIStyle)"WhiteLabel" );
			instructionsStyle.alignment = TextAnchor.MiddleCenter;

			var instructionsRect = Bounds;
			instructionsRect.x = Bounds.xMax + 40;
			instructionsRect.width = instructionsStyle.CalcSize( instructions ).x + 20;

			GUI.Label( instructionsRect, instructions, instructionsStyle );

		}

		private Rect getLinkTargetRect()
		{
			return new Rect( Bounds.xMax + 10, Bounds.center.y - 10f, 22, 24 );
		}

	}

#endif

}
