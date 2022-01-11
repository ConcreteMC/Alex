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
float4 DiffuseColor;
float4 AlphaTest;
float4 LightOffset;

float FogEnabled;
float FogStart;
float FogEnd;
float3 FogColor;
float2 UvScale;

Texture2D MyTexture;
sampler textureSampler : register(s0) = sampler_state {
    Texture = (MyTexture);
};

struct VertexToPixel  {
    float4 Position     : SV_POSITION;
    float4 TexCoords    : TEXCOORD0;
    float4 Color        : COLOR0;
    float FogFactor    : COLOR1;
};

float ComputeFogFactor(float d) 
{
    return saturate((d - FogStart) / (FogEnd - FogStart)) * FogEnabled;
}

VertexToPixel VertexShaderFunction(float4 inPosition : POSITION, float4 inTexCoords : TEXCOORD0, float4 inColor : COLOR0)  {
    VertexToPixel Output = (VertexToPixel)0;

    float4 worldPos = mul(inPosition, World);
    float4 viewPos = mul(worldPos, View);

    Output.Position = mul(viewPos, Projection);

    Output.TexCoords = inTexCoords;
    Output.Color = inColor;
    Output.FogFactor = ComputeFogFactor(distance(inPosition.xy, viewPos.xy)) * FogEnabled;

    return Output;
}

float4 PixelShaderFunction(VertexToPixel PSIn) : SV_Target  {
    float4 textureColor = MyTexture.Sample(textureSampler, PSIn.TexCoords * UvScale);
    
    float4 output;
    if (textureColor.a < 255 && textureColor.a > 0){
        textureColor.a = 255;
        output = textureColor * PSIn.Color;
    }
    else
    {
       output = textureColor;// * PSIn.Color;
    }

    output.rgb = lerp(output.rgb, FogColor, PSIn.FogFactor);
    

    clip((output.a < AlphaTest.x) ? AlphaTest.z : AlphaTest.w);

    return output;
}

technique Entity  {
    pass Pass0  {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}