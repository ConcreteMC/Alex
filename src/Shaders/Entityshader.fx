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

Texture Texture;
//: register(s0);
sampler2D textureSampler: register(s0) = sampler_state {
    Texture = <Texture>;
};

struct VertexToPixel  {
    float4 Position     : POSITION;
    float4 TexCoords    : TEXCOORD0;
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

PixelToFrame PixelShaderFunction(VertexToPixel PSIn)  {
    PixelToFrame Output = (PixelToFrame)0;

    float4 textureColor = tex2D(textureSampler, PSIn.TexCoords);
    
    if (textureColor.a < 255 && textureColor.a > 0){
        textureColor.a = 255;
        Output.Color = textureColor * PSIn.Color;
    }
    else
    {
        Output.Color = textureColor;// * PSIn.Color;
    }

    Output.Color.rgb = lerp(Output.Color.rgb, FogColor, PSIn.FogFactor);
    

    clip((Output.Color.a < AlphaTest.x) ? AlphaTest.z : AlphaTest.w);

    return Output;
}

technique Entity  {
    pass Pass0  {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}