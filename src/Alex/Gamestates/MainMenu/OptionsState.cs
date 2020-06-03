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
            AddGuiRow(CreateSlider($"{GuiRenderer.GetTranslation("options.fov")}: {{0}}", o => o.FieldOfVision, 30, 120, 1),
               CreateSlider(
                   f => $"Mouse Sensitivity: {(f)}%", o => o.MouseSensitivity, 0, 60, 1));

            AddGuiRow(
                CreateLinkButton<SoundOptionsState>("options.sounds"), CreateLinkButton<VideoOptionsState>("options.video"));
            
            AddGuiRow(CreateLinkButton<LanguageOptionsState>("options.language"),
                CreateLinkButton<ControlOptionsState>("options.controls"));

            AddGuiRow(CreateLinkButton<ResourcePackOptionsState>("options.resourcepack"), CreateLinkButton<NetworkOptionsState>("options.network"));
            
            base.OnInit(renderer);
        }
    }
}
