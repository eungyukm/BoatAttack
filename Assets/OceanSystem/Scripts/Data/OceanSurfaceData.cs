using UnityEngine;

namespace OceanSystem.Data
{
    [System.Serializable][CreateAssetMenu(fileName = "OceanSurfaceData", menuName = "OceanSystem/Surface Data", order = 0)]
    public class OceanSurfaceData : ScriptableObject
    {
        public float _waterMaxVisibility = 40.0f;
        public Gradient _absorptionRamp;
        public Gradient _scatterRamp;
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
}