using System.Globalization;
using Alex.Gui;

namespace Alex.Gamestates.MainMenu.Options
{
    public class SoundOptionsState : OptionsStateBase
    {
        public SoundOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
        {
            TitleTranslationKey = "options.sounds.title";

            var masterSlider = CreateSlider(v => $"{GuiRenderer.GetTranslation("soundCategory.master")}: {((int)(v * 100)).ToString(CultureInfo.InvariantCulture)}", options => options.SoundOptions.GlobalVolume, 0, 1D,
                0.01D);

            var musicSlider = CreateSlider(v => $"{GuiRenderer.GetTranslation("soundCategory.music")}: {((int)(v * 100)).ToString(CultureInfo.InvariantCulture)}", options => options.SoundOptions.MusicVolume, 0, 1D, 0.01);
            var effectSlider = CreateSlider(v => $"{GuiRenderer.GetTranslation("soundCategory.ambient")}: {((int)(v * 100)).ToString(CultureInfo.InvariantCulture)}", options => options.SoundOptions.SoundEffectsVolume, 0, 1D,
                0.01);
            
            AddGuiRow(masterSlider);
            AddGuiRow(musicSlider, effectSlider);
        }

        public string Format(double value)
        {
            return (value * 100).ToString(CultureInfo.InvariantCulture);
        }
    }
}
