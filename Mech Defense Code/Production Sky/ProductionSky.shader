Shader "ProductionSky"
{
	Properties
	{
		_Exposure("Exposure", Float) = 20

		[Header(Sky Precomputed Textures)]
		SkyMultipleScatteringRead("Multiple Scattering Texture", 2D) = "black" {}
		SkyTransmittanceRead("Transmittance Texture", 2D) = "black" {}

		[Header(Sky Properties)]
		[KeywordEnum(Compute Per Vertex, Compute Per Pixel)] _Sky_EnableHighQuality("Sky Dynamic Quality", Float) = 1
		_Sky_FixedCameraPosition("Fixed Camera Position", Vector) = (0,0.0005,0,0)
		_Sky_SunRadius("Sun Radius", Float) = 0.01
		_Sky_SunIntensity("Sun Intensity", Float) = 25

		[Header(Sky Night Properties)]
		[MaterialToggle] _Sky_Night_Enable("Enable Night Sky", Float) = 1
		[KeywordEnum(Compute Per Vertex, Compute Per Pixel)] _Sky_Night_EnableHighQuality("Night Sky Quality", Float) = 1
		_Sky_Night_Intensity("Night Sky Intensity", Float) = 61.93
		_Sky_Night_MoonDirection("Moon Direction", Vector) = (0,0,1,1)
		_Sky_Night_MoonIntensity("Moon Intensity", Float) = 61.93
		_Sky_Night_MoonRadius("Moon Radius", Float) = 61.93
		[MaterialToggle] _Sky_Night_MoonUseSunDirection("Use Reverse Sun Position For Moon", Float) = 1

		[Header(Static Sky Properties)]
		[MaterialToggle] _Sky_UseStaticCubemap("Use Static Cubemap (Faster)", Float) = 1
		_Sky_Static_FinalCubemap("Final Sky Cubemap", CUBE) = "white" {}
		_Sky_Static_TransmittanceCubemap("Transmittance Cubemap", CUBE) = "white" {}

		[Header(Cloud 3D Precomputed Textures)]
		[MaterialToggle] _Cloud3D_Enable("Enable 3D Clouds", Float) = 1
		_Cloud3D_NoiseTexture("Cloud Noise", 3D) = "black" {}

		[Header(Cloud 3D Shape Properties)]
		_Cloud3D_Height("Cloud Height", Float) = 4321.07
		_Cloud3D_Thickness("Cloud Thickness", Float) = 1523.31
		_Cloud3D_Density("Cloud Density", Float) = 0.17
		_Cloud3D_Distance("Cloud Distance", Float) = 327446
		_Cloud3D_FrequencyBase("Cloud Base Frequency", Float) = 2.35
		_Cloud3D_FrequencyLow("Cloud Low Frequency", Float) = 0.1
		_Cloud3D_FrequencyHigh("Cloud High Frequency", Float) = 4.73
		_Cloud3D_CoverageBase("Cloud Coverage Base", Float) = 0.3
		_Cloud3D_CoverageLow("Cloud Coverage Low", Float) = 0.41
		_Cloud3D_CoverageHigh("Cloud Coverage High", Float) = 0.37
		_Cloud3D_StrengthBase("Cloud Strength Base", Float) = 1.05
		_Cloud3D_StrengthLow("Cloud Strength Low", Float) = 2.43
		_Cloud3D_StrengthHigh("Cloud Strength High", Float) = 0.38
		_Cloud3D_BaseOffset("Cloud Base Position Offset", Vector) = (0,0,-0.17,0)
		_Cloud3D_LowOffset("Cloud Low Position Offset", Vector) = (0.34,0.18,0,0)
		_Cloud3D_HighOffset("Cloud High Position Offset", Vector) = (-5.19,0.52,-1.93,0)
		_Cloud3D_BaseWind("Cloud Base Wind", Vector) = (0.02,0,0,0)
		_Cloud3D_LowWind("Cloud Low Wind", Vector) = (0.01,0,0,0)
		_Cloud3D_HighWind("Cloud High Wind", Vector) = (0.015,-0.015,0,0)
		_Cloud3D_Test1("TEST 1", Float) = 1.4
		_Cloud3D_Test2("TEST 2", Float) = 334.12

		[Header(Cloud 3D Lighting Properties)]
		_Cloud3D_LightingSunColorScale("Sun Color Scale", Float) = 0.32
		_Cloud3D_LightingSunColorCurve("Sun Color Curve", Float) = 1.63
		_Cloud3D_LightingDirectLightIntensity("Sun Intensity", Float) = 1.76
		_Cloud3D_LightingBounceLightIntensity("Sun Bounce Intensity", Float) = 0.05
		_Cloud3D_LightingAmbientLightIntensity("Ambient Intensity", Float) = 0.73
		_Cloud3D_LightingAmbientDesaturate("Ambient Desaturation", Range(0.0,1.0)) = 0.851
		_Cloud3D_LightingPowderOutline("Powder Outline", Float) = 0.6

		[Header(Cloud 3D Display Properties)]
		_Cloud3D_HorizonCurveAmount("Cloud Horizon Curve Amount", Float) = -0.09
		_Cloud3D_HorizonCurveHeight("Cloud Horizon Curve Height", Float) = 0.12
		_Cloud3D_HorizonFadeAmount("Cloud Horizon Fade Amount", Float) = 10.03
		_Cloud3D_HorizonFadeHeight("Cloud Horizon Fade Height", Float) = 0.07
		_CloudDitherAmount("Dither Amount", Float) = 0.02

		[Header(Cloud 2D Precomputed Textures)]
		[MaterialToggle] _Cloud2D_Enable("Enable 2D Clouds", Float) = 1
		_Cloud2D_NoiseTexture("Cloud Noise", 2D) = "black" {}

		[Header(Cloud 2D Properties)]
		_Cloud2D_Density("Density", Float) = 1
		_Cloud2D_Thickness("Thickness", Float) = 1
		_Cloud2D_Altitude("Altitude", Float) = 1
		_Cloud2D_Distance("Distance", Float) = 1
		_Cloud2D_DitherAmount("Dither", Float) = 1
		_Cloud2D_LowFrequency("Low Frequency", Float) = 1
		_Cloud2D_BaseFrequency("Base Frequency", Float) = 1
		_Cloud2D_HighFrequency("High Frequency", Float) = 1
		_Cloud2D_NoiseFrequency("Noise Frequency", Float) = 1
		_Cloud2D_LowCoverage("Low Coverage", Float) = 1
		_Cloud2D_BaseCoverage("Base Coverage", Float) = 1
		_Cloud2D_HighCoverage("High Coverage", Float) = 1
		_Cloud2D_NoiseCoverage("Noise Coverage", Float) = 1
		_Cloud2D_LowStrength("Low Strength", Float) = 1
		_Cloud2D_BaseStrength("Base Strength", Float) = 1
		_Cloud2D_HighStrength("High Strength", Float) = 1
		_Cloud2D_NoiseStrength("Noise Strength", Float) = 1
		_Cloud2D_LowOffset("Low Offset", Vector) = (0,0,0,0)
		_Cloud2D_BaseOffset("Base Offset", Vector) = (0,0,0,0)
		_Cloud2D_HighOffset("High Offset", Vector) = (0,0,0,0)
		_Cloud2D_LowWind("Low Wind", Vector) = (0,0,0,0)
		_Cloud2D_BaseWind("Base Wind", Vector) = (0,0,0,0)
		_Cloud2D_HighWind("High Wind", Vector) = (0,0,0,0)

		[Header(Cloud 2D Lighting Properties)]
		_Cloud2D_ShadingSamples("Shading Samples", Range(2,32)) = 8
		_Cloud2D_LightingDirectLightIntensity("Direct Intensity", Float) = 1
		_Cloud2D_LightingBounceLightIntensity("Bounce Intensity", Float) = 1
		_Cloud2D_LightingAmbientLightIntensity("Ambient Intensity", Float) = 1
		_Cloud2D_LightingAmbientDesaturate("Ambient Desaturation", Range(0.0,1.0)) = 0.851

		[HideInInspector] skyGroundRadiusMM("Ground Radius MM", Float) = 6.36
		[HideInInspector] skyAtmosphereRadiusMM("Atmosphere Radius MM", Float) = 6.42
		[HideInInspector] skyRayleighAbsorptionBase("Rayleigh Absorption Base", Float) = 0
		[HideInInspector] skyMieScatteringBase("Mie Scatterin gBase", Float) = 29.3
		[HideInInspector] skyMieAbsorptionBase("Mie Absorption Base", Float) = 4.4
		[HideInInspector] sunTransmittanceSteps("Transmittance Steps", Int) = 40
		[HideInInspector] skyMultipleScatteringSteps("Multiple Scattering Steps", Int) = 20
		[HideInInspector] skyRaymarchSteps("Raymarch Steps", Int) = 20
		[HideInInspector] skyMultipleScatteringSquaredSamples("Multiple Scattering Squared Samples", Int) = 8
		[HideInInspector] skyTransmittanceResolution("Transmittance Resolution", Vector) = (256,64,0,0)
		[HideInInspector] skyMultipleScatteringResolution("Multiple Scattering Resolution", Vector) = (32,32,0,0)
		[HideInInspector] skyGroundAlbedo("Ground Albedo", Vector) = (0.3,0.3,0.3,0)
		[HideInInspector] skyRayleighScatteringBase("Rayleigh Scattering Base", Vector) = (5.802,13.558,33.1,0)
		[HideInInspector] skyOzoneAbsorptionBase("Ozone Absorption Base", Vector) = (0.65,1.881,0.085,-8.1)

		[HideInInspector] _Sky_Night_KelvinTempTint("Night Sky Kelvin Tint", Color) = (1,1,1,1)
		[HideInInspector] nightSkyRaymarchSteps("Night Sky Raymarch Steps", Int) = 20
	}
	SubShader
	{
		Tags{ "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }

		Cull Off ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl
			//#pragma multi_compile CLOUD3D_SAMPLES_8
			//#pragma multi_compile CLOUD3D_SHADING_SAMPLES_2
			//#pragma multi_compile CLOUD3D_SAMPLES_8 CLOUD3D_SAMPLES_16 CLOUD3D_SAMPLES_24 CLOUD3D_SAMPLES_32 CLOUD3D_SAMPLES_48 CLOUD3D_SAMPLES_64
			//#pragma multi_compile CLOUD3D_SHADING_SAMPLES_2 CLOUD3D_SHADING_SAMPLES_4 CLOUD3D_SHADING_SAMPLES_8 CLOUD3D_SHADING_SAMPLES_16

			//#define CLOUD3D_SAMPLES_8
			//#define CLOUD3D_SHADING_SAMPLES_2
			
			#include "ProductionSky.cginc" //includes UnityCG.cginc
			#include "ProductionSkyExtra.cginc"
			#include "ProductionSkyCloudsShared.cginc"
			#include "ProductionSkyClouds3D.cginc"
			#include "ProductionSkyClouds2D.cginc"
			#include "CloudNoise.cginc"

			float _Exposure;
			float _SkyExposure;
			float _Sky_SunRadius;
			float _Sky_SunIntensity;
			float3 _Sky_FixedCameraPosition;
			float _Sky_EnableHighQuality;

			float _Sky_Night_Enable;
			float _Sky_Night_EnableHighQuality;
			float _Sky_Night_Intensity;
			float3 _Sky_Night_KelvinTempTint;
			float3 _Sky_Night_MoonDirection;
			float _Sky_Night_MoonIntensity;
			float _Sky_Night_MoonRadius;
			float _Sky_Night_MoonUseSunDirection;

			float _Sky_ShowTransmittanceTerm;

			float _Sky_UseStaticCubemap;
			samplerCUBE	_Sky_Static_FinalCubemap;
			samplerCUBE	_Sky_Static_TransmittanceCubemap;

			float nightSkyRaymarchSteps;

			float _CloudDitherAmount;

			struct appdata_t
			{
				fixed4 vertex : POSITION;
				fixed2 tex : TEXCOORD;

				//instancing support
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				fixed4 pos : SV_POSITION;
				fixed3 vertex : TEXCOORD0;
				fixed2 uv : TEXCOORD1;
				fixed4 posWorld : TEXCOORD2;
				fixed3 viewDir : TEXCOORD3;
				fixed3 skyColorVertex : TEXCOORD4;
				fixed3 skyColorVertex_Transmittance : TEXCOORD5;
				fixed3 skyDistance : TEXCOORD6;
				fixed3 skyNightColorVertex : TEXCOORD7;

				//instancing support
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_t v)
			{
				v2f OUT;

				//instancing support
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, OUT);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				OUT.pos = UnityObjectToClipPos(v.vertex);
				OUT.vertex = -v.vertex;
				OUT.posWorld = mul(unity_ObjectToWorld, v.vertex);
				fixed3 eyeRay = normalize(mul((fixed3x3)unity_ObjectToWorld, v.vertex.xyz));
				OUT.viewDir = fixed3(-eyeRay);
				OUT.uv = v.tex;

				OUT.skyDistance = fixed3(0, 0, 0);
				OUT.skyColorVertex = fixed3(0, 0, 0);
				OUT.skyColorVertex_Transmittance = fixed3(0, 0, 0);
				OUT.skyNightColorVertex = fixed3(0, 0, 0);

				fixed3 vector_sunDirection = fixed3(0, 0, 0);
				fixed3 vector_moonDirection = fixed3(0, 0, 0);
				fixed3 vector_viewDirection = fixed3(0, 0, 0);
				fixed3 skyViewPosition = fixed3(0, 0, 0);
				fixed atmoDist = 0.0;
				fixed groundDist = 0.0;
				fixed tMax = 0.0;
				fixed3 skyTransmittance = fixed3(0, 0, 0);
				fixed3 skyColor = fixed3(0, 0, 0);

				//if we are not using a static cubemap to shade the sky
				if (_Sky_UseStaticCubemap < 1)
				{
					//if we are shading either day or night per vertex, we need to calculate some stuff
					if (_Sky_EnableHighQuality < 1 || _Sky_Night_EnableHighQuality < 1)
					{
						vector_sunDirection = normalize(_WorldSpaceLightPos0.xyz);
						vector_moonDirection = _Sky_Night_MoonUseSunDirection ? normalize(-_WorldSpaceLightPos0.xyz) : normalize(_Sky_Night_MoonDirection);
						vector_viewDirection = -normalize(mul((fixed3x3)unity_ObjectToWorld, OUT.vertex));
						skyViewPosition = _Sky_FixedCameraPosition + fixed3(0, skyGroundRadiusMM, 0);
						atmoDist = rayIntersectSphere2(skyViewPosition, vector_viewDirection, skyAtmosphereRadiusMM);
						groundDist = rayIntersectSphere1(skyViewPosition, vector_viewDirection, skyGroundRadiusMM);
						tMax = (groundDist < 0.0) ? atmoDist : groundDist;
						tMax = max(0.0f, tMax);

						OUT.skyDistance = fixed3(atmoDist, groundDist, tMax);
					}

					//rendering the sky per vertex
					if (_Sky_EnableHighQuality < 1)
					{
						OUT.skyColorVertex = skyRaymarchScattering(skyViewPosition, vector_viewDirection, vector_sunDirection, tMax, fixed(skyRaymarchSteps), skyTransmittance);
						OUT.skyColorVertex_Transmittance = skyTransmittance;
					}

					if (_Sky_Night_Enable > 0)
					{
						//rendering the night sky per vertex
						if (_Sky_Night_EnableHighQuality < 1)
						{
							OUT.skyNightColorVertex = skyRaymarchScattering(skyViewPosition, vector_viewDirection, vector_moonDirection, tMax, fixed(nightSkyRaymarchSteps), skyTransmittance);
						}
					}
				}

				return OUT;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed3 vector_cameraPosition = _WorldSpaceCameraPos;
				fixed3 vector_sunDirection = normalize(_WorldSpaceLightPos0.xyz);
				fixed3 vector_moonDirection = _Sky_Night_MoonUseSunDirection ? normalize(-_WorldSpaceLightPos0.xyz) : normalize(_Sky_Night_MoonDirection);
				fixed3 vector_viewDirection = -normalize(mul((fixed3x3)unity_ObjectToWorld, i.vertex));
				fixed2 vector_uv = i.uv.xy;
				fixed3 skyViewPosition = _Sky_FixedCameraPosition + fixed3(0, skyGroundRadiusMM, 0);

				fixed dither = lerp(1.0f, noise(vector_viewDirection * 1000000.0f), _CloudDitherAmount);
				fixed cosTheta = dot(vector_viewDirection, vector_sunDirection);
				cosTheta = max(0.0f, cosTheta);

				fixed atmoDist;
				fixed groundDist;
				fixed tMax;

				fixed cloud3DAlpha = 0.0f;
				fixed cloud2DAlpha = 0.0f;
				fixed3 skyTransmittance;
				fixed3 skyColor;
				fixed3 sunColor = SampleSunColor(vector_sunDirection, vector_viewDirection, SkyTransmittanceRead, _Cloud3D_LightingSunColorScale, _Cloud3D_LightingSunColorCurve);
				fixed3 finalSkyColor = float3(0,0,0);

				//Calculate Sky

				//if we are just using a static cubemap to shade the sky
				if (_Sky_UseStaticCubemap > 0)
				{
					float3 finalStaticCubemap = texCUBElod(_Sky_Static_FinalCubemap, float4(vector_viewDirection.xyz, 0)).rgb;
					float3 transmittanceStaticCubemap = texCUBElod(_Sky_Static_TransmittanceCubemap, float4(vector_viewDirection.xyz, 0)).rgb;

					finalSkyColor = finalStaticCubemap;
					skyColor = finalStaticCubemap;
					skyTransmittance = transmittanceStaticCubemap;
					sunColor = sunColor;

					groundDist = -1.0f;
				}
				else
				{
					//if we are shading the sky dynamically

					if (_Sky_EnableHighQuality > 0)
					{
						atmoDist = rayIntersectSphere2(skyViewPosition, vector_viewDirection, skyAtmosphereRadiusMM);
						groundDist = rayIntersectSphere1(skyViewPosition, vector_viewDirection, skyGroundRadiusMM);
						tMax = (groundDist < 0.0) ? atmoDist : groundDist;
						tMax = max(0.0f, tMax);

						skyColor = skyRaymarchScattering(skyViewPosition, vector_viewDirection, vector_sunDirection, tMax, fixed(skyRaymarchSteps), skyTransmittance);
					}
					else
					{
						atmoDist = i.skyDistance.x;
						groundDist = i.skyDistance.y;
						tMax = i.skyDistance.z;
						skyColor = i.skyColorVertex.xyz;
						skyTransmittance = i.skyColorVertex_Transmittance.xyz;
					}

					finalSkyColor += skyColor;

					//if night sky is enabled
					if (_Sky_Night_Enable > 0)
					{
						//Calculate Night Sky
						if (_Sky_Night_EnableHighQuality > 0)
						{
							atmoDist = rayIntersectSphere2(skyViewPosition, vector_viewDirection, skyAtmosphereRadiusMM);
							groundDist = rayIntersectSphere1(skyViewPosition, vector_viewDirection, skyGroundRadiusMM);
							tMax = (groundDist < 0.0) ? atmoDist : groundDist;
							tMax = max(0.0f, tMax);

							skyColor += skyRaymarchScattering(skyViewPosition, vector_viewDirection, vector_moonDirection, tMax, fixed(nightSkyRaymarchSteps), skyTransmittance) * _Sky_Night_Intensity * _Sky_Night_KelvinTempTint;
						}
						else
						{
							skyColor += i.skyNightColorVertex.rgb * _Sky_Night_Intensity * _Sky_Night_KelvinTempTint;
						}
					}
				}

				CloudSkyInfo info2D;
				info2D.skyColor = skyColor;
				info2D.sunColor = sunColor;
				info2D.transmittanceTerm = skyTransmittance;
				info2D.cosViewSunAngle = cosTheta;
				info2D.dither = lerp(1.0f, noise(vector_viewDirection * 1000000.0f), _Cloud2D_DitherAmount);

				//Calculate 2D Clouds
				if (_Cloud2D_Enable > 0)
				{
					float3 clouds2D = Compute2DClouds(float3(0, 0, 0), vector_viewDirection, vector_sunDirection, info2D, cloud2DAlpha);

					finalSkyColor = lerp(finalSkyColor, clouds2D, cloud2DAlpha);

					cloud3DAlpha += cloud2DAlpha;
				}

				//Calculate Volumetric Clouds
				fixed offsetedViewDirection = saturate(dot(-vector_viewDirection, fixed3(0, 1, 0)) + _Cloud3D_HorizonCurveHeight) * _Cloud3D_HorizonCurveAmount;
				fixed3 newViewDir = fixed3(vector_viewDirection.x, vector_viewDirection.y + offsetedViewDirection, vector_viewDirection.z);

				if (_Cloud3D_Enable > 0)
				{
					fixed skyIntensity = saturate(dot(vector_sunDirection, fixed3(0, 1, 0)));

					fixed3 clouds = ComputeVolumetricClouds(vector_sunDirection, newViewDir, skyColor * skyIntensity, sunColor, vector_uv, cloud3DAlpha, _Cloud3D_Density, dither, cosTheta, skyTransmittance);

					cloud3DAlpha -= saturate(dot(fixed3(0, -1.0f, 0), vector_viewDirection) + _Cloud3D_HorizonFadeHeight) * _Cloud3D_HorizonFadeAmount;
					cloud3DAlpha = saturate(cloud3DAlpha);

					finalSkyColor = lerp(finalSkyColor, clouds, cloud3DAlpha);
				}

				//compute sun disk
				//make sure the sun does not show up over the ground or clouds
				if (dot(vector_viewDirection, vector_sunDirection) > cos(_Sky_SunRadius) && groundDist < 0.0F && cloud3DAlpha < 0.5f)
				{
					finalSkyColor += max(0.0f, (skyTransmittance * (1 - cloud3DAlpha)) * _Sky_SunIntensity);
				}

				//compute moon disk
				//make sure the sun does not show up over the ground or clouds
				if (dot(vector_viewDirection, vector_moonDirection) > cos(_Sky_Night_MoonRadius) && groundDist < 0.0F && cloud3DAlpha < 0.5f)
				{
					finalSkyColor += max(0.0f, (skyTransmittance * (1 - cloud3DAlpha)) * _Sky_Night_MoonIntensity) * _Sky_Night_KelvinTempTint;
				}

				if (_Sky_ShowTransmittanceTerm > 0)
				{
					finalSkyColor = skyTransmittance;
				}

				finalSkyColor *= _Exposure;

				return fixed4(finalSkyColor, 1);
				//return fixed4(finalSkyColor, 1);
				//return fixed4(vector_viewDirection, 1);
			}

			ENDCG
		}
	}
}
