//ProductionSkyClouds2D.cginc

sampler2D_half _Cloud2D_NoiseTexture;

float _Cloud2D_Enable;
float _Cloud2D_Density;
float _Cloud2D_Thickness;
float _Cloud2D_Altitude;
float _Cloud2D_DitherAmount;
float _Cloud2D_Distance;
int _Cloud2D_ShadingSamples;

float _Cloud2D_NoiseFrequency;
float _Cloud2D_LowFrequency;
float _Cloud2D_BaseFrequency;
float _Cloud2D_HighFrequency;
float _Cloud2D_NoiseCoverage;
float _Cloud2D_LowCoverage;
float _Cloud2D_BaseCoverage;
float _Cloud2D_HighCoverage;
float _Cloud2D_NoiseStrength;
float _Cloud2D_LowStrength;
float _Cloud2D_BaseStrength;
float _Cloud2D_HighStrength;

float2 _Cloud2D_LowOffset;
float2 _Cloud2D_BaseOffset;
float2 _Cloud2D_HighOffset;

float2 _Cloud2D_LowWind;
float2 _Cloud2D_BaseWind;
float2 _Cloud2D_HighWind;

float _Cloud2D_LightingDirectLightIntensity;
float _Cloud2D_LightingBounceLightIntensity;
float _Cloud2D_LightingAmbientLightIntensity;
float _Cloud2D_LightingAmbientDesaturate;

//---------------------2D CLOUDS--------------------------
float SampleClouds2D(float2 position) 
{
    float time = _Time.x;
    float2 cloudLowPos = (position + _Cloud2D_LowOffset.xy) + float2(time * _Cloud2D_LowWind.x, time * _Cloud2D_LowWind.y);
    float2 cloudBasePos = (position + _Cloud2D_BaseOffset.xy) + float2(time * _Cloud2D_BaseWind.x, time * _Cloud2D_BaseWind.y);
    float2 cloudHighPos = (position + _Cloud2D_HighOffset.xy) + float2(time * _Cloud2D_HighWind.x, time * _Cloud2D_HighWind.y);
    float2 cloudNoisePos = (position + _Cloud2D_HighOffset.xy) + float2(time * _Cloud2D_HighWind.x, time * _Cloud2D_HighWind.y);
    cloudLowPos *= _Cloud2D_LowFrequency;
    cloudBasePos *= _Cloud2D_BaseFrequency;
    cloudHighPos *= _Cloud2D_HighFrequency;
    cloudHighPos *= _Cloud2D_NoiseFrequency;

    float cloud_low = tex2Dlod(_Cloud2D_NoiseTexture, float4(cloudLowPos, 0, 0)).r;
    float cloud_base = tex2Dlod(_Cloud2D_NoiseTexture, float4(cloudBasePos, 0, 0)).r;
    float cloud_high = tex2Dlod(_Cloud2D_NoiseTexture, float4(cloudHighPos, 0, 0)).r;
    float cloud_noise = tex2Dlod(_Cloud2D_NoiseTexture, float4(cloudNoisePos, 0, 0)).g;

    cloud_low = saturate((cloud_low - _Cloud2D_LowCoverage) * _Cloud2D_LowStrength);
    cloud_base = saturate((cloud_base - _Cloud2D_BaseCoverage) * _Cloud2D_BaseStrength);
    cloud_high = saturate((cloud_high - _Cloud2D_HighCoverage) * _Cloud2D_HighStrength);
    cloud_noise = saturate((cloud_noise - _Cloud2D_NoiseCoverage) * _Cloud2D_NoiseStrength);

    cloud_base -= cloud_low;
    cloud_base -= cloud_high;
    cloud_base -= cloud_noise;

    cloud_base = saturate(cloud_base);

    return cloud_base;
}

#define raySteps _Cloud2D_ShadingSamples

void TraceClouds2D(float3 viewPosition, float3 viewDirection, float3 lightDirection, float dither, out float scattering, out float transmittance) 
{
    scattering = 0.0;
    transmittance = 1.0;

    float tPlane = (_Cloud2D_Altitude - viewPosition.y) / viewDirection.y;

    if (tPlane < 0.0) return;

    float2 cloudPosition = viewPosition.xz + viewDirection.xz * tPlane;

    //if (cloudPosition.x > _Cloud2D_Distance || cloudPosition.x < -_Cloud2D_Distance || cloudPosition.y > _Cloud2D_Distance || cloudPosition.y < -_Cloud2D_Distance)
    //    cloudPosition = float2(0,0);

    float cloudFade = 1 - length(cloudPosition * _Cloud2D_Distance);
    float cloudNoise = SampleClouds2D(cloudPosition) * cloudFade;
    cloudNoise *= exp(cloudNoise) * _Cloud2D_Density;

    float cloudOpticalDepth = cloudNoise / abs(viewDirection.y);

    float2 rayStep = float2(0, 0);
    rayStep += (lightDirection.xz / lightDirection.y) / _Cloud2D_Thickness;
    rayStep /= float(raySteps);
    rayStep *= dither;

    float2 rayPosition = cloudPosition + rayStep;

    float lightOpticalDepth = 0.0;

    for (int i = 0; i < raySteps; ++i, rayPosition += rayStep)
    {
        float sampledNoise = SampleClouds2D(rayPosition) * cloudNoise;
        lightOpticalDepth += max(sampledNoise, 0.0) * (raySteps - i);
    }

    lightOpticalDepth /= float(raySteps);

    transmittance = exp(-cloudOpticalDepth);
    scattering = exp(1.0f - lightOpticalDepth) * (1.0f -transmittance);
}

float3 Compute2DClouds(float3 viewPosition, float3 viewDirection, float3 lightDirection, CloudSkyInfo info, out float cloudAlpha)
{
    float scattering = 0.0;
    float transmittance = 0.0;

    if (viewDirection.y <= 0)
        return float3(0,0,0);

    TraceClouds2D(viewPosition, viewDirection, lightDirection, info.dither, scattering, transmittance);

    cloudAlpha = 1.0f - transmittance;
    cloudAlpha = saturate(cloudAlpha);

    float3 finalColor = float3(0, 0, 0);

    float forwardScattering = HGPhase(info.cosViewSunAngle, 0.8f);
    float backScattering = HGPhase(-info.cosViewSunAngle, 0.8f);
    float bounceScattering = HGPhase(info.cosViewSunAngle, 0.7f);

    float mainScattering = (forwardScattering + backScattering);
    float visibility = scattering;

    float3 directLightingTerm = float3(0, 0, 0);
    directLightingTerm += visibility;
    directLightingTerm *= (mainScattering);
    directLightingTerm *= info.transmittanceTerm * info.sunColor;
    directLightingTerm = saturate(directLightingTerm);
    directLightingTerm *= _Cloud2D_LightingDirectLightIntensity;

    float3 bounceLightingTerm = float3(0.5, 0.5, 0.5);
    bounceLightingTerm *= (bounceScattering);
    bounceLightingTerm *= UNITY_PI;
    bounceLightingTerm *= info.transmittanceTerm * info.sunColor;
    bounceLightingTerm *= _Cloud2D_LightingBounceLightIntensity;

    float3 ambientLightingTerm = float3(0, 0, 0);
    ambientLightingTerm += info.skyColor * max(0.0f, 1 - (transmittance));
    ambientLightingTerm = lerp(ambientLightingTerm, Luminance(ambientLightingTerm), _Cloud2D_LightingAmbientDesaturate);
    ambientLightingTerm *= _Cloud2D_LightingAmbientLightIntensity;

    finalColor += directLightingTerm;
    finalColor += bounceLightingTerm;
    finalColor += ambientLightingTerm;

    return finalColor;
}