// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using StagPoint.Shared;

namespace StagPoint.Examples
{

	[AddComponentMenu( "StagPoint/Examples/Capture the Flag/Cover Node Generator" )]
	public class CoverPointGenerator : MonoBehaviour
	{

		[NonSerialized]
		public static CoverPointGenerator Instance;

		public List<CoverNode> coverNodes = new List<CoverNode>();

		[SerializeField]
		private Texture2D levelMap = null;

		private bool[ , ] viewable = null;
		private bool[ , ] walkable = null;

		void OnEnable()
		{
			Instance = this;
			generateCoverNodes();
		}

		void OnDisable()
		{
			Instance = null;
		}

		void OnDrawGizmos()
		{

			if( !this.enabled )
				return;

			generateCoverNodes();

			Gizmos.color = Color.magenta;
			for( int i = 0; i < coverNodes.Count; i++ )
			{
				coverNodes[ i ].OnDrawGizmos();
			}

		}

		#region Cover node querying

		public IEnumerable<CoverNode> FindAmbushNodes( Vector3 threatPosition, Vector3 defensePosition, float width, float nearAdjust, float farAdjust, LayerMask raycastLayers )
		{

			// Make sure that the cover nodes have been properly generated before attempting to query.
			if( coverNodes == null || coverNodes.Count == 0 )
			{
				generateCoverNodes();
			}

			// Ensure that this is treated as a 2D problem, and eliminate low-height walls in raycasts
			threatPosition.y = defensePosition.y = 1.25f;

			var toTarget = ( threatPosition - defensePosition ).normalized;
			var perp = Vector3.Cross( toTarget, Vector3.up ) * width;

			var boundingVolume = new List<LineSegment>()
		{
			new LineSegment( defensePosition - perp + toTarget * nearAdjust, defensePosition + perp + toTarget * nearAdjust ), // Near
			new LineSegment( threatPosition + perp + toTarget * farAdjust, threatPosition - perp + toTarget * farAdjust ), // Far
			new LineSegment( threatPosition - perp + toTarget * farAdjust, defensePosition - perp + toTarget * nearAdjust ), // Right
			new LineSegment( defensePosition + perp + toTarget * nearAdjust, threatPosition + perp + toTarget * farAdjust), // Left
		};

			for( int i = 0; i < coverNodes.Count; i++ )
			{

				var node = coverNodes[ i ];

				if( node.IsReserved )
					continue;

				if( !isInBounds( node, boundingVolume ) )
					continue;

				if( providesCoverInTargetDirection( node, threatPosition ) )
				{

					if( targetCanSeePosition( threatPosition, node.position, raycastLayers ) )
						continue;

					if( targetCanSeePosition( defensePosition, node.position, raycastLayers ) )
						yield return node;

				}

			}

		}

		public IEnumerable<CoverNode> FindCoverNodes( Vector3 threatPosition, Vector3 agentPosition, float width, float nearAdjust, float farAdjust, LayerMask raycastLayers )
		{

			// Make sure that the cover nodes have been properly generated before attempting to query.
			if( coverNodes == null || coverNodes.Count == 0 )
			{
				generateCoverNodes();
			}

			// Ensure that this is treated as a 2D problem, and eliminate low-height walls in raycasts
			threatPosition.y = agentPosition.y = 1.25f;

			var toTarget = ( threatPosition - agentPosition ).normalized;
			var perp = Vector3.Cross( toTarget, Vector3.up ) * width;

			var boundingVolume = new List<LineSegment>()
		{
			new LineSegment( agentPosition - perp + toTarget * nearAdjust, agentPosition + perp + toTarget * nearAdjust ), // Near
			new LineSegment( threatPosition + perp + toTarget * farAdjust, threatPosition - perp + toTarget * farAdjust ), // Far
			new LineSegment( threatPosition - perp + toTarget * farAdjust, agentPosition - perp + toTarget * nearAdjust ), // Right
			new LineSegment( agentPosition + perp + toTarget * nearAdjust, threatPosition + perp + toTarget * farAdjust), // Left
		};

			for( int i = 0; i < coverNodes.Count; i++ )
			{

				var node = coverNodes[ i ];

				if( node.IsReserved )
					continue;

				if( !isInBounds( node, boundingVolume ) )
					continue;

				var isValid = evaluateCover( node, threatPosition, raycastLayers );
				if( isValid )
				{
					yield return node;
				}

			}

		}

		private bool isInBounds( CoverNode node, List<LineSegment> bounds )
		{

			for( int i = 0; i < bounds.Count; i++ )
			{
				if( bounds[ i ].SideOfLine( node.position ) <= 0 )
					return false;
			}

			return true;

		}

		private bool evaluateCover( CoverNode node, Vector3 targetPosition, LayerMask layers )
		{

			if( !providesCoverInTargetDirection( node, targetPosition ) )
				return false;

			if( targetCanSeePosition( targetPosition, node.position, layers ) )
				return false;

			// Determine whether cover node has a firing position available for target
			for( int i = 0; i < 4; i++ )
			{

				if( node.visibilityRange[ i ] < 3 )
					continue;

				if( !node.canMoveInDirection[ i ] )
					continue;

				if( targetCanSeePosition( targetPosition, node.position + CoverNode.Directions[ i ], layers ) )
					return true;

			}

			return false;

		}

		private bool targetCanSeePosition( Vector3 targetPosition, Vector3 position, LayerMask layers )
		{

			// Ensure that raycasts happen at bot eye height. Bots can see over half-height walls.
			targetPosition.y = position.y = 1.25f;

			var vectorToNode = position - targetPosition;
			var ray = new Ray( targetPosition, vectorToNode );

			if( !Physics.Raycast( ray, vectorToNode.magnitude, layers ) )
			{
				return true;
			}

			return false;

		}

		private bool providesCoverInTargetDirection( CoverNode node, Vector3 targetPosition )
		{

			for( int i = 0; i < 4; i++ )
			{

				if( node.visibilityRange[ i ] == 0 )
				{

					var perp = Vector3.Cross( CoverNode.Directions[ i ], Vector3.up );

					if( LineSegment.SideOfLine( node.position, node.position + perp, targetPosition ) > 0 )
					{
						return true;
					}

				}

			}

			return false;

		}

		#endregion

		#region Cover node generation

		private void generateCoverNodes()
		{

			if( coverNodes.Count > 0 )
				return;

			loadMap();

			for( int x = 0; x < levelMap.width; x++ )
			{
				for( int y = 0; y < levelMap.height; y++ )
				{
					analyzeCover( x, y );
				}
			}

		}

		private void analyzeCover( int x, int y )
		{

			if( !walkable[ x, y ] )
				return;

			if( x == 0 || x == levelMap.width - 1 )
				return;

			if( y == 0 || y == levelMap.height - 1 )
				return;

			var distances = calculateViewDistances( x, y );
			if( Mathf.Min( distances ) > 0 || Mathf.Max( distances ) <= 3 )
				return;

			var coverDirections = new bool[] 
		{ 
			providesVerticalCover( x, y, 1 ),		// North
			providesHorizontalCover( x, y, 1 ),		// East
			providesVerticalCover( x, y, -1 ),		// South
			providesHorizontalCover( x, y, -1 ),	// West
		};

			if( !coverDirections.Contains( true ) )
				return;

			var moveDirections = new bool[]
		{
			canMoveTo( x, y + 1 ),
			canMoveTo( x + 1, y ),
			canMoveTo( x, y - 1 ),
			canMoveTo( x - 1, y )
		};

			var location = new Vector3(
				x - levelMap.width / 2 + 0.5f,
				0.15f,
				y - levelMap.height / 2 + 0.5f
			);

			NavMeshHit navHit;
			if( NavMesh.FindClosestEdge( location, out navHit, 0x255 ) )
			{
				location = navHit.position;
			}

			var node = new CoverNode()
			{
				position = location,
				visibilityRange = distances,
				coverDirection = coverDirections,
				canMoveInDirection = moveDirections
			};

			coverNodes.Add( node );

		}

		private bool canMoveTo( int x, int y )
		{

			if( x < 0 || x >= levelMap.width )
				return false;

			if( y < 0 || y >= levelMap.height )
				return false;

			return walkable[ x, y ];

		}

		private bool providesVerticalCover( int x, int y, int deltaY )
		{

			if( viewable[ x, y + deltaY ] )
				return false;

			var a = calculateViewDistance( x - 1, y, 0, deltaY, 5 );
			var b = calculateViewDistance( x + 1, y, 0, deltaY, 5 );

			if( a < 5 && b < 5 )
				return false;

			return true;

		}

		private bool providesHorizontalCover( int x, int y, int deltaX )
		{

			if( viewable[ x + deltaX, y ] )
				return false;

			var a = calculateViewDistance( x, y - 1, deltaX, 0, 5 );
			var b = calculateViewDistance( x, y + 1, deltaX, 0, 5 );

			if( a < 5 && b < 5 )
				return false;

			return true;

		}

		private float[] calculateViewDistances( int x, int y )
		{

			var result = new float[] 
		{ 
			calculateViewDistance( x, y, 0, 1 ), // North
			calculateViewDistance( x, y, 1, 0 ), // East
			calculateViewDistance( x, y, 0, -1 ), // South
			calculateViewDistance( x, y, -1, 0 ), // West
		};


			return result;

		}

		private float calculateViewDistance( int x, int y, int deltaX, int deltaY )
		{
			return calculateViewDistance( x, y, deltaX, deltaY, levelMap.height );
		}

		private float calculateViewDistance( int x, int y, int deltaX, int deltaY, int max )
		{

			var distance = 0f;

			for( int i = 0; i < max; i++ )
			{

				x += deltaX;
				if( x < 0 || x >= levelMap.width )
					break;

				y += deltaY;
				if( y < 0 || y >= levelMap.height )
					break;

				if( !viewable[ x, y ] )
					break;

				distance += 1;

			}

			return distance;

		}

		private void loadMap()
		{

			if( this.levelMap == null )
				throw new Exception( "No level map image was set" );

			this.viewable = new bool[ levelMap.width, levelMap.height ];
			this.walkable = new bool[ levelMap.width, levelMap.height ];

			for( int x = 0; x < levelMap.width; x++ )
			{
				for( int y = 0; y < levelMap.height; y++ )
				{
					var pixel = levelMap.GetPixel( x, y );
					this.viewable[ x, y ] = pixel.g > 0.4f;
					this.walkable[ x, y ] = pixel.g > 0.6f;
				}
			}

		}

		#endregion

	}

}
