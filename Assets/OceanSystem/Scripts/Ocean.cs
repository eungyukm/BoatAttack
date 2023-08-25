using System;
using OceanSystem.Data;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace OceanSystem
{
    [ExecuteAlways]
    public class Ocean : MonoBehaviour
    {
        private PlanarReflections _planarReflections;

        [SerializeField] RenderTexture _depthTex;
        public Texture bakedDepthTex;
        private Camera _depthCam;
        private Texture2D _rampTexture;
        
        [SerializeField]
        public OceanSurfaceData surfaceData;
        [SerializeField]
        private OceanResources resources;

        private static readonly int CameraRoll = Shader.PropertyToID("_CameraRoll");
        private static readonly int InvViewProjection = Shader.PropertyToID("_InvViewProjection");
        private static readonly int WaterDepthMap = Shader.PropertyToID("_WaterDepthMap");
        private static readonly int MaxDepth = Shader.PropertyToID("_MaxDepth");
        private static readonly int AbsorptionScatteringRamp = Shader.PropertyToID("_AbsorptionScatteringRamp");
        private static readonly int DepthCamZParams = Shader.PropertyToID("_VeraslWater_DepthCamParams");

        private void OnEnable()
        {
            Init();
            RenderPipelineManager.beginCameraRendering += BeginCameraRendering;

            if(resources == null)
            {
                resources = Resources.Load("OceanResources") as OceanResources;
            }
        }
        private void OnDisable() {
            Cleanup();
        }
        void Cleanup()
        {
            RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
            if (_depthCam)
            {
                _depthCam.targetTexture = null;
                SafeDestroy(_depthCam.gameObject);
            }
            if (_depthTex)
            {
                SafeDestroy(_depthTex);
            }
        }

        private void BeginCameraRendering(ScriptableRenderContext src, Camera cam)
        {
            if (cam.cameraType == CameraType.Preview) return;

            var roll = cam.transform.localEulerAngles.z;
            Shader.SetGlobalFloat(CameraRoll, roll);
            Shader.SetGlobalMatrix(InvViewProjection,
                (GL.GetGPUProjectionMatrix(cam.projectionMatrix, false) * cam.worldToCameraMatrix).inverse);

            if (surfaceData.meshType != MeshType.DynamicMesh)
            {
                return;
            }
            
            CreateDynamicMesh(cam);
        }

        private void CreateDynamicMesh(Camera cam)
        {
            const float quantizeValue = 6.25f;
            const float forwards = 10f;
            const float yOffset = -0.25f;

            var newPos = cam.transform.TransformPoint(Vector3.forward * forwards);
            newPos.y = yOffset;
            newPos.x = quantizeValue * (int)(newPos.x / quantizeValue);
            newPos.z = quantizeValue * (int)(newPos.z / quantizeValue);

            var matrix = Matrix4x4.TRS(newPos + transform.position, Quaternion.identity, transform.localScale);

            foreach (var mesh in resources.defaultWaterMeshes)
            {
                Graphics.DrawMesh(mesh,
                    matrix,
                    resources.defaultSeaMaterial,
                    gameObject.layer,
                    cam,
                    0,
                    null,
                    ShadowCastingMode.Off,
                    true,
                    null,
                    LightProbeUsage.Off,
                    null);
            }
        }

        private static void SafeDestroy(Object o)
        {
            if(Application.isPlaying)
                Destroy(o);
            else
                DestroyImmediate(o);
        }

        public void Init()
        {
            SetWaves();
            GenerateColorRamp();
            if (bakedDepthTex)
            {
                Shader.SetGlobalTexture(WaterDepthMap, bakedDepthTex);
            }

            if (!gameObject.TryGetComponent(out _planarReflections))
            {
                _planarReflections = gameObject.AddComponent<PlanarReflections>();
            }
            _planarReflections.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;

            if(resources == null)
            {
                resources = Resources.Load("OceanResources") as OceanResources;
            }

            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                CaptureDepthMap();
            }
        }

        private void SetWaves()
        {
            Shader.SetGlobalFloat(MaxDepth, surfaceData._waterMaxVisibility);
            
            switch(surfaceData.refType)
            {
                case ReflectionType.ReflectionProbe:
                    Shader.EnableKeyword("_REFLECTION_PROBES");
                    Shader.DisableKeyword("_REFLECTION_PLANARREFLECTION");
                    break;
                case ReflectionType.PlanarReflection:
                    Shader.DisableKeyword("_REFLECTION_PROBES");
                    Shader.EnableKeyword("_REFLECTION_PLANARREFLECTION");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Shader.EnableKeyword("_REFLECTION_PLANARREFLECTION");
        }
        
        private void GenerateColorRamp()
        {
            if(_rampTexture == null)
                _rampTexture = new Texture2D(128, 4, GraphicsFormat.R8G8B8A8_SRGB, TextureCreationFlags.None);
            _rampTexture.wrapMode = TextureWrapMode.Clamp;

            var defaultFoamRamp = resources.defaultFoamRamp;
            var cols = new Color[512];
            for (var i = 0; i < 128; i++)
            {
                cols[i] = surfaceData._absorptionRamp.Evaluate(i / 128f);
            }
            for (var i = 0; i < 128; i++)
            {
                cols[i + 128] = surfaceData._scatterRamp.Evaluate(i / 128f);
            }
            for (var i = 0; i < 128; i++)
            {
                cols[i + 256] = defaultFoamRamp.GetPixelBilinear(surfaceData._foamSettings.basicFoam.Evaluate(i / 128f) , 0.5f);
            }
            _rampTexture.SetPixels(cols);
            _rampTexture.Apply();
            Shader.SetGlobalTexture(AbsorptionScatteringRamp, _rampTexture);
        }
        
        [ContextMenu("Capture Depth")]
        public void CaptureDepthMap()
        {
            if(_depthCam == null)
            {
                var go =
                    new GameObject("depthCamera") {hideFlags = HideFlags.HideAndDontSave};
                _depthCam = go.AddComponent<Camera>();
            }

            var additionalCamData = _depthCam.GetUniversalAdditionalCameraData();
            additionalCamData.renderShadows = false;
            additionalCamData.requiresColorOption = CameraOverrideOption.Off;
            additionalCamData.requiresDepthOption = CameraOverrideOption.Off;

            var t = _depthCam.transform;
            var depthExtra = 4.0f;
            t.position = Vector3.up * (transform.position.y + depthExtra);
            t.up = Vector3.forward;

            _depthCam.enabled = true;
            _depthCam.orthographic = true;
            _depthCam.orthographicSize = 250;
            _depthCam.nearClipPlane =0.01f;
            _depthCam.farClipPlane = surfaceData._waterMaxVisibility + depthExtra;
            _depthCam.allowHDR = false;
            _depthCam.allowMSAA = false;
            _depthCam.cullingMask = (1 << 10);

            if (!_depthTex)
            {
                _depthTex = new RenderTexture(1024, 1024, 24, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
            }
                
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
            {
                _depthTex.filterMode = FilterMode.Point;
            }
            _depthTex.wrapMode = TextureWrapMode.Clamp;
            _depthTex.name = "WaterDepthMap";
            
            _depthCam.targetTexture = _depthTex;
            _depthCam.Render();
            Shader.SetGlobalTexture(WaterDepthMap, _depthTex);
            
            var _params = new Vector4(t.position.y, 250, 0, 0);
            
            Shader.SetGlobalVector(DepthCamZParams, _params);
            
            _depthCam.enabled = false;
            _depthCam.targetTexture = null;
        }
    }
}
