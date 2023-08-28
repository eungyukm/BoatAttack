using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace OceanSystem.Data
{
    [CustomEditor(typeof(OceanSurfaceData))]
    public class OceanSurfaceDataEditor : Editor
    {
        [SerializeField]
        ReorderableList waveList;

        private void OnValidate()
        {
            var init = serializedObject.FindProperty("_init");
            if (init?.boolValue == false)
            {
                Setup();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("깊이 투명도", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            var maxDepth = serializedObject.FindProperty("_waterMaxVisibility");
            EditorGUILayout.Slider(maxDepth, 3, 300, new GUIContent("Maximum Visibility"));
            
            EditorGUILayout.LabelField("바다 디테일", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            var bumpScale = serializedObject.FindProperty("_BumpScale");
            EditorGUILayout.Slider(bumpScale, 0, 2, new GUIContent("Detail Wave Amount"));
            EditorGUILayout.Space();
            
            DoSmallHeader("바다 컬러");
            var absorpRamp = serializedObject.FindProperty("_absorptionRamp");
            EditorGUILayout.PropertyField(absorpRamp, new GUIContent("Absorption Color"), true, null);
            var scatterRamp = serializedObject.FindProperty("_scatterRamp");
            EditorGUILayout.PropertyField(scatterRamp, new GUIContent("Scattering Color"), true, null);
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("파도 설정", EditorStyles.boldLabel);
            var WaveCount = serializedObject.FindProperty("_WaveCount");
            EditorGUILayout.IntSlider(WaveCount, 2, 6, new GUIContent("파도의 웨이브 수"));

            var AvgSwellHeight = serializedObject.FindProperty("_AvgSwellHeight");
            EditorGUILayout.Slider(AvgSwellHeight, 0.1f, 3.0f, new GUIContent("파도의 높이"));
            
            var WindDirection = serializedObject.FindProperty("_WindDirection");
            EditorGUILayout.IntSlider(WindDirection, -180, 180, new GUIContent("바람의 방향"));
            EditorGUILayout.Space();
            
            DoSmallHeader("메시 타입 설정");
            var meshType = serializedObject.FindProperty("meshType");
            meshType.enumValueIndex = GUILayout.Toolbar(meshType.enumValueIndex, meshType.enumDisplayNames);
            switch(meshType.enumValueIndex)
            {
                case 0:
                {
                    EditorGUILayout.HelpBox("Dynamic Mesh 사용", MessageType.Info);
                }
                    break;
                case 1:
                {
                    EditorGUILayout.HelpBox("Static Mesh 사용", MessageType.Info);
                }
                    break;
            }
            EditorGUILayout.Space();
            
            DoSmallHeader("반사 타입 설정");
            var refType = serializedObject.FindProperty("refType");
            refType.enumValueIndex = GUILayout.Toolbar(refType.enumValueIndex, refType.enumDisplayNames);
            switch(refType.enumValueIndex)
            {
                case 0:
                {
                    EditorGUILayout.HelpBox("Reflection Probe 사용", MessageType.Info);
                }
                    break;
                case 1:
                {
                    EditorGUILayout.HelpBox("Planr Reflection 사용", MessageType.Info);
                }
                    break;
            }
            EditorGUILayout.Space();
            
            DoSmallHeader("물거품");
            var foamSettings = serializedObject.FindProperty("_foamSettings");
            EditorGUILayout.BeginHorizontal();
            var basicFoam = foamSettings.FindPropertyRelative("basicFoam");
            basicFoam.animationCurveValue = EditorGUILayout.CurveField(basicFoam.animationCurveValue, Color.white, new Rect(Vector2.zero, Vector2.one));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorUtility.SetDirty(this);
            serializedObject.ApplyModifiedProperties();
        }

		void DoSmallHeader(string header)
		{
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.LabelField(header, EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel += 1;
		}

        void Setup()
		{
            OceanSurfaceData wsd = (OceanSurfaceData)target;
            wsd._init = true;
            wsd._absorptionRamp = DefaultAbsorptionGrad();
            wsd._scatterRamp = DefaultScatterGrad();
            EditorUtility.SetDirty(wsd);
        }

        Gradient DefaultAbsorptionGrad()
        {
            Gradient g = new Gradient();
            GradientColorKey[] gck = new GradientColorKey[5];
            GradientAlphaKey[] gak = new GradientAlphaKey[1];
            gak[0].alpha = 1;
            gak[0].time = 0;
            gck[0].color = Color.white;
            gck[0].time = 0f;
            gck[1].color = new Color(0.22f, 0.87f, 0.87f);
            gck[1].time = 0.082f;
            gck[2].color = new Color(0f, 0.47f, 0.49f);
            gck[2].time = 0.318f;
            gck[3].color = new Color(0f, 0.275f, 0.44f);
            gck[3].time = 0.665f;
            gck[4].color = Color.black;
            gck[4].time = 1f;
            g.SetKeys(gck, gak);
            return g;
        }

        Gradient DefaultScatterGrad()
        {
            Gradient g = new Gradient();
            GradientColorKey[] gck = new GradientColorKey[4];
            GradientAlphaKey[] gak = new GradientAlphaKey[1];
            gak[0].alpha = 1;
            gak[0].time = 0;
            gck[0].color = Color.black;
            gck[0].time = 0f;
            gck[1].color = new Color(0.08f, 0.41f, 0.34f);
            gck[1].time = 0.15f;
            gck[2].color = new Color(0.13f, 0.55f, 0.45f);
            gck[2].time = 0.42f;
            gck[3].color = new Color(0.21f, 0.62f, 0.6f);
            gck[3].time = 1f;
            g.SetKeys(gck, gak);
            return g;
        }
    }
}
