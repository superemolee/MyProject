using System;
using System.Runtime.InteropServices;

using UnityEngine;

namespace StagPoint.Shared
{

	public class LineSegment
	{

		#region Private fields

		private Vector3? _center;
		private Vector3? _dir;
		private float? _length;

		private Vector3 _start;
		private Vector3 _end;

		#endregion

		#region Public properties

		public Vector3 start
		{
			get { return _start; }
			set { _start = value; resetNullableFields(); }
		}

		public Vector3 end
		{
			get { return _end; }
			set { _end = value; resetNullableFields(); }
		}

		public float length
		{
			get
			{
				if( !this._length.HasValue )
					this._length = Vector3.Distance( start, end );

				return this._length.Value;
			}
		}

		public Vector3 direction
		{
			get
			{
				if( !this._dir.HasValue )
					this._dir = ( end - start ).normalized;

				return this._dir.Value;
			}
		}

		public Vector3 center
		{
			get
			{
				if( !this._center.HasValue )
					this._center = ( start + ( end - start ) * 0.5f );

				return this._center.Value;
			}
		}

		#endregion

		#region Constructor

		public LineSegment( Vector3 start, Vector3 end )
		{

			this.start = start;
			this.end = end;

			resetNullableFields();

		}

		#endregion

		#region Public methods

		public LineSegment Flip()
		{
			return new LineSegment( end, start );
		}

		public LineSegment Shrink( float amount )
		{

			amount *= 0.5f;

			var width = ( end - start ).magnitude;
			amount = Mathf.Min( amount, width * 0.5f - Mathf.Epsilon );

			Vector3 newA = start + Vector3.Normalize( center - start ) * amount;
			Vector3 newB = end + Vector3.Normalize( center - end ) * amount;

			return new LineSegment( newA, newB );

		}

		/// <summary>
		/// Returns true if the lines intersect, otherwise false. If the lines intersect, 
		/// intersectionPoint holds the intersection point.
		/// </summary>
		public bool Intersects2D( LineSegment line, out Vector3 intersectionPoint )
		{
			return Intersects2D( this, line, out intersectionPoint );
		}

		/// <summary>
		/// Returns true if the lines intersect, otherwise false.
		/// </summary>
		public bool Intersects2D( LineSegment line )
		{

			Vector3 intersectionPoint;

			if( Intersects2D( this, line, out intersectionPoint ) )
			{

				#region Ignore "false positives" where only the segment extremes intersect

				if( intersectionPoint == start || intersectionPoint == end )
					return false;

				if( intersectionPoint == line.start || intersectionPoint == line.end )
					return false;

				#endregion

				return true;

			}

			return false;

		}

		/// <summary>
		/// Returns true if the lines intersect, otherwise false. If the lines intersect, 
		/// intersectionPoint holds the intersection point.
		/// </summary>
		public bool Intersects2D( Vector3 start, Vector3 end, out Vector3 intersectionPoint )
		{
			return Intersects2D( this, new LineSegment( start, end ), out intersectionPoint );
		}

		/// <summary>
		/// Returns the distance from the test point to the line segment.
		/// </summary>
		/// <param name="test"></param>
		/// <returns></returns>
		public float DistanceFromLine( Vector3 test )
		{

			Vector3 v = start - end;
			Vector3 w = test - end;

			float c1 = Vector3.Dot( w, v );
			if( c1 <= 0 )
				return Vector3.Distance( test, end );

			float c2 = Vector3.Dot( v, v );
			if( c2 <= c1 )
				return Vector3.Distance( test, start );

			float b = c1 / c2;
			Vector3 Pb = end + b * v;

			return Vector3.Distance( test, Pb );

		}

		/// <summary>
		/// Determines which side of an infinite line a point lies. Return values are
		/// -1 for "left side", 0 for "on the line", and 1 for "right side".
		/// </summary>
		/// <param name="test">The point to test</param>
		/// <returns>-1 = Left side, 0 = On the line, 1 = Right side</returns>
		public float SideOfLine( Vector3 test )
		{
			return SideOfLine( start, end, test );
		}

		/// <summary>
		/// Returns the point on the line segment which is closest to the test point.
		/// </summary>
		/// <param name="test">The point to test</param>
		/// <param name="clamp">Whether to constrain the results to the line segment. If set to FALSE, the test will treat the line segment as an infinite line.</param>
		/// <returns></returns>
		public Vector3 ClosestPoint( Vector3 test, bool clamp )
		{

			// http://www.gamedev.net/community/forums/topic.asp?topic_id=198199&whichpage=1&#1250842

			Vector3 c = test - start;				// Vector from a to Point
			Vector3 v = ( end - start ).normalized;	// Unit Vector from a to b
			float t = Vector3.Dot( v, c );			// Intersection point Distance from a

			// Check to see if the point is on the line
			// if not then return the endpoint
			if( clamp )
			{
				if( t < 0 )
					return start;
				if( t > length )
					return end;
			}

			// get the distance to move from point a
			v *= t;

			// move from point a to the nearest point on the segment
			return start + v;

		}

		#endregion

		#region Static methods

		/// <summary>
		/// Returns true if the lines intersect, otherwise false. If the lines intersect, 
		/// intersectionPoint holds the intersection point.
		/// </summary>
		public static bool Intersects2D( LineSegment lhs, LineSegment rhs, out Vector3 intersectionPoint )
		{

			intersectionPoint = Vector3.zero;

			float deltaACy = lhs.start.z - rhs.start.z;
			float deltaDCx = rhs.end.x - rhs.start.x;
			float deltaACx = lhs.start.x - rhs.start.x;
			float deltaDCy = rhs.end.z - rhs.start.z;
			float deltaBAx = lhs.end.x - lhs.start.x;
			float deltaBAy = lhs.end.z - lhs.start.z;

			float denominator = deltaBAx * deltaDCy - deltaBAy * deltaDCx;
			float numerator = deltaACy * deltaDCx - deltaACx * deltaDCy;

			if( denominator == 0 )
			{
				if( numerator == 0 )
				{
					// Collinear. Potentially infinite intersection points.
					// Check and return one of them.
					if( lhs.start.x >= rhs.start.x && lhs.start.x <= rhs.end.x )
					{
						intersectionPoint = lhs.start;
						return true;
					}
					else if( rhs.start.x >= lhs.start.x && rhs.start.x <= lhs.end.x )
					{
						intersectionPoint = rhs.start;
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					// parallel
					return false;
				}
			}

			float r = numerator / denominator;
			if( r < 0 || r > 1 )
			{
				return false;
			}

			float s = ( deltaACy * deltaBAx - deltaACx * deltaBAy ) / denominator;
			if( s < 0 || s > 1 )
			{
				return false;
			}

			intersectionPoint = new Vector3( lhs.start.x + r * deltaBAx, 0, lhs.start.z + r * deltaBAy );

			return true;

		}

		/// <summary>
		/// Determines which side of an infinite line a point lies. Return values are
		/// -1 for "left side", 0 for "on the line", and 1 for "right side".
		/// </summary>
		/// <param name="a">The starting point of the line</param>
		/// <param name="b">The ending point of the line</param>
		/// <param name="point">The point to test</param>
		/// <returns>-1 = Left side, 0 = On the line, 1 = Right side</returns>
		public static float SideOfLine( Vector3 start, Vector3 end, Vector3 test )
		{

			float ax = end.x - start.x;
			float az = end.z - start.z;
			float bx = test.x - start.x;
			float bz = test.z - start.z;

			return Mathf.Sign( bx * az - ax * bz );

			//var dir = end - start;
			//var perp = new Vector3( -dir.z, 0, dir.x );
			//float d = Vector3.Dot( test - start, perp );

			//return Mathf.Sign( d );

		}

		public static Vector3 ClosestPointOnSegment( Vector3 start, Vector3 end, Vector3 test, bool clamp )
		{

			// http://www.gamedev.net/community/forums/topic.asp?topic_id=198199&whichpage=1&#1250842

			Vector3 c = test - start;				// Vector from start to end
			Vector3 v = ( end - start ).normalized;	// Unit Vector from start to end
			float d = ( end - start ).magnitude;	// Length of the line segment
			float t = Vector3.Dot( v, c );			// Intersection point distance from start

			// Check to see if the point is on the line
			// if not then return the endpoint
			if( clamp )
			{
				if( t < 0 )
					return start;
				if( t > d )
					return end;
			}

			// get the distance to move from point a
			v *= t;

			// move from point a to the nearest point on the segment
			return start + v;

		}

		public static float DistanceFromLine( Vector3 start, Vector3 end, Vector3 test )
		{

			Vector3 v = start - end;
			Vector3 w = test - end;

			float c1 = Vector3.Dot( w, v );
			if( c1 <= 0 )
				return Vector3.Distance( test, end );

			float c2 = Vector3.Dot( v, v );
			if( c2 <= c1 )
				return Vector3.Distance( test, start );

			float b = c1 / c2;
			Vector3 Pb = end + b * v;

			return Vector3.Distance( test, Pb );

		}

		#endregion

		#region Base class overrides

		public override string ToString()
		{
			return string.Format( "{0}->{1}", start, end );
		}

		#endregion

		#region Private static methods 

		private void resetNullableFields()
		{
			_center = null;
			_length = null;
			_dir = null;
		}

		#endregion 

	}

}
