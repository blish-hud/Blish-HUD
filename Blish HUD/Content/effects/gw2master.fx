#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_2_0
#define PS_SHADERMODEL ps_2_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float Roller;
bool directionIn;

sampler s0 = sampler_state
{
    AddressU = Clamp;
    AddressV = Clamp;
};

Texture2D Mask;
sampler MaskSample
{
    Texture = (Mask);
    AddressU = Clamp;
    AddressV = Clamp;
};

float4 GW2AlphaMask(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0
{
	float4 control_clr = tex2D(s0, TexCoords) * Color;
    float4 mask_clr    = tex2D(MaskSample, TexCoords);
	
	return float4(control_clr.xyz, control_clr.a * max(mask_clr.r, max(mask_clr.g, mask_clr.b)));
}

float4 GW2WipeNFeather(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0
{
	// R = Wipe in
	// G = Wipe out
	// B = Feathering
	
    float4 control_clr = tex2D(s0, TexCoords) * Color;
    float4 mask_clr    = tex2D(MaskSample, TexCoords);
	
	float a_dir = lerp(0.0, mask_clr.g, Roller);
	
	if (directionIn) {
		a_dir = lerp(0.0, 1.0, Roller);
	}
	
	return float4(mask_clr.xyz, clamp(mask_clr.a * (a_dir), 0.0, 1.0));
}

float4 GW2AlphaMaskWipe(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0
{
	if (directionIn) {
		return GW2WipeNFeather(Position, Color, TexCoords);
	}

	float4 control_clr = tex2D(s0, TexCoords) * Color;
    float4 mask_clr    = tex2D(MaskSample, TexCoords);
	
	//float opacity = max(mask_clr.r, max(mask_clr.g, mask_clr.b)) + Roller;
	
	//return float4(control_clr.xyz, control_clr.a * opacity);
	
	return control_clr;
}

technique
{
    pass
    {
        PixelShader = compile PS_SHADERMODEL GW2AlphaMaskWipe();
    }
}
