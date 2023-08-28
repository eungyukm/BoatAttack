#ifndef GERSTNER_WAVES_INCLUDED
#define GERSTNER_WAVES_INCLUDED

uniform uint _WaveCount;
half _AvgSwellHeight;
half _AvgWavelength;
half _WindDirection;
half _randomSeed;

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

struct WaveStruct
{
	float3 position;
	float3 normal;
};

WaveStruct GerstnerWave(half2 pos, float waveCountMulti, half amplitude, half direction, half wavelength, half omni, half2 omniPos)
{
	WaveStruct waveOut;
	
	float time = _Time.y;
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

// Seed에 따른 랜덤 생성
float Random(float seed)
{
	return frac(sin(seed * 12.9898 + 78.233) * 43758.5453);
}

inline void SampleWaves(float3 position, half opacity, out WaveStruct waveOut)
{
	half2 pos = position.xz;
	waveOut.position = 0;
	waveOut.normal = 0;
	half waveCountMulti = 1.0 / _WaveCount;
	half3 opacityMask = saturate(half3(3, 3, 1) * opacity);

	float a = _AvgSwellHeight;
	float d = _WindDirection;
	float l = _AvgWavelength;
	float r = 1.0f / _WaveCount;

	UNITY_LOOP
	for(uint i = 0; i < _WaveCount; i++)
	{
		Wave w;
		float p = lerp(0.5f, 1.5f, i * r);
		w.amplitude = a * p * lerp(0.8f, 1.2f, Random(_randomSeed + i));
		w.direction = d + lerp(-90.0f, 90.0f, Random(_randomSeed + i + 0.1));
		w.wavelength = l * p * lerp(0.6f, 1.4f, Random(_randomSeed + i + 0.2));
		w.omni = 0;
		w.origin = float2(0,0);
		
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