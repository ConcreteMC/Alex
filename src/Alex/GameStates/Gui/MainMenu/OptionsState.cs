using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.GameStates.Gui.Common;
using Alex.Gui.Elements;

namespace Alex.GameStates.Gui.MainMenu
{
    public class OptionsState : GuiMenuStateBase
    {
        private readonly GuiLabelledControlGroup _optionControls;
        private readonly Settings _settings;

        public OptionsState() : base()
        {
            Title = "Options";

            _settings = Alex.GameSettings;

            AddChild(new GuiBackButton());

            AddChild(_optionControls = new GuiLabelledControlGroup()
            {
                Anchor =  Alignment.CenterX,
                ChildAnchor = Alignment.TopFill,
                //HorizontalContentAlignment = HorizontalAlignment.FillParent,
                LabelPosition = LabelPosition.LeftOrControl,
                Y = Header.Height
            });

            _optionControls.AppendSlider("Mouse Sensitivity", _settings.MouseSensitivy, v => _settings.MouseSensitivy = v, 0.01d, 10.0d, 0.05d);
            _optionControls.AppendSlider("Render Distance", _settings.RenderDistance, v => _settings.RenderDistance = (int)v, 4.0d, 64.0d, 1.0d);

        }
    }
}
