#ifndef GERSTNER_WAVES_INCLUDED
#define GERSTNER_WAVES_INCLUDED

uniform uint 	_WaveCount;

struct Wave
{
	// 진푹
	float amplitude;
	// 방향
	float direction;
	// 길이
	float wavelength;
	float2 origin;
	float omni;
};

#if defined(USE_STRUCTURED_BUFFER)
StructuredBuffer<Wave> _WaveDataBuffer;
#else
half4 waveData[20];
#endif

struct WaveStruct
{
	float3 position;
	float3 normal;
};

WaveStruct GerstnerWave(half2 pos, float waveCountMulti, half amplitude, half direction, half wavelength, half omni, half2 omniPos)
{
	WaveStruct waveOut;
#if defined(_STATIC_SHADER)
	float time = 0;
#else
	float time = _Time.y;
#endif
	half3 wave = 0;
	half w = 6.28318 / wavelength;
	half wSpeed = sqrt(9.8 * w);
	half peak = 1.5;
	half qi = peak / (amplitude * w * _WaveCount);

	direction = radians(direction);
	half2 dirWaveInput = half2(sin(direction), cos(direction)) * (1 - omni);
	half2 omniWaveInput = (pos - omniPos) * omni;

	// 바람의 방향 계산
	half2 windDir = normalize(dirWaveInput + omniWaveInput);
	half dir = dot(windDir, pos - (omniPos * omni));

	// 방향 * wave 길이 * speed
	half calc = dir * w + -time * wSpeed;
	half cosCalc = cos(calc);
	half sinCalc = sin(calc);
	
	wave.xz = qi * amplitude * windDir.xy * cosCalc;
	wave.y = ((sinCalc * amplitude)) * waveCountMulti;
	
	half wa = w * amplitude;
	// 노말 계산
	half3 n = half3(-(windDir.xy * wa * cosCalc),
					1-(qi * wa * sinCalc));
	
	waveOut.position = wave * saturate(amplitude * 10000);
	waveOut.normal = (n.xzy * waveCountMulti);

	return waveOut;
}

inline void SampleWaves(float3 position, half opacity, out WaveStruct waveOut)
{
	half2 pos = position.xz;
	waveOut.position = 0;
	waveOut.normal = 0;
	half waveCountMulti = 1.0 / _WaveCount;
	half3 opacityMask = saturate(half3(3, 3, 1) * opacity);

	UNITY_LOOP
	for(uint i = 0; i < _WaveCount; i++)
	{
#if defined(USE_STRUCTURED_BUFFER)
		Wave w = _WaveDataBuffer[i];
#else
		Wave w;
		w.amplitude = waveData[i].x;
		w.direction = waveData[i].y;
		w.wavelength = waveData[i].z;
		w.omni = waveData[i].w;
		w.origin = waveData[i + 10].xy;
#endif
		WaveStruct wave = GerstnerWave(pos,
								waveCountMulti,
								w.amplitude,
								w.direction,
								w.wavelength,
								w.omni,
								w.origin);

		waveOut.position += wave.position;
		waveOut.normal += wave.normal;
	}
	waveOut.position *= opacityMask;
	waveOut.normal *= half3(opacity, 1, opacity);
}
#endif