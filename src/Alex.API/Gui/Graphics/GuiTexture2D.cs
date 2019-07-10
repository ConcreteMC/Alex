using Alex.API.Graphics.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;

namespace Alex.API.Gui.Graphics
{
    public struct GuiTexture2D : ITexture2D
    {
        public Color?            Color           { get; set; }
        public GuiTextures?      TextureResource { get; set; }
        public ITexture2D        Texture         { get; set; }
        public TextureRepeatMode RepeatMode      { get; set; }
        public Color?            Mask            { get; set; }
        public Vector2?          Scale           { get; set; }

        public bool HasValue => Texture != null || Color.HasValue || TextureResource.HasValue;

        public GuiTexture2D(ITexture2D texture) : this()
        {
            Texture = texture;
		}
		public GuiTexture2D(ITexture2D texture, TextureRepeatMode repeatMode = TextureRepeatMode.Stretch, Vector2? scale = null) : this()
		{
			Texture = texture;
			RepeatMode = repeatMode;
			Scale = scale;
		}

		public GuiTexture2D(GuiTextures guiTexture, TextureRepeatMode repeatMode = TextureRepeatMode.Stretch, Vector2? scale = null) : this()
        {
            TextureResource = guiTexture;
            RepeatMode = repeatMode;
            Scale = scale;
        }

        public bool TryResolveTexture(IGuiRenderer renderer)
        {
            if (!TextureResource.HasValue) return true;

            Texture = renderer.GetTexture(TextureResource.Value);
            return Texture != null;
        }

        public static implicit operator GuiTexture2D(TextureSlice2D texture)
        {
            return new GuiTexture2D(texture);
        }

        public static implicit operator GuiTexture2D(NinePatchTexture2D texture)
        {
            return new GuiTexture2D(texture);
        }

        public static implicit operator GuiTexture2D(Color color)
        {
            return new GuiTexture2D { Color = color };
        }

        public static implicit operator GuiTexture2D(GuiTextures textureResource)
        {
            return new GuiTexture2D { TextureResource = textureResource };
        }

        Texture2D ITexture2D.Texture => Texture.Texture;
        public Rectangle ClipBounds => Texture?.ClipBounds ?? Rectangle.Empty;
        public int Width => Texture?.Width ?? 0;
        public int Height => Texture?.Height ?? 0;
    }
}
