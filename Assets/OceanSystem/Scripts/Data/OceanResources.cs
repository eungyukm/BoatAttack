using UnityEngine;

namespace OceanSystem
{
	[System.Serializable][CreateAssetMenu(fileName = "OceanResources", menuName = "OceanSystem/Resource", order = 0)]
	public class OceanResources : ScriptableObject 
	{
		public Texture2D defaultFoamRamp;
        public Texture2D defaultFoamMap;
        public Texture2D defaultSurfaceMap;
        public Material defaultSeaMaterial;
        public Mesh[] defaultWaterMeshes;
	}
}
