using System;

namespace Alex.Interfaces
{
    public interface IVector4 : IEquatable<IVector4>
    {
        float X { get; }
        float Y { get; }
        float Z { get; }
        float W { get; }
    }
    
    public interface IVector3 : IEquatable<IVector3>
    {
        float X { get; set; }
        float Y { get; set; }
        float Z { get; set; }
    }
    
    public interface IVector2 : IEquatable<IVector2>
    {
        float X { get; }
        float Y { get; }
    }
    
    public interface IVector4I
    {
        int X { get; }
        int Y { get; }
        int Z { get; }
        int W { get; }
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