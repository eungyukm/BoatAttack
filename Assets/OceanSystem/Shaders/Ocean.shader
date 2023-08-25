﻿Shader "RM2/Ocean"
{
	Properties
	{
		_BumpScale("Detail Wave Amount", Range(0, 2)) = 0.2
		_DitherPattern ("Dithering Pattern", 2D) = "bump" {}
		[Toggle(_STATIC_SHADER)] _Static ("Static", Float) = 0
		_SurfaceMap ("SurfaceMap", 2D) = "white" {}
		_FoamMap ("FoamMap", 2D) = "white" {}
		
		// 파도
		_WaveCount ("WaveCount", Range(2,6)) = 4
		_AvgSwellHeight("AvgSwehllHeight", Range(0.1, 3.0)) = 0.4
		_AvgWavelength("AvgWavelength", Range(1, 120)) = 8
		_WindDirection("WindDirection", Range(-180, 180)) = -176 
		
		// 랜덤 seed
		_randomSeed("randomSeed", int) = 3123
		_MaxWaveHeight ("MaxWaveHeight", Range(0.1, 15.0))= 0.4
		[KeywordEnum(Off, SSS, Refraction, Reflection, Normal, Fresnel, WaterEffects, Foam, WaterDepth)] _Debug ("Debug mode", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent-100" "RenderPipeline" = "UniversalPipeline" }
		ZWrite On

		Pass
		{
			Name "WaterShading"
			Tags{"LightMode" = "UniversalForward"}

			HLSLPROGRAM
			#pragma prefer_hlslcc gles
			/////////////////SHADER FEATURES//////////////////
			#pragma shader_feature _REFLECTION_PROBES _REFLECTION_PLANARREFLECTION
			#pragma multi_compile _ _STATIC_SHADER
			#pragma shader_feature _DEBUG_OFF _DEBUG_SSS _DEBUG_REFRACTION _DEBUG_REFLECTION _DEBUG_NORMAL _DEBUG_FRESNEL _DEBUG_FOAM _DEBUG_WATERDEPTH
			
            // Lightweight Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
			
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
			
			#include "OceanCommon.hlsl"
			
			#pragma vertex OceanVertex
			#pragma fragment OceanFragment

			ENDHLSL
		}
	}
	FallBack "Hidden/InternalErrorShader"
}
