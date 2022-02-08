using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alex.Audio;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Common.Data.Servers;
using Alex.Common.GameStates;
using Alex.Common.Graphics.Typography;
using Alex.Common.Input;
using Alex.Common.Resources;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Entities;
using Alex.Gamestates;
using Alex.Gamestates.Debugging;
using Alex.Gamestates.InGame;
using Alex.Gamestates.Login;
using Alex.Gamestates.MainMenu;
using Alex.Graphics.Models.Blocks;
using Alex.Gui;
using Alex.Gui.Dialogs;
using Alex.Gui.Dialogs.Containers;
using Alex.Gui.Elements;
using Alex.Items;
using Alex.Net;
using Alex.Networking.Java.Packets;
using Alex.Networking.Java.Packets.Play;
using Alex.Particles;
using Alex.Plugins;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Services;
using Alex.Services.Discord;
using Alex.Utils;
using Alex.Utils.Auth;
using Alex.Utils.Caching;
using Alex.Utils.Skins;
using Alex.Utils.Threading;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Multiplayer.Bedrock;
using Alex.Worlds.Multiplayer.Java;
using Alex.Worlds.Singleplayer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using NLog;
using RocketUI;
using RocketUI.Debugger;
using RocketUI.Input;
using RocketUI.Input.Listeners;
using RocketUI.Utilities.Extensions;
using RocketUI.Utilities.Helpers;
using SimpleInjector;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = Microsoft.Xna.Framework.Color;
using Container = SimpleInjector.Container;
using GpuResourceManager = Alex.Common.Graphics.GpuResources.GpuResourceManager;
using Image = SixLabors.ImageSharp.Image;
using Point = Microsoft.Xna.Framework.Point;
using Size = RocketUI.Size;
using SpriteBatchExtensions = RocketUI.Utilities.Extensions.SpriteBatchExtensions;
using TextInputEventArgs = Microsoft.Xna.Framework.TextInputEventArgs;

namespace Alex
{
	public class Alex : Microsoft.Xna.Framework.Game
	{
		public static int MipMapLevel = 6;
		public static bool InGame { get; set; } = false;
		public static TimeSpan DeltaTimeSpan { get; private set; } = TimeSpan.Zero;
		public static TimeSpan TotalTimeSpan { get; private set; } = TimeSpan.Zero;
		public static float DeltaTime { get; private set; } = 0f;

		public static EntityModel PlayerModel { get; set; }
		public static Image<Rgba32> PlayerTexture { get; set; }

		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Alex));

		public static string Gpu { get; private set; } = "";
		public static string OperatingSystem { get; private set; } = "";

#if DIRECTX
		public const string RenderingEngine = "DirectX";
#else
		public const string RenderingEngine = "OpenGL";
#endif

		public static string DotnetRuntime { get; } =
			$"{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}";

		//public const string Version = "1.0 DEV";

		public static bool IsMultiplayer { get; set; } = false;

		public static IFont Font;

		private SpriteBatch _spriteBatch;

		public static Alex Instance { get; private set; }
		public GameStateManager GameStateManager { get; private set; }

		public ResourceManager Resources { get; private set; }

		public InputManager InputManager { get; private set; }
		public GuiRenderer GuiRenderer { get; private set; }
		public GuiManager GuiManager { get; private set; }
		public ParticleManager ParticleManager { get; private set; }

		public GraphicsDeviceManager DeviceManager { get; }

		public ManagedTaskManager UiTaskManager { get; }

		private LaunchSettings LaunchSettings { get; }
		public PluginManager PluginManager { get; }
		public FpsMonitor FpsMonitor { get; }

		public StorageSystem Storage { get; private set; }
		public ServerTypeManager ServerTypeManager { get; private set; }
		public OptionsProvider Options { get; private set; }

		private Point WindowSize { get; set; }
		public AudioEngine AudioEngine { get; set; }

		public IServiceProvider ServiceContainer { get; private set; }

		public Alex(LaunchSettings launchSettings)
		{
			WindowSize = new Point(1280, 750);
			EntityProperty.Factory = new AlexPropertyFactory();

			Instance = this;
			LaunchSettings = launchSettings;

			OperatingSystem =
				$"{System.Runtime.InteropServices.RuntimeInformation.OSDescription} ({System.Runtime.InteropServices.RuntimeInformation.OSArchitecture})";

			DeviceManager = new GraphicsDeviceManager(this)
			{
				// PreferMultiSampling = true,
				SynchronizeWithVerticalRetrace = false,
				GraphicsProfile = GraphicsProfile.HiDef,
				PreferHalfPixelOffset = false,
				PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8
			};

			DeviceManager.PreparingDeviceSettings += (sender, args) =>
			{
				Gpu = args.GraphicsDeviceInformation.Adapter.Description;

				args.GraphicsDeviceInformation.PresentationParameters.DepthStencilFormat = DepthFormat.Depth24Stencil8;

				DeviceManager.PreferredBackBufferFormat = SurfaceFormat.Color;
				//  DeviceManager.PreferMultiSampling = true;

				DeviceManager.PreferredBackBufferWidth = WindowSize.X;
				DeviceManager.PreferredBackBufferHeight = WindowSize.Y;
			};

			Content = new StreamingContentManager(base.Services, "assets");
			IsFixedTimeStep = false;
			this.Window.AllowUserResizing = true;

			this.Window.ClientSizeChanged += (sender, args) =>
			{
				if (DeviceManager.PreferredBackBufferWidth != Window.ClientBounds.Width
				    || DeviceManager.PreferredBackBufferHeight != Window.ClientBounds.Height)
				{
					if (DeviceManager.IsFullScreen)
					{
						DeviceManager.PreferredBackBufferWidth =
							GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;

						DeviceManager.PreferredBackBufferHeight =
							GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
					}
					else
					{
						DeviceManager.PreferredBackBufferWidth = Window.ClientBounds.Width;
						DeviceManager.PreferredBackBufferHeight = Window.ClientBounds.Height;
					}

					DeviceManager.ApplyChanges();
				}
			};


			JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
			{
				Converters = new List<JsonConverter>() { new Texture2DJsonConverter(GraphicsDevice) },
				Formatting = Formatting.Indented
			};

			ServerTypeManager = new ServerTypeManager();
			PluginManager = new PluginManager();

			Storage = new StorageSystem(LaunchSettings.WorkDir);
			Storage.TryCreateDirectory("assets");
			Storage.TryCreateDirectory(Path.Combine("assets", "resourcepacks"));

			Options = new OptionsProvider(Storage);
			Options.Load();

			MojangApi.Init(Storage);
			AudioEngine = new AudioEngine(Storage, Options);

			FpsMonitor = new FpsMonitor(this);
			FpsMonitor.UpdateOrder = 0;
			Components.Add(FpsMonitor);

			UiTaskManager = new ManagedTaskManager(this);
			Components.Add(UiTaskManager);

			TextureUtils.RenderThread = Thread.CurrentThread;
			TextureUtils.QueueOnRenderThread = (action, tag) => UiTaskManager.Enqueue(action, setup =>
			{
				setup.Tag = tag;
			});
		}

		/// <inheritdoc />
		protected override void OnExiting(object sender, EventArgs args)
		{
			//GpuResourceManager.ReportIncorrectlyDisposedBuffers = false;
			base.OnExiting(sender, args);
		}

		private void Window_TextInput(object sender, TextInputEventArgs e)
		{
			if (char.IsLetterOrDigit(e.Character) || char.IsPunctuation(e.Character) || char.IsSymbol(e.Character)
			    || char.IsWhiteSpace(e.Character))
			{
				if (e.Key == Keys.Tab)
					return;

				GuiManager.FocusManager.OnTextInput(this, e);
			}
		}

		private void WindowOnKeyDown(object? sender, InputKeyEventArgs e)
		{
			if (!e.Key.TryConvertKeyboardInput(out _))
			{
				var focusedElement = GuiManager.FocusManager.FocusedElement;

				if (focusedElement != null)
				{
					focusedElement.InvokeKeyInput('\0', e.Key);
				}
			}
		}

		protected override void Initialize()
		{
			Window.Title = $"Alex - {VersionUtils.GetVersion()} - {RenderingEngine}";
			Mouse.WindowHandle = Window.Handle;
			// InitCamera();
			this.Window.TextInput += Window_TextInput;
			this.Window.KeyDown += WindowOnKeyDown;

			if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				var currentAdapter = GraphicsAdapter.Adapters.FirstOrDefault(x => x == GraphicsDevice.Adapter);

				if (currentAdapter != null)
				{
					if (currentAdapter.IsProfileSupported(GraphicsProfile.HiDef))
					{
						DeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
					}
				}
			}

			base.InactiveSleepTime = TimeSpan.Zero;
			// base.InactiveSleepTime = TimeSpan.FromSeconds(1d / 30d); //Limit framerate to 30fps when window inactive

			GraphicsDevice.PresentationParameters.MultiSampleCount = 8;

			DeviceManager.ApplyChanges();

			base.Initialize();
			// RichPresenceProvider.Initialize();
		}

		private void RegisterServiceContainer()
		{
			SimpleInjector.Container container = new SimpleInjector.Container();
			container.Options.ResolveUnregisteredConcreteTypes = true;

			container.RegisterInstance<Game>(this);
			container.RegisterInstance<Alex>(this);
			container.RegisterInstance<IGraphicsDeviceService>(DeviceManager);
			container.RegisterInstance<GraphicsDevice>(GraphicsDevice);
			container.RegisterSingleton<IGuiRenderer, GuiRenderer>();
			container.RegisterSingleton<GuiManager>();
			container.RegisterSingleton<InputManager>();
			container.RegisterInstance<IServiceProvider>(container);

			container.RegisterSingleton<JavaServerQueryProvider>();
			container.RegisterSingleton<BedrockServerQueryProvider>();

			container.Register<IRegistryManager, RegistryManager>(Lifestyle.Singleton);

			container.RegisterSingleton<ResourceManager>();
			container.RegisterInstance<ServerTypeManager>(ServerTypeManager);
			container.Register<XboxAuthService>(Lifestyle.Singleton);

			container.Register<BlobCache>(Lifestyle.Singleton);
			container.Register<ResourcePackCache>(Lifestyle.Singleton);

			container.RegisterInstance<ContentManager>(Content);
			container.RegisterInstance<IStorageSystem>(Storage);
			container.RegisterInstance<IOptionsProvider>(Options);
			container.RegisterInstance<Audio.AudioEngine>(AudioEngine);

			container.RegisterSingleton<GuiPanoramaSkyBox>();

			// RocketUI
			container.Collection.Register<IInputListenerFactory>(
				new Type[]
				{
					typeof(AlexKeyboardInputListenerFactory), typeof(AlexMouseInputListenerFactory),
					typeof(AlexGamePadInputListenerFactory)
				}, Lifestyle.Singleton);

			container.Collection.Register<IHostedService>(typeof(Alex).Assembly);

			if (LaunchSettings.RocketDebugging)
			{
				container.RegisterSingleton<RocketDebugSocketServer>();
				container.Collection.Append<IHostedService, RocketDebugSocketServer>();
			}

			PluginManager.Initiate(container, Options, LaunchSettings);

			container.Verify();
			Services.AddService<IServiceProvider>(container);

			ServiceContainer = container;
			PluginManager.Setup(ServiceContainer);
		}

		protected override void LoadContent()
		{
			Stopwatch loadingStopwatch = Stopwatch.StartNew();

			RocketUI.GpuResourceManager.Init(GraphicsDevice);
			SpriteBatchExtensions.Init(GraphicsDevice);
			Common.Extensions.Init(GraphicsDevice);

			var builtInFont = ResourceManager.ReadResource("Alex.Resources.default_font.png");

			var image = Image.Load<Rgba32>(builtInFont);
			OnResourcePackPreLoadCompleted(image, MCJavaResourcePack.BitmapFontCharacters);

			var options = Options;

#if DIRECTX
            ResourceManager.EntityEffect = Content.Load<Effect>("Alex.Resources.Entityshader_dx.xnb").Clone();
			ResourceManager.BlockEffect = Content.Load<Effect>("Alex.Resources.Blockshader_dx.xnb").Clone();
			ResourceManager.LightingEffect = Content.Load<Effect>("Alex.Resources.Lightmap_dx.xnb").Clone();
#else
			ResourceManager.EntityEffect = Content.Load<Effect>("Alex.Resources.Entityshader.xnb").Clone();
			ResourceManager.BlockEffect = Content.Load<Effect>("Alex.Resources.Blockshader.xnb").Clone();
			ResourceManager.LightingEffect = Content.Load<Effect>("Alex.Resources.Lightmap.xnb").Clone();
#endif
			//	ResourceManager.BlockEffect.GraphicsDevice = GraphicsDevice;

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			RegisterServiceContainer();

			Resources = ServiceContainer.GetRequiredService<ResourceManager>();
			GuiRenderer = (GuiRenderer)ServiceContainer.GetRequiredService<IGuiRenderer>();
			GuiManager = ServiceContainer.GetRequiredService<GuiManager>();
			GuiManager.Enabled = true;
			GuiManager.DrawOrder = 100;

			GuiManager.ScaledResolution.TargetWidth = 320;
			GuiManager.ScaledResolution.TargetHeight = 240;
			GuiManager.ScaledResolution.GuiScale = Options.AlexOptions.VideoOptions.GuiScale.Value;
			Options.AlexOptions.VideoOptions.GuiScale.Bind(GuiScaleChanged);

			InputManager = GuiManager.InputManager;
			Components.Add(GuiManager);
			Components.Add(GuiManager.InputManager);

			GuiManager.Init();
			//GuiRenderer = (GuiRenderer)GuiManager.GuiRenderer;
			InputManager.GetOrAddPlayerManager(PlayerIndex.One); // Init player1's playermanager

			options.AlexOptions.VideoOptions.FancyGraphics.Bind(
				(value, newValue) => { Block.FancyGraphics = newValue; });

			Block.FancyGraphics = options.AlexOptions.VideoOptions.FancyGraphics.Value;

			options.AlexOptions.VideoOptions.UseVsync.Bind((value, newValue) => { SetVSync(newValue); });

			if (options.AlexOptions.VideoOptions.UseVsync.Value)
			{
				SetVSync(true);
			}

			options.AlexOptions.VideoOptions.Fullscreen.Bind((value, newValue) => { SetFullscreen(newValue); });

			if (options.AlexOptions.VideoOptions.Fullscreen.Value)
			{
				SetFullscreen(true);
			}

			options.AlexOptions.VideoOptions.LimitFramerate.Bind(
				(value, newValue) =>
				{
					SetFrameRateLimiter(newValue, options.AlexOptions.VideoOptions.MaxFramerate.Value);
				});

			options.AlexOptions.VideoOptions.MaxFramerate.Bind(
				(value, newValue) =>
				{
					SetFrameRateLimiter(options.AlexOptions.VideoOptions.LimitFramerate.Value, newValue);
				});

			if (options.AlexOptions.VideoOptions.LimitFramerate.Value)
			{
				SetFrameRateLimiter(true, options.AlexOptions.VideoOptions.MaxFramerate.Value);
			}

			options.AlexOptions.VideoOptions.Antialiasing.Bind(
				(value, newValue) => { SetAntiAliasing(newValue > 0, newValue); });

			options.AlexOptions.MiscelaneousOptions.Language.Bind(
				(value, newValue) => { GuiRenderer.SetLanguage(newValue); });

			options.AlexOptions.VideoOptions.SmoothLighting.Bind(
				(value, newValue) => { ResourcePackBlockModel.SmoothLighting = newValue; });

			ResourcePackBlockModel.SmoothLighting = options.AlexOptions.VideoOptions.SmoothLighting.Value;

			SetAntiAliasing(
				options.AlexOptions.VideoOptions.Antialiasing > 0, options.AlexOptions.VideoOptions.Antialiasing.Value);

			Components.Add(new GuiDebugHelper(this, GuiManager));

			GameStateManager = new GameStateManager(this, GraphicsDevice, _spriteBatch);
			GameStateManager.DrawOrder = 0;
			GameStateManager.UpdateOrder = 0;
			Components.Add(GameStateManager);

			var splash = new SplashScreen();
			GameStateManager.AddState("splash", splash);
			GameStateManager.SetActiveState(splash, false, false);

			ParticleManager = new ParticleManager(this, GraphicsDevice, Resources);
			ParticleManager.Enabled = Options.AlexOptions.VideoOptions.Particles.Value;

			Options.AlexOptions.VideoOptions.Particles.Bind((old, newValue) => { ParticleManager.Enabled = newValue; });

			Components.Add(ParticleManager);

			var skyBox = ServiceContainer.GetRequiredService<GuiPanoramaSkyBox>();
			skyBox.Enabled = true;
			Components.Add(skyBox);

			// GuiManager.ShowDialog(new BrowserDialog());
			//	Log.Info($"Initializing Alex...");
			ThreadPool.QueueUserWorkItem(
				(o) =>
				{
					try
					{
						Task.WaitAll(
							ServiceContainer.GetServices<IHostedService>()
							   .Select(s => s.StartAsync(CancellationToken.None)).ToArray());

						InitializeGame(splash).Wait();
					}
					catch (Exception ex)
					{
						Log.Error(ex, $"Could not initialize! {ex}");
					}
					finally
					{
						loadingStopwatch.Stop();
						Log.Info($"Startup time: {loadingStopwatch.Elapsed}");
					}
				});
		}

		private void GuiScaleChanged(int oldvalue, int newvalue)
		{
			GuiManager.ScaledResolution.GuiScale = newvalue;
		}

		private void SetAntiAliasing(bool enabled, int count)
		{
			UiTaskManager.Enqueue(
				() =>
				{
					// DeviceManager.PreferMultiSampling = enabled;
					GraphicsDevice.PresentationParameters.MultiSampleCount = count;

					DeviceManager.ApplyChanges();
				});
		}

		internal void ResetFrameRateLimiter()
		{
			SetFrameRateLimiter(
				Options.AlexOptions.VideoOptions.LimitFramerate.Value,
				Options.AlexOptions.VideoOptions.MaxFramerate.Value);
		}

		internal void SetFrameRateLimiter(bool enabled, int frameRateLimit)
		{
			base.IsFixedTimeStep = enabled;
			base.TargetElapsedTime = TimeSpan.FromSeconds(1d / frameRateLimit);
		}

		private void SetVSync(bool enabled)
		{
			UiTaskManager.Enqueue(
				() =>
				{
					DeviceManager.SynchronizeWithVerticalRetrace = enabled;
					DeviceManager.PreferredBackBufferWidth = WindowSize.X;
					DeviceManager.PreferredBackBufferHeight = WindowSize.Y;
					DeviceManager.ApplyChanges();
				});
		}

		private void SetFullscreen(bool enabled)
		{
			UiTaskManager.Enqueue(
				() =>
				{
					if (this.DeviceManager.IsFullScreen != enabled)
					{
						if (enabled)
						{
							WindowSize = Window.ClientBounds.Size;
						}
						else
						{
							DeviceManager.PreferredBackBufferWidth = WindowSize.X;
							DeviceManager.PreferredBackBufferHeight = WindowSize.Y;
							this.DeviceManager.ApplyChanges();
						}

						this.DeviceManager.IsFullScreen = enabled;
						this.DeviceManager.ApplyChanges();
					}
				});
		}

		protected override void UnloadContent()
		{
			base.UnloadContent();
			//ProfileManager.SaveProfiles();
			Options.Save();

			// GuiDebugHelper.Dispose();

			PluginManager.UnloadAll();

			RichPresenceProvider.ClearPresence();
		}

		public EventHandler OnBeginDraw;
		public EventHandler OnEndDraw;

		/// <inheritdoc />
		protected override bool BeginDraw()
		{
			OnBeginDraw?.Invoke(this, EventArgs.Empty);

			return base.BeginDraw();
		}

		/// <inheritdoc />
		protected override void EndDraw()
		{
			Metrics = GraphicsDevice.Metrics;
			OnEndDraw?.Invoke(this, EventArgs.Empty);
			base.EndDraw();
		}

		/// <inheritdoc />
		protected override void Update(GameTime gameTime)
		{
			long elapsedGameTimeTicks = gameTime.ElapsedGameTime.Ticks;
			DeltaTimeSpan = TimeSpan.FromTicks(elapsedGameTimeTicks);
			TotalTimeSpan += DeltaTimeSpan;
			DeltaTime = ((float)elapsedGameTimeTicks / TimeSpan.TicksPerSecond);

			base.Update(gameTime);

			GpuResourceManager.Update(gameTime, GraphicsDevice);
		}

		public GraphicsMetrics Metrics { get; private set; }

		private Task InitializeGame(IProgressReceiver progressReceiver)
		{
			progressReceiver.UpdateProgress(0, "Initializing...");
			MCPacketFactory.Load();
			//ConfigureServices();

			//var options = Services.GetService<IOptionsProvider>();

			//	Log.Info($"Loading resources...");
			var previousValue = base.IsFixedTimeStep;
			base.IsFixedTimeStep = false;

			if (!Resources.CheckResources(GraphicsDevice, progressReceiver, OnResourcePackPreLoadCompleted))
			{
				Console.WriteLine("Press enter to exit...");
				Console.ReadLine();
				Exit();

				return Task.CompletedTask;
			}

			base.IsFixedTimeStep = previousValue;

			ServerTypeManager.TryRegister("java", new JavaServerType(this));

			ServerTypeManager.TryRegister("bedrock", new BedrockServerType(this));

			//GuiRenderer.LoadResourcePack(Resources.ResourcePack, null);
			AnvilWorldProvider.LoadBlockConverter();

			PluginManager.LoadPlugins();
			PluginManager.EnablePlugins();

			var storage = ServiceContainer.GetRequiredService<IStorageSystem>();

			if (storage.TryReadBytes("skin.json", out var bytes))
			{
				Dictionary<string, EntityModel> models = new Dictionary<string, EntityModel>();
				MCBedrockResourcePack.LoadEntityModel(Encoding.UTF8.GetString(bytes), models);
				models = MCBedrockResourcePack.ProcessEntityModels(models);

				if (models.Count == 1)
				{
					PlayerModel = models.First().Value;
				}
				else if (models.Count > 0)
				{
					if (models.TryGetValue("geometry.humanoid.custom", out var entityModel) || models.TryGetValue(
						    "geometry.humanoid.customSlim", out entityModel))
					{
						PlayerModel = entityModel;
					}
				}
			}

			if (PlayerModel == null)
			{
				if (ModelFactory.TryGetModel("geometry.humanoid.customSlim", out var model))
				{
					//model.Name = "geometry.humanoid.customSlim";
					PlayerModel = model;
				}
			}
			else
			{
				Log.Info($"Player model loaded...");
			}

			if (PlayerModel != null && storage.TryReadBytes("skin.png", out byte[] skinBytes))
			{
				using (var skinImage = Image.Load<Rgba32>(skinBytes))
				{
					//var modelTextureSize = new Point(0, 0);

					if (PlayerModel.Description != null)
					{
						//      modelTextureSize.X = (int) PlayerModel.Description.TextureWidth;
						//     modelTextureSize.Y = (int) PlayerModel.Description.TextureHeight;
					}

					//  if (modelTextureSize.Y > skinImage.Height)
					//  {
					//      PlayerTexture = SkinUtils.ConvertSkin(skinImage, modelTextureSize.X, modelTextureSize.Y);
					// }
					// else
					{
						PlayerTexture = skinImage.Clone(); //.Clone<Rgba32>();
					}

					Log.Info($"Player skin loaded...");
				}
			}
			else
			{
				if (Resources.TryGetBitmap("entity/alex", out var img))
				{
					PlayerTexture = img;
				}
			}

			if (LaunchSettings.ModelDebugging)
			{
				GameStateManager.SetActiveState<ModelDebugState>(false, false);
			}
			else
			{
				GameStateManager.SetActiveState<TitleState>(false, false);
			}

			return Task.CompletedTask;
		}

		private void OnResourcePackPreLoadCompleted(Image<Rgba32> fontBitmap, string[] bitmapCharacters)
		{
			UiTaskManager.Enqueue(
				() =>
				{
					Font = new BitmapFont(
						GraphicsDevice, fontBitmap, bitmapCharacters[0].Length, bitmapCharacters.Length,
						bitmapCharacters);

					GuiManager.ApplyFont(Font);
				});
		}

		public void ConnectToServer(ServerTypeImplementation serverType,
			ServerConnectionDetails connectionDetails,
			PlayerProfile profile)
		{
			try
			{
				WorldProvider provider;
				NetworkProvider networkProvider;
				IsMultiplayer = true;

				if (serverType.TryGetWorldProvider(connectionDetails, profile, out provider, out networkProvider))
				{
					LoadWorld(provider, networkProvider, true);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"FCL: {ex.ToString()}");
			}
		}

		public void LoadWorld(WorldProvider worldProvider, NetworkProvider networkProvider, bool isServer = false)
		{
			var playState = new PlayingState(this, GraphicsDevice, worldProvider, networkProvider);
			var loadingDialog = GuiManager.CreateDialog<WorldLoadingDialog>();
			loadingDialog.ConnectingToServer = isServer;

			loadingDialog.CancelAction = () =>
			{
				loadingDialog.Close();
				worldProvider?.Dispose();

				GameStateManager.RemoveState(PlayingState.Key);
				IsMouseVisible = true;
			};

			//GuiManager.AddScreen(worldLoadingScreen);

			ThreadPool.QueueUserWorkItem(
				o =>
				{
					bool wasFixed = IsFixedTimeStep;
					IsFixedTimeStep = false;
					LoadResult result = LoadResult.Timeout;

					try
					{
						GameStateManager.RemoveState(PlayingState.Key);

						result = worldProvider.Load(loadingDialog.UpdateProgress);
						GameStateManager.AddState(PlayingState.Key, playState);

						if (networkProvider.IsConnected && result == LoadResult.Done)
						{
							var currentState = GameStateManager.GetActiveState();
							GameStateManager.SetActiveState(PlayingState.Key, false);
							GameStateManager.RemoveState(currentState);
							IsMouseVisible = false;
						}
					}
					finally
					{
						loadingDialog.Close();

						if (result != LoadResult.Done)
						{
							IsFixedTimeStep = wasFixed;
							if (result != LoadResult.Aborted && result != LoadResult.Kicked
							                                 && result != LoadResult.VersionMismatch)
							{
								DisconnectedDialog.Show(this, "multiplayer.status.cannot_connect", true);
							}

							worldProvider?.Dispose();
							GameStateManager.RemoveState(PlayingState.Key);
						}
					}
				});
		}
	}
}