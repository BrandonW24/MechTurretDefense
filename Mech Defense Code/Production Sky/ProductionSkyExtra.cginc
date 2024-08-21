//ProductionSkyExtra.cginc

fixed3 SampleSunColor(fixed3 sunDirection, fixed3 viewDirection, sampler2D tex, float colorScale, float colorCurve)
{
	fixed dotProduct = dot(sunDirection, fixed3(0, 1, 0)) * (1 - colorScale) + colorScale;
	dotProduct = saturate(dotProduct);
	dotProduct *= colorCurve;
	fixed3 transmittance = tex2Dlod(tex, fixed4(dotProduct, 0, 0, 0));

	return transmittance;
}

float NoiseHash(float2 NN)
{
	return frac(sin(dot(NN, float2(12.9898f, 78.233f))) * 43758.5453f);
}

float Noise(float2 p)
{
	float2 i = floor(p);
	float2 u = frac(p);
	u = u * u * (3.0 - 2.0 * u);
	float2 d = float2 (1.0, 0.0);
	
	float r = lerp(lerp(NoiseHash(i), NoiseHash(i + d.xy), u.x), lerp(NoiseHash(i + d.yx), NoiseHash(i + d.xx), u.x), u.y);
	
	return r * r;
}