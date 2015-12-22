// Copyright (c) 2014 StagPoint Consulting

#if UNITY_EDITOR
using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace StagPoint.Planning
{

	public static class DesignUtil
	{

		#region Static variables 

		[NonSerialized]
		private static Queue<Action> deferredFunctions = new Queue<Action>();

		[NonSerialized]
		private static Stack<string> nestedLayoutDebug = new Stack<string>();

		[NonSerialized]
		private static Dictionary<string, GUIStyle> styles = new Dictionary<string, GUIStyle>();

		public static GUIContent iconToolbarPlus;
		public static GUIContent iconToolbarPlusMore;
		public static GUIContent iconToolbarMinus;
		public static GUIStyle dragHandle;
		public static GUIStyle headerBackground;
		public static GUIStyle footerBackground;
		public static GUIStyle groupBackground;
		public static GUIStyle groupContainer;
		public static GUIStyle preButton;
		public static GUIStyle elementBackground;
		public static GUIStyle headerTabLeft;
		public static GUIStyle headerTabMid;
		public static GUIStyle headerTabRight;

		public static List<System.Type> definedTypes = new List<Type>();

		#endregion 

		#region Static constructor 

		static DesignUtil()
		{

			#region Cache all available types 

			foreach( Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies() )
			{
				try{
				foreach( System.Type type in assembly.GetTypes() )
				{

					var isUsable =
						type.IsPublic &&
						!type.IsGenericParameter &&
						!type.IsGenericTypeDefinition &&
						!type.IsImport && 
						!type.IsNested && 
						!type.IsGenericType &&
						!type.IsSpecialName &&
						!type.IsAbstract &&
						!type.IsInterface;

					if( isUsable )
					{
						definedTypes.Add( type );
					}

				}
				}catch{}
			}

			definedTypes = new List<Type>( definedTypes.OrderBy( x => x.Assembly.FullName ).ThenBy( x => x.FullName ) );

			#endregion 

			#region Initialize styles 

			//GUI.skin = EditorGUIUtility.GetBuiltinSkin( EditorSkin.Scene );

			iconToolbarPlus = EditorGUIUtility.IconContent( "Toolbar Plus", "Add to list" );
			iconToolbarPlusMore = EditorGUIUtility.IconContent( "Toolbar Plus More", "Choose to add to list" );
			iconToolbarMinus = EditorGUIUtility.IconContent( "Toolbar Minus", "Remove selection from list" );

			headerTabLeft = GetStyle( "headerTabLeft", (GUIStyle)"TL tab left" );
			headerTabLeft.fontStyle = FontStyle.Bold;

			headerTabMid = GetStyle( "headerTabMid", (GUIStyle)"TL tab mid" );
			headerTabMid.fontStyle = FontStyle.Bold;

			headerTabRight = GetStyle( "headerTabRight", (GUIStyle)"TL tab right" );
			headerTabRight.fontStyle = FontStyle.Bold;

			dragHandle = GetStyle( "drag_handle", (GUIStyle)"RL DragHandle" );
			headerBackground = GetStyle( "header_background", (GUIStyle)"RL Header" );
			footerBackground = GetStyle( "footer_background", (GUIStyle)"RL Footer" );
			groupBackground = GetStyle( "group_background", (GUIStyle)"RL Background" );
			preButton = GetStyle( "pre_button", (GUIStyle)"RL FooterButton" );
			elementBackground = GetStyle( "group_item_background", (GUIStyle)"RL Element" );

			footerBackground = GetStyle( "footer_background", (GUIStyle)"RL Footer" );
			footerBackground.fixedHeight = 20;

			groupContainer = GetStyle( "group_container", new GUIStyle() );
			groupContainer.margin = new RectOffset( 16, 16, 0, 0 );
			//groupContainer.padding = new RectOffset( 5, 5, 0, 0 );

			var typeGUIClip = typeof( GUI ).Assembly.GetType( "UnityEngine.GUIClip" );
			if( typeGUIClip != null )
			{
				var propVisibleRect = typeGUIClip.GetProperty( "visibleRect", BindingFlags.Static | BindingFlags.Public );
				if( propVisibleRect != null )
				{
					GetVisibleRect = (Func<Rect>)Delegate.CreateDelegate( typeof( Func<Rect> ), propVisibleRect.GetGetMethod() );
				}
			}

			#endregion 

			#region Initialize deferred execution 

			var chain = EditorApplication.update;
			EditorApplication.update = () =>
			{
				
				while( deferredFunctions.Count > 0 )
				{
					deferredFunctions.Dequeue()();	
				}

				if( chain != null )
				{
					chain();
				}

			};

			#endregion 

		}

		#endregion 

		#region Public delegates

		/// <summary>
		/// Gets visible rectangle within GUI.
		/// </summary>
		/// <remarks>
		/// <para>VisibleRect = TopmostRect + scrollViewOffsets</para>
		/// </remarks>
		public static Func<Rect> GetVisibleRect;

		#endregion 

		#region Public methods

		public static Rect GetClipRect()
		{

			var type = System.Type.GetType( "UnityEngine.GUIClip,UnityEngine" );
			var property = type.GetProperty( "visibleRect" );

			return (Rect)property.GetValue( null, null );

		}

		public static void Defer( Action callback )
		{
			deferredFunctions.Enqueue( callback );
		}

		public static void MarkUndo( UnityEngine.Object target, string label )
		{
			Undo.RegisterCompleteObjectUndo( target, label );
			EditorUtility.SetDirty( target );
		}

		public static void DoGroup( string key, string headerText, Action callback, Action footerCallback )
		{

			if( BeginGroup( key, headerText ) )
			{
				
				callback();
				
				EndGroup( true );
				
				if( footerCallback != null )
				{
					footerCallback();
				}

			}
			else
			{
				EndGroup( false );
			}

		}

		public static bool BeginGroup( string key, string headerText )
		{

			BeginVertical( "group begin", groupContainer );

			if( !GroupFoldout( key, headerText ) )
			{
				return false;
			}

			var headerRect = GUILayoutUtility.GetLastRect();

			BeginVertical( "group background", groupBackground, GUILayout.MinHeight( 10 ) );

			BeginHorizontal( "group body", GUILayout.Width( headerRect.width ) );
			GUILayout.Space( groupBackground.border.left * 0.5f );

			BeginVertical( "group content" );

			return true;

		}

		public static void EndGroup( bool expanded )
		{

			if( expanded )
			{

				GUILayout.Space( 5 );
				EndVertical( "group content" );

				GUILayout.Space( groupBackground.border.right * 0.5f );
				EndHorizontal( "group body" );

				EndVertical( "group background" );

			}

			EndVertical( "group begin" );

		}

		private static bool GroupFoldout( string key, string text )
		{

			var headerStyle = DesignUtil.GetStyle( "group_header", (GUIStyle)"RL Header" );
			headerStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color( 1f, 1f, 1f, 0.7f ) : new Color( 0f, 0f, 0f, 0.7f );
			headerStyle.fontStyle = FontStyle.Bold;
			headerStyle.fontSize = 12;
			headerStyle.padding.left = 8;
			headerStyle.fixedWidth = 0f;
			headerStyle.fixedHeight = 20f;
			headerStyle.alignment = TextAnchor.MiddleLeft;
			headerStyle.margin = new RectOffset();

			var expanded = EditorPrefs.GetBool( key, true );
			if( expanded )
				text = "\u25BC" + (char)0x200a + text;
			else
				text = "\u25BA" + (char)0x200a + text; 

			GUI.backgroundColor = expanded ? Color.white : Color.white * 0.95f;
			if( !GUILayout.Toggle( true, text, headerStyle, GUILayout.ExpandWidth( true ) ) )
			{
				expanded = !expanded;
				EditorPrefs.SetBool( key, expanded );
			}

			GUI.backgroundColor = Color.white;

			return expanded;

		}

		public static void Header( string text )
		{

			var headerStyle = DesignUtil.GetStyle( "Header", (GUIStyle)"TL Selection H2" );
			headerStyle.richText = true;
			headerStyle.alignment = TextAnchor.MiddleLeft;
			headerStyle.padding = new RectOffset( 10, 0, 0, 0 );
			headerStyle.margin.top = 0;
			headerStyle.margin.bottom = 10;

			GUILayout.Label( text, headerStyle );

		}

		public static InspectorSection BeginSection( string header )
		{
			GUILayout.Label( header, "HeaderLabel" );
			return new InspectorSection();
		}

		public static void DrawVertSeparator()
		{

			GUILayout.Space( 12f );

			if( Event.current.type == EventType.Repaint )
			{

				Texture2D tex = EditorGUIUtility.whiteTexture;

				Rect rect = GUILayoutUtility.GetLastRect();

				var savedColor = GUI.color;
				GUI.color = new Color( 0f, 0f, 0f, 0.25f );

				GUI.DrawTexture( new Rect( 0f, 0f, 4f, rect.height ), tex );
				GUI.DrawTexture( new Rect( 4f, 0f, 1f, rect.height ), tex );
				GUI.DrawTexture( new Rect( 94, 0f, 1f, rect.height ), tex );

				GUI.color = savedColor;

			}

		}

		public static bool SectionFoldout( string key, string title )
		{
			return SectionFoldout( key, new GUIContent( title ) );
		}

		public static bool SectionFoldout( string key, GUIContent title )
		{

			var style = GetStyle( "foldout", (GUIStyle)"IN Title" );
			style.fontStyle = FontStyle.Bold;
			style.richText = true;
			style.contentOffset = new Vector2( 0, 2 );

			var hash = title.text.GetHashCode();
			EditorGUIUtility.GetControlID( hash, FocusType.Passive );

			var config = EditorPrefs.GetBool( key, true );

			GUILayout.Space( 20 );
			var rect = GUILayoutUtility.GetLastRect();
			rect.x = 0;
			rect.y += 3;
			rect.width = Screen.width;

			if( config != GUI.Toggle( rect, config, title, style ) )
			{
				config = !config;
				EditorPrefs.SetBool( key, config );
			}

			return config;

		}

		public static bool Foldout( bool isExpanded, string title )
		{
			return Foldout( isExpanded, new GUIContent( title ), Screen.width );
		}

		public static bool Foldout( bool isExpanded, string title, int width )
		{
			return Foldout( isExpanded, new GUIContent( title ), width );
		}

		public static bool Foldout( bool isExpanded, GUIContent title, int width )
		{

			var style = GetStyle( "custom-foldout", (GUIStyle)"Foldout" );
			style.margin = new RectOffset( 0, 0, 5, 5 );
			style.richText = true;
			style.clipping = TextClipping.Clip;

			if( isExpanded != GUILayout.Toggle( isExpanded, title, style, GUILayout.Width( width ) ) )
			{
				isExpanded = !isExpanded;
			}

			var rect = GUILayoutUtility.GetLastRect();
			GUILayout.Space( rect.height );

			return isExpanded;

		}

		public static void DrawHorzLine()
		{
			DrawHorzLine( 0, 0 );
		}

		public static void DrawHorzLine( int leftMargin, int rightMargin )
		{

			GUILayout.Space( 10f );

			if( Event.current.type == EventType.Repaint )
			{

				Texture2D tex = EditorGUIUtility.whiteTexture;

				Rect rect = GUILayoutUtility.GetLastRect();

				var savedColor = GUI.color;
				GUI.color = EditorGUIUtility.isProSkin ? new Color( 1, 1, 1, 0.25f ) : new Color( 0f, 0f, 0f, 0.25f );

				GUI.DrawTexture( new Rect( leftMargin, rect.yMin + 5f, Screen.width - rightMargin, 1f ), tex );

				GUI.color = savedColor;

			}

		}

		public static void DrawDoubleLine()
		{

			var lineStyle = GetStyle( "horz-separator-double", (GUIStyle)"WindowBottomResize" );
			lineStyle.margin = new RectOffset( 0, 0, 3, 3 );

			GUILayout.Box( GUIContent.none, lineStyle, GUILayout.ExpandWidth( true ) );

		}

		public static void DrawHorzSeparator()
		{

			GUILayout.Space( 12f );

			if( Event.current.type == EventType.Repaint )
			{

				Texture2D tex = EditorGUIUtility.whiteTexture;

				Rect rect = GUILayoutUtility.GetLastRect();

				var savedColor = GUI.color;
				GUI.color = new Color( 0f, 0f, 0f, 0.25f );

				GUI.DrawTexture( new Rect( 0f, rect.yMin + 6f, Screen.width, 4f ), tex );
				GUI.DrawTexture( new Rect( 0f, rect.yMin + 6f, Screen.width, 1f ), tex );
				GUI.DrawTexture( new Rect( 0f, rect.yMin + 9f, Screen.width, 1f ), tex );

				GUI.color = savedColor;

			}

		}

		public static string MemoField( string label, string value )
		{

			BeginHorizontal( "memo field" );
			{

				GUILayout.Label( label, GUILayout.Width( EditorGUIUtility.labelWidth - 4 ) );

				var wordWrap = EditorStyles.textField.wordWrap;
				EditorStyles.textField.wordWrap = true;

				value = EditorGUILayout.TextArea( value, GUILayout.MinHeight( 75 ), GUILayout.Width( EditorGUIUtility.fieldWidth - 3 ) );

				EditorStyles.textField.wordWrap = wordWrap;

			}
			EndHorizontal( "memo field" );

			return value;

		}

		public static Enum EnumField( string label, Enum value )
		{

			BeginHorizontal( "text field" );
			{

				GUILayout.Label( label, GUILayout.Width( EditorGUIUtility.labelWidth - 4 ) );
				value = EditorGUILayout.EnumPopup( value );

			}
			EndHorizontal( "text field" );

			return value;

		}

		public static GUIStyle GetStyle( string styleName )
		{
			return GetStyle( styleName, (GUIStyle)styleName );
		}

		public static GUIStyle GetStyle( GUIStyle original )
		{
			return GetStyle( original.name, original );
		}

		public static GUIStyle GetStyle( string styleName, GUIStyle original )
		{

			GUIStyle style = null;
			if( styles.TryGetValue( styleName, out style ) )
				return style;

			style = styles[ styleName ] = new GUIStyle( original );

			return style;

		}

		public static object EditValue( string label, Type variableType, object value )
		{

			// Due to the fact that the developer can change the underlying type of a 
			// Precondition's parameter (for instance) or other value which is being edited,
			// we do a simple check for valid data type conversion here. Otherwise, the 
			// editor will throw exceptions and there is no way to recover.
			var actualType = ( value == null ) ? typeof( object ) : value.GetType();
			if( !variableType.IsAssignableFrom( actualType ) )
			{
				value = variableType.IsValueType ? Activator.CreateInstance( variableType ) : null;
			}

			if( variableType == typeof( float ) )
				return EditorGUILayout.FloatField( label, (float)value );
			else if( variableType == typeof( int ) )
				return EditorGUILayout.IntField( label, (int)value );
			else if( variableType == typeof( bool ) )
				return EditorGUILayout.Toggle( label, (bool)value );
			else if( variableType == typeof( string ) )
				return EditorGUILayout.TextField( label, value != null ? value.ToString().Trim() : string.Empty ).Trim();
			else if( variableType == typeof( Vector2 ) )
				return EditorGUILayout.Vector2Field( label, (Vector2)value );
			else if( variableType == typeof( Vector3 ) )
				return EditorGUILayout.Vector3Field( label, (Vector3)value );
			else if( variableType == typeof( Vector4 ) )
				return EditorGUILayout.Vector4Field( label, (Vector4)value );
			else if( variableType == typeof( Color ) )
				return EditorGUILayout.ColorField( label, (Color)value );
			else if( variableType == typeof( Color32 ) )
				return (Color32)EditorGUILayout.ColorField( label, (Color32)value );
			else if( variableType == typeof( Rect ) )
				return EditorGUILayout.RectField( label, (Rect)value );
			else if( variableType.IsEnum )
				return EditorGUILayout.EnumPopup( label, (Enum)value );
			else if( typeof( UnityEngine.Object ).IsAssignableFrom( variableType ) )
				return EditorGUILayout.ObjectField( label, value as UnityEngine.Object, variableType, false );

			EditorGUILayout.LabelField( "Cannot edit " + variableType.Name, (GUIStyle)"CN StatusWarn" );

			return value;

		}

		internal static object EditValue( Type variableType, object value )
		{

			// Due to the fact that the developer can change the underlying type of a 
			// Precondition's parameter (for instance) or other value which is being edited,
			// we do a simple check for valid data type conversion here. Otherwise, the 
			// editor will throw exceptions and there is no way to recover.
			var actualType = ( value == null ) ? typeof( object ) : value.GetType();
			if( !variableType.IsAssignableFrom( actualType ) )
			{
				value = variableType.IsValueType ? Activator.CreateInstance( variableType ) : null;
			}

			if( variableType == typeof( float ) )
				return EditorGUILayout.FloatField( (float)value );
			else if( variableType == typeof( int ) )
				return EditorGUILayout.IntField( (int)value );
			else if( variableType == typeof( bool ) )
				return EditorGUILayout.Toggle( (bool)value );
			else if( variableType == typeof( string ) )
				return EditorGUILayout.TextField( value != null ? value.ToString().Trim() : string.Empty ).Trim();
			else if( variableType == typeof( Vector2 ) )
				return EditorGUILayout.Vector2Field( GUIContent.none, (Vector2)value );
			else if( variableType == typeof( Vector3 ) )
				return EditorGUILayout.Vector3Field( GUIContent.none, (Vector3)value );
			else if( variableType == typeof( Vector4 ) )
				return EditorGUILayout.Vector4Field( string.Empty, (Vector4)value );
			else if( variableType == typeof( Color ) )
				return EditorGUILayout.ColorField( GUIContent.none, (Color)value );
			else if( variableType == typeof( Color32 ) )
				return (Color32)EditorGUILayout.ColorField( (Color32)value );
			else if( variableType == typeof( Rect ) )
				return EditorGUILayout.RectField( (Rect)value );
			else if( variableType.IsEnum )
				return EditorGUILayout.EnumPopup( (Enum)value );
			else if( typeof( UnityEngine.Object ).IsAssignableFrom( variableType ) )
				return EditorGUILayout.ObjectField( value as UnityEngine.Object, variableType, false );

			EditorGUILayout.LabelField( "Cannot edit " + variableType.Name, (GUIStyle)"CN StatusWarn" );
			return value;

		}

		#endregion 

		#region Replacements for horizontal and vertical layouts

		private static void BeginVertical( string key, params GUILayoutOption[] options )
		{
			BeginVertical( key, GUIStyle.none, options );
		}

		private static void BeginVertical( string key, GUIStyle style, params GUILayoutOption[] options )
		{
			nestedLayoutDebug.Push( "Vertical - " + key );
			EditorGUILayout.BeginVertical( style, options );
		}

		private static void EndVertical( string key )
		{
			EndVertical( key, GUIStyle.none );
		}

		private static void EndVertical( string key, GUIStyle style )
		{
			validateLayoutStack( "Vertical - " + key );
			EditorGUILayout.EndVertical();
		}

		private static void BeginHorizontal( string key, params GUILayoutOption[] options )
		{
			BeginHorizontal( key, GUIStyle.none, options );
		}

		private static void BeginHorizontal( string key, GUIStyle style, params GUILayoutOption[] options )
		{
			nestedLayoutDebug.Push( "Horizontal - " + key );
			EditorGUILayout.BeginHorizontal( style, options );
		}

		private static void EndHorizontal( string key )
		{
			EndHorizontal( key, GUIStyle.none );
		}

		private static void EndHorizontal( string key, GUIStyle style )
		{
			validateLayoutStack( "Horizontal - " + key );
			EditorGUILayout.EndHorizontal();
		}

		private static void validateLayoutStack( string key )
		{

			var debug = nestedLayoutDebug.Count > 0 ? nestedLayoutDebug.Pop() : "**EMPTY**";
			if( debug != key )
				Debug.LogError( string.Format( "Invalid nested layout. Expected: {0}, Actual: {1}", key, debug ) );

		}

		#endregion

		#region Nested classes 

		public class InspectorSection : IDisposable
		{
			public InspectorSection()
			{
				EditorGUI.indentLevel += 1;
			}

			#region IDisposable Members

			public void Dispose()
			{
				EditorGUI.indentLevel -= 1;
			}

			#endregion
		}

		#endregion 

	}

	public class MonoScriptHelper
	{

		#region Private variables

		private static List<ScriptInfo> allScripts = null;

		#endregion

		#region Public methods

		public static MonoScript GetMonoScriptFromType( Type type )
		{

			if( type == null )
				return null;

			if( allScripts == null )
			{
				findAllScripts();
			}

			for( int i = 0; i < allScripts.Count; i++ )
			{
				var script = allScripts[ i ];
				if( script.Type.Equals( type ) )
					return script.Script;
			}

			return null;

		}

		public static List<FieldInfo> GetFields( Type type )
		{

			if( type == null )
				return null;

			if( allScripts == null )
			{
				findAllScripts();
			}

			for( int i = 0; i < allScripts.Count; i++ )
			{
				var script = allScripts[ i ];
				if( script.Type == type )
					return script.Fields;
			}

			return null;

		}

		#endregion

		#region Private utility methods

		/// <summary>
		/// Return a list containing a ScriptInfo instance for 
		/// each MonoScript defined in the current project.
		/// Note that Unity defines a MonoScript even for types 
		/// defined in referenced assemblies, not just user scripts.
		/// </summary>
		/// <returns></returns>
		private static void findAllScripts()
		{

			// Get the list of all MonoScript instances in the project that
			// are not abstract or unclosed generic types
			allScripts = Resources
				.FindObjectsOfTypeAll( typeof( MonoScript ) )
				.Where( x =>
					x.GetType() == typeof( MonoScript )
				)
				.Cast<MonoScript>()
				.Select( x => new ScriptInfo( x ) )
				.Where( x => x.Type != null && !x.Type.IsAbstract && !x.Type.IsGenericType )
				.ToList();

		}

		#endregion

		#region Nested types

		/// <summary>
		/// Used to determine the likelihood that a particular MonoScript
		/// is a match for a component with the "Missing Script" issue
		/// </summary>
		private class ScriptInfo
		{

			#region Private data members

			private MonoScript script;
			private Type type;
			private List<FieldInfo> fields;

			#endregion

			#region Constructor

			public ScriptInfo( MonoScript script )
			{
				this.script = script;
				this.type = script.GetClass();
				this.fields = GetAllFields( type ).ToList();
			}

			#endregion

			#region Public properties

			public MonoScript Script { get { return this.script; } }

			public Type Type { get { return this.type; } }

			public string Name { get { return type.Name; } }

			public List<FieldInfo> Fields { get { return fields; } }

			#endregion

			#region Private methods

			/// <summary>
			/// Returns all instance fields on an object, including inherited fields
			/// </summary>
			private static FieldInfo[] GetAllFields( Type type )
			{

				// http://stackoverflow.com/a/1155549/154165

				if( type == null )
					return new FieldInfo[ 0 ];

				BindingFlags flags =
					BindingFlags.Public |
					BindingFlags.NonPublic |
					BindingFlags.Instance |
					BindingFlags.DeclaredOnly;

				return
					type.GetFields( flags )
					.Concat( GetAllFields( type.BaseType ) )
					.Where( f => !f.IsDefined( typeof( HideInInspector ), true ) )
					.ToArray();

			}

			#endregion

		}

		#endregion

	}

}

#endif
