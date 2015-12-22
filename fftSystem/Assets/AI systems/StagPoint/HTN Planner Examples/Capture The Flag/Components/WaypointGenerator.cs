// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace StagPoint.Examples
{

	[AddComponentMenu( "StagPoint/Examples/Capture the Flag/Waypoint Generator" )]
	public class WaypointGenerator : MonoBehaviour
	{

		[NonSerialized]
		public static WaypointGenerator Instance;

		public Texture2D levelMap;

		[NonSerialized]
		public List<Vector3> waypoints = new List<Vector3>();

		[NonSerialized]
		private bool[ , ] walkable = null;

		void OnEnable()
		{
			Instance = this;
			generateWaypoints();
		}

		void OnDisable()
		{
			Instance = null;
		}

		void OnDrawGizmos()
		{

			if( !this.enabled )
				return;

			if( waypoints.Count == 0 )
			{
				generateWaypoints();
			}

			Gizmos.color = Color.blue;
			for( int i = 0; i < waypoints.Count; i++ )
			{
				Gizmos.DrawSphere( waypoints[ i ], 0.25f );
			}

		}

		private void loadMap()
		{

			if( this.levelMap == null )
				throw new Exception( "No level map image was set" );

			this.walkable = new bool[ levelMap.width, levelMap.height ];

			for( int x = 0; x < levelMap.width; x++ )
			{
				for( int y = 0; y < levelMap.height; y++ )
				{
					var pixel = levelMap.GetPixel( x, y );
					this.walkable[ x, y ] = pixel.g > 0.6f;
				}
			}

		}

		private void generateWaypoints()
		{

			loadMap();

			var work = new bool[ levelMap.width, levelMap.height ];

			for( int x = 3; x < levelMap.width - 3; x += 3 )
			{
				for( int y = 3; y < levelMap.height - 3; y += 3 )
				{
					if( walkable[ x, y ] )
					{

						var isNearObstacle = false;

						for( int distance = 1; distance < 3; distance++ )
						{

							var omit =
								!walkable[ x - distance, y ] ||
								!walkable[ x + distance, y ] ||
								!walkable[ x, y - distance ] ||
								!walkable[ x, y + distance ];

							if( omit )
							{
								isNearObstacle = true;
								break;
							}

						}

						work[ x, y ] = !isNearObstacle;

					}
				}
			}

			for( int x = 0; x < levelMap.width; x++ )
			{

				for( int y = 0; y < levelMap.height; y++ )
				{

					if( !work[ x, y ] )
						continue;

					var location = new Vector3(
						x - levelMap.width / 2 + 0.5f,
						0.15f,
						y - levelMap.height / 2 + 0.5f
					);

					waypoints.Add( location );

				}

			}

		}

	}

}
