namespace Alex.Interfaces
{
    public interface IVector3
    {
        float X { get; }
        float Y { get; }
        float Z { get; }
    }
    
    public interface IVector2
    {
        double X { get; }
        double Y { get; }
    }
    
    public interface IVector3I
    {
        int X { get; }
        int Y { get; }
        int Z { get; }
    }
    
    public interface IVector2I
    {
        int X { get; }
        int Y { get; }
    }
}