//ProductionSkyClouds3D.cginc

sampler3D_half _Cloud3D_NoiseTexture;

/*
#ifdef CLOUD3D_SAMPLES_64
	#define _Cloud3D_RaymarchSamples 64
#elif CLOUD3D_SAMPLES_48
	#define _Cloud3D_RaymarchSamples 48
#elif CLOUD3D_SAMPLES_32
	#define _Cloud3D_RaymarchSamples 32
#elif CLOUD3D_SAMPLES_24
	#define _Cloud3D_RaymarchSamples 24
#elif CLOUD3D_SAMPLES_16
	#define _Cloud3D_RaymarchSamples 16
#elif CLOUD3D_SAMPLES_8
	#define _Cloud3D_RaymarchSamples 8
#else
	#define _Cloud3D_RaymarchSamples 24
#endif

#ifdef CLOUD3D_SHADING_SAMPLES_2
	#define _Cloud3D_RaymarchShadingSamples 2
#elif CLOUD3D_SHADING_SAMPLES_4
	#define _Cloud3D_RaymarchShadingSamples 4
#elif CLOUD3D_SHADING_SAMPLES_8
	#define _Cloud3D_RaymarchShadingSamples 8
#elif CLOUD3D_SHADING_SAMPLES_16
	#define _Cloud3D_RaymarchShadingSamples 16
#else
	#define _Cloud3D_RaymarchShadingSamples 4
#endif
*/

#define _Cloud3D_RaymarchSamples 8
#define _Cloud3D_RaymarchShadingSamples 2

float _Cloud3D_Enable;
float _Cloud3D_LightingDirectLightIntensity;
float _Cloud3D_LightingBounceLightIntensity;
float _Cloud3D_LightingAmbientLightIntensity;
float _Cloud3D_LightingAmbientDesaturate;
float _Cloud3D_Height;
float _Cloud3D_Thickness;
float _Cloud3D_Coverage;
float _Cloud3D_Density;
float _Cloud3D_Distance;
float _Cloud3D_LightingPowderOutline;
float _Cloud3D_FrequencyBase;
float _Cloud3D_FrequencyLow;
float _Cloud3D_CoverageBase;
float _Cloud3D_CoverageLow;
float _Cloud3D_FrequencyHigh;
float _Cloud3D_CoverageHigh;
float _Cloud3D_HorizonCurveAmount;
float _Cloud3D_HorizonCurveHeight;
float _Cloud3D_StrengthBase;
float _Cloud3D_StrengthLow;
float _Cloud3D_StrengthHigh;
float _Cloud3D_HorizonFadeAmount;
float _Cloud3D_HorizonFadeHeight;
float _Cloud3D_LightingSunColorScale;
float _Cloud3D_LightingSunColorCurve;
float4 _Cloud3D_BaseOffset;
float4 _Cloud3D_LowOffset;
float4 _Cloud3D_HighOffset;
float4 _Cloud3D_BaseWind;
float4 _Cloud3D_LowWind;
float4 _Cloud3D_HighWind;

float _Cloud3D_Test1;
float _Cloud3D_Test2;

//---------------------3D CLOUDS--------------------------
float SampleClouds(float3 p, float density)
{
	float3 unclampedPosition = p;
	float3 clampedPosition = p;

	float minHeight = _Cloud3D_Height;
	float maxHeight = (_Cloud3D_Thickness + _Cloud3D_Height);

	float time = _Time.x;
	float3 baseOffset = _Cloud3D_BaseOffset.xyz + (_Cloud3D_BaseWind.xyz * time);
	float3 lowOffset = _Cloud3D_LowOffset.xyz + (_Cloud3D_LowWind.xyz * time);
	float3 highOffset = _Cloud3D_HighOffset.xyz + (_Cloud3D_HighWind.xyz * time);

	if (clampedPosition.z < -_Cloud3D_Distance || clampedPosition.z > _Cloud3D_Distance || clampedPosition.x < -_Cloud3D_Distance || clampedPosition.x > _Cloud3D_Distance)
	{
		return 0.0;
	}

	float clouds_baseFrequency = tex3D(_Cloud3D_NoiseTexture, (baseOffset + clampedPosition * 0.00001) * _Cloud3D_FrequencyBase).r;
	float clouds_lowFrequency = tex3D(_Cloud3D_NoiseTexture, (lowOffset + clampedPosition * 0.00001) * _Cloud3D_FrequencyLow).g;
	float clouds_highFrequency = tex3D(_Cloud3D_NoiseTexture, (highOffset + clampedPosition * 0.00001) * _Cloud3D_FrequencyHigh).g;

	clouds_baseFrequency -= _Cloud3D_CoverageBase;
	clouds_lowFrequency -= _Cloud3D_CoverageLow;
	clouds_highFrequency -= _Cloud3D_CoverageHigh;

	clouds_baseFrequency *= _Cloud3D_StrengthBase;
	clouds_lowFrequency *= _Cloud3D_StrengthLow;
	clouds_highFrequency *= _Cloud3D_StrengthHigh;

	float top = 0.0024 * _Cloud3D_Test1;
	float bottom = 0.0001;

	float horizonHeight = p.y - _Cloud3D_Height;
	float treshHold = (1.0 - exp2(-bottom * horizonHeight)) * exp2(-top * (horizonHeight - _Cloud3D_Test2));

	float clouds = clouds_lowFrequency;

	clouds -= saturate(clouds_baseFrequency);
	clouds -= saturate(clouds_highFrequency);
	clouds = saturate(clouds);
	clouds *= treshHold;

	return clouds;
}

float ComputeLightVisibility(float3 p, float3 Sun, float density, float dither, float thickness, float opticalDepth)//direct light
{
	int steps = _Cloud3D_RaymarchShadingSamples;
	float stepSize = thickness / float(steps);
				
	float3 increment = Sun * stepSize;
	float3 position = increment * (1 * opticalDepth) + p;

	float transmittance = 0.0;

	[unroll(_Cloud3D_RaymarchShadingSamples)]
	for (int i = 0; i < steps; i++, position += increment)
	{
		transmittance += SampleClouds(position + dither, density);
	}

	return exp(-transmittance * stepSize);
}

float3 ComputeCloudLighting(float opticalDepth, float3 p, float3 sunColor, float3 skyLight, float3 sun, float density, float dither, float cosTheta, float index, float3 skyTransmittance)//goodies in here
{
	float beerTerm = BeerTerm(_Cloud3D_Density, opticalDepth);
	float powderTerm = PowderTerm(opticalDepth, _Cloud3D_LightingPowderOutline, p, cosTheta);

	float forwardScattering = HGPhase(cosTheta, 0.8f);
	float backScattering = HGPhase(-cosTheta, 0.8f);
	float bounceScattering = HGPhase(cosTheta, 0.7f);

	float mainScattering = (forwardScattering + backScattering);

	float visibility = ComputeLightVisibility(p, sun, density, dither, _Cloud3D_Thickness, opticalDepth);

	float3 directSunlighting = visibility;
	directSunlighting *= (beerTerm * mainScattering) ;
	directSunlighting *= skyTransmittance * sunColor;
	directSunlighting = saturate(directSunlighting);
	directSunlighting *= _Cloud3D_LightingDirectLightIntensity;

	float3 bounceLighting = float3(0.5, 0.5, 0.5);
	bounceLighting *= (beerTerm * bounceScattering);
	bounceLighting *= index / UNITY_PI;
	bounceLighting *= skyTransmittance * sunColor;
	bounceLighting *= _Cloud3D_LightingBounceLightIntensity;

	float3 ambientLighting = float3(0.5, 0.5, 0.5);
	//ambientLighting *= index / UNITY_PI;
	//ambientLighting = pow(ambientLighting, 1.0f / 2.2f);
	//ambientLighting *= clamp(skyLight, 0.001f, 1.0f) * _Cloud3D_LightingAmbientLightIntensity;
	//ambientLighting = lerp(ambientLighting, Luminance(ambientLighting), _Cloud3D_LightingAmbientDesaturate);
	//ambientLighting *= opticalDepth;
	ambientLighting *= (beerTerm);
	//ambientLighting *= index / UNITY_PI;
	//ambientLighting = pow(ambientLighting, 1.0f / 2.2f);
	ambientLighting *= max(0.0f, skyLight) * _Cloud3D_LightingAmbientLightIntensity;
	ambientLighting = lerp(ambientLighting, Luminance(ambientLighting), _Cloud3D_LightingAmbientDesaturate);

	float3 finalLighting = directSunlighting + ambientLighting + bounceLighting;
	finalLighting *= opticalDepth;
	//float3 finalLighting = float3(powderTerm, powderTerm, powderTerm);

	finalLighting = max(0.0f, finalLighting);

	return finalLighting;
}

float3 ComputeVolumetricClouds(float3 sun, float3 worldPos, float3 skyLight, float3 sunColor, float2 uv, out float cloudsAlpha, float density, float dither, float cosTheta, float3 skyTransmittance)
{
	int samples = _Cloud3D_RaymarchSamples;

	float steps = 1.0 / float(samples);

	float3 startPosition = (worldPos * _Cloud3D_Height) / worldPos.y;

	float3 increment = worldPos * _Cloud3D_Thickness / clamp(worldPos.y, 0.1, 1.0) * steps;
	increment *= dither;

	float stepLength = length(increment);

	float3 currCloudPosition = increment + startPosition;
	currCloudPosition *= dither;

	float3 scattering = float3(0, 0, 0);
	float transmittance = 1.0;
	float den = density;
	float test = 0.0f;

	[unroll(_Cloud3D_RaymarchSamples)]
	for (int i = 0; i < samples; i++, currCloudPosition += increment)
	{
		float opticalDepth = SampleClouds(currCloudPosition, den) * stepLength;

		opticalDepth = max(0.0f, opticalDepth);

		//float distanceFactor = distance(worldPos, currCloudPosition);
		//distanceFactor /= _Cloud3D_Distance;

		//distanceFactor = (distanceFactor * _CloudTest3) + _CloudTest4;
		//distanceFactor = saturate(distanceFactor);

		//opticalDepth *= 1 - distanceFactor;

		//cloudLighting = lerp(cloudLighting, skyLight, distanceFactor);

		float3 cloudLighting = ComputeCloudLighting(opticalDepth, currCloudPosition, sunColor, skyLight, sun, den, dither, cosTheta, i, skyTransmittance) * transmittance * BeerTerm(_Cloud3D_Density, opticalDepth);

			//_Cloud3D_Distance

		//test += opticalDepth;
		scattering += cloudLighting;
		transmittance *= BeerTerm(_Cloud3D_Density, opticalDepth);
		//transmittance = lerp(transmittance, 1.0f, distanceFactor);
	}

	//test /= samples;
	cloudsAlpha = max(0.0f, 1.0f - transmittance);
		
	return scattering;
	//return float3(test, test, test);
}