using System;
using System.Collections.Generic;
using Alex.Common.Utils;
using Alex.Gui;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gamestates.MainMenu.Options
{
	public class VideoOptionsState : OptionsStateBase
	{
		private Slider GuiScaleGlider { get; set; }
		private Slider FpsSlider { get; set; }
		private ToggleButton FrameRateLimiter { get; set; }
		private Slider RenderDistance { get; set; }
		private Slider Brightness { get; set; }
		private ToggleButton VSync { get; set; }
		private ToggleButton Fullscreen { get; set; }
		private ToggleButton Skybox { get; set; }
		private Slider Antialiasing { get; set; }
		private ToggleButton CustomSkins { get; set; }
		private ToggleButton GraphicsMode { get; set; }
		private ToggleButton ParticleToggle { get; set; }
		private ToggleButton EntityCulling { get; set; }
		private ToggleButton Fog { get; set; }
		private Slider EntityRenderDistance { get; set; }
		private ToggleButton ShowHud { get; set; }

		public VideoOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
		{
			TitleTranslationKey = "options.videoTitle";
		}

		private bool _didInit = false;

		protected override void Initialize(IGuiRenderer renderer)
		{
			if (!_didInit)
			{
				_didInit = true;

				AddGuiRow(
					RenderDistance = CreateSlider(
						"Render Distance: {0} chunks", o => Options.VideoOptions.RenderDistance, 2, 32, 1),
					GuiScaleGlider = CreateSlider(
						v => $"GUI Scale: {((int)v == 0 ? "Auto" : v.ToString("0"))}",
						options => options.VideoOptions.GuiScale, 0, 3, 1));

				AddGuiRow(
					GraphicsMode = CreateToggle("Fancy Graphics: {0}", options => options.VideoOptions.FancyGraphics),
					Brightness = CreateSlider("Brightness: {0}%", o => Options.VideoOptions.Brightness, 0, 100, 1));

				AddGuiRow(
					Antialiasing = CreateSlider(
						v =>
						{
							string subText = $"x{v:0}";

							return $"Antialiasing: {((int)v == 0 ? "Disabled" : subText)}";
						}, options => options.VideoOptions.Antialiasing, 0, 16, 2),
					ShowHud = CreateToggle("Show HUD: {0}", o => o.VideoOptions.DisplayHud));

				AddGuiRow(
					FrameRateLimiter = CreateToggle(
						"Limit Framerate: {0}", options => options.VideoOptions.LimitFramerate),
					FpsSlider = CreateSlider(
						$"{GuiRenderer.GetTranslation("options.framerateLimit")}: {{0}} fps",
						o => Options.VideoOptions.MaxFramerate, 30, 120, 1));

				AddGuiRow(
					VSync = CreateToggle(
						$"{GuiRenderer.GetTranslation("options.vsync")}: {{0}}",
						o => { return Options.VideoOptions.UseVsync; }),
					Fullscreen = CreateToggle("Fullscreen: {0}", o => { return Options.VideoOptions.Fullscreen; }));

				// AddGuiRow(
				//      GraphicsMode = CreateToggle("Fancy Graphics: {0}", options => options.VideoOptions.FancyGraphics));

				AddGuiRow(
					Skybox = CreateToggle("Render Skybox: {0}", options => options.VideoOptions.Skybox),
					CustomSkins = CreateToggle(
						"Custom entity models: {0}", options => options.VideoOptions.CustomSkins));

				AddGuiRow(
					ParticleToggle = CreateToggle("Render Particles: {0}", options => options.VideoOptions.Particles),
					EntityCulling = CreateToggle("Entity Culling: {0}", options => options.VideoOptions.EntityCulling));

				AddGuiRow(
					Fog = CreateToggle("Render Fog: {0}", options => options.VideoOptions.Fog),
					EntityRenderDistance = CreateSlider(
						"Entity Render Distance: {0}", options => options.VideoOptions.EntityRenderDistance, 2, 32, 1));

				AddDescription(
					Fog, $"{TextColor.Bold}Render Fog:{TextColor.Reset}",
					"Renders fog to smooth out the render distance");

				AddDescription(ShowHud, "Show HUD", "");

				Descriptions.Add(
					RenderDistance,
					$"{TextColor.Bold}Render Distance:{TextColor.Reset}\n{TextColor.Red}High values may decrease performance significantly!\n");

				//  Descriptions.Add(Depthmap,
				//     $"{TextColor.Bold}Use DepthMap:{TextColor.Reset}\n{TextColor.Bold}{TextColor.Red}EXPERIMENTAL FEATURE{TextColor.Reset}\nHeavy performance impact");
				Descriptions.Add(
					Skybox,
					$"{TextColor.Bold}Render Skybox:{TextColor.Reset}\nEnabled: Renders skybox in game\nDisabled: May improve performance slightly");

				Descriptions.Add(
					Antialiasing,
					$"{TextColor.Bold}Antialiasing:{TextColor.Reset}\nImproves sharpness on textures\nMay significantly impact performance on lower-end hardware");

				Descriptions.Add(
					FrameRateLimiter,
					$"{TextColor.Bold}Limit Framerate:{TextColor.Reset}\nLimit the framerate to value set in Max Framerate slider\n");

				Descriptions.Add(
					FpsSlider,
					$"{TextColor.Bold}Max Framerate:{TextColor.Reset}\nOnly applies if Limit Framerate is set to true\nLimit's the game's framerate to set value.");

				Descriptions.Add(
					VSync,
					$"{TextColor.Bold}Use VSync:{TextColor.Reset}\nEnabled: Synchronizes the framerate with the monitor's refresh rate.\n");

				Descriptions.Add(
					CustomSkins,
					$"{TextColor.Bold}Custom entity models:{TextColor.Reset}\nEnabled: Shows custom entity models. May impact performance heavily!\nDisabled: Do not show custom models, may improve performance.");

				//     Descriptions.Add(ClientSideLighting, $"{TextColor.Bold}Client Side Lighting:{TextColor.Reset}\nEnabled: Calculate lighting on the client.\nDisabled: May improve chunk loading performance");

				//    Descriptions.Add(ChunkMeshInRam, $"{TextColor.Bold}Meshes in RAM:{TextColor.Reset}\nEnabled: May significantly improve chunk processing performance (High memory usage)\nDisabled: Do not keep chunks meshes in memory (Lower memory usage)");

				//  Descriptions.Add(SmoothLighting, $"{TextColor.Bold}Smooth Lighting:{TextColor.Reset}\nEnabled: Smoother transition in lighting.\nDisabled: May improve chunk loading performance");

				Descriptions.Add(
					GraphicsMode,
					$"{TextColor.Bold}Fancy graphics:{TextColor.Reset}\nEnabled: Use alpha blending for rendering.\nDisabled: May improve performance, no alpha blending.");

				Descriptions.Add(
					ParticleToggle,
					$"{TextColor.Bold}Render Particles:{TextColor.Reset}\nEnabled: Looks cooler, but may impact performance.\nDisabled: Hides all particles, may improve performance.");

				Descriptions.Add(
					EntityCulling,
					$"{TextColor.Bold}Entity Culling:{TextColor.Reset}\nEnabled: Culls entity models, may improve performance.\nDisabled: Do not cull entities, heavily impacts performance.");

				Descriptions.Add(
					EntityRenderDistance,
					$"{TextColor.Bold}Entity Render Distance:{TextColor.Reset}\n{TextColor.Red}High values may decrease performance significantly!\n");
			}

			base.Initialize(renderer);
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

		private void FrameRateLimiterOnValueChanged(object sender, bool e)
		{
			FpsSlider.Enabled = e;
		}
	}
}