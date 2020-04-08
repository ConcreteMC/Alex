#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif


float4x4 World;
float4x4 View;
float4x4 Projection;

struct VertexShaderInput
{
  float4 Position : POSITION0;
  float3 Normal : NORMAL0;
  float2 TexCoords : TEXCOORD0;
  float4 Color : COLOR0;
};

struct VertexShaderOutput
{
  float4 Position : POSITION0;
  float3 Normal : NORMAL0;
  float2 TexCoords : TEXCOORD0;
  float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
  VertexShaderOutput output;

  float4 worldPosition = mul(input.Position, World);
  float4x4 viewProjection = mul(View, Projection);

  output.Position = mul(worldPosition, viewProjection);
  output.Normal = input.Normal;
  output.TexCoords = input.TexCoords;
  output.Color = input.Color;

  return output;
}
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
  return input.Color;
}

technique Technique1
{
 pass Pass1
 {
   VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
   PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
 }
}