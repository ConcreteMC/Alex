using System;
using System.Collections.Generic;
using System.Text;
using Alex.GameStates.Gui.Common;

namespace Alex.GameStates.Gui.MainMenu.Options
{
    public class VideoOptionsState : OptionsStateBase
    {
        public VideoOptionsState()
        {
            TitleTranslationKey = "options.videoTitle";

            AddGuiRow(CreateSlider("Render Distance: {0} chunks", o => Options.VideoOptions.RenderDistance));
        }
    }
}
