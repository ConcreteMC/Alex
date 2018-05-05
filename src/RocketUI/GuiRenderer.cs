using System;
using System.Resources;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using RocketUI.Elements;
using RocketUI.Elements.Controls;
using RocketUI.Graphics;
using RocketUI.Graphics.Textures;
using RocketUI.Utilities;

namespace RocketUI
{
    public class GuiRenderer : IGuiRenderer
    {
        public GuiScaledResolution ScaledResolution { get; set; }
        public GraphicsDevice GraphicsDevice { get; private set; }
        public IFont Font { get; set; }
        public IFont DebugFont { get; set; }

        private IGuiResourceProvider _resourceManager;
        public Game Game { get; }
        public GuiRenderer(Game game, IGuiResourceProvider resourceManager)
        {
            Game = game;
            _resourceManager = resourceManager;
        }

        public void Init(GuiManager guiManager, GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
            ScaledResolution = guiManager.ScaledResolution;
        }

        public GuiTexture2D GetTexture(string textureResource)
        {
            _resourceManager.TryGetGuiTexture(textureResource, out var texture);
            return texture;
        }

        private static readonly Regex GuiTextureNameRegex = new Regex(@"^(Gui)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public bool TryGetGuiTexture(Type type, string property, out GuiTexture2D guiTexture)
        {
            return _resourceManager.TryGetGuiTexture(GuiTextureNameRegex.Replace(type.Name, "").ToLowerInvariant() + ":" + property.ToLowerInvariant(), out guiTexture);
        }
        public bool TryGetGuiSoundEffect(Type type, string @event, out SoundEffect soundEffect)
        {
            return _resourceManager.TryGetSoundEffect(GuiTextureNameRegex.Replace(type.Name, "").ToLowerInvariant() + ":" + @event.ToLowerInvariant(), out soundEffect);
        }

        public Texture2D GetTexture2D(string textureResource)
        {
            if(_resourceManager.TryGetTexture2D(textureResource, out var texture))
                return texture.Texture;

            return GetTexture(textureResource).Texture?.Texture;
        }

        public void ResolveDefaultTextures(VisualElement element)
        {
            var type = element.GetType();

            if (TryGetGuiTexture(type, "Background", out var background))
                element.Background = background;
            
            if (TryGetGuiTexture(type, "BackgroundOverlay", out var backgroundOverlay))
                element.BackgroundOverlay = backgroundOverlay;
            
            if (element is Control control)
            {
                if (TryGetGuiTexture(type, "DisabledBackground", out var disabledBackground))
                    control.DisabledBackground = disabledBackground;

                if (TryGetGuiTexture(type, "HighlightedBackground", out var highlightedBackground))
                    control.HighlightedBackground = highlightedBackground;

                if (TryGetGuiTexture(type, "FocusedBackground", out var focusedBackground))
                    control.FocusedBackground = focusedBackground;
            }
        }

        public void ResolveSoundEffects(Control control)
        {
            var type = control.GetType();

            if(TryGetGuiSoundEffect(type, nameof(control.HighlightActivate), out var effect1))
                control.HighlightActivate += CreateControlSoundEffectEvent(effect1);

            if(TryGetGuiSoundEffect(type, nameof(control.HighlightDeactivate), out var effect2))
                control.HighlightDeactivate += CreateControlSoundEffectEvent(effect2);

            if(TryGetGuiSoundEffect(type, nameof(control.FocusActivate), out var effect3))
                control.FocusActivate += CreateControlSoundEffectEvent(effect3);

            if(TryGetGuiSoundEffect(type, nameof(control.FocusDeactivate), out var effect4))
                control.FocusDeactivate += CreateControlSoundEffectEvent(effect4);

            if(TryGetGuiSoundEffect(type, nameof(control.CursorDown), out var effect5))
                control.CursorDown += CreateControlSoundEffectEvent(effect5);

            if(TryGetGuiSoundEffect(type, nameof(control.CursorUp), out var effect6))
                control.CursorUp += CreateControlSoundEffectEvent(effect6);

            if(TryGetGuiSoundEffect(type, nameof(control.CursorMove), out var effect7))
                control.CursorMove += CreateControlSoundEffectEvent(effect7);

            if(TryGetGuiSoundEffect(type, nameof(control.CursorEnter), out var effect8))
                control.CursorEnter += CreateControlSoundEffectEvent(effect8);

            if(TryGetGuiSoundEffect(type, nameof(control.CursorLeave), out var effect9))
                control.CursorLeave += CreateControlSoundEffectEvent(effect9);

            if(TryGetGuiSoundEffect(type, nameof(control.KeyInput), out var effect10))
                control.KeyInput += CreateControlSoundEffectEvent(effect10);
            
        }

        private EventHandler CreateControlSoundEffectEvent(SoundEffect effect)
        {
            return (sender, e) => effect.Play();
        }

        public string GetTranslation(string key)
        {
            return "";
        }

        public Vector2 Project(Vector2 point)
        {
            return Vector2.Transform(point, ScaledResolution.TransformMatrix);
        }

        public Vector2 Unproject(Vector2 screen)
        {
            return Vector2.Transform(screen, ScaledResolution.InverseTransformMatrix);
        }
    }
}
