// Copyright (c) 2014 StagPoint Consulting

using UnityEngine;
using System.Collections;

using System.Diagnostics;

using StagPoint.Core;
using StagPoint.Planning;

namespace StagPoint.Examples
{

	public class CoverNode
	{

		/// <summary>
		/// Direction vectors corresponding to the values stored in arrays such as visibilityRange, coverDirection, etc.
		/// </summary>
		public static Vector3[] Directions = new Vector3[] { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };

		/// <summary>
		/// The location of this cover node
		/// </summary>
		public Vector3 position;

		/// <summary>
		/// Returns the range of visibility in the North, East, South, and West directions (in that order)
		/// </summary>
		public float[] visibilityRange = new float[] { 0f, 0f, 0f, 0f };

		/// <summary>
		/// Indicates whether an agent can move in the indicated direction from this cover node
		/// </summary>
		public bool[] canMoveInDirection = new bool[] { false, false, false, false };

		/// <summary>
		/// Indicates whether this node provides cover in the North, East, South, or West direction (in that order)
		/// </summary>
		public bool[] coverDirection = new bool[] { false, false, false, false };


		private CTF_Agent reservedFor;

		public bool IsReserved
		{
			get { return reservedFor != null && reservedFor.gameObject != null; }
		}

		public void ReserveFor( CTF_Agent agent )
		{
			reservedFor = agent;
		}

		[Conditional( "UNITY_EDITOR" )]
		internal void OnDrawGizmos()
		{

			Gizmos.color = Color.magenta;
			Gizmos.DrawSphere( this.position, 0.15f );

			for( int i = 0; i < 4; i++ )
			{
				if( coverDirection[ i ] )
				{
					Gizmos.DrawLine( position, position + Directions[ i ] * 1 );
				}
			}

		}

	}

}
