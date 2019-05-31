﻿#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_2_0
#define PS_SHADERMODEL ps_2_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float Roller;
float VerticalDraw;

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

Texture2D Overlay;
sampler OverlaySample = sampler_state
{
    Texture = (Overlay);
    AddressU = Clamp;
    AddressV = Clamp;
};

float4 PSScroll(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0
{
    float2 TexCoords2 = float2(TexCoords.x, TexCoords.y / VerticalDraw);

    if (TexCoords.y > VerticalDraw)
    {
        return tex2D(s0, TexCoords) * Color;
    }
    
    float4 mask_clr = tex2D(MaskSample, TexCoords2) * Color;
    float4 overlay_clr = tex2D(OverlaySample, TexCoords2) * Color;
	
    float alpha_comb = lerp(mask_clr.b - mask_clr.r - 1.0, 1.0, Roller);

    if (alpha_comb <= 0)
    {
        return float4(0, 0, 0, 0);
    }

    return overlay_clr;
}

float4 PSControl(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0
{
    return tex2D(s0, TexCoords) * Color;
}


float4 PSCoverage(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0
{
	// R = Wipe in
	// G = Wipe out
	// B = Feathering
	
	
}

technique
{
    pass scroll
    {
        PixelShader = compile PS_SHADERMODEL PSScroll();
    }
    pass control
    {
        PixelShader = compile PS_SHADERMODEL PSControl();
    }
}
