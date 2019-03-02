#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_2_0
#define PS_SHADERMODEL ps_2_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

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

float4 PixelShaderFunction(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0
{
    float4 tex     = tex2D(s0, TexCoords);
    float4 bitMask = tex2D(MaskSample, TexCoords);

    float howSolid = (bitMask.r + bitMask.g + bitMask.b) / 3;


    //return tex;
    return float4(tex.r, tex.g, tex.b, howSolid * tex.a);
}

technique
{
    pass
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}
