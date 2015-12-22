// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace StagPoint.Planning
{

	public enum EffectType
	{
		Add,
		Subtract,
		SetValue
	}

	public enum ConditionType
	{
		EqualTo,
		NotEqualTo,
		GreaterThan,
		GreaterThanOrEqual,
		LessThan,
		LessThanOrEqual,
		IsNull,
		IsNotNull,
		IsTrue,
		IsFalse
	}

}
