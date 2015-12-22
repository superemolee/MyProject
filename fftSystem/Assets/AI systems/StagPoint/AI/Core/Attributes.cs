// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagPoint.Core
{

	/// <summary>
	/// When specified on an Action method, indicates that the method should not be interrupted
	/// by generating a new plan.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method )]
	public class NotInterruptableAttribute : System.Attribute
	{
	}

	/// <summary>
	/// Used to inform the serialization system that a type's name has changed, and allows
	/// the system to deserialize a type that was originally serialized under a different name.
	/// </summary>
	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Enum )]
	public class TypeRenameAttribute : System.Attribute
	{
	
		#region Public fields 

		/// <summary>
		/// The name previous name of the type
		/// </summary>
		public string TypeName = string.Empty;

		#endregion 

		#region Constructor 

		/// <summary>
		/// Initializes a new instance of the TypeRenameAttribute class.
		/// </summary>
		public TypeRenameAttribute( string typeName )
			: base()
		{
			this.TypeName = typeName;
		}

		#endregion 

	}

	/// <summary>
	/// Allows the developer to specify a default value for any method parameter. The value
	/// specified will be used in the graph editor to assign default values when editing 
	/// method arguments.
	/// </summary>
	[AttributeUsage( AttributeTargets.Parameter )]
	public class DefaultValueAttribute : System.Attribute
	{

		#region Public fields 

		/// <summary>
		/// The value that will be assigned by default
		/// </summary>
		public object Value;

		#endregion 

		#region Constructor 

		public DefaultValueAttribute( object value )
		{
			this.Value = value;
		}

		#endregion 

	}

	/// <summary>
	/// This is a marker attribute that informs the planner to provide an evaluated script
	/// value for the indicated parameter
	/// </summary>
	public class ScriptParameterAttribute : System.Attribute
	{
	}

	/// <summary>
	/// Specifies that the class on which this attribute is defined will contain static 
	/// methods that can be used for Hierarchical Task Network operators, effects, or 
	/// pre-conditions.
	/// </summary>
	[AttributeUsage( AttributeTargets.Class )]
	public class FunctionLibraryAttribute : System.Attribute
	{

		#region Public fields 

		/// <summary>
		/// The name that will be displayed for the library in the designer interface. This 
		/// name can contain forward slashes (like a file path) that can be used to categorize
		/// the library. For example, "Planning/Commander/Analysis" will be result in a 
		/// hierarchical menu which allows the user to "drill down" into Planning > Commander > Analysis.
		/// </summary>
		public string LibraryName = string.Empty;

		#endregion 

		#region Constructor 

		/// <summary>
		/// Initializes a new instance of the FunctionLibraryAttribute class.
		/// </summary>
		public FunctionLibraryAttribute( string libraryName )
			: base()
		{
			this.LibraryName = libraryName;
		}

		#endregion 

	}

	/// <summary>
	/// When declared on a public field or property, this attribute provides the opportunity
	/// to specify the name of the corresponding Blackboard variable that is data-bound to 
	/// the field.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class BlackboardVariableAttribute : System.Attribute
	{

		#region Public fields 

		/// <summary>
		/// The name of the Blackboard variable that will be databound to the field
		/// </summary>
		public string VariableName = string.Empty;

		#endregion 

		#region Constructors 

		public BlackboardVariableAttribute() 
			: base() { }

		public BlackboardVariableAttribute( string variableName )
			: base()
		{
			this.VariableName = variableName;
		}

		#endregion 

	}

	/// <summary>
	/// Specifies that the field or property on which this attribute is defined should not
	/// be included when databinding an object's fields to a Blackboard.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class DoNotDatabindAttribute : System.Attribute
	{
	}

}
