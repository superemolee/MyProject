// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using StagPoint.Core;

namespace StagPoint.Planning
{

	[Serializable]
	public partial class TaskNetworkGraph : ScriptableObject, ISerializationCallbackReceiver
	{

		#region Public fields 

		[NonSerialized]
		public System.Type DomainType = null;

		[NonSerialized]
		public List<GraphNodeBase> Nodes = new List<GraphNodeBase>();

		public Blackboard BlackboardDefinition;

		/// <summary>
		/// Used to indicate whether the graph has been initialized at runtime. 
		/// </summary>
		public bool IsInitialized { get; private set; }

		#endregion 

		#region Custom serialization variables 

		[SerializeField]
		[HideInInspector]
		private byte[] serializedData = null;

		[SerializeField]
		private List<UnityEngine.Object> serializedObjects = new List<UnityEngine.Object>();

		[SerializeField]
#pragma warning disable 414
		private int version = 0;
#pragma warning restore 414

		#endregion

		#region Public properties

		/// <summary>
		/// Returns the root node for the Hierarchical Task Network
		/// </summary>
		public RootNode RootNode
		{
			get
			{

				ensureRootNodeExists();
				
				for( int i = 0; i < Nodes.Count; i++ )
				{
					if( Nodes[ i ] is RootNode )
					{
						return (RootNode)Nodes[ i ];
					}
				}
				
				return null;

			}
		}

		#endregion 

		#region Public methods 

		public void Initialize( object agent, Blackboard blackboard )
		{

			if( IsInitialized )
				return;

			if( agent == null )
				throw new ArgumentNullException( "agent" );

			if( blackboard == null )
				throw new ArgumentNullException( "blackboard" );

			if( !DomainType.IsAssignableFrom( agent.GetType() ) )
				throw new ArgumentException( string.Format( "The agent argument must be of type " + DomainType.FullName ) );

			IsInitialized = true;

			RootNode.Initialize( agent, blackboard );

		}

		public void Validate()
		{

			ensureRootNodeExists();

			var allNodes = this.GetAllNodes();
			for( int i = 0; i < allNodes.Count; i++ )
			{
				allNodes[ i ].Validate();
			}

		}

		public List<GraphNodeBase> GetAllNodes()
		{

			if( Nodes == null )
				Nodes = new List<GraphNodeBase>();

			ensureRootNodeExists();

			var allNodes = new List<GraphNodeBase>();
			getAllNodes( RootNode, allNodes );

			return allNodes;

		}

		public List<GraphNodeBase> GetVisibleNodes()
		{

			if( Nodes == null )
				Nodes = new List<GraphNodeBase>();

			ensureRootNodeExists();

			var allNodes = new List<GraphNodeBase>();
			getVisibleNodes( RootNode, allNodes );

			return allNodes;

		}

		public GraphNodeBase GetParentNode( GraphNodeBase node )
		{

			for( int i = 0; i < Nodes.Count; i++ )
			{
				var parent = getParent( Nodes[ i ], node );
				if( parent != null )
					return parent;
			}

			return null;

		}

		#endregion 

		#region Private utility methods 

		private GraphNodeBase getParent( GraphNodeBase branch, GraphNodeBase node )
		{

			if( branch.ChildNodes.Contains( node ) )
				return branch;

			for( int i = 0; i < branch.ChildNodes.Count; i++ )
			{

				var parent = getParent( branch.ChildNodes[ i ], node );
				if( parent != null )
					return parent;

			}

			return null;

		}

		private void ensureRootNodeExists()
		{

			if( Nodes == null || Nodes.Count == 0 )
			{


				Debug.LogWarning( "Graph's nodes collection is empty: " + this.name );

				if( Nodes == null )
				{
					Nodes = new List<GraphNodeBase>();
				}

				var rootNode = new RootNode()
				{
					Bounds = new Rect( 100, 100, 300, 30 ),
					Name = "Hierarchical Task Network",
					Notes = "This is the start node of your Hierarchical Task Network"
				};

				Nodes.Add( rootNode );

			}

		}

		private void getAllNodes( GraphNodeBase node, List<GraphNodeBase> allNodes )
		{

			if( node == null )
				return;

			if( allNodes.Contains( node ) )
				return;

			allNodes.Add( node );

			for( int i = 0; i < node.ChildNodes.Count; i++ )
			{
				getAllNodes( node.ChildNodes[ i ], allNodes );
			}

		}

		private void getVisibleNodes( GraphNodeBase node, List<GraphNodeBase> allNodes )
		{

			allNodes.Add( node );

			if( node.ChildNodes.Count == 0 || !node.IsExpanded )
				return;

			for( int i = 0; i < node.ChildNodes.Count; i++ )
			{
				getVisibleNodes( node.ChildNodes[ i ], allNodes );
			}

		}

		private void updateHierarchy( GraphNodeBase node, int nodeIndex, int depth )
		{

			node.Index = nodeIndex;
			node.Depth = depth;

			for( int i = 0; i < node.ChildNodes.Count; i++ )
			{

				var child = node.ChildNodes[ i ];
				child.Parent = node;

				updateHierarchy( child, i, depth + 1 );

			}

		}

		#endregion 

		#region ISerializationCallbackReceiver Members

		public void OnAfterDeserialize()
		{

			if( serializedData == null || serializedData.Length == 0 )
				return;

			using( var store = new SerializationHelper( serializedData, serializedObjects ) )
			{

				this.DomainType = store.Read() as System.Type;

				Nodes.Clear();
				Nodes.AddRange( ( (IEnumerable<GraphNodeBase>)store.Read() ).Distinct() );

				if( !store.EndOfFile )
				{
					store.Read();
				}

			}

			ensureRootNodeExists();

			updateHierarchy( this.RootNode, 0, 0 );

		}

		public void OnBeforeSerialize()
		{

			using( var store = new SerializationHelper( serializedObjects ) )
			{

				store.Write( this.DomainType );

				store.Write( Nodes );
				serializedData = store.GetBuffer();

			}

		}

		#endregion

	}

#if UNITY_EDITOR

	/*****************************
	 * Design-time support only *
	 ****************************/

	public partial class TaskNetworkGraph : ScriptableObject, ISerializationCallbackReceiver
	{

		public void MarkDirty()
		{
			this.version += 1;
		}

		public GraphNodeBase HitTest( Vector2 pos )
		{

			for( int i = 0; i < Nodes.Count; i++ )
			{
				var hit = hitTest( Nodes[ i ], pos );
				if( hit != null )
					return hit;
			}

			return null;

		}

		public GraphNodeBase FindByUID( string uid )
		{

			for( int i = 0; i < Nodes.Count; i++ )
			{
				var test = findByUID( Nodes[ i ], uid );
				if( test != null )
					return test;
			}

			return null;

		}

		private GraphNodeBase findByUID( GraphNodeBase node, string uid )
		{

			if( node == null )
				return null;

			if( node.UID == uid )
				return node;

			for( int i = 0; i < node.ChildNodes.Count; i++ )
			{
				var test = findByUID( node.ChildNodes[ i ], uid );
				if( test != null )
					return test;
			}

			return null;

		}

		private GraphNodeBase hitTest( GraphNodeBase node, Vector2 pos )
		{

			var hitType = node.HitTest( pos );
			if( hitType != NodeHitType.None )
				return node;

			if( !node.IsExpanded )
				return null;

			for( int i = 0; i < node.ChildNodes.Count; i++ )
			{
				var hit = hitTest( node.ChildNodes[ i ], pos );
				if( hit != null )
					return hit;
			}

			return null;

		}

	}

#endif

}
