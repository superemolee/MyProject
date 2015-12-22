// Copyright (c) 2014 StagPoint Consulting

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace StagPoint.Planning
{

	public class DesignerMenuItem
	{
		
		public string Text;
		public Action Callback;

		public DesignerMenuItem( string text, Action callback )
		{
			this.Text = text;
			this.Callback = callback;
		}

	}

}

#endif