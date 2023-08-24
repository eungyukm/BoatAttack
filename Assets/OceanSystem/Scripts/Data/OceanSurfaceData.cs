using Unity.Mathematics;
using UnityEngine;

namespace OceanSystem.Data
{
    [System.Serializable][CreateAssetMenu(fileName = "OceanSurfaceData", menuName = "OceanSystem/Surface Data", order = 0)]
    public class OceanSurfaceData : ScriptableObject
    {
        public float _waterMaxVisibility = 40.0f;
        public Gradient _absorptionRamp;
        public Gradient _scatterRamp;
        public int randomSeed = 3234;
        public BasicWaves _basicWaveSettings = new BasicWaves(1.5f, 45.0f, 5.0f);
        public FoamSettings _foamSettings = new FoamSettings();
        [SerializeField]
        public bool _init = false;
    }

    [System.Serializable]
    public struct Wave
    {
        public float amplitude;
        public float direction;
        public float wavelength;
        public float2 origin;
        public float onmiDir;

        public Wave(float amp, float dir, float length, float2 org, bool omni)
        {
            amplitude = amp;
            direction = dir;
            wavelength = length;
            origin = org;
            onmiDir = omni ? 1 : 0;
        }
    }

    [System.Serializable]
    public class BasicWaves
    {
        public int numWaves = 6;
        public float amplitude;
        public float direction;
        public float wavelength;

        public BasicWaves(float amp, float dir, float len)
        {
            numWaves = 6;
            amplitude = amp;
            direction = dir;
            wavelength = len;
        }
    }

    [System.Serializable]
    public class FoamSettings
    {
        public AnimationCurve basicFoam;

        // Foam curves
        public FoamSettings()
        {
            basicFoam = new AnimationCurve(new Keyframe[2]{new Keyframe(0.25f, 0f),
                                                                    new Keyframe(1f, 1f)});
        }
    }
}