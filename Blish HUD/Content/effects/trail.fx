#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_2_0
#define PS_SHADERMODEL ps_2_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif


float TotalMilliseconds;
float FlowSpeed;
float3 PlayerPosition;

float Opacity;

float FadeNear;
float FadeFar;

float PlayerFadeRadius;
bool FadeCenter;

float FadeDistance;

float4 TintColor;
float3 CameraPosition;

float4x4 PlayerView;
float4x4 WorldViewProjection;

Texture2D Texture : register(t0);
sampler2D TextureSampler : register(s0)
{
    Texture = (Texture);
};

Texture2D FadeTexture : register(t1);
sampler2D FadeTextureSampler : register(s1)
{
    Texture = (FadeTexture);
};


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
    float3 ProjectedPosition : float3;
    float  Distance : float;
};

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};

// NOTE: The path is drawn backwards 
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);

    // Get distance player is from current spot in trail (so that we can fade it out a bit)
    output.Distance = distance(input.Position.xyz, PlayerPosition) / 0.0254f;

    output.ProjectedPosition = normalize(mul(input.Position, PlayerView).xyz) * (distance(CameraPosition, PlayerPosition) * 0.1);

    // Pass on to PS (some redundant for later)
    output.Color = input.Color * Opacity;

    // make the trail slowly move along the path
    output.TextureCoordinate = float2(input.TextureCoordinate.x, input.TextureCoordinate.y + (TotalMilliseconds / 1000) * FlowSpeed);

    return output;
}

void DissolvePosition(float2 texCoord, float2 projectedPosition)
{
    float3 color = tex2D(FadeTextureSampler, texCoord).rgb;
    half val = 0.21 * color.r + 0.71 * color.b + 0.071 * color.g;
    clip(val * clamp(length(projectedPosition), 0.0, 1.0) - 0.08f);
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;

    // Handle fade far (first since it'll clip and can skip the rest of this if it's too far away)
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

    output.Color = tex2D(TextureSampler, input.TextureCoordinate) * TintColor * input.Color * Opacity;

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
