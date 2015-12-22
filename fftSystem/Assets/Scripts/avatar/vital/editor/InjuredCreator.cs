using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class InjuredCreator : EditorWindow
{

	GameObject injured;
	GameObject injuredLOD;
	bool m_createLOD = false; 
	
	List<Fracture.FractureKind> m_fractureKinds=new List<Fracture.FractureKind>();
	//SerializedObject m_fkSO = new SerializedObject(m_fractureKinds);
	
	VitalModell m_vital = null;
	
	bool m_hasFracture = false;
	bool m_poisening = false;
	bool m_wound = false;
	bool m_blockedBreathing = false;
	bool m_unconsciness = false;
	bool m_burnging = false;
	
	
	// Add menu named "My Window" to the Window menu
	[MenuItem ("TI/Injured Avatar Creator")]
	static void Init ()
	{
		// Get existing open window or if none, make a new one:
		InjuredCreator window = (InjuredCreator)EditorWindow.GetWindow (typeof(InjuredCreator));
		
	}
    
	void OnGUI ()
	{
        
		injured = (GameObject)EditorGUILayout.ObjectField ("Injured", injured, typeof(GameObject), true, null);
		
		m_createLOD= EditorGUILayout.BeginToggleGroup("create LOD version", m_createLOD);
		
		injuredLOD = (GameObject)EditorGUILayout.ObjectField ("Injured LOD", injuredLOD, typeof(GameObject), true, null);
				
		Rect r = EditorGUILayout.BeginVertical ("Button");
		if (GUI.Button(r, GUIContent.none)) {
			
			if(m_createLOD)
			{
				CreateLOD(injured, injuredLOD);
			}
		}
		GUILayout.Label("Create LOD");
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndToggleGroup();
		
		m_hasFracture= EditorGUILayout.BeginToggleGroup("Has Fractures", m_hasFracture);
		for(int i = 0; i < m_fractureKinds.Count;++i)
		{
			m_fractureKinds[i].bloodLossPerMinute = EditorGUILayout.FloatField( m_fractureKinds[i].bloodLossPerMinute,"Bloodloss per Minute");
			m_fractureKinds[i].fractureType = (Fracture.FractureType)EditorGUILayout.EnumPopup("FractureType",m_fractureKinds[i].fractureType);
		//	m_fractureKinds[i].regionMorph = (MegaMorphChan)EditorGUILayout.ObjectField(m_fractureKinds[i].regionMorph, typeof(MegaMorphChan),true, null);
			
		}
		EditorGUILayout.EndToggleGroup();
		
		m_wound= EditorGUILayout.BeginToggleGroup("Has Wounds", m_wound);
		
		EditorGUILayout.EndToggleGroup();
		
		m_blockedBreathing= EditorGUILayout.BeginToggleGroup("Has blocked breathing", m_blockedBreathing);
		
		EditorGUILayout.EndToggleGroup();
		
		m_unconsciness= EditorGUILayout.BeginToggleGroup("Is unconsciness", m_unconsciness);
		
		EditorGUILayout.EndToggleGroup();
				
		m_burnging= EditorGUILayout.BeginToggleGroup("Has burnings", m_burnging);
			
		EditorGUILayout.EndToggleGroup();
	}
	
	
	void CreateLOD(GameObject injured, GameObject injuredlod){
			GameObject charbase = (GameObject)Instantiate (injured);
			GameObject charlod = (GameObject)Instantiate (injuredlod);
			charlod.transform.parent = charbase.transform;
			
			Transform meshbaseTrans = findChild(injured.name, charbase.transform);
			SkinnedMeshRenderer meshbase = meshbaseTrans.GetComponent<SkinnedMeshRenderer>();
			
			Transform meshlodTrans = findChild(injuredLOD.name, charlod.transform);
			SkinnedMeshRenderer meshlod = meshlodTrans.GetComponent<SkinnedMeshRenderer>();
			
			for(int i = 0;i < meshbase.sharedMaterials.Length;++i)
			{
				meshlod.sharedMaterials[i].shader =  meshbase.sharedMaterials[i].shader;
				
				meshlod.sharedMaterials[i].mainTexture = meshbase.sharedMaterials[i].mainTexture;
				
				
				Debug.Log(meshlod.sharedMaterials[i].name);
			}
			
			
			
			LODGroup lod = charbase.AddComponent<LODGroup> ();
			SerializedObject obj = new SerializedObject (lod);
			
			SerializedProperty propmaster = obj.FindProperty ("m_LODs.Array.data[0].renderers");

			propmaster.arraySize = 1;

			SerializedProperty propMasterRenderer = propmaster.GetArrayElementAtIndex (0).FindPropertyRelative ("renderer");

			
			
			Transform child = findChild(injured.name, charbase.transform);
			propMasterRenderer.objectReferenceValue = child.renderer;

			
			for (int i = 1; i < lod.lodCount; i++) {

				SerializedProperty prop = obj.FindProperty ("m_LODs.Array.data[" + i.ToString () + "].renderers");

				prop.arraySize = 1;

				SerializedProperty propRenderer = prop.GetArrayElementAtIndex (0).FindPropertyRelative ("renderer");

 
				child = findChild(injuredLOD.name, charlod.transform);
				
				propRenderer.objectReferenceValue = child.renderer;

			}
			obj.ApplyModifiedProperties ();
		
	}
	
	Transform findChild(string sname, Transform t)
	{
		for(int i= 0; i < t.childCount;++i)
		{
			if(t.GetChild(i).name == sname)
			{
				return t.GetChild(i);
			}
		}
		return null;
	}
}