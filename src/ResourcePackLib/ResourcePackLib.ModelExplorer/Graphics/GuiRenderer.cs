using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;
using RocketUI.Audio;
using RocketUI.Serialization.Xaml;

namespace ResourcePackLib.ModelExplorer.Graphics;

public class GuiRenderer : IGuiRenderer
{
    public GuiScaledResolution ScaledResolution { get; set; }

    private GraphicsDevice _graphicsDevice;
    private ContentManager _content;
    private StyleSheet _styleSheet = new StyleSheet();

    private Dictionary<GuiTextures, TextureSlice2D> _textureCache = new Dictionary<GuiTextures, TextureSlice2D>();
    private Dictionary<string, TextureSlice2D> _pathedTextureCache = new Dictionary<string, TextureSlice2D>();

    public void Init(GraphicsDevice graphics, IServiceProvider serviceProvider)
    {
        _graphicsDevice = graphics;
        _content = ((Game)serviceProvider.GetService(typeof(Game))).Content;
        Font = (WrappedSpriteFont)_content.Load<SpriteFont>("Fonts/Default");

        LoadTexturesFromContent();
        LoadStyleSheets();
    }

    private void LoadTexturesFromContent()
    {
        var gui = _content.Load<Texture2D>("Gui/Gui");
        _pathedTextureCache["Gui/Gui"] = gui;

        LoadTextureFromSpriteSheet(GuiTextures.ControlDefault, gui, new Rectangle(0, 0, 100, 20), new Thickness(2));
        LoadTextureFromSpriteSheet(GuiTextures.ControlHover, gui, new Rectangle(0, 20, 100, 20), new Thickness(2));
        LoadTextureFromSpriteSheet(GuiTextures.ControlFocused, gui, new Rectangle(0, 40, 100, 20), new Thickness(2));

        LoadTextureFromSpriteSheet(GuiTextures.ButtonDefault, gui, new Rectangle(100, 0, 60, 20), new Thickness(2));
        LoadTextureFromSpriteSheet(GuiTextures.ButtonHover, gui, new Rectangle(100, 20, 60, 20), new Thickness(2));
        LoadTextureFromSpriteSheet(GuiTextures.ButtonFocused, gui, new Rectangle(100, 40, 60, 20), new Thickness(2));
    }

    private void LoadStyleSheets()
    {
        RocketXamlLoader.Load<StyleSheet>(_styleSheet, "ResourcePackLib.ModelExplorer.Scenes.Styles.xaml");
    }

    private TextureSlice2D LoadTextureFromEmbeddedResource(GuiTextures guiTexture, byte[] resource)
    {
        //_textureCache[guiTexture] = TextureUtils.ImageToTexture2D(_graphicsDevice, resource);
        return _textureCache[guiTexture];
    }

    private void LoadTextureFromSpriteSheet(GuiTextures guiTexture, Texture2D spriteSheet, Rectangle sliceRectangle, Thickness ninePatchThickness)
    {
        _textureCache[guiTexture] = new NinePatchTexture2D(spriteSheet.Slice(sliceRectangle), ninePatchThickness);
    }

    private void LoadTextureFromSpriteSheet(GuiTextures guiTexture, Texture2D spriteSheet, Rectangle sliceRectangle)
    {
        _textureCache[guiTexture] = spriteSheet.Slice(sliceRectangle);
    }


    public IFont Font { get; set; }

    public ISoundEffect GetSoundEffect(GuiSoundEffects soundEffects)
    {
        return default;
    }

    public TextureSlice2D GetTexture(GuiTextures guiTexture)
    {
        if (_textureCache.TryGetValue(guiTexture, out var texture))
        {
            return texture;
        }

        return (TextureSlice2D)RocketUI.GpuResourceManager.CreateTexture2D(1, 1);
    }

    public TextureSlice2D GetTexture(string texturePath)
    {
        texturePath = texturePath.ToLowerInvariant();

        if (!_pathedTextureCache.TryGetValue(texturePath, out TextureSlice2D texture))
        {
            texture = Texture2D.FromFile(_graphicsDevice, texturePath);
            _pathedTextureCache.Add(texturePath, texture);
        }

        return texture;
    }

    public Texture2D GetTexture2D(GuiTextures guiTexture)
    {
        return GetTexture(guiTexture).Texture;
    }

    public string GetTranslation(string key) => key;

    public Vector2 Project(Vector2 point) => Vector2.Transform(point, ScaledResolution.TransformMatrix);

    public Vector2 Unproject(Vector2 screen) => Vector2.Transform(screen, ScaledResolution.InverseTransformMatrix);

    public IStyle[] ResolveStyles(Type elementType, string[] classNames)
    {
        return _styleSheet.ResolveStyles(elementType, classNames);
    }
}