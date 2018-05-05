using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using RocketUI.Graphics.Textures;

namespace RocketUI
{
    public interface IGuiResourceProvider
    {
        IFont PrimaryFont { get; }
        IFont DebugFont { get; }

        bool TryGetFont(string key, out IFont font);
        bool TryGetTranslation(string key, out string translation);
        bool TryGetTexture2D(string key, out ITexture2D texture);
        bool TryGetGuiTexture(string key, out GuiTexture2D texture);
        bool TryGetSoundEffect(string key, out SoundEffect soundEffect);
    }
}
