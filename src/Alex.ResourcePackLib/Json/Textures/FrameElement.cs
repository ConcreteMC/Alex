namespace Alex.ResourcePackLib.Json.Textures
{
	public struct FrameElement
	{
		public TextureFrame FrameInfo;
		public long? Integer;

		public static implicit operator FrameElement(TextureFrame frame) => new FrameElement { FrameInfo = frame };

		public static implicit operator FrameElement(long integer) => new FrameElement { Integer = integer };
	}
}