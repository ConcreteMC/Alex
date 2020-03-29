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

            var masterSlider = CreateSlider("Master Volume: {0}", options => options.SoundOptions.GlobalVolume, 0, 1D,
                0.01D);

            var musicSlider = CreateSlider("Music: {0}", options => options.SoundOptions.MusicVolume, 0, 1D, 0.01);
            var effectSlider = CreateSlider("Effects: {0}", options => options.SoundOptions.SoundEffectsVolume, 0, 1D,
                0.01);
            
            masterSlider.ValueFormatter = Format;
            musicSlider.ValueFormatter = Format;
            effectSlider.ValueFormatter = Format;

            AddGuiRow(masterSlider);
            AddGuiRow(musicSlider, effectSlider);
        }

        public string Format(double value)
        {
            return (value * 100).ToString(CultureInfo.InvariantCulture);
        }
    }
}
