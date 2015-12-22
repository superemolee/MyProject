// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using StagPoint.Shared;

namespace StagPoint.Examples
{

	public class DebugRender
	{

		#region Render materials

		private static Material _solidColor = null;
		private static Material _simpleMaterial = null;
		private static Material _solidLine = null;

		/// <summary>
		/// Provides a shared material suitable for simple drawing operations.
		/// (Such as debug visualizations.)
		/// </summary>
		public static Material SolidColor
		{
			get
			{
				if( !_solidColor )
				{
					_solidColor = new Material( @"
Shader ""Solid Color"" {
	Properties {
		_Color (""Main Color"", Color) = (1,1,1,1)
	}
	SubShader {
		Pass { 
			Color [_Color] 
			Cull Off
			//ZWrite Off Fog { Mode Off }
			//Offset 0, -1
		}
	}
}" );
					_solidColor.hideFlags = HideFlags.HideAndDontSave;
					_solidColor.shader.hideFlags = HideFlags.HideAndDontSave;
				}
				return _solidColor;
			}
		}

		/// <summary>
		/// Provides a shared material suitable for simple drawing operations.
		/// (Such as debug visualizations.)
		/// </summary>
		public static Material SolidLine
		{
			get
			{
				if( !_solidLine )
				{
					_solidLine = new Material( @"
Shader ""Solid Line"" {
	Properties {
		_Color (""Main Color"", Color) = (1,1,1,1)
	}
	SubShader {
		Pass { 
			Color [_Color] 
			Cull Off
			//ZWrite Off Cull Off Fog { Mode Off }
			Offset -1, -1
		}
	}
}" );
					_solidLine.hideFlags = HideFlags.HideAndDontSave;
					_solidLine.shader.hideFlags = HideFlags.HideAndDontSave;
				}
				return _solidLine;
			}
		}

		/// <summary>
		/// Provides a shared material suitable for simple drawing operations.
		/// (Such as debug visualizations.)
		/// </summary>
		public static Material SimpleMaterial
		{
			get
			{
				if( !_simpleMaterial )
				{
					_simpleMaterial = new Material(
						"Shader \"Lines/Colored Blended\" {"
						+ "SubShader { Pass { "
						+ "	BindChannels { Bind \"Color\",color } "
						+ "	Blend SrcAlpha OneMinusSrcAlpha "
						+ "	ZWrite Off Cull Off Fog { Mode Off } "
						+ " Offset -1, -1 "
						+ "} } }" );
					_simpleMaterial.hideFlags = HideFlags.HideAndDontSave;
					_simpleMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
				}
				return _simpleMaterial;
			}
		}

		#endregion

		#region Static data

		private static Vector3[] discPoints = new Vector3[ 12 ];

		#endregion

		public static void DrawLabel( string text, Vector2 position, TextAnchor anchor )
		{
			DrawLabel( text, position, anchor, GUI.skin.label );
		}

		public static void DrawLabel( string text, Vector2 position, TextAnchor anchor, GUIStyle style )
		{

			var guiContent = new GUIContent( text );
			var size = style.CalcSize( guiContent );
			var half = size * 0.5f;

			var offset = Vector2.zero;
			switch( anchor )
			{
				case TextAnchor.LowerCenter:
					offset.x -= half.x;
					offset.y -= size.y;
					break;
				case TextAnchor.LowerLeft:
					offset.y -= size.y;
					break;
				case TextAnchor.LowerRight:
					offset.x -= size.x;
					offset.y -= size.y;
					break;
				case TextAnchor.MiddleCenter:
					offset.x -= half.x;
					offset.y -= half.y;
					break;
				case TextAnchor.MiddleLeft:
					offset.y -= half.y;
					break;
				case TextAnchor.MiddleRight:
					offset.x -= size.x;
					offset.y -= half.y;
					break;
				case TextAnchor.UpperCenter:
					offset.x -= half.x;
					break;
				case TextAnchor.UpperLeft:
					break;
				case TextAnchor.UpperRight:
					offset.x -= size.x;
					break;
				default:
					break;
			}

			var rect = new Rect( position.x + offset.x, position.y + offset.y, size.x, size.y );
			GUI.Label( rect, guiContent, style );

		}

		public static void DrawLine( Vector3 start, Vector3 end, Color color )
		{

			GL.PushMatrix();
			{

				SolidLine.SetPass( 0 );
				SolidLine.color = color;

				GL.Color( color );

				GL.Begin( GL.LINES );
				{
					GL.Vertex( start );
					GL.Vertex( end );
				}
				GL.End();

			}
			GL.PopMatrix();

		}

		public static void DrawPolyLine( Vector3[] points, Color color )
		{

			GL.PushMatrix();
			{

				SolidLine.SetPass( 0 );
				SolidLine.color = color;

				GL.Color( color );

				GL.Begin( GL.LINES );
				{

					for( int i = 1; i < points.Length; i++ )
					{
						GL.Vertex( points[ i - 1 ] );
						GL.Vertex( points[ 0 ] );
					}
				}
				GL.End();

			}
			GL.PopMatrix();

		}

		public static void DrawSolidDisc( Vector3 center, Vector3 normal, float radius, Color color )
		{

			Vector3 from = Vector3.Cross( normal, Vector3.up );
			if( (double)from.sqrMagnitude < 1.0 / 1000.0 )
				from = Vector3.Cross( normal, Vector3.right );

			DrawSolidArc( center, normal, from, 360f, radius, color );

		}

		public static void DrawSolidArc( Vector3 center, Vector3 normal, Vector3 from, float angle, float radius, Color color )
		{

			calculateArcPoints( discPoints, center, normal, from, angle, radius );

			GL.PushMatrix();
			{

				SimpleMaterial.SetPass( 0 );
				SimpleMaterial.color = color;

				GL.Color( color );

				GL.Begin( GL.TRIANGLES );
				{
					for( int index = 1; index < discPoints.Length; ++index )
					{
						GL.Color( color );
						GL.Vertex( center );
						GL.Vertex( discPoints[ index - 1 ] );
						GL.Vertex( discPoints[ index ] );
						GL.Vertex( center );
						GL.Vertex( discPoints[ index ] );
						GL.Vertex( discPoints[ index - 1 ] );
					}
				}
				GL.End();
			}
			GL.PopMatrix();

		}

		private static void calculateArcPoints( Vector3[] dest, Vector3 center, Vector3 normal, Vector3 from, float angle, float radius )
		{

			from.Normalize();

			var count = dest.Length;

			Quaternion quaternion = Quaternion.AngleAxis( angle / (float)( count - 1 ), normal );
			Vector3 vector3 = from * radius;

			for( int index = 0; index < count; ++index )
			{
				dest[ index ] = center + vector3;
				vector3 = quaternion * vector3;
			}

		}

	}

}
