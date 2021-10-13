namespace Alex.ResourcePackLib.Abstraction
{
	public interface IColor
	{
		byte R { get; set; }
		byte G { get; set; }
		byte B { get; set; }
		uint PackedValue { get; set; }
	}
}