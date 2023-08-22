using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace WaterSystem.Data
{
	[System.Serializable][CreateAssetMenu(fileName = "WaterSettingsData", menuName = "WaterSystem/Settings", order = 0)]
    public class WaterSettingsData : ScriptableObject
    {
	    public ReflectionType refType = ReflectionType.PlanarReflection; // How the reflecitons are generated
		// planar
		public PlanarReflections.PlanarReflectionSettings planarSettings; // Planar reflection settings
    }
    
	[System.Serializable]
	public enum ReflectionType
	{
		PlanarReflection
	}
}
