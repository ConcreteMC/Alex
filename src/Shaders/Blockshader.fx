#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
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

Texture Texture;
//: register(s0);
sampler2D textureSampler: register(s0) = sampler_state {
    Texture = <Texture>;
};

struct VertexToPixel  {
    float4 Position     : POSITION;
    float4 WorldPos     : TEXCOORD2;
    float2 TexCoords    : TEXCOORD0;
    float4 Lighting   : TEXCOORD1;
    float4 Color        : COLOR0;
    float FogFactor    : COLOR1;
    float4 Normal : NORMAL;
};

struct PixelToFrame  {
    float4 Color        : COLOR0;
};

float ComputeFogFactor(float d) 
{
    return saturate((d - FogStart) / (FogEnd - FogStart)) * FogEnabled;
}

VertexToPixel VertexShaderFunction(float4 inPosition : POSITION, float4 inNormal : NORMAL, float4 inTexCoords : TEXCOORD0, float4 inColor : COLOR0, float2 lightValues : TEXCOORD01)  {
    VertexToPixel Output = (VertexToPixel)0;

    float4 worldPos = mul(inPosition, World);
    float4 viewPos = mul(worldPos, View);

    Output.Position = mul(viewPos, Projection);
    Output.Lighting = max(clamp(lightValues.x * LightOffset, 0, 15), lightValues.y);
    Output.Color = inColor;
    Output.WorldPos = worldPos;

    float totalFrames = floor(inTexCoords.w / inTexCoords.z);
    float index = floor(ElapsedTime % totalFrames);

    Output.TexCoords = float2(inTexCoords.x, inTexCoords.y);
    if (ElapsedTime > 0 && totalFrames > 1){
        Output.TexCoords += float2(0, inTexCoords.z * index);
    }

    Output.TexCoords *= UvScale;
    //if (Output.TexCoords.y >= inTexCoords.w){
        
    //}
    //Output.TexCoords *= scale;

    Output.FogFactor = ComputeFogFactor(distance(CameraPosition, worldPos));
    Output.Normal = inNormal;

    return Output;
}

PixelToFrame PixelShaderFunction(VertexToPixel PSIn)  {
    PixelToFrame result = (PixelToFrame)0;

    float4 baseColor = 0.086f;
    float4 textureColor = tex2D(textureSampler, PSIn.TexCoords);
    
    float4 colorValue = pow((1.0f / 16.0f) * PSIn.Lighting, 1.2f) + baseColor;

    float4 output = textureColor * PSIn.Color;
    output.r *= colorValue;
    output.g *= colorValue;
    output.b *= colorValue;
    
    output.rgb = lerp(output.rgb, FogColor, PSIn.FogFactor);
    clip((output.a < AlphaTest.x) ? AlphaTest.z : AlphaTest.w);

    result.Color = output;

    return result;
   // return output * float4();
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

float4 RenderDepthMapPS(OUT_DEPTH In) : COLOR
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

        VertexShader = compile vs_2_0 RenderDepthMapVS();
        PixelShader  = compile ps_2_0 RenderDepthMapPS();
    }
}