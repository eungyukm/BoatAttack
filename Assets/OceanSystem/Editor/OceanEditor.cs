using OceanSystem.Data;
using UnityEngine;
using UnityEditor;

namespace OceanSystem
{
    [CustomEditor(typeof(Ocean))]
    public class OceanEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Ocean w = (Ocean)target;
            
            var waterSurfaceData = serializedObject.FindProperty("surfaceData");
            EditorGUILayout.PropertyField(waterSurfaceData, true);
            if(waterSurfaceData.objectReferenceValue != null)
            {
                CreateEditor((OceanSurfaceData)waterSurfaceData.objectReferenceValue).OnInspectorGUI();
            }

            serializedObject.ApplyModifiedProperties();

            if(GUI.changed)
            {
                w.Init();
            }
        }
    }
}
