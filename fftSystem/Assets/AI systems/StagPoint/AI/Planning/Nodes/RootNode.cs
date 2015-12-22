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

	public partial class RootNode : GraphNodeBase
	{

		public RootNode()
			: base()
		{
			this.Name = "Hierarchical Task Network";
			this.Notes = "This is the root node of the Hierarchical Task Network. Planning will start here, and search each child node until it finds one that can be fully decomposed into a plan.";
		}

		public override bool CanAttach( GraphNodeBase other )
		{

			if( other == null || other.ChildNodes.Contains( this ) || other == this || ChildNodes.Contains( other ) )
				return false;

			return ( other is CompositeNode );

		}

		public override bool CanAttach<T>()
		{
			return
				typeof( CompositeNode ).IsAssignableFrom( typeof( T ) );
		}

		public override void OnAfterDeserialize()
		{
			this.IsExpanded = true;
			base.OnAfterDeserialize();
		}

	}

#if UNITY_EDITOR

	/*****************************
	 * Design-time support only *
	 ****************************/

	public partial class RootNode : GraphNodeBase
	{

		protected override string getNodeType()
		{
			return "Root Node";
		}

		protected override string getHelpDescription()
		{
			return "This is the start node from which all planning begins.\n\nEach child node will be evaluated in turn until one is found that can be fully decomposed into a valid plan.";
		}

		protected override GUIStyle getBoxStyle()
		{

			var result = DesignUtil.GetStyle( "htn_root_node", (GUIStyle)"flow node 0" );

			result.fontStyle = FontStyle.Bold;
			result.fontSize = 14;
			result.richText = true;
			//result.normal.textColor = Color.black;

			return result;

		}

		protected override string getLabel()
		{
			return "<size=14><b>" + this.Name + "</b></size>";
		}

	}

#endif

}
