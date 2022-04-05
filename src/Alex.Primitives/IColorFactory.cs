namespace Alex.Interfaces
{
	public interface IColorFactory
	{
		IColor Color(byte r, byte g, byte b, byte a);
		IColor Color(byte r, byte g, byte b);

		IColor Color(uint rgba);
		IColor Color(IVector4 vector);
		IColor Color(IVector3 vector);
	}
}