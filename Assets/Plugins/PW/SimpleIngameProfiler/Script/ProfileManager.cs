using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

namespace Playwith.TA
{
    public enum ProfilerState
    {
        Information,
        Summary,
        Memory
    }
    public class ProfileManager : MonoBehaviour
    {
        #region SerializeField
        // 프로파일 State
        [FormerlySerializedAs("_profilerState")] public ProfilerState profilerState = ProfilerState.Summary;
        [SerializeField] private bool enable = true;
        // VSync 제거, 목표 프레임 설정
        [SerializeField] private int limitFPS = 300;
        [SerializeField] private float updateSecond = 0.1f;
        [SerializeField] private float fPSUpdateSecond = 10;
    
        public TMP_Text infoText;
        public TMP_Text simpleText;
        public TMP_Text memoryText;
        #endregion

        #region Local Property

        // 기본 정보
        private string _deviceName;
        private string _os;
        private float _memory;
        private int _screenSizeX;
        private int _screenSizeY;
        private float _hertz;
        private float _dpi;
        
        //FPS
        private float _fpsNow;
        private float _fpsMin;
        private float _fpsMax;
        private float _fpsAvg;
        private List<float> _fpsChecking = new List<float>();
    
        //CPU
        private string _cpuName;
        private int _cpuCount;
        private float _cpuFrequency;
        
        //GPU
        private string _gpuName;
    
        ProfilerRecorder _totalReservedMemoryRecorder;
        ProfilerRecorder _totalMemoryRecorder;
        ProfilerRecorder _totalCommittedMemoryRecorder;
        ProfilerRecorder _systemMemoryRecorder;
        ProfilerRecorder _gcReservedMemoryRecorder;
        ProfilerRecorder _textureMemoryRecorder;
        ProfilerRecorder _meshMemoryRecorder;
        ProfilerRecorder _materialMemoryRecorder;

        ProfilerRecorder _triangleRecorder;
        ProfilerRecorder _setPassCallRecorder;
        ProfilerRecorder _drawCallRecorder;
    
        string _statsText;
        //String 성능 문제 회피를 위한 StringBuilder
        StringBuilder _builder = new StringBuilder(300);

        private float _deltaTime = 0.0f;

        //Frame Timing Manager
        readonly FrameTiming[] _frameTimings = new FrameTiming[1];
        // private float _cpuTime;
        private float _mainThreadTime;
        private float _renderThreadTime;
        private float _gpuTime;

        #endregion

        #region Event
        private void OnDisable()
        {
            _triangleRecorder.Dispose();
            _setPassCallRecorder.Dispose();
            _drawCallRecorder.Dispose();

            _totalReservedMemoryRecorder.Dispose();
            _systemMemoryRecorder.Dispose();
            _gcReservedMemoryRecorder.Dispose();
            _textureMemoryRecorder.Dispose();
            _meshMemoryRecorder.Dispose();
            _materialMemoryRecorder.Dispose();
        }
        #endregion
        
        // 바이트 단위, 개수 단위 변환
        private static readonly string[] Units = { "B", "KB", "MB", "GB", "TB" };
        private static readonly string[] Simbol = { "", "K", "M", "B", "T" };

        private void Awake()
        {
            if (enable == false)
            {
                OnDisable();
                return;
            }
            
            Application.targetFrameRate = limitFPS;
            
            DontDestroyOnLoad(this);
        }
        
        private void Start()
        {
            SetActiveProfile(profilerState);
        }

        private void OnSummary()
        {
            _triangleRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            _setPassCallRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            _drawCallRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            
            _totalCommittedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        }

        private void OffSummary()
        {
            _triangleRecorder.Dispose();
            _setPassCallRecorder.Dispose();
            _drawCallRecorder.Dispose();

            _totalReservedMemoryRecorder.Dispose();
        }

        private void OnMemory()
        {
            _totalReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
            _totalCommittedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
            _totalMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");

            _systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
            _gcReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
            _textureMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Memory");
            _meshMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Mesh Memory");
            _materialMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Material Memory");
        }

        private void OffMemory()
        {
            _totalReservedMemoryRecorder.Dispose();
            _totalCommittedMemoryRecorder.Dispose();
            _totalMemoryRecorder.Dispose();
            _systemMemoryRecorder.Dispose();
            _gcReservedMemoryRecorder.Dispose();
            _textureMemoryRecorder.Dispose();
            _meshMemoryRecorder.Dispose();
            _materialMemoryRecorder.Dispose();
        }

        public void SetActiveProfile(ProfilerState state)
        {
            profilerState = state;
            GetInfo();
            StopAllCoroutines();
            switch (profilerState)
            {
                case ProfilerState.Information:
                    break;
                case ProfilerState.Summary:
                    OffMemory();
                    OnSummary();
                    StartCoroutine(Summery());
                    StartCoroutine(ResetValue());
                    break;
                case ProfilerState.Memory:
                    OffSummary();
                    OnMemory();
                    StartCoroutine(MemoryCheck());
                    break;
            }
        }
        
        private void Update()
        {
            if (enable == false)
                return;

            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        }

        #region Logic
        
        private void CaptureFrameTiming()
        {
            FrameTimingManager.CaptureFrameTimings();
            FrameTimingManager.GetLatestTimings((uint)_frameTimings.Length, _frameTimings);
            _mainThreadTime = (float)_frameTimings[0].cpuMainThreadFrameTime;
            _renderThreadTime = (float)_frameTimings[0].cpuRenderThreadFrameTime;
            _gpuTime = (float)_frameTimings[0].gpuFrameTime;
        }
        
        public void GetInfo()
        {
            _deviceName = SystemInfo.deviceModel;
            _os = SystemInfo.operatingSystem;
            _screenSizeX = Screen.width;
            _screenSizeY = Screen.height;
            _hertz = (float)Screen.currentResolution.refreshRateRatio.value;
            // 현재 적용되고 있는 DPI 팩터 계수 = 실제 렌더링 사이즈 / 기본 렌더링 사이즈
            if (Application.platform != RuntimePlatform.Android) { _dpi = Screen.dpi; }
            else
            {
                int renderingSizeX = Screen.width;
                int nativeSizeX = DisplayMetricsAndroid.WidthPixels;
                // divide 의 경우 순환소수가 나올 수 있어 형변환등이 반드시 필요.
                double realDPIFactor = renderingSizeX / (double)nativeSizeX;
                _dpi = (float)Convert.ToDouble(realDPIFactor * Screen.dpi);
            }

            _cpuName = SystemInfo.processorType;
            _cpuCount = SystemInfo.processorCount;
            _cpuFrequency = SystemInfo.processorFrequency;

            _gpuName = $"{SystemInfo.graphicsDeviceName} | {SystemInfo.graphicsDeviceType}";
            _memory = SystemInfo.systemMemorySize;

            _builder.Clear();
            OnDrawInfo();
            _statsText = _builder.ToString();
            infoText.text = _statsText;
        }
        IEnumerator Summery()
        {
            while (true)
            {
                yield return new WaitForSeconds(updateSecond);
                CaptureFrameTiming();
                GetSummery();
            }
        }
        
        public void GetSummery()
        {
            _builder.Clear();

            _fpsNow = 1.0f / _deltaTime;  //초당 프레임
            if (_fpsNow < _fpsMin)  //새로운 최저 fps가 나왔다면 worstFps 바꿔줌.
                _fpsMin = _fpsNow;
            if (_fpsNow > _fpsMax)
                _fpsMax = _fpsNow;
            _fpsChecking.Add(_fpsNow);

            OnDrawSummery();
            _statsText = _builder.ToString();
            simpleText.text = _statsText;
        }
        void OnDrawSummery()
        {
            // 사이즈
            _builder.AppendLine($"  Screen : {_screenSizeX} x {_screenSizeY}@{_hertz}Hz[{_dpi}]");
            // FPS current Min Max Avg
            _builder.AppendLine($"  FPS : Now {_fpsNow:F0} | Min {_fpsMin:F0} | Max {_fpsMax:F0} | Avg {_fpsAvg:F0}");
            _builder.AppendLine($"  Main {_mainThreadTime:F2}ms | Render {_renderThreadTime:F2}ms | GPU {_gpuTime:F2}ms");
            _builder.AppendLine($"");

            // Render 정보
            _builder.AppendLine($"  Tris                   : {ReadableSize(_triangleRecorder.LastValue)}");
            _builder.AppendLine($"  SetPass Calls  : {_setPassCallRecorder.LastValue}");
            _builder.AppendLine($"  Draw Calls       : {_drawCallRecorder.LastValue}");
            // 메모리 사용량
            _builder.AppendLine($"  Memory           : {ReadableFileSize(_totalCommittedMemoryRecorder.LastValue)} / {ReadableFileSize(_memory, 2)}");
        }
        private void OnDrawInfo()
        {
#if !UNITY_EDITOR
        _builder.AppendLine($"  Device : {_deviceName}");
#endif
            _builder.AppendLine($"  OS : {_os}");
            _builder.AppendLine($"  Screen : {_screenSizeX} x {_screenSizeY}@{_hertz}Hz [{_dpi}]");
            _builder.AppendLine($"");
            _builder.AppendLine($"  CPU : {_cpuName}");
            _builder.AppendLine($"              --{_cpuCount} Core {_cpuFrequency / 1024:0.#}GHz");
            _builder.AppendLine($"  GPU : {_gpuName}");
            _builder.AppendLine($"  Memory : {ReadableFileSize(_memory, 2)} ");
        }
        private void OnDrawMemory()
        {
            _builder.AppendLine($"  Total Used Memory    : {ReadableFileSize(_totalMemoryRecorder.LastValue)} / {ReadableFileSize(_memory, 2)}");
            _builder.AppendLine($"  Total Reserved Memory    : {ReadableFileSize(_totalReservedMemoryRecorder.LastValue)} / {ReadableFileSize(_memory, 2)}");
            _builder.AppendLine($"  Total Committed Memory    : {ReadableFileSize(_totalCommittedMemoryRecorder.LastValue)} / {ReadableFileSize(_memory, 2)}");
            _builder.AppendLine($"  Texture     : {ReadableFileSize(_textureMemoryRecorder.LastValue)}");
            _builder.AppendLine($"  Meshes    : {ReadableFileSize(_meshMemoryRecorder.LastValue)}");
            _builder.AppendLine($"  Materials  : {ReadableFileSize(_materialMemoryRecorder.LastValue)}");
            _builder.AppendLine($"  GC Alloc   : {ReadableFileSize(_gcReservedMemoryRecorder.LastValue)}");
        }
        
        IEnumerator ResetValue()
        {
            while (true)
            {
                yield return new WaitForSeconds(fPSUpdateSecond);
                _fpsMin = 1000f;
                _fpsMax = 0f;
                _fpsAvg = _fpsChecking.Average();
                _fpsChecking.Clear();
            }
        }

        IEnumerator MemoryCheck()
        {
            while (true)
            {
                yield return new WaitForSeconds(updateSecond);
                GetMemory();
            }
        }
        
        public void GetMemory()
        {
            _builder.Clear();

            OnDrawMemory();
            _statsText = _builder.ToString();
            memoryText.text = _statsText;
        }
        #endregion

        #region Utility
        static string ReadableFileSize(double size, int unit = 0)
        {

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }

            return $"{size:0.0#} {Units[unit]}";
        }
        static string ReadableSize(double size)
        {
            int unit = 0;
            while (size >= 1000)
            {
                size /= 1000;
                ++unit;
            }
            return $"{size:0.#} {Simbol[unit]}";
        }
        #endregion

    }
    
    #region Android Native
    // Native 해상도를 가져오기 위한 클래스
    // 유니티에서 제공하는 Screen.width, Screen.currentResolution.width 둘 모두 변경된 해상도를 가져옴.
    // currentResolution.width에서 가져온 해상도의 경우 테스트 해봐야하겠지만, 추측으로는 URP Asset의 Render Scale 같은 부분에서 영향을 받지 않을까 생각됨
    public class DisplayMetricsAndroid
    {

        // The logical density of the display
        public static float Density { get; protected set; }

        // The screen density expressed as dots-per-inch
        public static int DensityDPI { get; protected set; }

        // The absolute height of the display in pixels
        public static int HeightPixels { get; protected set; }

        // The absolute width of the display in pixels
        public static int WidthPixels { get; protected set; }

        // A scaling factor for fonts displayed on the display
        public static float ScaledDensity { get; protected set; }

        // The exact physical pixels per inch of the screen in the X dimension
        public static float XDPI { get; protected set; }

        // The exact physical pixels per inch of the screen in the Y dimension
        public static float YDPI { get; protected set; }

        static DisplayMetricsAndroid()
        {
            // Early out if we're not on an Android device
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }

            // 자바 코드에서는 다음과 같음:
            //
            // metricsInstance = new DisplayMetrics();
            // UnityPlayer.currentActivity.getWindowManager().getDefaultDisplay().getMetrics(metricsInstance);
            //
            // ... 안드로이드 레퍼런스 문서
            // http://developer.android.com/reference/android/util/DisplayMetrics.html

            using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"),
            metricsClass = new AndroidJavaClass("android.util.DisplayMetrics"))
            {
                using (
                AndroidJavaObject metricsInstance = new AndroidJavaObject("android.util.DisplayMetrics"),
                activityInstance = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"),
                windowManagerInstance = activityInstance.Call<AndroidJavaObject>("getWindowManager"),
                displayInstance = windowManagerInstance.Call<AndroidJavaObject>("getDefaultDisplay")
                )
                {
                    displayInstance.Call("getRealMetrics", metricsInstance);
                    Density = metricsInstance.Get<float>("density");
                    DensityDPI = metricsInstance.Get<int>("densityDpi");
                    HeightPixels = metricsInstance.Get<int>("heightPixels");
                    WidthPixels = metricsInstance.Get<int>("widthPixels");
                    ScaledDensity = metricsInstance.Get<float>("scaledDensity");
                    XDPI = metricsInstance.Get<float>("xdpi");
                    YDPI = metricsInstance.Get<float>("ydpi");
                }
            }
        }
    }
    #endregion
}

