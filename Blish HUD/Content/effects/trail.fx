#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_2_0
#define PS_SHADERMODEL ps_2_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float TotalMilliseconds;
int FlowSpeed;
float4 PlayerPosition;

float FadeOutDistance;
float FullClip;

float TotalLength;
float FadeDistance;

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
	float Distance : float;
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
	output.Distance = distance(input.Position, PlayerPosition);
	
	// Fade the trail out at the end
    output.Color = input.Color * clamp(input.TextureCoordinate.y / FadeDistance, 0.0, 1.0);
	
	// TODO: Fade the trail in at the beginning
	// output.Color = output.Color * clamp((input.TextureCoordinate.y - FadeDistance) / TotalLength, 0.0, 1.0);
	
	// Fade the trail out when the character is close to it
	output.Color = output.Color * clamp(output.Distance / FadeDistance, 0.0, 1.0);
	
	// Fade the trail when it is far away from the character
	output.Color = output.Color * clamp(1.0 - (output.Distance - FadeOutDistance) / FullClip, 0.0, 1.0);
	
	// makes the trail move (don't remember why these particular numbers were chosen...)
    output.TextureCoordinate = float2(input.TextureCoordinate.x, input.TextureCoordinate.y + TotalMilliseconds / (800.0 * (11 - FlowSpeed)));
	
    return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;
	
	// Handle far clip (first since it'll clip and can skip the rest of this if it's too far away)
	// float farDist = farClip - distance(input.VrtPos, PlayerPosition);
	// clip(farDist);
	
	// Handle near clip
	// float nearDist = clamp(input.Distance / nearClip, 0.0, 1.0);
	
	//float startOne = Length - 1;
	//float sdist = clamp((input.TextureCoordinate.y - startOne) / Length, 0.0, 1.0);
		
    output.Color = tex2D(TextureSampler, input.TextureCoordinate) * input.Color; // * nearDist;
	
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
