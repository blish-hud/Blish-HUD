#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_2_0
#define PS_SHADERMODEL ps_2_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float  TextureWidth;
float4 GlowColor;

float Opacity = 1.0f;

sampler s0 = sampler_state {
    AddressU = Clamp;
    AddressV = Clamp;
};

float4 DrawGlow(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0 {
    float Pixel = 2 / TextureWidth;
    
    float4 clr = float4(0, 0, 0, 0);
    
    clr += tex2D(s0, float2(TexCoords.x - Pixel, TexCoords.y));
    clr += tex2D(s0, float2(TexCoords.x - Pixel - Pixel, TexCoords.y));
    clr += tex2D(s0, float2(TexCoords.x - Pixel, TexCoords.y + Pixel));
    clr += tex2D(s0, float2(TexCoords.x        , TexCoords.y + Pixel));
    clr += tex2D(s0, float2(TexCoords.x        , TexCoords.y + Pixel + Pixel));
    clr += tex2D(s0, float2(TexCoords.x + Pixel, TexCoords.y + Pixel));
    clr += tex2D(s0, float2(TexCoords.x + Pixel, TexCoords.y        ));
    clr += tex2D(s0, float2(TexCoords.x + Pixel + Pixel, TexCoords.y));
    clr += tex2D(s0, float2(TexCoords.x + Pixel, TexCoords.y - Pixel));
    clr += tex2D(s0, float2(TexCoords.x        , TexCoords.y - Pixel));
    clr += tex2D(s0, float2(TexCoords.x        , TexCoords.y - Pixel - Pixel));
    clr += tex2D(s0, float2(TexCoords.x - Pixel, TexCoords.y - Pixel));
	
    return float4(clr.rgb * 12, clr.a * 0.1) * GlowColor * Opacity;
}

float4 DrawIcon(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0 {
    return tex2D(s0, TexCoords) * Color * Opacity;
}

float4 DrawSilhouette(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0 {
	return tex2D(s0, TexCoords) * float4(0, 0, 0, 1) * Opacity;
}

technique
{
    pass glow
    {
        PixelShader = compile PS_SHADERMODEL DrawGlow();
    }
    pass icon
    {
        PixelShader = compile PS_SHADERMODEL DrawIcon();
    }
}