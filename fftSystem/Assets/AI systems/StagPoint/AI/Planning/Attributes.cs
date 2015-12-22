// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StagPoint.Core;

namespace StagPoint.Planning
{

	[AttributeUsage( AttributeTargets.Method )]
	public class OperatorAttribute : System.Attribute
	{

		public string Label { get; set; }
		public string Description { get; set; }

		public OperatorAttribute()
			: base()
		{
		}

		public OperatorAttribute( string label )
			: base()
		{
			this.Label = label;
		}

		public OperatorAttribute( string label, string description )
			: this( label )
		{
			this.Description = description;
		}

	}

	[AttributeUsage( AttributeTargets.Method )]
	public class ConditionAttribute : System.Attribute
	{

		public string Label { get; private set; }

		public ConditionAttribute( string label )
			: base()
		{
			this.Label = label;
		}

	}

	[AttributeUsage( AttributeTargets.Method )]
	public class EffectAttribute : System.Attribute
	{

		public string Label { get; private set; }

		public EffectAttribute( string label )
			: base()
		{
			this.Label = label;
		}

	}

}
