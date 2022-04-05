namespace Alex.Interfaces
{
	public interface IVectorFactory
	{
		IVector2 Vector2Zero { get; }
		IVector2I Vector2IZero { get; }
		IVector2 Vector2(float x, float y);
		IVector2I Vector2I(int x, int y);
		
		IVector3 Vector3Zero { get; }
		IVector3I Vector3IZero { get; }
		IVector3 Vector3(float x, float y, float z);
		IVector3I Vector3I(int x, int y, int z);
		
		IVector4 Vector4Zero { get; }
		IVector4 Vector4(float x, float y, float z, float w);
		IVector4I Vector4I(int x, int y, int z, double w);
	}
}