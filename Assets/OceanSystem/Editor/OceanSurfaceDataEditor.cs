﻿using UnityEngine;
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
            EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            var maxDepth = serializedObject.FindProperty("_waterMaxVisibility");
            EditorGUILayout.Slider(maxDepth, 3, 300, new GUIContent("Maximum Visibility"));
            DoSmallHeader("Coloring Controls");
            var absorpRamp = serializedObject.FindProperty("_absorptionRamp");
            EditorGUILayout.PropertyField(absorpRamp, new GUIContent("Absorption Color"), true, null);
            var scatterRamp = serializedObject.FindProperty("_scatterRamp");
            EditorGUILayout.PropertyField(scatterRamp, new GUIContent("Scattering Color"), true, null);
            DoSmallHeader("Surface Foam");
            var foamSettings = serializedObject.FindProperty("_foamSettings");
            EditorGUILayout.Space();
            
            DoSmallHeader("refType");
            var refType = serializedObject.FindProperty("refType");
            refType.enumValueIndex = GUILayout.Toolbar(refType.enumValueIndex, refType.enumDisplayNames);
            switch(refType.enumValueIndex)
            {
                case 0:
                {
                    // probe
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
