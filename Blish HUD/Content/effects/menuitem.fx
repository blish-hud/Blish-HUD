#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_2_0
#define PS_SHADERMODEL ps_2_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float Roller;
float Opacity = 1.0f;

sampler s0 = sampler_state
{
    AddressU = Clamp;
    AddressV = Clamp;
};

Texture2D Mask;
sampler MaskSample
{
    Texture = (Mask);
    /*AddressU = Clamp;
    AddressV = Clamp;*/
};

Texture2D Overlay;
sampler OverlaySample = sampler_state
{
    Texture = (Overlay);
    AddressU = Clamp;
    AddressV = Clamp;
};

float4 PixelShaderFunction(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0
{
    float4 control_clr = tex2D(s0, TexCoords) * Color;
    float4 mask_clr = tex2D(MaskSample, TexCoords);
    float4 overlay_clr = tex2D(OverlaySample, TexCoords);
	
    float alpha_comb = lerp(mask_clr.b - 1.0 - mask_clr.r, 1.0, Roller);

    if (alpha_comb <= 0)
    {
        return control_clr;
    }

    return overlay_clr * Opacity;
}

technique
{
    pass
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}
