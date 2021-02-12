using Alex.API.Gui.Graphics;
using Alex.Gamestates.MainMenu.Options;
using Alex.Gui;

namespace Alex.Gamestates.MainMenu
{
    public class OptionsState : OptionsStateBase
    {
        public OptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
        {
            TitleTranslationKey = "options.title";
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            AddGuiRow(
                CreateSlider($"{GuiRenderer.GetTranslation("options.fov")}: {{0}}", o => o.FieldOfVision, 30, 120, 1),
                CreateSlider(f => $"Mouse Sensitivity: {(f)}%", o => o.MouseSensitivity, 0, 60, 1));

            AddGuiRow(
                CreateLinkButton<SoundOptionsState>("options.sounds", "Sound Settings..."),
                CreateLinkButton<VideoOptionsState>("options.video", "Graphics Settings..."));

            AddGuiRow(
                CreateLinkButton<LanguageOptionsState>("options.language", "Language Settings..."),
                CreateLinkButton<ControlOptionsState>("options.controls", "Controls..."));

            AddGuiRow(
                CreateLinkButton<ResourcePackOptionsState>("options.resourcepack", "Resource Packs"),
                CreateLinkButton<MiscellaneousOptionsState>("options.miscellaneous", "Miscellaneous Settings..."));

            base.OnInit(renderer);
        }
    }
}
