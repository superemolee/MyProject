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

	[TypeRename( "StagPoint.Planning.MethodNode" )]
	public partial class CompositeNode : GraphNodeBase
	{

		#region Public fields 

		public DecompositionMode Mode = DecompositionMode.SelectAll;

		#endregion 

		#region Base class overrides 

		public override bool CanAttach( GraphNodeBase other )
		{
			if( other == null || other.ChildNodes.Contains( this ) || other == this || ChildNodes.Contains( other ) )
				return false;

			return ( other is CompositeNode ) || ( other is OperatorNode ) || ( other is LinkNode );
		}

		public override bool CanAttach<T>()
		{
			return
				typeof( OperatorNode ).IsAssignableFrom( typeof( T ) ) ||
				typeof( LinkNode ).IsAssignableFrom( typeof( T ) ) ||
				typeof( CompositeNode ).IsAssignableFrom( typeof( T ) );
		}

		#endregion 

	}

#if UNITY_EDITOR

	/*****************************
	 * Design-time support only *
	 ****************************/

	public partial class CompositeNode : GraphNodeBase
	{

		public override void OnInspectorGUI( TaskNetworkGraph graph )
		{

			DesignUtil.Header( getNodeType() );

			GUIUtility.GetControlID( this.UID.GetHashCode() ^ this.GetHashCode(), FocusType.Native );

			var labelStyle = DesignUtil.GetStyle( "node-type-description", (GUIStyle)"AboutWIndowLicenseLabel" );
			labelStyle.margin = new RectOffset( 5, 5, 3, 3 );
			labelStyle.padding = new RectOffset( 5, 5, 5, 5 );

			GUILayout.Label( getHelpDescription(), labelStyle );
			GUILayout.Space( 10 );

			if( DesignUtil.SectionFoldout( "node-general", "General" ) )
			{
				
				this.Name = EditorGUILayout.TextField( "Name", this.Name );
				this.Notes = DesignUtil.MemoField( "Notes", this.Notes );
				this.Mode = (DecompositionMode)DesignUtil.EnumField( "Decomp. Mode", this.Mode );

				var enabled = EditorGUILayout.Toggle( "Enabled", this.IsEnabled );
				if( enabled != this.IsEnabled )
				{
					setEnabled( enabled );
				}

			}

			GUILayout.Space( 5 );

		}

		protected override string getNodeType()
		{
			return string.Format( "Composite Node ({0})", this.Mode == DecompositionMode.SelectOne ? "Selector" : "Sequence" );
		}

		protected override string getHelpDescription()
		{
			
			var description = "Composite nodes may contain any number of subtasks of any type.\n\n";

			if( this.Mode == DecompositionMode.SelectOne )
				description += "Each subtask will be evaluated in turn, until one is found that can be added to the plan.";
			else
				description += "Each subtask will be evaluated in turn. If any subtask fails to return a plan, then this composite node will also fail to return a plan.";

			return description;

		}

		protected override string getLabel()
		{
			return "<size=14><b>" + this.Name + "</b></size>";
		}

		protected override GUIStyle getBoxStyle()
		{

			var result = DesignUtil.GetStyle( "htn_method_node", "flow node 3" );

			if( this.Mode == DecompositionMode.SelectOne )
				result = DesignUtil.GetStyle( "htn_sequence_node", "flow node 1" );
				
			result.fontStyle = FontStyle.Bold;
			result.fontSize = 12;
			result.richText = true;

			return result;

		}

	}

#endif

}
