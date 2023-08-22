using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace WaterSystem.Data
{
	[System.Serializable][CreateAssetMenu(fileName = "WaterSettingsData", menuName = "WaterSystem/Settings", order = 0)]
    public class WaterSettingsData : ScriptableObject
    {
	    // planar
		public PlanarReflections.PlanarReflectionSettings planarSettings; // Planar reflection settings
    }
}
