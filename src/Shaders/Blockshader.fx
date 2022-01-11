#if OPENGL
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 Projection;
float4x4 View;

float4x4 LightProjection;
float4x4 LightView;

float3 CameraPosition;
float CameraFarDistance;

float4 DiffuseColor;
float4 AlphaTest;
float4 LightOffset;

float3 AmbientColor;
float4 LightDirection;

float FogEnabled;
float FogStart;
float FogEnd;
float3 FogColor;

float ElapsedTime;
float2 UvScale;
float ApplyAnimations;

Texture2D MyTexture;
sampler textureSampler : register(s0) = sampler_state {
    Texture = (MyTexture);
};

struct VertexToPixel  {
    float4 Position     : SV_POSITION;
    float4 WorldPos     : TEXCOORD2;
    float2 TexCoords    : TEXCOORD0;
    float4 Lighting   : TEXCOORD1;
    float4 Color        : COLOR0;
    float FogFactor    : COLOR1;
    float4 Normal : NORMAL;
};

float ComputeFogFactor(float d) 
{
      return saturate((d - FogStart) / (FogEnd - FogStart)) * FogEnabled;
   // return saturate((FogEnd - d) / (FogEnd - FogStart)) * FogEnabled;
}

float2 ApplyFrameOffset(float4 inTexCoords, float2 uv){
    if (inTexCoords.z != inTexCoords.w){
        float yFrames = floor(inTexCoords.w / inTexCoords.z);
        float index = floor(ElapsedTime);

        return uv + float2(0, floor(index % yFrames) * inTexCoords.z);
    }

    return uv;
}

VertexToPixel VertexShaderFunction(float4 inPosition : POSITION, float4 inNormal : NORMAL, float4 inTexCoords : TEXCOORD0, float4 inColor : COLOR0, float2 lightValues : TEXCOORD01)  {
    VertexToPixel Output = (VertexToPixel)0;

    inPosition.w = 1.0f;

    float4 cameraPosition = float4(CameraPosition, 1.0f);
    float4 worldPos = mul(inPosition, World);
    float4 viewPos = mul(worldPos, View);

    Output.Position = mul(viewPos, Projection);
    Output.Lighting = max(clamp(lightValues.x * LightOffset, 0, 15), lightValues.y);
    Output.Color = inColor;
    Output.WorldPos = worldPos;

    float2 uv = float2(inTexCoords.x, inTexCoords.y);

    if (ApplyAnimations > 0.0f)
        uv = ApplyFrameOffset(inTexCoords, uv);

    uv *= UvScale;

    Output.TexCoords = uv;
    Output.FogFactor = ComputeFogFactor(length(cameraPosition - worldPos));
    Output.Normal = inNormal;

    return Output;
}

float4 PixelShaderFunction(VertexToPixel PSIn) : SV_Target  {
    float baseColor = 0.086f;
    float4 textureColor = MyTexture.Sample(textureSampler, PSIn.TexCoords);
    //float4 fogColor = float4(FogColor.x, FogColor.y, FogColor.z, 1.0f);

    float colorValue = pow((1.0f / 16.0f) * PSIn.Lighting, 1.2f) + baseColor;

    float4 output = textureColor * PSIn.Color;
    output.rgb *= colorValue;

    output.rgb = lerp(output.rgb, FogColor, PSIn.FogFactor);
    clip((output.a < AlphaTest.x) ? AlphaTest.z : AlphaTest.w);

    return output;// PSIn.FogFactor * output + (1.0 - PSIn.FogFactor) * fogColor;
}

technique Block  {
    pass Pass0  {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}

struct OUT_DEPTH
{
     float4 Position : POSITION;
     float Distance : TEXCOORD0;
};

OUT_DEPTH RenderDepthMapVS(float4 inPosition: POSITION)
{
    OUT_DEPTH Out;

    // Translate the vertex using matWorldViewProj.
    //Out.Position = mul(vPos, World * View * Projection);
    //float4 worldPos = mul(inPosition, World);
    //float4 viewPos = mul(worldPos, View);

    Out.Position = mul(mul(inPosition, LightView), Projection);

    // Get the distance of the vertex between near and far clipping plane in matWorldViewProj.

    Out.Distance.x = 1.0f - (Out.Position.z/CameraFarDistance); 
 
    return Out;
}

float4 RenderDepthMapPS(OUT_DEPTH In) : SV_Target
{
    return float4(In.Distance.x,0, 0,1);
}

technique DepthMapShader
{
    pass P0
    {
        AlphaBlendEnable = false;
        ZEnable = true;
        ZWriteEnable = true;

        VertexShader = compile VS_SHADERMODEL RenderDepthMapVS();
        PixelShader  = compile PS_SHADERMODEL RenderDepthMapPS();
    }
}