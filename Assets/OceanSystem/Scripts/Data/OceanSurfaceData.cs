using UnityEngine;

namespace OceanSystem.Data
{
    [System.Serializable][CreateAssetMenu(fileName = "OceanSurfaceData", menuName = "OceanSystem/Surface Data", order = 0)]
    public class OceanSurfaceData : ScriptableObject
    {
        public float _waterMaxVisibility = 40.0f;
        public float _BumpScale = 0.2f;
        public Gradient _absorptionRamp;
        public Gradient _scatterRamp;
        
        public int _WaveCount = 3;
        public float _AvgSwellHeight = 0.4f;
        public int _AvgWavelength = 8;
        public int _WindDirection = -176;
        
        public CausticType _Caustics = CausticType.CausticOn;
        public float _CausticsSize = 0.2f;
        public float _CausticsSpeed = 0.01f;
        public float _CausticDistance = 0.1f;
        
        public FoamSettings _foamSettings = new FoamSettings();
        public ReflectionType refType = ReflectionType.PlanarReflection;
        public MeshType meshType = MeshType.DynamicMesh;
        [SerializeField]
        public bool _init = false;
    }

    [System.Serializable]
    public class FoamSettings
    {
        public AnimationCurve basicFoam;
        
        public FoamSettings()
        {
            basicFoam = new AnimationCurve(new Keyframe[2]{new Keyframe(0.25f, 0f),
                                                                    new Keyframe(1f, 1f)});
        }
    }
    
    [System.Serializable]
    public enum ReflectionType
    {
        ReflectionProbe,
        PlanarReflection
    }

    [System.Serializable]
    public enum MeshType
    {
        DynamicMesh,
        StaticMesh
    }

    [System.Serializable]
    public enum CausticType
    {
        CausticOn,
        CausticOff
    }
}