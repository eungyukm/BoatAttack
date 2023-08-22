using UnityEditor;

namespace WaterSystem.Data
{
    [CustomEditor(typeof(WaterSettingsData))]
    public class WaterSettingsDataEditor : Editor
    {
		public override void OnInspectorGUI()
        {
			var geomType = serializedObject.FindProperty("waterGeomType");
			EditorGUILayout.PropertyField(geomType);
			var planarSettings = serializedObject.FindProperty("planarSettings");
			EditorGUILayout.PropertyField(planarSettings, true);
			serializedObject.ApplyModifiedProperties();
		}
	}
}
