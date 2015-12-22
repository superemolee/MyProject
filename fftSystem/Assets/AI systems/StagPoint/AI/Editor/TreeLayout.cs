using System;
using System.Collections.Generic;

namespace StagPoint.TreeLayout
{

	using UnityEngine;
	using StagPoint.Planning;

	public class TreeLayout
	{

		#region Public fields

		public TreeOrientation Orientation = TreeOrientation.Top;
		public NodeAlignment NodeJustification = NodeAlignment.Top;

		public int MaxDepth = int.MaxValue;
		public float LevelSeparation = 200f;
		public float SiblingSeparation = 15f;
		public float SubtreeSeparation = 50f;
		public float TopXAdjustment = 0f;
		public float TopYAdjustment = 0f;
		public float RootYOffset = 0;
		public float RootXOffset = 0;

		#endregion

		#region Private runtime values

		private Dictionary<int, float> maxLevelHeight = new Dictionary<int, float>();
		private Dictionary<int, float> maxLevelWidth = new Dictionary<int, float>();
		private Dictionary<int, Node> previousLevelNode = new Dictionary<int, Node>();

		private Node rootNode = new Node();

		#endregion

		#region Public properties

		public Node Root
		{
			get { return rootNode.FirstChild; }
			set
			{
				rootNode.nodeChildren.Clear();
				rootNode.AddChild( value );
			}
		}

		#endregion

		#region Public methods

		public static void PerformLayout( GraphNodeBase graphNode )
		{

			var tree = new TreeLayout();
			tree.Root = new Node( graphNode );

			populateChildNodes( tree.Root );

			#region Perform tree layout 

			firstWalk( tree, tree.rootNode, 0 );

			if( tree.Orientation == TreeOrientation.Left || tree.Orientation == TreeOrientation.Top )
			{
				tree.RootXOffset = tree.TopXAdjustment + tree.rootNode.x;
				tree.RootYOffset = tree.TopYAdjustment + tree.rootNode.y;
			}
			else
			{
				tree.RootXOffset = tree.TopXAdjustment + tree.rootNode.x;
				tree.RootYOffset = tree.TopYAdjustment + tree.rootNode.y;
			}

			secondWalk( tree, tree.rootNode, 0, 0, 0 );

			#endregion 

			#region Update source graph 

			var start = tree.Root;

			var offsetX = start.graphNode.Bounds.x - start.x;
			var offsetY = start.graphNode.Bounds.y - start.y;

			start.UpdateGraph( offsetX, offsetY );

			#endregion 

		}

		public float GetNodeSize( Node node )
		{

			if( this.Orientation == TreeOrientation.Top || this.Orientation == TreeOrientation.Bottom )
				return node.width;
			else
				return node.height;

		}

		#endregion

		#region Layout algorithm

		private static void firstWalk( TreeLayout tree, Node node, int level )
		{

			Node leftSibling = null;

			node.x = 0;
			node.y = 0;
			node.prelim = 0;
			node.modifier = 0;
			node.leftNeighbor = null;
			node.rightNeighbor = null;

			tree.setLevelHeight( node, level );
			tree.setLevelWidth( node, level );
			tree.setNeighbors( node, level );

			if( node.ChildCount == 0 || level == tree.MaxDepth )
			{

				leftSibling = node.LeftSibling;
				if( leftSibling != null )
					node.prelim = leftSibling.prelim + tree.GetNodeSize( leftSibling ) + tree.SiblingSeparation;
				else
					node.prelim = 0;

			}
			else
			{

				var n = node.ChildCount;
				for( var i = 0; i < n; i++ )
				{
					var iChild = node.GetChildAt( i );
					firstWalk( tree, iChild, level + 1 );
				}

				var midPoint = node.GetCenter( tree );
				midPoint -= tree.GetNodeSize( node ) / 2;
				leftSibling = node.LeftSibling;
				if( leftSibling != null )
				{
					node.prelim = leftSibling.prelim + tree.GetNodeSize( leftSibling ) + tree.SiblingSeparation;
					node.modifier = node.prelim - midPoint;
					apportion( tree, node, level );
				}
				else
				{
					node.prelim = midPoint;
				}

			}

		}

		private static void apportion( TreeLayout tree, Node node, int level )
		{

			var firstChild = node.FirstChild;
			var firstChildLeftNeighbor = firstChild.leftNeighbor;
			var j = 1;

			for( var k = tree.MaxDepth - level; firstChild != null && firstChildLeftNeighbor != null && j <= k; )
			{

				var modifierSumRight = 0f;
				var modifierSumLeft = 0f;
				var rightAncestor = firstChild;
				var leftAncestor = firstChildLeftNeighbor;

				for( var l = 0; l < j; l++ )
				{
					rightAncestor = rightAncestor.nodeParent;
					leftAncestor = leftAncestor.nodeParent;
					modifierSumRight += rightAncestor.modifier;
					modifierSumLeft += leftAncestor.modifier;
				}

				var totalGap = (
						firstChildLeftNeighbor.prelim +
						modifierSumLeft +
						tree.GetNodeSize( firstChildLeftNeighbor ) +
						tree.SubtreeSeparation
					) - (
						firstChild.prelim +
						modifierSumRight
					);

				if( totalGap > 0 )
				{
					var subtreeAux = node;
					var numSubtrees = 0;
					for( ; subtreeAux != null && subtreeAux != leftAncestor; subtreeAux = subtreeAux.LeftSibling )
						numSubtrees++;

					if( subtreeAux != null )
					{
						var subtreeMoveAux = node;
						var singleGap = totalGap / numSubtrees;
						for( ; subtreeMoveAux != leftAncestor; subtreeMoveAux = subtreeMoveAux.LeftSibling )
						{
							subtreeMoveAux.prelim += totalGap;
							subtreeMoveAux.modifier += totalGap;
							totalGap -= singleGap;
						}

					}
				}

				j++;

				if( firstChild.ChildCount == 0 )
					firstChild = tree.getLeftmost( node, 0, j );
				else
					firstChild = firstChild.FirstChild;

				if( firstChild != null )
					firstChildLeftNeighbor = firstChild.leftNeighbor;

			}

		}

		private static void secondWalk( TreeLayout tree, Node node, int level, float X, float Y )
		{

			if( level <= tree.MaxDepth )
			{

				var xTmp = tree.RootXOffset + node.prelim + X;
				var yTmp = tree.RootYOffset + Y;
				var maxsizeTmp = 0f;
				var nodesizeTmp = 0f;
				var flag = false;

				if( tree.Orientation == TreeOrientation.Top || tree.Orientation == TreeOrientation.Bottom )
				{
					maxsizeTmp = tree.maxLevelHeight[ level ];
					nodesizeTmp = node.height;
				}
				else
				{
					maxsizeTmp = tree.maxLevelWidth[ level ];
					flag = true;
					nodesizeTmp = node.width;
				}

				switch( tree.NodeJustification )
				{
					case NodeAlignment.Top:
						node.x = xTmp;
						node.y = yTmp;
						break;

					case NodeAlignment.Center:
						node.x = xTmp;
						node.y = yTmp + ( maxsizeTmp - nodesizeTmp ) / 2;
						break;

					case NodeAlignment.Bottom:
						node.x = xTmp;
						node.y = ( yTmp + maxsizeTmp ) - nodesizeTmp;
						break;
				}

				if( flag )
				{
					var swapTmp = node.x;
					node.x = node.y;
					node.y = swapTmp;
				}

				if( tree.Orientation == TreeOrientation.Bottom )
				{
					node.y = -node.y - nodesizeTmp;
				}
				else if( tree.Orientation == TreeOrientation.Right )
				{
					node.x = -node.x - nodesizeTmp;
				}

				if( node.ChildCount != 0 )
				{
					secondWalk( tree, node.FirstChild, level + 1, X + node.modifier, Y + maxsizeTmp + tree.LevelSeparation );
				}

				var rightSibling = node.RightSibling;
				if( rightSibling != null )
				{
					secondWalk( tree, rightSibling, level, X, Y );
				}

			}

		}

		private void setLevelHeight( Node node, int level )
		{

			if( !this.maxLevelHeight.ContainsKey( level ) )
				this.maxLevelHeight[ level ] = 0f;

			if( this.maxLevelHeight[ level ] < node.height )
				this.maxLevelHeight[ level ] = node.height;

		}

		private void setLevelWidth( Node node, int level )
		{

			if( !this.maxLevelWidth.ContainsKey( level ) )
				this.maxLevelWidth[ level ] = 0f;

			if( this.maxLevelWidth[ level ] < node.width )
				this.maxLevelWidth[ level ] = node.width;

		}

		private void setNeighbors( Node node, int level )
		{

			if( !previousLevelNode.ContainsKey( level ) )
			{
				previousLevelNode[ level ] = null;
			}

			node.leftNeighbor = this.previousLevelNode[ level ];

			if( node.leftNeighbor != null )
				node.leftNeighbor.rightNeighbor = node;

			this.previousLevelNode[ level ] = node;

		}

		private Node getLeftmost( Node node, int level, int maxlevel )
		{

			if( level >= maxlevel )
				return node;

			if( node.ChildCount == 0 )
				return null;

			var n = node.ChildCount;
			for( var i = 0; i < n; i++ )
			{

				var iChild = node.GetChildAt( i );
				var leftmostDescendant = this.getLeftmost( iChild, level + 1, maxlevel );

				if( leftmostDescendant != null )
					return leftmostDescendant;

			}

			return null;

		}

		#endregion

		#region Private utility methods 

		private static void populateChildNodes( Node node )
		{

			var graphNode = node.graphNode;
			for( int i = 0; i < graphNode.ChildNodes.Count; i++ )
			{
				var childNode = new Node( graphNode.ChildNodes[ i ] );
				node.AddChild( childNode );
				populateChildNodes( childNode );
			}

		}

		#endregion 

		#region Nested types

		public class Node
		{

			#region Public fields

			public GraphNodeBase graphNode;

			public float x = 0;
			public float y = 0;
			public float width;
			public float height;

			public int siblingIndex = 0;
			public int index = 0;

			public float prelim = 0;
			public float modifier = 0;

			public Node leftNeighbor = null;
			public Node rightNeighbor = null;
			public Node nodeParent = null;
			public List<Node> nodeChildren = new List<Node>();

			#endregion

			#region Constructor

			internal Node()
			{
				this.graphNode = null;
				this.width = 2;
				this.height = 2;
			}

			public Node( GraphNodeBase graphNode )
			{

				this.graphNode = graphNode;

				this.width = graphNode.Bounds.width;
				this.height = graphNode.Bounds.height;

			}

			#endregion

			#region Public properties

			public int Level
			{
				get
				{
					if( this.nodeParent == null )
						return 0;
					else
						return this.nodeParent.Level + 1;
				}
			}

			public Node LeftSibling
			{
				get
				{
					if( this.leftNeighbor != null && this.leftNeighbor.nodeParent == this.nodeParent )
						return this.leftNeighbor;
					else
						return null;
				}
			}

			public Node RightSibling
			{
				get
				{
					if( this.rightNeighbor != null && this.rightNeighbor.nodeParent == this.nodeParent )
						return this.rightNeighbor;
					else
						return null;
				}
			}

			public int ChildCount
			{
				get
				{
					return this.nodeChildren.Count;
				}
			}

			public Node FirstChild
			{
				get
				{
					return this.nodeChildren[ 0 ];
				}
			}

			public Node LastChild
			{
				get
				{
					return this.nodeChildren[ this.nodeChildren.Count - 1 ];
				}
			}

			#endregion

			#region Public methods

			public void UpdateGraph( float offsetX, float offsetY )
			{

				if( this.graphNode != null )
				{
					this.graphNode.Bounds.x = this.x + offsetX;
					this.graphNode.Bounds.y = this.y + offsetY;
				}

				for( int i = 0; i < nodeChildren.Count; i++ )
				{
					nodeChildren[ i ].UpdateGraph( offsetX, offsetY );
				}

			}

			public void AddChild( Node child )
			{
				this.nodeChildren.Add( child );
				child.nodeParent = this;
			}

			public Node GetChildAt( int index )
			{
				return this.nodeChildren[ index ];
			}

			public float GetCenter( TreeLayout tree )
			{

				var first = this.FirstChild;
				var last = this.LastChild;

				return first.prelim + ( ( last.prelim - first.prelim ) + tree.GetNodeSize( last ) ) / 2;

			}

			#endregion

		}

		#endregion

	}

	public enum TreeOrientation
	{
		Top = 0,
		Bottom = 1,
		Right = 2,
		Left = 3
	}

	public enum NodeAlignment
	{
		Top = 0,
		Center = 1,
		Bottom = 2
	}

}
