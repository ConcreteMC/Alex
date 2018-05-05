using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RocketUI.Graphics.Textures
{
    public struct GuiTexture2D : ITexture2D
    {
        public Color?            Color           { get; set; }
        public string            TextureResource { get; set; }
        public ITexture2D        Texture         { get; set; }
        public TextureRepeatMode RepeatMode      { get; set; }
        public Color?            Mask            { get; set; }
        public Vector2?          Scale           { get; set; }
        public Vector2?          Offset { get; set; }

        public bool HasValue => Texture != null || Color.HasValue || !string.IsNullOrEmpty(TextureResource);

        public GuiTexture2D(ITexture2D texture) : this()
        {
            Texture = texture;
        }
        public GuiTexture2D(ITexture2D texture, TextureRepeatMode repeatMode = TextureRepeatMode.Stretch, Vector2? scale = null) : this()
        {
            Texture = texture;
            RepeatMode = repeatMode;
            Scale      = scale;
        }

        public GuiTexture2D(string textureResource, TextureRepeatMode repeatMode = TextureRepeatMode.Stretch, Vector2? scale = null) : this()
        {
            TextureResource = textureResource;
            RepeatMode = repeatMode;
            Scale = scale;
        }

        public bool TryResolveTexture(IGuiResourceProvider resources)
        {
            if (Texture != null) return true;
            if (string.IsNullOrEmpty(TextureResource)) return true;

            if(resources.TryGetTexture2D(TextureResource, out var texture))
            {
                Texture = texture;
            }
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

        public static implicit operator GuiTexture2D(string textureResource)
        {
            return new GuiTexture2D { TextureResource = textureResource };
        }

        Texture2D ITexture2D.Texture => Texture?.Texture;
        public Rectangle ClipBounds => Texture?.ClipBounds ?? Rectangle.Empty;
        public int Width => Texture?.Width ?? 0;
        public int Height => Texture?.Height ?? 0;
    }
}
