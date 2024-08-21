//ProductionSkyCloudsShared.cginc

struct CloudSkyInfo
{
	float3 skyColor;
	float3 sunColor;
	float3 transmittanceTerm;
	float cosViewSunAngle;
	float dither;
};

float HGPhase(float cosViewSunAngle, float g)
{
	return (1.0 / (4.0 * UNITY_PI)) * ((1.0 - pow(g, 2.0)) / pow(1.0 - 2.0 * g * cosViewSunAngle + pow(g, 2.0), 1.5));
}

float BeerTerm(float density, float densityAtSample)
{
	return exp(-density * densityAtSample);
}

float PowderTerm(float density, float outline, float densityAtSample, float cosTheta)
{
	float powder = 1.0f - exp(-density * densityAtSample * 2.0f);
	powder = saturate(powder * outline * 2.0f);
	return lerp(1.0f, powder, smoothstep(0.5f, -0.5f, cosTheta));
}