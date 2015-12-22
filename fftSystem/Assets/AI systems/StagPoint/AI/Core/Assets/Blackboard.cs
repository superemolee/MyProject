// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

namespace StagPoint.Core
{

	using StagPoint.Planning.Components;

	public delegate void BlackboardChangedHandler( Blackboard State, string Key );

	[Serializable]
	public partial class Blackboard : ScriptableObject, ISerializationCallbackReceiver, IDisposable
	{

		#region Public events

		/// <summary>
		/// This event will be raised any time any BlackboardVariable's value is changed
		/// </summary>
		public event BlackboardChangedHandler StateChanged;

		#endregion

		#region Custom serialization variables

		[SerializeField]
		[HideInInspector]
		private byte[] serializedData = null;

		[SerializeField]
		private List<UnityEngine.Object> serializedObjects = new List<UnityEngine.Object>();

		#endregion

		#region Public fields 

		public string Name = "Blackboard";

		#endregion 

		#region Private fields

		protected Blackboard parent = null;

		[SerializeField]
		protected Dictionary<string, BlackboardVariable> data = new Dictionary<string, BlackboardVariable>();

		#endregion

		#region Object pooling

		private static Stack<Blackboard> pool = new Stack<Blackboard>();

		public static Blackboard Obtain()
		{
			lock( pool )
			{
				return ( pool.Count > 0 ) ? pool.Pop() : ScriptableObject.CreateInstance<Blackboard>();
			}
		}

		public void Release()
		{

			lock( pool )
			{

				if( !pool.Contains( this ) )
				{

					pool.Push( this );

					foreach( var variable in data.Values )
					{
						// Note: Not currently allowing descendent classes to use object pooling
						if( variable.GetType() == typeof( BlackboardVariable ) )
						{
							variable.Release();
						}
					}

				}

				this.parent = null;
				this.data.Clear();

			}

		}

		#endregion

		#region Constructor

		/// <summary>
		/// Do not use. Use the Blackboard.Obtain() method instead. 
		/// </summary>
		public Blackboard()
		{
		}

		#endregion

		#region Stack functions

		/// <summary>
		/// Returns a new instance of the Blackboard class that "overrides" this instance. This is typically used
		/// only during planning, when the planner needs to keep track of the derived world state according to the
		/// planning that is in progress.
		/// </summary>
		public virtual Blackboard Push()
		{

			var instance = Blackboard.Obtain();

			instance.parent = this;

			return instance;

		}

		/// <summary>
		/// Releases this instance of the Blackboard class and returns this instance's Parent. Used during planning to 
		/// keep track of the derived world state at any given point in the plan.
		/// </summary>
		/// <returns></returns>
		public virtual Blackboard Pop()
		{

			var parent = this.parent;

			this.Release();

			return parent;

		}

		#endregion

		#region Public properties

		/// <summary>
		/// Gets or sets the parent Blackboard
		/// </summary>
		public Blackboard Parent
		{
			get { return this.parent; }
			set { this.parent = value; }
		}

		/// <summary>
		/// Gets the number of variables defined in this Blackboard instance
		/// </summary>
		public int Count
		{
			get
			{

				if( parent == null )
					return data.Count;

				var keys = (IEnumerable<string>)data.Keys;

				var loop = this.parent;
				while( loop != null )
				{
					keys = keys.Concat( parent.data.Keys );
					loop = loop.parent;
				}

				return keys.Distinct().Count();


			}
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Clears all "local" data from the blackboard. Does not clear data belonging to any parent Blackboard.
		/// </summary>
		public void Clear()
		{
			this.data.Clear();
		}

		/// <summary>
		/// Returns a clone of this Blackboard.
		/// </summary>
		public Blackboard Clone()
		{

			var clone = Blackboard.Obtain();

			foreach( var entry in data )
			{
				clone.data[ entry.Key ] = entry.Value.Clone();
			}

			return clone;

		}

		/// <summary>
		/// Gets a collection containing the names of all variables in this Blackboard instance
		/// </summary>
		public List<string> GetKeys()
		{
			return GetKeys( false );
		}

		/// <summary>
		/// Gets a collection containing the names of all variables in this Blackboard instance
		/// </summary>
		/// <param name="localOnly">If set to TRUE, will only return keys that are stored "locally" in this 
		/// Blackboard instance. If set to FALSE, will also return keys in all parent Blackboard instances.</param>
		public List<string> GetKeys( bool localOnly )
		{

			if( parent == null )
				return data.Keys.ToList();

			var keys = (IEnumerable<string>)data.Keys;

			if( localOnly )
				return keys.ToList();

			var loop = this.parent;
			while( loop != null )
			{
				keys = keys.Concat( parent.data.Keys );
				loop = loop.parent;
			}

			return keys.Distinct().ToList();

		}

		/// <summary>
		/// Creates a new BlackboardVariable instance with the specified name and type, adds it to the 
		/// list of variables, and returns a reference to the instance.
		/// </summary>
		/// <param name="key">The name of the variable</param>
		/// <param name="type">The data type that the variable will contain</param>
		public BlackboardVariable Add( string key, System.Type type )
		{

			var defaultValue = type.IsValueType ? Activator.CreateInstance( type ) : null;
			if( type == typeof( string ) )
				defaultValue = string.Empty;

			var variable = new BlackboardVariable( key, type, defaultValue );

			this.data[ key ] = variable;

			return variable;

		}

		/// <summary>
		/// Adds a BlackboardVariable instance to the collection
		/// </summary>
		/// <param name="variable"></param>
		public void Add( BlackboardVariable variable )
		{
			this.data[ variable.Name ] = variable;
		}

		/// <summary>
		/// Removes the named variable from the list
		/// </summary>
		/// <param name="key">The name of the BlackboardVariable to remove</param>
		/// <returns>Returns TRUE if the named variable was found in the list and removed, FALSE otherwise.</returns>
		public bool Remove( string key )
		{
			return data.Remove( key );
		}

		/// <summary>
		/// Data binds all of the given object instance's public fields and properties to variables in this Blackboard.
		/// </summary>
		/// <param name="instance">The object instance whose public fields and properties should be data bound to this Blackboard</param>
		public void DataBind( object instance )
		{
			DataBind( instance, BlackboardBindingMode.AllPublicFields );
		}

		/// <summary>
		/// Data binds all of the given object instance's public fields and properties to variables in this Blackboard.
		/// </summary>
		/// <param name="instance">The object instance whose public fields and properties should be data bound to this Blackboard</param>
		/// <param name="includeAll">If set to TRUE (default), all public fields and properties will be data bound. If set to FALSE,
		/// only those fields and properties which have a DataBoundVariable attribute defined will be data bound.</param>
		public void DataBind( object instance, BlackboardBindingMode mode )
		{

			if( instance == null )
				throw new ArgumentNullException( "instance" );

			var instanceType = instance.GetType();

			var members = instanceType.GetMembers( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
			for( int i = 0; i < members.Length; i++ )
			{

				var member = members[ i ];
				if( member.DeclaringType != instanceType )
					continue;

				if( member is FieldInfo || member is PropertyInfo )
				{

					if( member.GetCustomAttributes( typeof( DoNotDatabindAttribute ), true ).Length == 0 )
					{

						var dataBindAttribute = (BlackboardVariableAttribute)member.GetCustomAttributes( typeof( BlackboardVariableAttribute ), true ).FirstOrDefault();
						if( dataBindAttribute == null && ( mode == BlackboardBindingMode.AttributeControlled ) )
							continue;

						var hasCustomVariableName = 
							( dataBindAttribute != null ) && 
							!string.IsNullOrEmpty( dataBindAttribute.VariableName );

						var variableName = hasCustomVariableName 
							? dataBindAttribute.VariableName 
							: member.Name;

						DataBind( variableName, instance, member.Name );

					}

				}

			}

		}

		/// <summary>
		/// Binds the named blackboard variable to use the provided function delegates when setting or
		/// getting the variable's value.
		/// </summary>
		/// <param name="key">The name of the blackboard variable</param>
		/// <param name="getter">The function delegate that will be invoked to return the variable's value</param>
		/// <param name="setter">The function delegate that will be invoked when setting the variable's value</param>
		public BoundBlackboardVariable DataBind( string key, Func<object> getter, Action<object> setter )
		{

			if( string.IsNullOrEmpty( key ) )
				throw new ArgumentNullException( "key" );

			if( getter == null )
				throw new ArgumentNullException( "getter" );

			var variable = new BoundBlackboardVariable( key, getter, setter );

			this.data[ key ] = variable;

			return variable;

		}

		/// <summary>
		/// Binds the named blackboard variable to the named field on the target object instance.
		/// </summary>
		/// <param name="key">The name of the blackboard variable to be bound</param>
		/// <param name="target">The target object instance whose field or property will be data-bound</param>
		/// <param name="fieldName">The name of the field or property to be data-bound. Note that on some platforms such 
		/// as Windows Phone 8 and Windows App Store, this field or property must be declared as public.</param>
		public BoundBlackboardVariable DataBind( string key, object target, string fieldName )
		{

			if( string.IsNullOrEmpty( key ) )
				throw new ArgumentNullException( "key" );

			if( target == null )
				throw new ArgumentNullException( "target" );

			var variable = new BoundBlackboardVariable( key, target, fieldName );

			this.data[ key ] = variable;

			return variable;

		}

		/// <summary>
		/// Returns TRUE if the named variable exists in the Blackboard, and FALSE otherwise
		/// </summary>
		/// <param name="key">The name of the BlackboardVariable instance</param>
		/// <returns></returns>
		public bool Contains( string key )
		{

			Blackboard store = this;

			while( store != null )
			{

				if( store.data.ContainsKey( key ) )
				{
					return true;
				}

				store = store.parent;

			}

			return false;

		}

		/// <summary>
		/// Returns the data type of the named BlackboardVariable instance
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public System.Type GetVariableType( string key )
		{

			var variable = GetVariable( key );
			if( variable == null )
				throw new KeyNotFoundException( string.Format( "Variable {0} does not exist", key ) );

			return variable.DataType;

		}

		/// <summary>
		/// Returns the named BlackboardVariable instance. 
		/// </summary>
		/// <param name="key">The name of the BlackboardVariable instance to retrieve</param>
		public BlackboardVariable GetVariable( string key )
		{

			Blackboard store = this;

			while( store != null )
			{

				BlackboardVariable variable = null;
				if( store.data.TryGetValue( key, out variable ) )
				{
					return variable;
				}

				store = store.parent;

			}

			throw new KeyNotFoundException( string.Format( "No variable named {0} could be found", key ) );

		}

		/// <summary>
		/// Returns the value of the named variable
		/// </summary>
		/// <typeparam name="T">The data type of the variable</typeparam>
		/// <param name="key">The name of the variable whose value is to be returned</param>
		public T GetValue<T>( string key )
		{

			Blackboard store = this;

			while( store != null )
			{

				BlackboardVariable variable = null;
				if( store.data.TryGetValue( key, out variable ) )
				{
					return variable.GetValue<T>();
				}

				store = store.parent;

			}

			throw new KeyNotFoundException( string.Format( "No variable named {0} could be found", key ) );

		}

		/// <summary>
		/// Sets the value of the named variable
		/// </summary>
		/// <typeparam name="T">The data type of the variable</typeparam>
		/// <param name="key">The name of the variable whose value is to be assigned</param>
		/// <param name="value">The value to assign to the variable</param>
		public void SetValue<T>( string key, T value )
		{

			BlackboardVariable variable = null;

			var raiseEvent = false;

			if( data.TryGetValue( key, out variable ) )
			{
				raiseEvent = variable.SetValue<T>( value );
			}
			else
			{

				raiseEvent = true;

				BlackboardVariable newVariable = null;

				#region Ensure that data type does not change in stacked Blackboards

				Blackboard store = this;
				while( store != null )
				{

					BlackboardVariable previous = null;
					if( store.data.TryGetValue( key, out previous ) )
					{
						newVariable = previous.Clone();
						break;
					}

					store = store.parent;

				}

				if( newVariable == null )
				{
					newVariable = new BlackboardVariable( key, typeof( T ), value );
				}

				#endregion

				newVariable.SetValue<T>( value );

				data[ key ] = newVariable;

			}

			if( raiseEvent )
			{
				raiseChangedEvent( key );
			}

		}

		#endregion

		#region Private utility methods

		protected void raiseChangedEvent( string key )
		{
			if( StateChanged != null )
			{
				StateChanged( this, key );
			}
		}

		#endregion

		#region ISerializationCallbackReceiver Members

		public void OnAfterDeserialize()
		{

			if( serializedData == null || serializedData.Length == 0 )
				return;

			using( var store = new SerializationHelper( serializedData, serializedObjects ) )
			{
				
				var temp = ((Dictionary<string, BlackboardVariable>)store.Read()).Values.ToList();
				
				// Always make sure that the keys match the variable names
				data.Clear();
				for( int i = 0; i < temp.Count; i++ )
				{
					
					var variable = temp[ i ];
					
					if( string.IsNullOrEmpty( variable.Name ) || variable.DataType == typeof( Blackboard ) )
					{
						data.Remove( variable.Name );
						continue;
					}

					data[ variable.Name ] = variable;

				}

				serializedObjects.Clear();
				serializedData = null;

			}

		}

		public void OnBeforeSerialize()
		{

			serializedObjects.Clear();

			using( var store = new SerializationHelper( serializedObjects ) )
			{
				store.Write( this.data );
				serializedData = store.GetBuffer();
			}

		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			this.Release();
		}

		#endregion

	}

#if UNITY_EDITOR

	public partial class Blackboard : ScriptableObject, ISerializationCallbackReceiver
	{

		public void FillFromType( Type type )
		{

			var members = type.GetMembers( BindingFlags.Public | BindingFlags.Instance );
			for( int i = 0; i < members.Length; i++ )
			{

				var member = members[ i ];
				if( member.DeclaringType != type )
					continue;

				if( member is FieldInfo || member is PropertyInfo )
				{

					if( member.GetCustomAttributes( typeof( DoNotDatabindAttribute ), true ).Length == 0 )
					{

						var dataBindAttribute = (BlackboardVariableAttribute)member.GetCustomAttributes( typeof( BlackboardVariableAttribute ), true ).FirstOrDefault();

						var variableName = ( dataBindAttribute != null && !string.IsNullOrEmpty( dataBindAttribute.VariableName ) ) ? dataBindAttribute.VariableName : member.Name;
						var variableType = ( member is FieldInfo ) ? ( (FieldInfo)member ).FieldType : ( (PropertyInfo)member ).PropertyType;

						if( typeof( Blackboard ).IsAssignableFrom( variableType ) )
							continue;

						if( typeof( TaskNetworkPlanner ).IsAssignableFrom( variableType ) )
							continue;

						this.Add( variableName, variableType );

					}

				}

			}

		}

		/// <summary>
		/// Renames the BlackboardVariable instance
		/// </summary>
		/// <param name="oldName">The name of the variable to be renamed</param>
		/// <param name="newName">The desired new name of the variable</param>
		public void Rename( string oldName, string newName )
		{
			
			if( Contains( newName ) )
			{
				Debug.LogError( string.Format( "Variable '{0}' already exists in the collection. Rename aborted.", newName ) );
				return;
			}

			var variable = GetVariable( oldName );
			variable.Name = newName;

			data.Remove( oldName );
			data[ newName ] = variable;

		}

	}

#endif

}
