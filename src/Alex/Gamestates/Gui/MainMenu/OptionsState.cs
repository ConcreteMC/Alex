using Alex.GameStates.Gui.MainMenu.Options;

namespace Alex.GameStates.Gui.MainMenu
{
    public class OptionsState : OptionsStateBase
    {
        public OptionsState() : base()
        {
            TitleTranslationKey = "options.title";

            AddGuiRow(CreateSlider("FOV: {0}", o => o.FieldOfVision, 30, 120, 1),              CreateLinkButton<VideoOptionsState>("options.video"));

            AddGuiRow(CreateLinkButton<ResourcePackOptionsState>("options.resourcepack"), CreateLinkButton<SoundOptionsState>("options.sounds"));
            AddGuiRow(CreateLinkButton<LanguageOptionsState>("options.language"),         CreateLinkButton<ControlOptionsState>("options.controls"));
        }
    }
}
