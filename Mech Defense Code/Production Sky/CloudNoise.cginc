//CloudNoise.cginc

//variables
float noiseBasicNoiseSize;
float noiseFrequency;
float noiseAmplitude;
float noiseCoverage;
int noiseFBMSamples;

float worleyAmount;
float worleyJitter;
float worleySize;
bool worleyManhattanDistance;

float mod289(float x) 
{ 
	return x - floor(x * (1.0 / 289.0)) * 289.0; 
}

float4 mod289(float4 x)
{ 
	return x - floor(x * (1.0 / 289.0)) * 289.0; 
}

float4 perm(float4 x)
{
	return mod289(((x * 34.0) + 1.0) * x); 
}

float noise(float3 p) {
	float3 a = floor(p);
	float3 d = p - a;
	d = d * d * (3.0 - 2.0 * d);

	float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
	float4 k1 = perm(b.xyxy);
	float4 k2 = perm(k1.xyxy + b.zzww);

	float4 c = k2 + a.zzzz;
	float4 k3 = perm(c);
	float4 k4 = perm(c + 1.0);

	float4 o1 = frac(k3 * (1.0 / 41.0));
	float4 o2 = frac(k4 * (1.0 / 41.0));

	float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
	float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);

	return o4.y * d.y + o4.x * (1.0 - d.y);
}


float FBM3D(float3 p, int samples)
{
	p *= noiseFrequency;

	float v = noiseCoverage;
	float a = noiseAmplitude;

	float3 shift = float3(100, 100, 100);

	for (int i = 0; i < samples; ++i) 
	{
		v += a * noise(p);
		p = p * 2.0 + shift;
		a *= 0.5;
	}

	return v;
}

float3 permute(float3 x)
{
	return fmod((34.0 * x + 1.0) * x, 289.0);
}

float3 dist(float3 x, float3 y, float3 z, bool manhattanDistance) 
{
	return manhattanDistance ? abs(x) + abs(y) + abs(z) : (x * x + y * y + z * z);
}

float2 worley(float3 P, float jitter, bool manhattanDistance) 
{
	P *= worleySize;

	float K = 0.142857142857; // 1/7
	float Ko = 0.428571428571; // 1/2-K/2
	float  K2 = 0.020408163265306; // 1/(7*7)
	float Kz = 0.166666666667; // 1/6
	float Kzo = 0.416666666667; // 1/2-1/6*2

	float3 Pi = fmod(floor(P), 289.0);
	float3 Pf = frac(P) - 0.5;

	float3 Pfx = Pf.x + float3(1.0, 0.0, -1.0);
	float3 Pfy = Pf.y + float3(1.0, 0.0, -1.0);
	float3 Pfz = Pf.z + float3(1.0, 0.0, -1.0);

	float3 p = permute(Pi.x + float3(-1.0, 0.0, 1.0));
	float3 p1 = permute(p + Pi.y - 1.0);
	float3 p2 = permute(p + Pi.y);
	float3 p3 = permute(p + Pi.y + 1.0);

	float3 p11 = permute(p1 + Pi.z - 1.0);
	float3 p12 = permute(p1 + Pi.z);
	float3 p13 = permute(p1 + Pi.z + 1.0);

	float3 p21 = permute(p2 + Pi.z - 1.0);
	float3 p22 = permute(p2 + Pi.z);
	float3 p23 = permute(p2 + Pi.z + 1.0);

	float3 p31 = permute(p3 + Pi.z - 1.0);
	float3 p32 = permute(p3 + Pi.z);
	float3 p33 = permute(p3 + Pi.z + 1.0);

	float3 ox11 = frac(p11 * K) - Ko;
	float3 oy11 = fmod(floor(p11 * K), 7.0) * K - Ko;
	float3 oz11 = floor(p11 * K2) * Kz - Kzo; // p11 < 289 guaranteed

	float3 ox12 = frac(p12 * K) - Ko;
	float3 oy12 = fmod(floor(p12 * K), 7.0) * K - Ko;
	float3 oz12 = floor(p12 * K2) * Kz - Kzo;

	float3 ox13 = frac(p13 * K) - Ko;
	float3 oy13 = fmod(floor(p13 * K), 7.0) * K - Ko;
	float3 oz13 = floor(p13 * K2) * Kz - Kzo;

	float3 ox21 = frac(p21 * K) - Ko;
	float3 oy21 = fmod(floor(p21 * K), 7.0) * K - Ko;
	float3 oz21 = floor(p21 * K2) * Kz - Kzo;

	float3 ox22 = frac(p22 * K) - Ko;
	float3 oy22 = fmod(floor(p22 * K), 7.0) * K - Ko;
	float3 oz22 = floor(p22 * K2) * Kz - Kzo;

	float3 ox23 = frac(p23 * K) - Ko;
	float3 oy23 = fmod(floor(p23 * K), 7.0) * K - Ko;
	float3 oz23 = floor(p23 * K2) * Kz - Kzo;

	float3 ox31 = frac(p31 * K) - Ko;
	float3 oy31 = fmod(floor(p31 * K), 7.0) * K - Ko;
	float3 oz31 = floor(p31 * K2) * Kz - Kzo;

	float3 ox32 = frac(p32 * K) - Ko;
	float3 oy32 = fmod(floor(p32 * K), 7.0) * K - Ko;
	float3 oz32 = floor(p32 * K2) * Kz - Kzo;

	float3 ox33 = frac(p33 * K) - Ko;
	float3 oy33 = fmod(floor(p33 * K), 7.0) * K - Ko;
	float3 oz33 = floor(p33 * K2) * Kz - Kzo;

	float3 dx11 = Pfx + jitter * ox11;
	float3 dy11 = Pfy.x + jitter * oy11;
	float3 dz11 = Pfz.x + jitter * oz11;

	float3 dx12 = Pfx + jitter * ox12;
	float3 dy12 = Pfy.x + jitter * oy12;
	float3 dz12 = Pfz.y + jitter * oz12;

	float3 dx13 = Pfx + jitter * ox13;
	float3 dy13 = Pfy.x + jitter * oy13;
	float3 dz13 = Pfz.z + jitter * oz13;

	float3 dx21 = Pfx + jitter * ox21;
	float3 dy21 = Pfy.y + jitter * oy21;
	float3 dz21 = Pfz.x + jitter * oz21;

	float3 dx22 = Pfx + jitter * ox22;
	float3 dy22 = Pfy.y + jitter * oy22;
	float3 dz22 = Pfz.y + jitter * oz22;

	float3 dx23 = Pfx + jitter * ox23;
	float3 dy23 = Pfy.y + jitter * oy23;
	float3 dz23 = Pfz.z + jitter * oz23;

	float3 dx31 = Pfx + jitter * ox31;
	float3 dy31 = Pfy.z + jitter * oy31;
	float3 dz31 = Pfz.x + jitter * oz31;

	float3 dx32 = Pfx + jitter * ox32;
	float3 dy32 = Pfy.z + jitter * oy32;
	float3 dz32 = Pfz.y + jitter * oz32;

	float3 dx33 = Pfx + jitter * ox33;
	float3 dy33 = Pfy.z + jitter * oy33;
	float3 dz33 = Pfz.z + jitter * oz33;

	float3 d11 = dist(dx11, dy11, dz11, manhattanDistance);
	float3 d12 = dist(dx12, dy12, dz12, manhattanDistance);
	float3 d13 = dist(dx13, dy13, dz13, manhattanDistance);
	float3 d21 = dist(dx21, dy21, dz21, manhattanDistance);
	float3 d22 = dist(dx22, dy22, dz22, manhattanDistance);
	float3 d23 = dist(dx23, dy23, dz23, manhattanDistance);
	float3 d31 = dist(dx31, dy31, dz31, manhattanDistance);
	float3 d32 = dist(dx32, dy32, dz32, manhattanDistance);
	float3 d33 = dist(dx33, dy33, dz33, manhattanDistance);

	float3 d1a = min(d11, d12);
	d12 = max(d11, d12);
	d11 = min(d1a, d13); // Smallest now not in d12 or d13
	d13 = max(d1a, d13);
	d12 = min(d12, d13); // 2nd smallest now not in d13
	float3 d2a = min(d21, d22);
	d22 = max(d21, d22);
	d21 = min(d2a, d23); // Smallest now not in d22 or d23
	d23 = max(d2a, d23);
	d22 = min(d22, d23); // 2nd smallest now not in d23
	float3 d3a = min(d31, d32);
	d32 = max(d31, d32);
	d31 = min(d3a, d33); // Smallest now not in d32 or d33
	d33 = max(d3a, d33);
	d32 = min(d32, d33); // 2nd smallest now not in d33
	float3 da = min(d11, d21);
	d21 = max(d11, d21);
	d11 = min(da, d31); // Smallest now in d11
	d31 = max(da, d31); // 2nd smallest now not in d31
	d11.xy = (d11.x < d11.y) ? d11.xy : d11.yx;
	d11.xz = (d11.x < d11.z) ? d11.xz : d11.zx; // d11.x now smallest
	d12 = min(d12, d21); // 2nd smallest now not in d21
	d12 = min(d12, d22); // nor in d22
	d12 = min(d12, d31); // nor in d31
	d12 = min(d12, d32); // nor in d32
	d11.yz = min(d11.yz, d12.xy); // nor in d12.yz
	d11.y = min(d11.y, d12.z); // Only two more to go
	d11.y = min(d11.y, d11.z); // Done! (Phew!)

	return sqrt(d11.xy); // F1, F2
}