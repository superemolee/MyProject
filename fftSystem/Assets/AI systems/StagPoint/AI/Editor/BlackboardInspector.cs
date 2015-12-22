/* Copyright 2014-2015 StagPoint */
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using StagPoint.Core;
using StagPoint.Planning;
using StagPoint.Planning.Components;

using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( Blackboard ), true )]
public class BlackboardInspector : Editor
{

	#region Main menu 

	[MenuItem( "Assets/Create/StagPoint/Blackboard", false )]
	[MenuItem( "Tools/StagPoint/HTN Planner/Create Blackboard", false )]
	public static Blackboard CreateNewBlackboard()
	{
		return CreateNewBlackboard( "NewBlackboard" );
	}

	public static Blackboard CreateNewBlackboard( string defaultName )
	{

		var path = EditorUtility.SaveFilePanel( "New Blackboard", "Assets", defaultName, "asset" );
		if( string.IsNullOrEmpty( path ) )
			return null;

		var blackboard = ScriptableObject.CreateInstance<Blackboard>();

		Path.ChangeExtension( path, ".asset" );

		AssetDatabase.CreateAsset( blackboard, makeRelativePath( path ) );
		AssetDatabase.Refresh();
		AssetDatabase.ImportAsset( path, ImportAssetOptions.ForceSynchronousImport );

		Selection.activeObject = blackboard;

		return blackboard;

	}

	/// <summary>
	/// Makes a file path relative to the Unity project's path
	/// </summary>
	private static string makeRelativePath( string path )
	{

		if( string.IsNullOrEmpty( path ) )
		{
			return "";
		}

		return path.Substring( path.IndexOf( "Assets/", StringComparison.OrdinalIgnoreCase ) );

	}

	#endregion 

	#region Private variables

	private string controlPrefix = "";
	private string editVariableName = "";
	private string lastFocusedControl = "";
	private string selectedVariable = "";

	[NonSerialized]
	private bool sortKeys = false; 

	#endregion 

	#region Public fields 

	public bool IsProxied = false;

	#endregion 

	#region Editor overrides

	public override void OnInspectorGUI()
	{

		if( EditorApplication.isCompiling )
		{
			EditorGUILayout.HelpBox( "Compiling Please Wait...", MessageType.Warning );
			return;
		}

		var blackboard = target as Blackboard;
		var evt = Event.current;

		if( string.IsNullOrEmpty( controlPrefix ) )
		{
			controlPrefix = "_" + System.Guid.NewGuid().ToString().Replace( "-", "" );
			editVariableName = lastFocusedControl = "";
		}

		if( evt.type == EventType.ValidateCommand && evt.commandName == "UndoRedoPerformed" )
		{
			EditorGUIUtility.hotControl = EditorGUIUtility.keyboardControl = 0;
		}

		int inspectorControlID = EditorGUIUtility.GetControlID( controlPrefix.GetHashCode(), FocusType.Native );

		if( !IsProxied )
		{

			DesignUtil.Header( "Blackboard Definition" );

			#region Edit general properties

			GUILayout.Space( 5 );

			var name = EditorGUILayout.TextField( "Name", blackboard.Name );
			if( name != blackboard.Name )
			{
				DesignUtil.MarkUndo( blackboard, "Change blackboard name" );
				blackboard.Name = name;
			}

			var parent = EditorGUILayout.ObjectField( "Parent", blackboard.Parent, typeof( Blackboard ), false ) as Blackboard;
			if( parent != blackboard.Parent && parent != blackboard )
			{
				DesignUtil.MarkUndo( blackboard, "Assign Blackboard Parent" );
				blackboard.Parent = parent;
			}

			GUILayout.Space( 3 );

			#endregion

			DesignUtil.DrawHorzLine();

		}

		if( !DesignUtil.BeginGroup( "blackboard", "Blackboard Variables" ) )
		{
			DesignUtil.EndGroup( false );
			GUILayout.Space( 5 );
			return;
		}
		else
		{

			if( blackboard.Count == 0 )
			{
				GUILayout.Label( "No variables defined" );
			}
			else
			{

				var fieldWidth = ( Screen.width - 65 ) * 0.5f;

				GUILayout.BeginHorizontal( GUILayout.Width( Screen.width ) );
				{

					if( GUILayout.Button( "Variable Name", DesignUtil.headerTabLeft, GUILayout.Width( fieldWidth + 2 ) ) )
					{
						sortVariables( blackboard );
					}

					GUILayout.Label( Application.isPlaying ? "Current Value" : "Default Value", DesignUtil.headerTabMid, GUILayout.Width( fieldWidth + 5 ) );
					GUILayout.Label( " ", DesignUtil.headerTabRight, GUILayout.Width( 20 ) );
				}
				GUILayout.EndHorizontal();

				var keyToRemove = string.Empty;
				var showCommitMessage = false;

				var keys = blackboard.GetKeys( false );
				if( sortKeys )
				{
					keys.Sort();
				}

				for( int i = 0; i < keys.Count; i++ )
				{

					var key = keys[ i ];
					var variable = blackboard.GetVariable( key );

					var rowStyle = DesignUtil.GetStyle( "row_style", "label" );
					rowStyle.margin = new RectOffset();
					rowStyle.padding = new RectOffset();
					rowStyle.contentOffset = Vector2.zero;
					rowStyle.fixedWidth = Screen.width - 40;

					GUILayout.BeginHorizontal( rowStyle, GUILayout.Width( Screen.width - 40 ), GUILayout.Height( 15 ) );
					{
						
						#region Edit variable

						var controlName = controlPrefix + "_Variable_" + i;
						GUI.SetNextControlName( controlName );

						// Auto-select the variable for renaming after adding new variable
						if( variable.Name == selectedVariable )
						{
							GUI.FocusControl( controlName );
							selectedVariable = "";
						}

						var focusedControl = GUI.GetNameOfFocusedControl();

						if( focusedControl == controlName )
						{

							if( lastFocusedControl != controlName )
							{
								editVariableName = variable.Name;
								lastFocusedControl = controlName;
							}

							editVariableName = GUILayout.TextField( editVariableName, GUILayout.Width( fieldWidth - 5 ) );
							showCommitMessage = editVariableName != variable.Name;

							if( evt.isKey )
							{
								if( evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter )
								{
									if( editVariableName != variable.Name )
									{
										DesignUtil.MarkUndo( target, "Rename Blackboard Variable" );
										evt.Use();
										blackboard.Rename( variable.Name, editVariableName );
										editVariableName = variable.Name;
									}
									EditorGUIUtility.hotControl = EditorGUIUtility.keyboardControl = 0;
								}
								else if( evt.keyCode == KeyCode.Escape )
								{
									editVariableName = variable.Name;
									EditorGUIUtility.hotControl = EditorGUIUtility.keyboardControl = 0;
									evt.Use();
								}
							}

						}
						else
						{
							GUILayout.TextField( variable.Name, GUILayout.Width( fieldWidth - 5 ) );
						}

						#endregion

						EditorGUI.BeginChangeCheck();
						{
							editVariableData( variable, fieldWidth );
						}
						if( EditorGUI.EndChangeCheck() )
						{
							EditorUtility.SetDirty( blackboard );
						}

						if( GUILayout.Button( DesignUtil.iconToolbarMinus, DesignUtil.elementBackground, GUILayout.Width( 20 ) ) )
						{
							keyToRemove = key;
						}

					}
					GUILayout.EndHorizontal();

					if( showCommitMessage )
					{
						var commitMessage = "You must press ENTER to commit the changes and rename the variable.\nPress ESC or leave the field to cancel";
						EditorGUILayout.LabelField( commitMessage, (GUIStyle)"HelpBox", GUILayout.Width( Screen.width - 45 ) );
						showCommitMessage = false;
					}

					GUILayout.Space( 3 );

				}

				if( Application.isPlaying )
				{
					if( GUILayout.Button( "Refresh Blackboard", GUILayout.Width( Screen.width - 45 ) ) )
					{
						this.Repaint();
					}
				}

				if( !string.IsNullOrEmpty( keyToRemove ) )
				{
					DesignUtil.MarkUndo( blackboard, "Remove Blackboard variable" );
					blackboard.Remove( keyToRemove );
					lastFocusedControl = "";
				}

			}

			DesignUtil.EndGroup( true );

			GUILayout.BeginHorizontal( DesignUtil.groupContainer, GUILayout.Width( Screen.width - 33 ) );
			{
				GUILayout.FlexibleSpace();
				if( GUILayout.Button( DesignUtil.iconToolbarPlus, DesignUtil.footerBackground, GUILayout.Width( 30 ) ) )
				{

					editVariableName = "";
					lastFocusedControl = "";
					selectedVariable = "";

					EditorGUIUtility.hotControl = EditorGUIUtility.keyboardControl = inspectorControlID;

					showAddMenu();

				}
			}
			GUILayout.EndHorizontal();

		}

	}

	#endregion 

	#region Private utility methods 

	private void sortVariables( Blackboard blackboard )
	{

		var keys = blackboard.GetKeys( true );
		var variables = new List<BlackboardVariable>();

		foreach( var key in keys )
		{
			variables.Add( blackboard.GetVariable( key ) );
		}

		blackboard.Clear();

		variables.Sort( ( lhs, rhs ) => lhs.Name.CompareTo( rhs.Name ) );

		foreach( var variable in variables )
		{
			blackboard.Add( variable );
		}

	}

	private void editVariableData( BlackboardVariable variable, float fieldWidth )
	{

		if( variable.DataType == typeof( float ) )
			variable.SetValue<float>( EditorGUILayout.FloatField( variable.GetValue<float>(), GUILayout.Width( fieldWidth ) ) );
		else if( variable.DataType == typeof( int ) )
			variable.SetValue<int>( EditorGUILayout.IntField( variable.GetValue<int>(), GUILayout.Width( fieldWidth ) ) );
		else if( variable.DataType == typeof( bool ) )
			variable.SetValue<bool>( EditorGUILayout.Toggle( variable.GetValue<bool>(), GUILayout.Width( fieldWidth ) ) );
		else if( variable.DataType == typeof( string ) )
			variable.SetValue<string>( EditorGUILayout.TextField( variable.GetValue<string>(), GUILayout.Width( fieldWidth ) ) );
		else if( variable.DataType == typeof( Vector2 ) )
			variable.SetValue<Vector2>( EditorGUILayout.Vector2Field( GUIContent.none, variable.GetValue<Vector2>(), GUILayout.Width( fieldWidth ) ) );
		else if( variable.DataType == typeof( Vector3 ) )
			variable.SetValue<Vector3>( EditorGUILayout.Vector3Field( GUIContent.none, variable.GetValue<Vector3>(), GUILayout.Width( fieldWidth ) ) );
		else if( variable.DataType == typeof( Vector4 ) )
			variable.SetValue<Vector4>( EditorGUILayout.Vector4Field( string.Empty, variable.GetValue<Vector4>(), GUILayout.Width( fieldWidth ) ) );
		else if( variable.DataType == typeof( Color ) )
			variable.SetValue<Color>( EditorGUILayout.ColorField( variable.GetValue<Color>(), GUILayout.Width( fieldWidth ) ) );
		else if( variable.DataType == typeof( Color32 ) )
			variable.SetValue<Color32>( (Color32)EditorGUILayout.ColorField( variable.GetValue<Color32>(), GUILayout.Width( fieldWidth ) ) );
		else if( variable.DataType == typeof( Rect ) )
			variable.SetValue<Rect>( EditorGUILayout.RectField( variable.GetValue<Rect>(), GUILayout.Width( fieldWidth ) ) );
		else if( variable.DataType != null && variable.DataType.IsEnum )
			variable.SetValue<Enum>( editEnum( variable, fieldWidth ) );
		else if( typeof( UnityEngine.Object ).IsAssignableFrom( variable.DataType ) )
			variable.SetValue<UnityEngine.Object>( EditorGUILayout.ObjectField( variable.GetValue<UnityEngine.Object>(), variable.DataType, false, GUILayout.Width( fieldWidth ) ) as UnityEngine.Object );
		else if( typeof( IList ).IsAssignableFrom( variable.DataType ) )
		{
			var list = variable.GetValue<IList>();
			if( list != null )
			{
				EditorGUILayout.LabelField( "Cannot edit", (GUIStyle)"CN StatusWarn", GUILayout.Width( fieldWidth ) );
				//if( GUILayout.Button( "Edit List", "minibutton" ) )
				//{
				//	EditorUtility.DisplayDialog( "Not Implemented", "This functionality is not yet implemented", "OK" );
				//}
			}
			else
			{
				EditorGUILayout.LabelField( "Value is NULL", (GUIStyle)"CN StatusWarn", GUILayout.Width( fieldWidth ) );
			}
		}
		else
		{
			EditorGUILayout.LabelField( "Cannot edit", (GUIStyle)"CN StatusWarn", GUILayout.Width( fieldWidth ) );
		} 

	}

	private Enum editEnum( BlackboardVariable variable, float fieldWidth )
	{

		var content = DesignUtil.iconToolbarPlusMore;

		var storedValue = variable.GetValue<System.Enum>();
		if( storedValue == null )
			storedValue = Activator.CreateInstance( variable.DataType ) as System.Enum;

		var enumValue = EditorGUILayout.EnumPopup( storedValue, GUILayout.Width( fieldWidth - 20 ) );

		if( GUILayout.Button( content, GUIStyle.none, GUILayout.Width( 20 ) ) )
		{
			showEnumMenu( variable );
		}

		return enumValue;

	}

	private void showEnumMenu( BlackboardVariable variable )
	{

		var buildHandler = new Func<System.Type, GenericMenu.MenuFunction>( ( type ) =>
		{
			return new GenericMenu.MenuFunction( () =>
			{
				variable.DataType = type;
				variable.SetValue<Enum>( (Enum)Activator.CreateInstance( type ) );
			} );
		} );

		var menu = new GenericMenu();

		menu.AddItem( new GUIContent( "Status" ), false, buildHandler( typeof( StagPoint.Core.TaskStatus ) ) );
		menu.AddSeparator( "/" );

		// NOTE: We can use typeof( Blackboard ).Assembly to determine the project's assembly only because
		// it is expected that this library will be distributed as source code. This obviously would not 
		// work if it was distributed as a compiled .dll library.
		var projectAssembly = typeof( Blackboard ).Assembly;

		var projectEnumTypes = DesignUtil.definedTypes.Where( x => x.Assembly == projectAssembly && x.IsEnum );
		foreach( var enumType in projectEnumTypes )
		{
			var namespaceName = !string.IsNullOrEmpty( enumType.Namespace ) ? enumType.Namespace : "GLOBAL";
			var text = string.Format( "Project/{0}/{1}", namespaceName, enumType.Name );
			menu.AddItem( new GUIContent( text ), false, buildHandler( enumType ) );
		}

		menu.AddSeparator( "/" );

		var allEnumTypes = DesignUtil.definedTypes.Where( x => x.Assembly != projectAssembly && x.IsEnum );
		foreach( var enumType in allEnumTypes )
		{
			var text = string.Format( "External/{0}/{1}/{2}", extractAssemblyName( enumType.Assembly.FullName ), enumType.Namespace, enumType.Name );
			menu.AddItem( new GUIContent( text ), false, buildHandler( enumType ) );
		}

		menu.ShowAsContext();

	}

	private string extractAssemblyName( string name )
	{
		var index = name.IndexOf( ',' );
		if( index == -1 )
			return name;
		else
			return name.Substring( 0, index );
	}
	
	private void showAddMenu()
	{

		var menu = new GenericMenu();

		menu.AddItem( new GUIContent( "int" ), false, buildAddItemHandler( typeof( int ) ) );
		menu.AddItem( new GUIContent( "float" ), false, buildAddItemHandler( typeof( float ) ) );
		menu.AddItem( new GUIContent( "bool" ), false, buildAddItemHandler( typeof( bool ) ) );
		menu.AddItem( new GUIContent( "string" ), false, buildAddItemHandler( typeof( string ) ) );
		menu.AddItem( new GUIContent( "Enum" ), false, buildAddItemHandler( typeof( StagPoint.Core.TaskStatus ) ) );
		menu.AddItem( new GUIContent( "Color" ), false, buildAddItemHandler( typeof( Color ) ) );
		menu.AddItem( new GUIContent( "Rect" ), false, buildAddItemHandler( typeof( Rect ) ) );
		menu.AddItem( new GUIContent( "Vector2" ), false, buildAddItemHandler( typeof( Vector2 ) ) );
		menu.AddItem( new GUIContent( "Vector3" ), false, buildAddItemHandler( typeof( Vector3 ) ) );
		menu.AddItem( new GUIContent( "GameObject" ), false, buildAddItemHandler( typeof( GameObject ) ) );

		menu.AddSeparator( "/" );

		var projectAssembly = typeof( Blackboard ).Assembly;

		var projectEnumTypes = DesignUtil.definedTypes.Where( x => x.Assembly == projectAssembly && !x.IsEnum && !typeof( Delegate ).IsAssignableFrom( x ) );
		foreach( var enumType in projectEnumTypes )
		{
			var namespaceName = !string.IsNullOrEmpty( enumType.Namespace ) ? enumType.Namespace : "GLOBAL";
			var text = string.Format( "Project/{0}/{1}", namespaceName, enumType.Name );
			menu.AddItem( new GUIContent( text ), false, buildAddItemHandler( enumType ) );
		}

		var allEnumTypes = DesignUtil.definedTypes.Where( x => x.Assembly != projectAssembly && !x.IsEnum && !typeof( Delegate ).IsAssignableFrom( x ) );
		foreach( var enumType in allEnumTypes )
		{
			var text = string.Format( "External/{0}/{1}/{2}", extractAssemblyName( enumType.Assembly.FullName ), enumType.Namespace, enumType.Name );
			menu.AddItem( new GUIContent( text ), false, buildAddItemHandler( enumType ) );
		}

		fillAddFromClassMenu( menu );

		menu.ShowAsContext();

	}

	private void fillAddFromClassMenu( GenericMenu menu )
	{

		if( !IsProxied )
			return;

		var selectedObject = Selection.activeGameObject;
		if( selectedObject == null )
			return;

		var components = selectedObject
			.GetComponents( typeof( MonoBehaviour ) )
			.Where( x => canFillFromClass( x ) )
			.ToList();

		if( components.Count == 0 )
			return;

		menu.AddSeparator( "/" );

		for( int i = 0; i < components.Count; i++ )
		{
			var component = components[ i ];
			var itemText = new GUIContent( "Fill from class/" + component.GetType().Name );
			menu.AddItem( itemText, false, () => { fillBlackboardFromClass( component ); } );
		}

	}

	private void fillBlackboardFromClass( Component component )
	{

		var blackboard = (Blackboard)target;

		Undo.RegisterCompleteObjectUndo( blackboard, "Modify Blackboard" );

		blackboard.FillFromType( component.GetType() );

	}

	private bool canFillFromClass( Component component )
	{

		if( component == null )
			return false;

		var type = component.GetType();
		if( string.IsNullOrEmpty( type.Namespace ) )
			return true;

		var libraryNamespace = typeof( Blackboard ).Namespace;
		libraryNamespace = libraryNamespace.Substring( 0, libraryNamespace.IndexOf( "." ) );

		if( type.Namespace.StartsWith( libraryNamespace ) )
			return false;

		if( type.Namespace.StartsWith( "UnityEngine" ) )
			return false;

		return true;

	}

	private GenericMenu.MenuFunction buildAddItemHandler( System.Type type )
	{

		var blackboard = target as Blackboard;

		var key = "NEW " + friendlyTypeName( type ) + " ";
		for( int i = 1; i < 100; i++ )
		{
			if( !blackboard.Contains( key + i ) )
			{
				key += i;
				break;
			}
		}

		return new GenericMenu.MenuFunction( () =>
		{

			var controlName = controlPrefix + "_Variable_" + blackboard.GetKeys( true ).Count;

			DesignUtil.MarkUndo( blackboard, string.Format( "New {0} Blackboard variable", type.Name ) );
			blackboard.Add( key, type );

			selectedVariable = key;
			editVariableName = key;

			GUI.FocusControl( controlName );

		} );

	}

	private static string friendlyTypeName( System.Type type )
	{

		if( type == typeof( int ) )
			return "int";
		if( type == typeof( float ) )
			return "float";
		if( type == typeof( bool ) )
			return "bool";
		if( type.IsEnum )
			return "Enum";

		return type.Name;

	}

	#endregion 

}
