using System;
using Alex.API.Gui;
using Alex.API.Gui.Elements.Controls;
using Alex.Gui;

namespace Alex.GameStates.Gui.MainMenu.Options
{
    public class VideoOptionsState : OptionsStateBase
    {
        private GuiSlider GuiScaleGlider { get; }
        public VideoOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
        {
            TitleTranslationKey = "options.videoTitle";

            AddGuiRow(CreateSlider("Render Distance: {0} chunks", o => Options.VideoOptions.RenderDistance, 2, 32, 1),
                GuiScaleGlider = CreateSlider("GUI Scale: {0}", options => options.VideoOptions.GuiScale, 0, 3, 1));
            
            AddGuiRow(CreateSlider("Chunk Processing Threads: {0}", o => Options.VideoOptions.ChunkThreads, 1, Environment.ProcessorCount, 1), 
                CreateSlider("Brightness: {0}%", o => Options.VideoOptions.Brightness, 0,
                    100, 1)/*,
                CreateSlider("Max Framerate: {0} fps", o => Options.VideoOptions.MaxFramerate, 1, 120, 1)*/);

            AddGuiRow(CreateToggle("Use VSync: {0}", o => { return Options.VideoOptions.UseVsync; }),
                CreateToggle("Fullscreen: {0}", o => { return Options.VideoOptions.Fullscreen; }));
        }
    }
}
