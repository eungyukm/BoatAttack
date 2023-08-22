using UnityEngine;
using UnityEditor;
using WaterSystem.Data;

namespace WaterSystem
{
    [CustomEditor(typeof(Water))]
    public class WaterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Water w = (Water)target;

            var waterSettingsData = serializedObject.FindProperty("settingsData");
            EditorGUILayout.PropertyField(waterSettingsData, true);
            if(waterSettingsData.objectReferenceValue != null)
            {
                CreateEditor((WaterSettingsData)waterSettingsData.objectReferenceValue).OnInspectorGUI();
            }

            var waterSurfaceData = serializedObject.FindProperty("surfaceData");
            EditorGUILayout.PropertyField(waterSurfaceData, true);
            if(waterSurfaceData.objectReferenceValue != null)
            {
                CreateEditor((WaterSurfaceData)waterSurfaceData.objectReferenceValue).OnInspectorGUI();
            }

            serializedObject.ApplyModifiedProperties();

            if(GUI.changed)
            {
                w.Init();
            }
        }
    }
}
