//ProductionSky.cginc

#include "UnityCG.cginc"

#ifdef COMPUTE_SHADER
	RWTexture2D<float4> SkyTransmittanceWrite;
	RWTexture2D<float4> SkyMultipleScatteringWrite;
	RWTexture2D<float4> SkyViewWrite;
	Texture2D<float4> SkyTransmittanceRead;
	Texture2D<float4> SkyMultipleScatteringRead;
	Texture2D<float4> SkyViewRead;

	#define TEX2D tex2D_ComputeShader
	#define SAMPLER2D Texture2D<float4>
#else
	sampler2D_half SkyMultipleScatteringRead;
    sampler2D_half SkyTransmittanceRead;
    sampler2D_half SkyViewRead;
	
	#define TEX2D tex2Dlod
	#define SAMPLER2D sampler2D_half
#endif

SamplerState _PointClamp;
SamplerState _LinearClamp;

float4 TEX2D(Texture2D<float4> tex, float2 uv)
{
    return tex.SampleLevel(_LinearClamp, uv, 0);
}

//------------------- variables
float skyGroundRadiusMM;
float skyAtmosphereRadiusMM;
float skyRayleighAbsorptionBase;
float skyMieScatteringBase;
float skyMieAbsorptionBase;

float skyTransmittanceSteps;
float skyMultipleScatteringSteps;
int skyMultipleScatteringSquaredSamples;
int skyRaymarchSteps;
int skyViewLUTRaymarchSteps;

float2 skyTransmittanceResolution;
float2 skyMultipleScatteringResolution;

float3 skyGroundAlbedo;
float3 skyRayleighScatteringBase;
float3 skyOzoneAbsorptionBase;

//||||||||||||||||||||||||||||||||||||||||||||| COMMON |||||||||||||||||||||||||||||||||||||||||||||
//||||||||||||||||||||||||||||||||||||||||||||| COMMON |||||||||||||||||||||||||||||||||||||||||||||
//||||||||||||||||||||||||||||||||||||||||||||| COMMON |||||||||||||||||||||||||||||||||||||||||||||

float getMiePhase(float cosTheta) 
{
    float g = 0.8;
    float scale = 3.0 / (8.0 * UNITY_PI);

    float num = (1.0 - g * g) * (1.0 + cosTheta * cosTheta);
    float denom = (2.0 + g * g) * pow((1.0 + g * g - 2.0 * g * cosTheta), 1.5);

    return scale * num / denom;
}

float getRayleighPhase(float cosTheta) 
{
    float k = 3.0 / (16.0 * UNITY_PI);
    return k * (1.0 + cosTheta * cosTheta);
}

void getScatteringValues(float3 pos, out float3 rayleighScattering, out float mieScattering, out float3 extinction)
{
    float altitudeKM = (length(pos) - skyGroundRadiusMM) * 1000.0;

    float rayleighDensity = exp(-altitudeKM / 8.0);
    float mieDensity = exp(-altitudeKM / 1.2);

    rayleighScattering = skyRayleighScatteringBase * rayleighDensity;
    float rayleighAbsorption = skyRayleighAbsorptionBase * rayleighDensity;

    mieScattering = skyMieScatteringBase * mieDensity;
    float mieAbsorption = skyMieAbsorptionBase * mieDensity;

    float3 ozoneAbsorption = skyOzoneAbsorptionBase * max(0.0, 1.0 - abs(altitudeKM - 25.0) / 15.0);

    extinction = rayleighScattering + rayleighAbsorption + mieScattering + mieAbsorption + ozoneAbsorption;
}

float safeacos(const float x) 
{
    return acos(clamp(x, -1.0, 1.0));
}


// From https://gamedev.stackexchange.com/questions/96459/fast-ray-sphere-collision-code.
float rayIntersectSphere(float3 ro, float3 rd, float rad)
{
    float b = dot(ro, rd);
    float c = dot(ro, ro) - rad * rad;

    if (c > 0.0f && b > 0.0) 
        return -1.0;

    float discr = b * b - c;

    if (discr < 0.0) 
        return -1.0;

    if (discr > b * b) 
        return (-b + sqrt(discr));

    return -b - sqrt(discr);
}

// From https://gamedev.stackexchange.com/questions/96459/fast-ray-sphere-collision-code.
float rayIntersectSphere1(float3 ro, float3 rd, float rad)
{
    float b = dot(ro, rd);
    float c = dot(ro, ro) - rad * rad;

    if (c > 0.0f && b > 0.0) 
        return -1.0;

    float discr = b * b - c;

    if (discr < 0.0) 
        return -1.0;

    return -b - sqrt(discr);
}

// From https://gamedev.stackexchange.com/questions/96459/fast-ray-sphere-collision-code.
float rayIntersectSphere2(float3 ro, float3 rd, float rad)
{
    float b = dot(ro, rd);
    float c = dot(ro, ro) - rad * rad;

    if (c > 0.0f && b > 0.0) 
        return -1.0;

    float discr = b * b - c;

    if (discr > 0.0) 
        return (-b + sqrt(discr));

    if (discr < b * b) 
        return -1.0;

    return -b - sqrt(discr);
}

float3 getValFromTLUT(SAMPLER2D tex, float2 bufferRes, float3 pos, float3 sunDir)
{
    float height = length(pos);
    float3 up = pos / height;
    float sunCosZenithAngle = dot(sunDir, up);
    float2 uv = float2(skyTransmittanceResolution.x * clamp(0.5 + 0.5 * sunCosZenithAngle, 0.0, 1.0), skyTransmittanceResolution.y * max(0.0, min(1.0, (height - skyGroundRadiusMM) / (skyAtmosphereRadiusMM - skyGroundRadiusMM))));

    uv /= bufferRes;

#ifdef COMPUTE_SHADER
    return TEX2D(tex, uv).rgb;
#else
    return TEX2D(tex, float4(uv, 0, 0)).rgb;
#endif
}

float3 getValFromMultiScattLUT(SAMPLER2D tex, float2 bufferRes, float3 pos, float3 sunDir)
{
    float height = length(pos);
    float3 up = pos / height;
    float sunCosZenithAngle = dot(sunDir, up);
    float2 uv = float2(skyMultipleScatteringResolution.x * clamp(0.5 + 0.5 * sunCosZenithAngle, 0.0, 1.0), skyMultipleScatteringResolution.y * max(0.0, min(1.0, (height - skyGroundRadiusMM) / (skyAtmosphereRadiusMM - skyGroundRadiusMM))));

    uv /= bufferRes;

#ifdef COMPUTE_SHADER
    return TEX2D(tex, uv).rgb;
#else
    return TEX2D(tex, float4(uv, 0, 0)).rgb;
#endif
}

//||||||||||||||||||||||||||||||||||||||||||||| TRANSMITTANCE |||||||||||||||||||||||||||||||||||||||||||||
//||||||||||||||||||||||||||||||||||||||||||||| TRANSMITTANCE |||||||||||||||||||||||||||||||||||||||||||||
//||||||||||||||||||||||||||||||||||||||||||||| TRANSMITTANCE |||||||||||||||||||||||||||||||||||||||||||||
// Generates the Transmittance LUT. Each pixel coordinate corresponds to a height and sun zenith angle, and the value is the transmittance from that point to sun, through the atmosphere.

float3 getSunTransmittance(float3 pos, float3 sunDir)
{
    if (rayIntersectSphere(pos, sunDir, skyGroundRadiusMM) > 0.0) 
    {
        return float3(0, 0, 0);
    }

    float atmoDist = rayIntersectSphere(pos, sunDir, skyAtmosphereRadiusMM);
    float t = 0.0;

    float3 transmittance = float3(1, 1, 1);

    for (float i = 0.0; i < skyTransmittanceSteps; i += 1.0) 
    {
        float newT = ((i + 0.3) / skyTransmittanceSteps) * atmoDist;
        float dt = newT - t;
        t = newT;

        float3 newPos = pos + t * sunDir;

        float3 rayleighScattering;
        float3 extinction;
        float mieScattering;
        getScatteringValues(newPos, rayleighScattering, mieScattering, extinction);

        transmittance *= exp(-dt * extinction);
    }

    return transmittance;
}

//||||||||||||||||||||||||||||||||||||||||||||| MULTIPLE SCATTERING |||||||||||||||||||||||||||||||||||||||||||||
//||||||||||||||||||||||||||||||||||||||||||||| MULTIPLE SCATTERING |||||||||||||||||||||||||||||||||||||||||||||
//||||||||||||||||||||||||||||||||||||||||||||| MULTIPLE SCATTERING |||||||||||||||||||||||||||||||||||||||||||||
// Generates multiple-scattering LUT. Each pixel coordinate corresponds to a height and sun zenith angle, and the value is the multiple scattering approximation

float3 getSphericalDir(float theta, float phi) 
{
    float cosPhi = cos(phi);
    float sinPhi = sin(phi);
    float cosTheta = cos(theta);
    float sinTheta = sin(theta);
    return float3(sinPhi * sinTheta, cosPhi, sinPhi * cosTheta);
}

// Calculates Equation (5) and (7) from the paper.
void getMulScattValues(float3 pos, float3 sunDir, out float3 lumTotal, out float3 fms) 
{
    lumTotal = float3(0, 0, 0);
    fms = float3(0, 0, 0);

    float invSamples = 1.0 / float(skyMultipleScatteringSquaredSamples * skyMultipleScatteringSquaredSamples);

    for (int i = 0; i < skyMultipleScatteringSquaredSamples; i++) 
    {
        for (int j = 0; j < skyMultipleScatteringSquaredSamples; j++) 
        {
            // This integral is symmetric about theta = 0 (or theta = PI), so we
            // only need to integrate from zero to PI, not zero to 2*PI.
            float theta = UNITY_PI * (float(i) + 0.5) / float(skyMultipleScatteringSquaredSamples);
            float phi = safeacos(1.0 - 2.0 * (float(j) + 0.5) / float(skyMultipleScatteringSquaredSamples));
            float3 rayDir = getSphericalDir(theta, phi);

            float atmoDist = rayIntersectSphere(pos, rayDir, skyAtmosphereRadiusMM);
            float groundDist = rayIntersectSphere(pos, rayDir, skyGroundRadiusMM);
            float tMax = atmoDist;

            if (groundDist > 0.0) 
            {
                tMax = groundDist;
            }

            float cosTheta = dot(rayDir, sunDir);

            float miePhaseValue = getMiePhase(cosTheta);
            float rayleighPhaseValue = getRayleighPhase(-cosTheta);

            float3 lum = float3(0, 0, 0);
            float3 lumFactor = float3(0, 0, 0);
            float3 transmittance = float3(1, 1, 1);
            float t = 0.0;

            for (float stepI = 0.0; stepI < skyMultipleScatteringSteps; stepI += 1.0) 
            {
                float newT = ((stepI + 0.3) / skyMultipleScatteringSteps) * tMax;
                float dt = newT - t;
                t = newT;

                float3 newPos = pos + t * rayDir;

                float3 rayleighScattering;
                float3 extinction;
                float mieScattering;
                getScatteringValues(newPos, rayleighScattering, mieScattering, extinction);

                float3 sampleTransmittance = exp(-dt * extinction);

                // Integrate within each segment.
                float3 scatteringNoPhase = rayleighScattering + mieScattering;
                float3 scatteringF = (scatteringNoPhase - scatteringNoPhase * sampleTransmittance) / extinction;
                lumFactor += transmittance * scatteringF;



                // This is slightly different from the paper, but I think the paper has a mistake?
                // In equation (6), I think S(x,w_s) should be S(x-tv,w_s).
                float3 sunTransmittance = getValFromTLUT(SkyTransmittanceRead, skyTransmittanceResolution.xy, newPos, sunDir);

                float3 rayleighInScattering = rayleighScattering * rayleighPhaseValue;
                float mieInScattering = mieScattering * miePhaseValue;
                float3 inScattering = (rayleighInScattering + mieInScattering) * sunTransmittance;

                // Integrated scattering within path segment.
                float3 scatteringIntegral = (inScattering - inScattering * sampleTransmittance) / extinction;

                lum += scatteringIntegral * transmittance;
                transmittance *= sampleTransmittance;
            }

            if (groundDist > 0.0) 
            {
                float3 hitPos = pos + groundDist * rayDir;

                if (dot(pos, sunDir) > 0.0) 
                {
                    hitPos = normalize(hitPos) * skyGroundRadiusMM;
                    lum += transmittance * skyGroundAlbedo * getValFromTLUT(SkyTransmittanceRead, skyTransmittanceResolution.xy, hitPos, sunDir);
                }
            }

            fms += lumFactor * invSamples;
            lumTotal += lum * invSamples;
        }
    }
}


//||||||||||||||||||||||||||||||||||||||||||||| MAIN SHADER |||||||||||||||||||||||||||||||||||||||||||||
//||||||||||||||||||||||||||||||||||||||||||||| MAIN SHADER |||||||||||||||||||||||||||||||||||||||||||||
//||||||||||||||||||||||||||||||||||||||||||||| MAIN SHADER |||||||||||||||||||||||||||||||||||||||||||||

float3 skyRaymarchScattering(float3 pos, float3 rayDir, float3 sunDir, float tMax, float numSteps, out float3 transmittance)
{
	float cosTheta = dot(rayDir, sunDir);

	float miePhaseValue = getMiePhase(cosTheta);
	float rayleighPhaseValue = getRayleighPhase(-cosTheta);

	float3 lum = float3(0, 0, 0);
	transmittance = float3(1, 1, 1);
	float t = 0.0;

	for (float i = 0.0; i < numSteps; i += 1.0)
	{
		float newT = ((i + 0.3) / numSteps) * tMax;
		float dt = newT - t;
		t = newT;

		float3 newPos = pos + t * rayDir;

		float3 rayleighScattering;
		float3 extinction;
		float mieScattering;
		getScatteringValues(newPos, rayleighScattering, mieScattering, extinction);

		float3 sampleTransmittance = exp(-dt * extinction);

		float3 sunTransmittance = getValFromTLUT(SkyTransmittanceRead, skyTransmittanceResolution.xy, newPos, sunDir);
		float3 psiMS = getValFromMultiScattLUT(SkyMultipleScatteringRead, skyMultipleScatteringResolution.xy, newPos, sunDir);

		float3 rayleighInScattering = rayleighScattering * (rayleighPhaseValue * sunTransmittance + psiMS);
		float3 mieInScattering = mieScattering * (miePhaseValue * sunTransmittance + psiMS);
		float3 inScattering = (rayleighInScattering + mieInScattering);

		// Integrated scattering within path segment.
		float3 scatteringIntegral = (inScattering - inScattering * sampleTransmittance) / extinction;

		lum += scatteringIntegral * transmittance;

		transmittance *= sampleTransmittance;
	}

	return lum;
}