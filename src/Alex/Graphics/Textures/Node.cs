using SixLabors.ImageSharp;

namespace Alex.Graphics.Textures
{
	/// <summary>
	/// A node in the Atlas structure
	/// </summary>
	public class Node
	{
		/// <summary>
		/// Bounds of this node in the atlas
		/// </summary>
		public Rectangle Bounds;

		/// <summary>
		/// Texture this node represents
		/// </summary>
		public TextureInfo Texture;
        
		/// <summary>
		/// If this is an empty node, indicates how to split it when it will  be used
		/// </summary>
		public SplitType SplitType;
	}
}