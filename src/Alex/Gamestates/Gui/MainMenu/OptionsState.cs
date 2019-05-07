using System;
using Alex.API.Data.Options;
using Alex.API.GameStates;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.GameStates.Gui.Common;
using Alex.GameStates.Gui.MainMenu.Options;
using Alex.Gui.Elements;

namespace Alex.GameStates.Gui.MainMenu
{
    public class OptionsState : OptionsStateBase
    {
        public OptionsState() : base()
        {
            TitleTranslationKey = "options.title";

            AddGuiRow(CreateSlider("FOV", o => o.FieldOfVision, 30, 120, 1),              CreateLinkButton<VideoOptionsState>("options.video"));

            AddGuiRow(CreateLinkButton<ResourcePackOptionsState>("options.resourcepack"), CreateLinkButton<SoundOptionsState>("options.sounds"));
            AddGuiRow(CreateLinkButton<LanguageOptionsState>("options.language"),         CreateLinkButton<ControlOptionsState>("options.controls"));
        }
    }
}
