using System;
using Alex.API.Gui;
using Alex.API.Gui.Elements.Controls;
using Alex.Gui;

namespace Alex.GameStates.Gui.MainMenu.Options
{
    public class VideoOptionsState : OptionsStateBase
    {
        private GuiSlider GuiScaleGlider { get; }
        private GuiSlider FpsSlider { get; }
        private GuiToggleButton FrameRateLimiter { get; }
        public VideoOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
        {
            TitleTranslationKey = "options.videoTitle";

            AddGuiRow(CreateSlider("Render Distance: {0} chunks", o => Options.VideoOptions.RenderDistance, 2, 32, 1),
                GuiScaleGlider = CreateSlider(v => $"GUI Scale: {((int)v == 0 ? "Auto" : v.ToString("0") )}", options => options.VideoOptions.GuiScale, 0, 3, 1));

            AddGuiRow(CreateSlider("Chunk Processing Threads: {0}", o => Options.VideoOptions.ChunkThreads, 1, Environment.ProcessorCount, 1), 
                CreateSlider("Brightness: {0}%", o => Options.VideoOptions.Brightness, 0,
                    100, 1));

            FpsSlider =
                CreateSlider("Max Framerate: {0} fps", o => Options.VideoOptions.MaxFramerate, 1, 120, 1);
            
            FrameRateLimiter = CreateToggle("Limit Framerate: {0}", options => options.VideoOptions.LimitFramerate);
            
            AddGuiRow( FrameRateLimiter, FpsSlider);

            AddGuiRow(CreateToggle("Use VSync: {0}", o => { return Options.VideoOptions.UseVsync; }), 
                CreateToggle("Fullscreen: {0}", o => { return Options.VideoOptions.Fullscreen; }));
        }

        protected override void OnShow()
        {
            base.OnShow();
            FrameRateLimiter.ValueChanged += FrameRateLimiterOnValueChanged;
            FpsSlider.Enabled = FrameRateLimiter.Value;
        }

        protected override void OnHide()
        {
            base.OnHide();
            FrameRateLimiter.ValueChanged -= FrameRateLimiterOnValueChanged;
        }
        
        private void FrameRateLimiterOnValueChanged(object? sender, bool e)
        {
            FpsSlider.Enabled = e;
        }
    }
}
