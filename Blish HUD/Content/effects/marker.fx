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

float TotalLength;
float FadeDistance;

float4x4 PlayerViewProjection;
float4x4 WorldViewProjection;
Texture2D Texture : register(t0);
sampler TextureSampler : register(s0)
{
    Texture = (Texture);
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

	// Pass on to PS (some redundant for later)
    output.Color = input.Color * Opacity;
	
	// make the trail slowly move along the path
    output.TextureCoordinate = float2(input.TextureCoordinate.x, input.TextureCoordinate.y + (TotalMilliseconds / 1000) * FlowSpeed);

    return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;
	
	float a = 1.0;
	
	// Handle fade far (first since it'll clip and can skip the rest of this if it's too far away)
	//clip(FadeFar - input.Distance);
	if (FadeFar - input.Distance) {
		a = 0;
	}

    float nearDist = input.Distance - FadeNear; // (input.TextureCoordinate.y * 0.0254f + input.Distance)
	
	// Handle fade near
	float nearDistFade = 1.0 - clamp(nearDist / (FadeFar - FadeNear), 0.0, 1.0);
    
    output.Color = tex2D(TextureSampler, input.TextureCoordinate) * nearDistFade * input.Color * Opacity * a;
	
    //clip(output.Color.a - 0.02);
	
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
