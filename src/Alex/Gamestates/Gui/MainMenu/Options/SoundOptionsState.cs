using System;
using System.Globalization;
using Alex.Gui;
using RocketUI;

namespace Alex.GameStates.Gui.MainMenu.Options
{
    public class SoundOptionsState : OptionsStateBase
    {
        public SoundOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
        {
            TitleTranslationKey = "options.sounds.title";

            var masterSlider = CreateSlider(v => $"Master Volume: {((int)(v * 100)).ToString(CultureInfo.InvariantCulture)}", options => options.SoundOptions.GlobalVolume, 0, 1D,
                0.01D);

            var musicSlider = CreateSlider(v => $"Music: {((int)(v * 100)).ToString(CultureInfo.InvariantCulture)}", options => options.SoundOptions.MusicVolume, 0, 1D, 0.01);
            var effectSlider = CreateSlider(v => $"Effects: {((int)(v * 100)).ToString(CultureInfo.InvariantCulture)}", options => options.SoundOptions.SoundEffectsVolume, 0, 1D,
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
