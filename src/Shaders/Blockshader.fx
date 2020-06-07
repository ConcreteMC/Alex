float4x4 World;
float4x4 Projection;
float4x4 View;
float4 DiffuseColor;
float4 AlphaTest;
float4 LightOffset;

float3 LightSource1;
float LightSource1Strength;

float FogEnabled;
float FogStart;
float FogEnd;
float3 FogColor;

Texture Texture;
//: register(s0);
sampler2D textureSampler: register(s0) = sampler_state {
    Texture = <Texture>;
};

struct VertexToPixel  {
    float4 Position     : POSITION;
    float4 TexCoords    : TEXCOORD0;
    float4 Lighting   : TEXCOORD01;
    float4 Color        : COLOR0;
    float FogFactor    : COLOR1;
};

struct PixelToFrame  {
    float4 Color        : COLOR0;
};

float ComputeFogFactor(float d) 
{
    return saturate((d - FogStart) / (FogEnd - FogStart)) * FogEnabled;
}

VertexToPixel VertexShaderFunction(float4 inPosition : POSITION, float4 inTexCoords : TEXCOORD0, float4 inColor : COLOR0, float4 blockLight : TEXCOORD01, float4 skyLight : TEXCOORD02)  {
    VertexToPixel Output = (VertexToPixel)0;

    float4 worldPos = mul(inPosition, World);
    float4 viewPos = mul(worldPos, View);

    float4 lighting = clamp(skyLight * LightOffset, 0, 15);

    Output.Position = mul(viewPos, Projection);

    Output.TexCoords = inTexCoords;
    Output.Lighting = max(lighting, blockLight);
    Output.Color = inColor;
    Output.FogFactor = ComputeFogFactor(distance(inPosition.xy, viewPos.xy)) * FogEnabled;

    return Output;
}

PixelToFrame PixelShaderFunction(VertexToPixel PSIn)  {
    PixelToFrame Output = (PixelToFrame)0;

    float4 baseColor = 0.086f;
    float4 textureColor = tex2D(textureSampler, PSIn.TexCoords);
    
    float4 colorValue = pow((1.0f / 16.0f) * PSIn.Lighting, 1.2f) + baseColor;

    Output.Color = textureColor * PSIn.Color;
    Output.Color.r *= colorValue;
    Output.Color.g *= colorValue;
    Output.Color.b *= colorValue;
    
    Output.Color.rgb = lerp(Output.Color.rgb, FogColor, PSIn.FogFactor);
    

    clip((Output.Color.a < AlphaTest.x) ? AlphaTest.z : AlphaTest.w);

    return Output;
}

technique Block  {
    pass Pass0  {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}