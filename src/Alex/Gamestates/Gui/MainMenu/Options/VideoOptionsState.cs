using System;

namespace Alex.GameStates.Gui.MainMenu.Options
{
    public class VideoOptionsState : OptionsStateBase
    {
        public VideoOptionsState()
        {
            TitleTranslationKey = "options.videoTitle";

            AddGuiRow(CreateSlider("Render Distance: {0} chunks", o => Options.VideoOptions.RenderDistance, 2, 32, 1)/*,
                CreateSlider("Max Framerate: {0} fps", o => Options.VideoOptions.MaxFramerate, 1, 120, 1)*/);
            
            AddGuiRow(CreateSlider("Chunk Processing Threads: {0}", o => Options.VideoOptions.ChunkThreads, 1, Environment.ProcessorCount, 1));
        }
    }
}
