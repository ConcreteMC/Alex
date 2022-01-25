namespace ResourcePackLib.ModelExplorer.Geometry;

public class TruModelMesh
{
    public int NumVertices { get; internal set; }
    public int VertexOffset { get; internal set; }
    public int PrimitiveCount { get; internal set; }
    public int IndexOffset { get; internal set; }
    
    public TruModelMesh()
    {
        
    }
}