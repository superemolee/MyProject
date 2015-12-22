using UnityEngine;
using System.Collections;

namespace Calculation
{
	public class Calculator
	{

		// position structure 
		// 0.0 0.0 0.0
		// subtract the ambiguis from plan structure
		public static string PositionToString (Vector3 pos)
		{
			return pos.x.ToString () + " " + pos.y.ToString () + " " + pos.z.ToString ();
		}
	
		public static Vector3 StringToPosition (string pos_string)
		{
			float x = float.Parse (pos_string.Substring (0, pos_string.IndexOf (' ')));
			pos_string = pos_string.Substring (pos_string.IndexOf (' ') + 1);
			float y = float.Parse (pos_string.Substring (0, pos_string.IndexOf (' ')));
			float z = float.Parse (pos_string.Substring (pos_string.IndexOf (' ') + 1));
			return new Vector3 (x, y, z);
		}
	}
}
