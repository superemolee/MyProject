// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

namespace StagPoint.Core
{

	public delegate void BlackboardVariableChangedHandler( BlackboardVariable variable );

	[Serializable]
	public class BlackboardVariable
	{

		#region Object pooling 

		private static List<BlackboardVariable> pool = new List<BlackboardVariable>();

		public static BlackboardVariable Obtain( string name, System.Type dataType, object value )
		{

			BlackboardVariable instance = null;

			lock( pool )
			{

				if( pool.Count > 0 )
				{

					var lastIndex = pool.Count - 1;

					instance = pool[ lastIndex ];
					pool.RemoveAt( lastIndex );

				}
				else
				{
					instance = new BlackboardVariable();
				}

			}

			instance.Name = name;
			instance.DataType = dataType;
			instance.setValueInternal( value );

			return instance;

		}

		public void Release()
		{
			lock( pool )
			{

				this.Name = string.Empty;
				this.DataType = null;
				this.setValueInternal( null );

				pool.Add( this );

			}
		}

		#endregion 

		#region Public events

		/// <summary>
		/// This event is raised any time the value has been changed
		/// </summary>
		public event BlackboardVariableChangedHandler ValueChanged;

		#endregion

		#region Public fields

		/// <summary>
		/// The name by which this variable can be accessed in the Blackboard
		/// </summary>
		public string Name;

		/// <summary>
		/// The System.Type representing the data type of this variable
		/// </summary>
		public Type DataType;

		/// <summary>
		/// The raw System.Object value of this variable. For most purposes, you 
		/// should use the type-safe GetValue() and SetValue() methods instead 
		/// of accessing this field directly.
		/// </summary>
		[SerializeField]
		public object RawValue; 

		#endregion

		#region Constructor 

		/// <summary>
		/// Initializes a new instance of the BlackboardVariable class
		/// </summary>
		public BlackboardVariable()
		{
		}

		/// <summary>
		/// Initializes a new instance of this class
		/// </summary>
		/// <param name="name">The name of the blackboard variable</param>
		/// <param name="dataType">The System.Type of the data that will be stored in this variable</param>
		/// <param name="defaultValue">The initial value of this variable</param>
		public BlackboardVariable( string name, System.Type dataType, object defaultValue )
		{
			this.Name = name;
			this.DataType = dataType;
			this.setValueInternal( defaultValue );
		}

		#endregion 

		#region Public methods

		/// <summary>
		/// Returns a clone of this BlackboardVariableBase instance
		/// </summary>
		public BlackboardVariable Clone()
		{
			return Obtain( this.Name, this.DataType, this.getValueInternal() );
		}

		/// <summary>
		/// Returns the variable's value
		/// </summary>
		/// <typeparam name="T">The data type to convert the value to</typeparam>
		public virtual T GetValue<T>()
		{

			var rawValue = this.getValueInternal();

			var specifiedType = typeof( T );

			if( !specifiedType.IsAssignableFrom( this.DataType ) )
			{
				return (T)Convert.ChangeType( rawValue, specifiedType );
			}

			if( specifiedType.IsValueType && rawValue == null )
			{
				return (T)this.setValueInternal( default( T ) );
			}
			else if( specifiedType.IsEnum && rawValue == null )
			{
				return (T)Activator.CreateInstance( this.DataType );
			}

			return (T)rawValue;

		}

		/// <summary>
		/// Sets the variable's value
		/// </summary>
		/// <typeparam name="T">The data type of the value to be set</typeparam>
		/// <param name="value">The value to assign to the variable</param>
		/// <returns>Returns TRUE if the value specified is different than the previous value, FALSE otherwise</returns>
		public virtual bool SetValue<T>( T value )
		{

			var valueType = typeof( T );

			try
			{

				object convertedValue = value;

				if( !object.Equals( this.getValueInternal(), convertedValue ) )
				{
					this.setValueInternal( convertedValue );
					raiseChangedEvent();
					return true;
				}

				return false;

			}
			catch( Exception err )
			{
				Debug.LogError( string.Format( "Error setting value of blackboard variable ${0} to {1} value - {2}", this.Name, valueType.Name, err ) );
				return false;
			}

		}

		#endregion

		#region Protected utility methods 

		protected virtual object getValueInternal()
		{
			return this.RawValue;
		}

		protected virtual object setValueInternal( object value )
		{
			this.RawValue = value;
			return value;
		}

		protected void raiseChangedEvent()
		{
			if( ValueChanged != null )
			{
				ValueChanged( this );
			}
		}

		#endregion

		#region Comparison functions 

		internal bool EqualTo( object value )
		{

			var rawValue = this.getValueInternal();

			if( DataType == typeof( float ) )
			{
				return Mathf.Approximately( (float)rawValue, Convert.ToSingle( value ) );
			}
			else if( DataType == typeof( int ) || DataType.IsEnum )
			{
				return (int)rawValue == Convert.ToInt32( value );
			}

			return object.Equals( rawValue, value );

		}

		internal bool NotEqualTo( object value )
		{
			return !EqualTo( value );
		}

		internal bool GreaterThan( object value )
		{

			var rawValue = this.getValueInternal();

			if( DataType == typeof( float ) )
			{
				return (float)rawValue > Convert.ToSingle( value );
			}
			else if( DataType == typeof( int ) )
			{
				return (int)rawValue > Convert.ToInt32( value );
			}

			throw new InvalidOperationException( string.Format( "Cannot compare {0} and {1} data types", DataType, value == null ? typeof( object ) : value.GetType() ) );

		}

		internal bool GreaterThanOrEqual( object value )
		{

			var rawValue = this.getValueInternal();

			if( DataType == typeof( float ) )
			{
				return (float)rawValue >= Convert.ToSingle( value );
			}
			else if( DataType == typeof( int ) )
			{
				return (int)rawValue >= Convert.ToInt32( value );
			}

			throw new InvalidOperationException( string.Format( "Cannot compare {0} and {1} data types", DataType, value == null ? typeof( object ) : value.GetType() ) );

		}

		internal bool LessThan( object value )
		{

			var rawValue = this.getValueInternal();

			if( DataType == typeof( float ) )
			{
				return (float)rawValue < Convert.ToSingle( value );
			}
			else if( DataType == typeof( int ) )
			{
				return (int)rawValue < Convert.ToInt32( value );
			}

			throw new InvalidOperationException( string.Format( "Cannot compare {0} and {1} data types", DataType, value == null ? typeof( object ) : value.GetType() ) );

		}

		internal bool LessThanOrEqual( object value )
		{

			var rawValue = this.getValueInternal();

			if( DataType == typeof( float ) )
			{
				return (float)rawValue <= Convert.ToSingle( value );
			}
			else if( DataType == typeof( int ) )
			{
				return (int)rawValue <= Convert.ToInt32( value );
			}

			throw new InvalidOperationException( string.Format( "Cannot compare {0} and {1} data types", DataType, value == null ? typeof( object ) : value.GetType() ) );

		}

		internal bool IsNull()
		{

			var currentValue = this.getValueInternal();

			if( currentValue is Component )
			{
				// Special hack needed due to Unity's magic behind-the-scenes handling of destroyed GameObject instances
				var component = (Component)currentValue;
				return component == null || component.gameObject == null;
			}
			else if( currentValue is GameObject )
			{
				return ( (GameObject)currentValue ) == null;
			}

			return currentValue == null;

		}

		internal bool IsNotNull()
		{
			return !IsNull();
		}

		internal bool IsTrue()
		{
			return (bool)this.getValueInternal();
		}

		internal bool IsFalse()
		{
			return !(bool)this.getValueInternal();
		}
		
		#endregion 

		#region Called during plan generation 

		internal virtual void Add( object value )
		{

			var rawValue = this.getValueInternal();

			if( DataType == typeof( float ) )
			{
				SetValue( (float)rawValue + Convert.ToSingle( value ) );
			}
			else if( DataType == typeof( int ) )
			{
				SetValue( (int)rawValue + Convert.ToInt32( value ) );
			}
			else
			{
				var valueType = ( value == null ) ? typeof( object ) : value.GetType();
				throw new InvalidOperationException( string.Format( "Cannot add {0} and {1} data types", DataType, valueType ) );
			}

		}

		internal virtual void Subtract( object value )
		{

			var rawValue = this.getValueInternal();

			if( DataType == typeof( float ) )
			{
				SetValue( (float)rawValue - Convert.ToSingle( value ) );
			}
			else if( DataType == typeof( int ) )
			{
				SetValue( (int)rawValue - Convert.ToInt32( value ) );
			}
			else
			{
				var valueType = ( value == null ) ? typeof( object ) : value.GetType();
				throw new InvalidOperationException( string.Format( "Cannot subtract {0} and {1} data types", DataType, valueType ) );
			}

		}

		internal virtual void Assign( object value )
		{

			var rawValue = this.getValueInternal();

			var valueType = ( value == null ) ? typeof( object ) : value.GetType();

			object convertedValue = value;

			if( !this.DataType.IsAssignableFrom( valueType ) )
			{
				convertedValue = Convert.ChangeType( value, this.DataType );
			}

			if( !object.Equals( rawValue, value ) )
			{
				this.setValueInternal( convertedValue );
				raiseChangedEvent();
			}

		}

		#endregion 

	}

	public class BoundBlackboardVariable : BlackboardVariable
	{

		#region Private runtime variables 

		private object target;
		private Action<object> setter;
		private Func<object> getter;

		#endregion 

		#region Constructor 

		/// <summary>
		/// Initializes a new instance of the BoundBlackboardVariable class to use
		/// custom delegates when getting or setting the blackboard variable's data.
		/// </summary>
		/// <param name="name">The name of the blackboard variable</param>
		/// <param name="getter">The function delegate that will be invoked to return the variable's value</param>
		/// <param name="setter">The function delegate that will be invoked when setting the variable's value</param>
		public BoundBlackboardVariable( string name, Func<object> getter, Action<object> setter )
		{

			this.Name = name;

			this.getter = getter;
			this.setter = setter ?? ( ( value ) => { } );

			this.RawValue = getter();
			this.DataType = this.RawValue != null ? this.RawValue.GetType() : typeof( object );

		}

		/// <summary>
		/// Initializes a new instance of the BoundBlackboardVariable class to use
		/// reflection when getting or setting the blackboard variable's data.
		/// </summary>
		/// <param name="name">The name of the blackboard variable</param>
		/// <param name="target">The target object whose field will be bound to the blackboard variable</param>
		/// <param name="fieldName">The name of the field on the target that will be bound to the blackboard variable.</param>
		public BoundBlackboardVariable( string name, object target, string fieldName )
		{

			this.Name = name;
			this.target = target;

			var targetType = target.GetType();

			var member = getMember( targetType, fieldName );
			if( member == null )
				throw new FieldAccessException( string.Format( "Field or property {0} is not accessible on type {1}", fieldName, targetType.Name ) );

			if( member is FieldInfo )
				bindField( (FieldInfo)member );
			else
				bindProperty( (PropertyInfo)member );

			base.setValueInternal( getter() );

		}

		#endregion

		#region Base class overrides 

		protected override object getValueInternal()
		{
			return this.RawValue = getter();
		}

		protected override object setValueInternal( object value )
		{
			setter( this.RawValue = value );
			return value;
		}

		#endregion 

		#region Private utility methods

		private static MemberInfo getMember( Type type, string propertyName )
		{
#if ( !UNITY_EDITOR && UNITY_METRO )
			var typeInfo = type.GetTypeInfo();
			return typeInfo.DeclaredMembers.FirstOrDefault( x => ( (x is FieldInfo) || (x is PropertyInfo) ) && x.Name == propertyName );
#else
			return type.GetMember( propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic ).FirstOrDefault();
#endif
		}

		private static MethodInfo getGetMethod( PropertyInfo property )
		{
#if ( !UNITY_EDITOR && UNITY_METRO )
			return property.GetMethod;
#else
			return property.GetGetMethod();
#endif
		}

		private static MethodInfo getSetMethod( PropertyInfo property )
		{
#if ( !UNITY_EDITOR && UNITY_METRO )
			return property.SetMethod;
#else
			return property.GetSetMethod();
#endif
		}

		private void bindField( FieldInfo field )
		{

			if( field.IsLiteral )
			{
				setter = ( value ) => { };
			}
			else
			{
				this.setter = ( value ) =>
				{
					field.SetValue( this.target, value );
				};
			}

			this.getter = () =>
			{
				return field.GetValue( this.target );
			};

			this.DataType = field.FieldType;

		}

		private void bindProperty( PropertyInfo property )
		{

			var setMethod = getSetMethod( property );
			if( setMethod == null )
			{
				this.setter = ( value ) => { };
			}
			else
			{

				var paramArray = new object[ 1 ];

				this.setter = ( value ) =>
				{
					paramArray[ 0 ] = value;
					setMethod.Invoke( this.target, paramArray );
				};

			}

			var getMethod = getGetMethod( property );
			if( getMethod == null )
				throw new FieldAccessException( string.Format( "Cannot read from property {0}.{1}", property.DeclaringType.Name, property.Name ) );

			this.getter = () =>
			{
				return getMethod.Invoke( this.target, null );
			};

			this.DataType = property.PropertyType;

		}

		#endregion 

	}

}
