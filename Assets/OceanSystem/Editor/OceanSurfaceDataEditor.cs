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
            if(init?.boolValue == false)
                Setup();

            var standardHeight = EditorGUIUtility.singleLineHeight;
            var standardLine = standardHeight + EditorGUIUtility.standardVerticalSpacing;
            waveList = new ReorderableList(serializedObject, serializedObject.FindProperty("_waves"), true, true, true, true);
            waveList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
			{
                var element = waveList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                
                var preWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = rect.width * 0.2f;
                Rect ampRect = new Rect(rect.x, rect.y + standardLine, rect.width * 0.5f, standardHeight);
                var waveAmp = element.FindPropertyRelative("amplitude");
                waveAmp.floatValue = EditorGUI.Slider(ampRect, "Swell Height", waveAmp.floatValue, 0.1f, 30f);
                
                Rect lengthRect = new Rect(rect.x + ampRect.width, rect.y + standardLine, rect.width * 0.5f, standardHeight);
                var waveLen = element.FindPropertyRelative("wavelength");
                waveLen.floatValue = EditorGUI.Slider(lengthRect, "Wavelength", waveLen.floatValue, 1.0f, 200f);
                EditorGUIUtility.labelWidth = preWidth;
                
                Rect dirToggleRect = new Rect(rect.x, rect.y + 2 + standardLine * 2, rect.width * 0.5f, standardHeight);
                Rect omniToggleRect = new Rect(rect.x + rect.width * 0.5f, dirToggleRect.y, rect.width * 0.5f, standardHeight);
                Rect containerRect = new Rect(rect.x, dirToggleRect.y + 1, rect.width, standardLine * 3.2f);
                
                var waveType = element.FindPropertyRelative("onmiDir");
                var wTypeBool = (int)waveType.floatValue == 1 ? true : false;
                GUI.Box(containerRect, "", EditorStyles.helpBox );
                wTypeBool = !GUI.Toggle(dirToggleRect, !wTypeBool, "Directional", EditorStyles.miniButtonLeft);
                wTypeBool = GUI.Toggle(omniToggleRect, wTypeBool, "Omni-directional", EditorStyles.miniButtonRight);
                waveType.floatValue = wTypeBool ? 1 : 0;

                Rect dirRect = new Rect(rect.x + 4, dirToggleRect.y + standardLine, rect.width - 8, standardHeight);
                Rect buttonRect = new Rect(rect.x + 4, dirRect.y + standardLine + 2, rect.width - 8, standardHeight);
                
                if(!wTypeBool)
                {
                    var waveDir = element.FindPropertyRelative("direction");
                    waveDir.floatValue = EditorGUI.Slider(dirRect, "Facing Direction", waveDir.floatValue, -180.0f, 180.0f);
                    if(GUI.Button(buttonRect, "Align with Scene Camera"))
                        waveDir.floatValue = CameraRelativeDirection();
                }
                else
                {
                    EditorGUIUtility.wideMode = true;
                    var waveOrig = element.FindPropertyRelative("origin");
                    waveOrig.vector2Value = EditorGUI.Vector2Field(dirRect, "Point of Origin", waveOrig.vector2Value);
                    if(GUI.Button(buttonRect, "Project Origin from Scene Camera"))
                        waveOrig.vector2Value = CameraRelativeOrigin(waveOrig.vector2Value);
                }
            };
            
            waveList.onCanRemoveCallback = (ReorderableList l) =>
            {
                return l.count > 1;
            };
            
            waveList.onRemoveCallback = (ReorderableList l) =>
            {
                if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the wave?", "Yes", "No"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(l);
                }
            };
            
            waveList.onAddCallback = (ReorderableList l) =>
            {
                var index = l.serializedProperty.arraySize;
                if (index < 10)
                {
                    l.serializedProperty.arraySize++;
                    l.index = index;
                    var element = l.serializedProperty.GetArrayElementAtIndex(index);
                    element.FindPropertyRelative("amplitude").floatValue = Random.Range(0.25f, 1.5f);
                    element.FindPropertyRelative("direction").floatValue = Random.Range(-180f, 180f);
                    element.FindPropertyRelative("wavelength").floatValue = Random.Range(2f, 8f);
                }
                else
                {
                    EditorUtility.DisplayDialog("Warning!", "You have reached the limit of 10 waves for this Water.", "Close");
                }
            };
            
            waveList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Wave List");
            };
            
            waveList.elementHeightCallback = (index) =>
            {
                var elementHeight = standardLine * 6f;
                return elementHeight;
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            var maxDepth = serializedObject.FindProperty("_waterMaxVisibility");
            EditorGUILayout.Slider(maxDepth, 3, 300, new GUIContent("Maximum Visibility", maxDepthTT));
            DoSmallHeader("Coloring Controls");
            var absorpRamp = serializedObject.FindProperty("_absorptionRamp");
            EditorGUILayout.PropertyField(absorpRamp, new GUIContent("Absorption Color", absorpRampTT), true, null);
            var scatterRamp = serializedObject.FindProperty("_scatterRamp");
            EditorGUILayout.PropertyField(scatterRamp, new GUIContent("Scattering Color", scatterRampTT), true, null);
            // Foam Ramps
            DoSmallHeader("Surface Foam");
            var foamSettings = serializedObject.FindProperty("_foamSettings");
            var foamType = foamSettings.FindPropertyRelative("foamType");
            foamType.intValue = GUILayout.Toolbar(foamType.intValue, foamTypeOptions);

            EditorGUILayout.Space();

            switch (foamType.intValue)
            {
			case 0: //// Auto ////
				{
					EditorGUILayout.HelpBox("Automatic will distribute the foam suitable for an average swell", MessageType.Info);
				}
				break;
			case 1: //// Simple ////
				{
					EditorGUILayout.BeginHorizontal();
					DoInlineLabel("Foam Profile", foamCurveTT, 50f);
					var basicFoam = foamSettings.FindPropertyRelative("basicFoam");
					basicFoam.animationCurveValue = EditorGUILayout.CurveField(basicFoam.animationCurveValue, Color.white, new Rect(Vector2.zero, Vector2.one));
					EditorGUILayout.EndHorizontal();
				}
				break;
            }

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.LabelField("Wave Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            EditorGUILayout.Space();

            var basicSettings = serializedObject.FindProperty("_basicWaveSettings");
            
            var autoCount = basicSettings.FindPropertyRelative("numWaves");
            EditorGUILayout.IntSlider(autoCount, 1, 10, new GUIContent("Wave Count", waveCountTT), null);
            var avgHeight = basicSettings.FindPropertyRelative("amplitude");
            EditorGUILayout.Slider(avgHeight, 0.1f, 30.0f, new GUIContent("Avg Swell Height", avgHeightTT), null);
            var avgWavelength = basicSettings.FindPropertyRelative("wavelength");
            EditorGUILayout.Slider(avgWavelength, 1.0f, 200.0f, new GUIContent("Avg Wavelength", avgWavelengthTT), null);
            EditorGUILayout.BeginHorizontal();
            var windDir = basicSettings.FindPropertyRelative("direction");
            EditorGUILayout.Slider(windDir, -180.0f, 180.0f, new GUIContent("Wind Direction", windDirTT), null);
            if(GUILayout.Button(new GUIContent("Align to scene camera", alignButtonTT)))
                windDir.floatValue = CameraRelativeDirection();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var randSeed = serializedObject.FindProperty("randomSeed");
            randSeed.intValue = EditorGUILayout.IntField(new GUIContent("Random Seed", randSeedTT), randSeed.intValue);
            if (GUILayout.Button("Randomize Waves"))
            {
                randSeed.intValue = System.DateTime.Now.Millisecond * 100 - System.DateTime.Now.Millisecond;
            }
            EditorGUILayout.EndHorizontal();
            EditorUtility.SetDirty(this);
            serializedObject.ApplyModifiedProperties();
        }

		void DoSmallHeader(string header)
		{
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.LabelField(header, EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel += 1;
		}

        void DoInlineLabel(string label, string tooltip, float width)
		{
            var preWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = width;
            EditorGUILayout.LabelField(new GUIContent(label, tooltip));
            EditorGUIUtility.labelWidth = preWidth;
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

        float CameraRelativeDirection()
        {
            float degrees = 0;

            Vector3 camFwd = UnityEditor.SceneView.lastActiveSceneView.camera.transform.forward;
            camFwd.y = 0f;
            camFwd.Normalize();
            float dot = Vector3.Dot(-Vector3.forward, camFwd);
            degrees = Mathf.LerpUnclamped(90.0f, 180.0f, dot);
            if(camFwd.x < 0)
                degrees *= -1f;

            return Mathf.RoundToInt(degrees * 1000) / 1000;
        }

        Vector2 CameraRelativeOrigin(Vector2 original)
        {
            Camera sceneCam = UnityEditor.SceneView.lastActiveSceneView.camera;

            float angle = 90f - Vector3.Angle(sceneCam.transform.forward, Vector3.down);
            if (angle > 0.1f)
            {
                Vector3 intersect = Vector2.zero;
                float hypot = (sceneCam.transform.position.y) * (1 / Mathf.Sin(Mathf.Deg2Rad * angle));
                Vector3 fwd = sceneCam.transform.forward * hypot;
                intersect = fwd + sceneCam.transform.position;
                return new Vector2(intersect.x, intersect.z);
            }
            else
            {
                return original;
            }
        }
        static string[] foamTypeOptions = new string[3] { "Automatic", "Simple Curve", "Density Curves" };


        ////TOOLTIPS////
        private string maxDepthTT = "This controls the max depth of the waters transparency/visiblility, the absorption and scattering gradients map to this depth. Units:Meters";
        private string absorpRampTT = "This gradient controls the color of the water as it gets deeper, darkening the surfaces under the water as they get deeper.";
        private string scatterRampTT = "This gradient controls the 'scattering' of the water from shallow to deep, lighting the water as there becomes more of it.";
        private string waveCountTT = "Number of waves the automatic setup creates, if aiming for mobile set to 6 or less";
        private string avgHeightTT = "The average height of the waves. Units:Meters";
        private string avgWavelengthTT = "The average wavelength of the waves. Units:Meters";
        private string windDirTT = "The general wind direction, this is in degrees from Z+";
        private string alignButtonTT = "This aligns the wave direction to the current scene view camera facing direction";
        private string foamCurveTT = "This curve control the foam propagation. X is wave height and Y is foam opacity";
        private string foam3curvesTT = "These three curves control the Lite, Medium and Dense foam propagation. X is wave height and Y is foam opacity";
        private string randSeedTT = "This seed controls the automatic wave generation";
    }
}
