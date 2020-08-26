#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_2_0
#define PS_SHADERMODEL ps_2_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float3 PlayerPosition;

float Opacity;

float FadeNear;
float FadeFar;

float4 TintColor;

float PlayerFadeRadius;
bool FadeCenter;
float3 CameraPosition;

matrix World;
matrix View;
matrix Projection;
matrix PlayerView;

Texture2D Texture : register(t0);
sampler TextureSampler : register(s0) {
    Texture = (Texture);
};

Texture2D FadeTexture : register(t1);
sampler FadeTextureSampler : register(s1) {
    Texture = (FadeTexture);
};


struct VSInput {
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VSOutput {
    float4 Position : SV_Position;
    float2 TextureCoordinate : TEXCOORD0;
    float  Distance : float;
    float3 ProjectedPosition : float3;
};

struct PixelShaderOutput {
    float4 Color : COLOR0;
};

matrix Billboard(matrix modelView) {
    matrix billboardView = modelView;

    billboardView[0][0] = 1.0;
    billboardView[0][1] = 0.0;
    billboardView[0][2] = 0.0;

    billboardView[1][0] = 0.0;
    billboardView[1][1] = 1.0;
    billboardView[1][2] = 0.0;

    billboardView[2][0] = 0.0;
    billboardView[2][1] = 0.0;
    billboardView[2][2] = 1.0;

    return billboardView;
}

VSOutput VertexShaderFunction(VSInput input) {
    VSOutput output;

    matrix modelView = mul(World, View);

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    viewPosition = mul(input.Position, modelView);

    //viewPosition = mul(input.Position, tbb);
    output.Position = mul(viewPosition, Projection);

    // Get distance player is from marker
    output.Distance = distance(worldPosition.xyz, PlayerPosition) / 0.0254f;

    output.ProjectedPosition = normalize(mul(worldPosition, PlayerView).xyz) * (distance(CameraPosition, PlayerPosition) * 0.1);

    // make the trail slowly move along the path
    output.TextureCoordinate = input.TextureCoordinate.xy;

    return output;
}

void DissolvePosition(float2 position, float2 projectedPosition)
{
    float3 color = tex2D(FadeTextureSampler, position).rgb;
    half val = 0.21 * color.r + 0.71 * color.b + 0.071 * color.g;
    clip(val * clamp(length(projectedPosition), 0.0, 1.0) - 0.1f);
}


PixelShaderOutput PixelShaderFunction(VSOutput input) {
    PixelShaderOutput output;

    clip(FadeFar - input.Distance);

    if (FadeCenter && length(input.ProjectedPosition.xy) < PlayerFadeRadius)
    {
	    DissolvePosition(input.TextureCoordinate, input.ProjectedPosition.xy);
    }
    else
    {
        float3 color = tex2D(FadeTextureSampler, input.TextureCoordinate).rgb;
        half val = 0.21 * color.r + 0.71 * color.b + 0.071 * color.g;
        float nearDist = input.Distance - FadeNear;
        clip(val - clamp(nearDist / (FadeFar - FadeNear), 0.0, 1.0));
    }

    output.Color = tex2D(TextureSampler, input.TextureCoordinate) * TintColor * Opacity;

    return output;
}

technique
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}
