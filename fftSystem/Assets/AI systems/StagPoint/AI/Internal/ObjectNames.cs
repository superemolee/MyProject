#if !UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Runtime replacement for built-in UnityEditor.ObjectNames class
/// </summary>
internal class ObjectNames
{

	internal static string NicifyVariableName( string name )
	{
		return name;
	}

}

#endif