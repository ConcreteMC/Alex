float4x4 World;
float4x4 Projection;
float4x4 View;
float4 DiffuseColor;
float4 FogVector;
float3 FogColor;
float4 AlphaTest;
float4 LightOffset;

Texture Texture;

sampler2D textureSampler = sampler_state  {
    Texture = <Texture>;
    MipFilter = Point;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexToPixel  {
    float4 Position     : POSITION;
    float4 TexCoords    : TEXCOORD0;
    float4 BlockLight   : TEXCOORD01;
    float4 SkyLight     : TEXCOORD02;
    float4 Color        : COLOR0;
};

struct PixelToFrame  {
    float4 Color        : COLOR0;
};

VertexToPixel VertexShaderFunction(float4 inPosition : POSITION, float4 inTexCoords : TEXCOORD0, float4 inColor : COLOR0, float4 blockLight : TEXCOORD01, float4 skyLight : TEXCOORD02)  {
    VertexToPixel Output = (VertexToPixel)0;

    float4 worldPos = mul(inPosition, World);
    float4 viewPos = mul(worldPos, View);

    Output.Position = mul(viewPos, Projection);
    Output.TexCoords = inTexCoords;
    Output.BlockLight = blockLight;
    Output.SkyLight = skyLight;
    Output.Color = inColor;

    return Output;
}

PixelToFrame PixelShaderFunction(VertexToPixel PSIn)  {
    PixelToFrame Output = (PixelToFrame)0;

    float4 baseColor = 0.086f;
    float4 textureColor = tex2D(textureSampler, PSIn.TexCoords);
    float4 modifiedSkyLight = clamp(PSIn.SkyLight * LightOffset, 0, 15);

    float4 lighting = max(PSIn.BlockLight, modifiedSkyLight);
    
    float4 colorValue = pow(lighting / 16.0f, 1.4f) + baseColor;

    Output.Color = textureColor * PSIn.Color;

    Output.Color.r *= colorValue;
    Output.Color.g *= colorValue;
    Output.Color.b *= colorValue;

    clip((Output.Color.a < AlphaTest.x) ? AlphaTest.z : AlphaTest.w);

    return Output;
}

technique Block  {
    pass Pass0  {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}