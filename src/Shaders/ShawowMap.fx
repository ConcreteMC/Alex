#if OPENGL
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;

struct VertexShaderOutput
{
	float4 position : SV_POSITION;
    float2 depth : TEXCOORD0;
};

struct PixelToFrame  {
    float4 Color : SV_Target;
};

VertexShaderOutput VSShadowMap(float4 position : POSITION)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	position.w = 1.0f;
	output.position = mul(position, WorldViewProjection);
    output.depth = output.position.zw;
    return output;
}

PixelToFrame PSShadowMap(VertexShaderOutput input)
{
	PixelToFrame Output = (PixelToFrame)0;
	Output.Color = float4(input.depth.x / input.depth.y, 0, 0, 1);
	return Output;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL VSShadowMap();
		PixelShader = compile PS_SHADERMODEL PSShadowMap();
	}
};