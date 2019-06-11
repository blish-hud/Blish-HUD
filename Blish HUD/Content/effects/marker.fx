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

matrix View;
matrix Projection;
matrix World;

Texture2D Texture : register(t0);
sampler TextureSampler : register(s0) {
    Texture = (Texture);
};

struct VSInput {
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VSOutput {
    float4 Position : SV_Position;
    float2 TextureCoordinate : TEXCOORD0;
	float  Distance : float;
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
	
	// make the trail slowly move along the path
    output.TextureCoordinate = input.TextureCoordinate.xy;

    return output;
}

float GetFadeNearFar(VSOutput input) {
	if (FadeNear >= 0 && FadeFar >= 0) {
		// Handle fade far (first since it'll clip and can skip the rest of this if it's too far away)
		clip(FadeFar - input.Distance);
		
		float nearDist = input.Distance - FadeNear; // (input.TextureCoordinate.y * 0.0254f + input.Distance)
	
		// Handle fade near
		return 1.0 - clamp(nearDist / (FadeFar - FadeNear), 0.0, 1.0);
	}
	
	return 1.0;
}

PixelShaderOutput PixelShaderFunction(VSOutput input) {
    PixelShaderOutput output;
	
	float distanceFade = GetFadeNearFar(input);
    
    output.Color = tex2D(TextureSampler, input.TextureCoordinate) * distanceFade * Opacity;
	
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
