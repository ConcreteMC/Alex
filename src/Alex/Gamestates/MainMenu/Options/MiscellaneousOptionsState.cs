using System;
using Alex.Common;
using Alex.Common.Utils;
using Alex.Gui;
using Alex.Interfaces;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gamestates.MainMenu.Options
{
	public class MiscellaneousOptionsState : OptionsStateBase
	{
		//private Slider                       NetworkProcessingThreads { get; set; }
		private Slider ProcessingThreads { get; set; }
		private ToggleButton ChunkCaching { get; set; }
		private ToggleButton ServerResources { get; set; }
		private ToggleButton NetworkDebugInfo { get; set; }
		private Slider AntiLagModifier { get; set; }
		private ToggleButton SkipFrames { get; set; }

		public MiscellaneousOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
		{
			Title = "Miscellaneous";

			Header.AddChild(
				new TextElement()
				{
					Anchor = Alignment.BottomCenter,
					Text = "WARNING: These settings might break your game!",
					TextColor = (Color)TextColor.Yellow.ToXna()
				});
			// TitleTranslationKey = "options.videoTitle";
		}

		private bool _didInit = false;

		protected override void Initialize(IGuiRenderer renderer)
		{
			if (!_didInit)
			{
				_didInit = true;


				//  NetworkProcessingThreads = CreateSlider(
				//     "Network Threads: {0}", o => o.NetworkOptions.NetworkThreads, 1, Environment.ProcessorCount,
				//      1);
				ProcessingThreads = CreateSlider(
					"Processing Threads: {0}", o => o.MiscelaneousOptions.ChunkThreads, 1, Environment.ProcessorCount,
					1);

				ServerResources = CreateToggle("Server Resources: {0}", o => o.MiscelaneousOptions.LoadServerResources);

				ChunkCaching = CreateToggle("Chunk Caching: {0}", o => o.MiscelaneousOptions.UseChunkCache);

				NetworkDebugInfo = CreateToggle(
					"Network Info: {0}", o => o.MiscelaneousOptions.ShowNetworkInfoByDefault);


				AntiLagModifier = CreateSlider(
					"Anti Lag Modifier: {0}", o => o.MiscelaneousOptions.AntiLagModifier, 0d, 1d, 0.05d);

				SkipFrames = CreateToggle("Skip Frames: {0}", o => o.MiscelaneousOptions.SkipFrames);

				AddGuiRow(ProcessingThreads, NetworkDebugInfo);
				AddGuiRow(ServerResources, ChunkCaching);
				AddGuiRow(SkipFrames, AntiLagModifier);

				AddDescription(
					ProcessingThreads, "Processing Threads",
					"The maximum amount of concurrent chunk updates to execute.",
					"If you are experiencing lag spikes, try lowering this value.");

				//  AddDescription(
				//      NetworkProcessingThreads, "Network Workers",
				//       "The amount of threads that get assigned to datagram processing",
				//      "Note: A restart is required for this setting to take affect.");

				AddDescription(
					ChunkCaching, "Chunk Caching (Bedrock Only)",
					"Reduced network traffic but increased disk I/O usage.",
					$"{TextColor.Prefix}{TextColor.Red.Code}Unstable feature, doesn't work reliably.");

				AddDescription(
					ServerResources, "Server Resources (Bedrock Only)", "Load server resource packs",
					$"{TextColor.Prefix}{TextColor.Red.Code}Experimental feature, doesn't support all features.");

				AddDescription(
					NetworkDebugInfo, "Network Latency Info", "If enabled, shows the network debug info by default.",
					"You can always press 'F3 + N' while in-game to toggle it");

				AddDescription(
					AntiLagModifier, "Anti-Lag Modifier", "Is used to determine when the game is considered laggy.");

				AddDescription(
					SkipFrames, "Skip Frames", "Skip frames when a background task takes too long to execute");
			}

			base.Initialize(renderer);
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);
		}
	}
}