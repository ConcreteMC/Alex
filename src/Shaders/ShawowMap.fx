#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;

struct VertexShaderOutput
{
	float4 position : SV_Position;
    	float2 depth : TEXCOORD0;
};

VertexShaderOutput VSShadowMap(float3 position : SV_Position)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.position = mul(float4(position, 1), WorldViewProjection);
    	output.depth = output.position.zw;
    	return output;
}

float4 PSShadowMap(VertexShaderOutput input) : COLOR
{
	return float4(input.depth.x / input.depth.y, 0, 0, 1);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL VSShadowMap();
		PixelShader = compile PS_SHADERMODEL PSShadowMap();
	}
};