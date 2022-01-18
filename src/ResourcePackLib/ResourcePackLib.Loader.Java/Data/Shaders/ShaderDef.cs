namespace ResourcePackLib.Loader.Java.Data.Shaders;

public class ShaderDefBlend
{
    public string Func { get; set; }
    
    public string SrcRgb { get; set; }
    
    public string DstRgb { get; set; }
}

public class ShaderDefSampler
{
    public string Name { get; set; }
}

public class ShaderDefUniform
{
    public string Name { get; set; }
    public string Type { get; set; }
    public int Count { get; set; }
    public float[] Values { get; set; }
}

public class ShaderDef
{
    public ShaderDefBlend Blend { get; set; }
    public string Vertex { get; set; }
    public string Fragment { get; set; }
    public string[] Attributes { get; set; }
    public ShaderDefSampler[] Samplers { get; set; }
    public ShaderDefUniform[] Uniforms { get; set; }
}