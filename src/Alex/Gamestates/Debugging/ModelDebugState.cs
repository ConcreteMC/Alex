using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Alex.API;
using Alex.API.Graphics;
using Alex.API.Resources;
using Alex.API.Utils;
using Alex.API.Utils.Vectors;
using Alex.Blocks;
using Alex.Blocks.State;
using Alex.Entities;
using Alex.Gamestates.Common;
using Alex.Gamestates.InGame;
using Alex.Graphics.Models.Entity;
using Alex.Gui.Elements;
using Alex.Gui.Elements.Context3D;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Worlds;
using Alex.Worlds.Chunks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RocketUI;
using Color = Microsoft.Xna.Framework.Color;
using DateTime = System.DateTime;
using GpuResourceManager = Alex.API.Graphics.GpuResourceManager;
using Task = System.Threading.Tasks.Task;

namespace Alex.Gamestates.Debugging
{
	public class ModelDebugState : GuiGameStateBase
	{
		private StackContainer _wrap;
		private GuiModelExplorerView _modelExplorerView;
		private readonly StackMenu _mainMenu;

		//  private FirstPersonCamera Camera { get; } = new FirstPersonCamera(16, Vector3.Zero, Vector3.Zero);

		private readonly GuiDebugInfo _debugInfo;
		private          long         _ramUsage = 0;
		private          long         _threadsUsed, _maxThreads, _complPortUsed, _maxComplPorts;

		//private DebugModelRenderer _modelRenderer;

		public ModelDebugState()
		{
			_debugInfo = new GuiDebugInfo();

			Background = Color.DeepSkyBlue;

			BlockModelExplorer  = new BlockModelExplorer(Alex.Instance, null);
			EntityModelExplorer = new EntityModelExplorer(Alex.Instance, null);

			ModelExplorer = BlockModelExplorer;
			
			AddChild(_modelExplorerView = new GuiModelExplorerView(ModelExplorer, new Vector3(8f, 0f, 8f), new Vector3(0f, 0f, 0f))
			{
				Anchor = Alignment.Fill,
				Background = Color.TransparentBlack,
				BackgroundOverlay = new Color(Color.Black, 0.15f),

				Margin = new Thickness(0),

				Width  = 92,
				Height = 128,

				AutoSizeMode = AutoSizeMode.GrowAndShrink,

				//Anchor = Alignment.BottomRight,

				// Width = 100,
				// Height = 100
			});

			AddChild(_mainMenu = new StackMenu()
			{
				Margin  = new Thickness(0, 0,  15, 0),
				Padding = new Thickness(0, 50, 0,  0),
				Width   = 125,
				Anchor  = Alignment.FillY | Alignment.MinX,

				ChildAnchor       = Alignment.CenterY | Alignment.FillX,
				BackgroundOverlay = new Color(Color.Black, 0.35f)
			});

			//_wrap.AddChild(_modelRenderer = new DebugModelRenderer(Alex)
			//{
			//	Anchor = Alignment.Fill,
			//	// Width = 100,
			//	// Height = 100
			//});



			_mainMenu.AddMenuItem("Skip",          () => { Task.Run(() => { ModelExplorer.Skip(); }); });
			_mainMenu.AddMenuItem("Next",          NextModel);
			_mainMenu.AddMenuItem("Previous",      PrevModel);
			_mainMenu.AddMenuItem("Switch Models", () =>
			{
				SwitchModelExplorers();
				_modelExplorerView.ModelExplorer = ModelExplorer;
			});

			//AddChild(_mainMenu);

			_debugInfo.AddDebugRight(() => Alex.DotnetRuntime);
			//_debugInfo.AddDebugRight(() => MemoryUsageDisplay);
			_debugInfo.AddDebugRight(() => $"RAM: {PlayingState.GetBytesReadable(_ramUsage, 2)}");
			_debugInfo.AddDebugRight(() =>
										 $"GPU: {PlayingState.GetBytesReadable(GpuResourceManager.GetMemoryUsage, 2)}");
			_debugInfo.AddDebugRight(() =>
			{
				return
					$"Threads: {(_threadsUsed):00}/{_maxThreads}\nCompl.ports: {_complPortUsed:00}/{_maxComplPorts}";
			});

			_debugInfo.AddDebugRight(() =>
			{
				if (ModelExplorer == null)
					return string.Empty;

				return ModelExplorer.GetDebugInfo();
			});

			_keyState = Keyboard.GetState();
		}
		
		public ModelExplorer ModelExplorer { get; set; }
		private BlockModelExplorer BlockModelExplorer { get; }
		private EntityModelExplorer EntityModelExplorer { get; }

		public void SwitchModelExplorers()
		{
			if (ModelExplorer == BlockModelExplorer)
			{
				ModelExplorer = EntityModelExplorer;
			}
			else if (ModelExplorer == EntityModelExplorer)
			{
				ModelExplorer = BlockModelExplorer;
			}
		}


		protected override void OnShow()
		{
			base.OnShow();
			Alex.GuiManager.AddScreen(_debugInfo);

			Alex.IsMouseVisible = true;
		}

		protected override void OnHide()
		{
			base.OnHide();
			Alex.GuiManager.RemoveScreen(_debugInfo);

			Alex.IsMouseVisible = false;
		}

		private void PrevModel()
		{
			ModelExplorer.Previous();
		}

		private void NextModel()
		{
			ModelExplorer.Next();
		}

		private DateTime _previousMemUpdate = DateTime.UtcNow;

		//private Vector3 _rotation = Vector3.Zero;
		private KeyboardState _keyState = default;
		private float         _i        = 0;

		protected override void OnUpdate(GameTime gameTime)
		{
			if (_modelExplorerView.Width != RootScreen.Width || _modelExplorerView.Height != RootScreen.Height)
			{
				_modelExplorerView.Width  = RootScreen.Width;
				_modelExplorerView.Height = RootScreen.Height;
			}

			base.OnUpdate(gameTime);
			
			var now = DateTime.UtcNow;
			if (now - _previousMemUpdate > TimeSpan.FromSeconds(5))
			{
				_previousMemUpdate = now;

				//Task.Run(() =>
				{
					_ramUsage = Environment.WorkingSet;

					ThreadPool.GetMaxThreads(out int maxThreads, out int maxCompletionPorts);
					ThreadPool.GetAvailableThreads(out int availableThreads, out int availableComplPorts);
					_threadsUsed   = maxThreads - availableThreads;
					_complPortUsed = maxCompletionPorts - availableComplPorts;

					_maxThreads    = maxThreads;
					_maxComplPorts = maxCompletionPorts;
				} //);
			}

/*
	//		var location = _modelExplorerView.EntityPosition;
			var keyState = Keyboard.GetState();

			// if (keyState != _keyState)
			{
				var modifier = (keyState.IsKeyDown(Keys.LeftShift) ? 3f : 1f);
				if (keyState.IsKeyDown(Keys.LeftControl))
					modifier *= 10f;

				var delta = (float) gameTime.ElapsedGameTime.TotalSeconds * modifier;

				if (keyState.IsKeyDown(Keys.LeftAlt))
				{
					//var rotation = _modelExplorerView.Rotation;
					//if (keyState.IsKeyDown(Keys.W))
					//{
					//	rotation.X += delta;
					//}
					//else if (keyState.IsKeyDown(Keys.S))
					//{
					//	rotation.X -= delta;
					//}

					//if (keyState.IsKeyDown(Keys.A))
					//{
					//	rotation.Y += delta;
					//}
					//else if (keyState.IsKeyDown(Keys.D))
					//{
					//	rotation.Y -= delta;
					//}

					//_modelRenderer.CameraRotation = rotation;
				}
				else
				{
					var pitch = location.Pitch;
					var yaw = location.Yaw;
					var headYaw = location.HeadYaw;

					if (keyState.IsKeyDown(Keys.W))
					{
						pitch += delta;
					}
					else if (keyState.IsKeyDown(Keys.S))
					{
						pitch -= delta;
					}

					if (keyState.IsKeyDown(Keys.A))
					{
						yaw += delta;
						headYaw += delta;
					}
					else if (keyState.IsKeyDown(Keys.D))
					{
						yaw -= delta;
						headYaw -= delta;
					}

					_modelExplorerView.SetEntityRotation(yaw, pitch, headYaw);
				}

				if (keyState.IsKeyUp(Keys.R) && _keyState.IsKeyDown(Keys.R))
				{
					//_rotation = Vector3.Zero;
				}
			}

			// _i += (float) gameTime.ElapsedGameTime.TotalSeconds;

			//_modelRenderer.EntityPosition = location; // new PlayerLocation(Math.Cos(_i) * 6, 0, Math.Sin(_i) * 6);

			_keyState = keyState;*/

		}
	}

	public class EntityModelExplorer : ModelExplorer
	{
		private KeyValuePair<ResourceLocation, EntityDescription>[] _entityDefinitions;
		private int                                                          _index = 0;

		private GraphicsDevice GraphicsDevice { get; }
		private Alex           Alex           { get; }
		private World          World          { get; }

		public EntityModelExplorer(Alex alex, World world)
		{
			Alex           = alex;
			World          = world;
			GraphicsDevice = alex.GraphicsDevice;

			_entityDefinitions = alex.Resources.BedrockResourcePack.EntityDefinitions.ToArray();
		}

		private EntityModelRenderer _currentRenderer = null;

		private void SetVertices()
		{
			var def = _entityDefinitions[_index];

			EntityModelRenderer renderer = null;

			//if (def.Value != null)
			//{
			//	renderer = EntityFactory.GetEntityRenderer(def.Key, null);
			//}

			if (def.Value != null && def.Value.Geometry != null && def.Value.Geometry.ContainsKey("default") &&
				def.Value.Textures != null)
			{
				EntityModel model;
				if (ModelFactory.TryGetModel(def.Value.Geometry["default"],
											 out model) && model != null)
				{
					var textures = def.Value.Textures;
					string texture;
					if (!textures.TryGetValue("default", out texture))
					{
						texture = textures.FirstOrDefault().Value;
					}

					if (Alex.Resources.BedrockResourcePack.Textures.TryGetValue(texture,
																				out var bmp))
					{
						PooledTexture2D t = TextureUtils.BitmapToTexture2D(Alex.GraphicsDevice, bmp.Value);

						renderer = new EntityModelRenderer(model, t);
						renderer.Scale = 1f / 16f;
					}
				}
			}

			_currentRenderer = renderer;
		}

		public override void Next()
		{
			if (_index + 1 >= _entityDefinitions.Length)
			{
				_index = 0;
			}
			else
			{
				_index++;
			}

			SetVertices();
		}

		public override void Previous()
		{
			if (_index - 1 < 0)
			{
				_index = _entityDefinitions.Length - 1;
			}
			else
			{
				_index--;
			}

			SetVertices();
		}

		public override void Skip()
		{
			return;
			int start        = _index;
			var currentState = _entityDefinitions[start];

			for (int i = start; i < _entityDefinitions.Length; i++)
			{
				var state = _entityDefinitions[i];
				/*if (!string.Equals(currentState.Name, state.Name))
				{
					_index = i;
					SetVertices();
					break;
				}*/
			}
		}

		//private PlayerLocation Location { get; set; } = new PlayerLocation(Vector3.Zero);

		public override void SetLocation(PlayerLocation location)
		{
			//Location = location;

			//  Location.Yaw = Location.HeadYaw = MathUtils.RadianToDegree(rotation.Y);
			//  Location.Pitch = MathUtils.RadianToDegree(rotation.Z);
		}

		private int _previousIndex = -1;

		public override string GetDebugInfo()
		{
			var block = _entityDefinitions[_index];

			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"{block.Key}");

			return sb.ToString();
		}
		
		private float _rot = 0f;
		/// <inheritdoc />
		public override void UpdateContext3D(IUpdateArgs args, IGuiRenderer guiRenderer)
		{
			_rot += (float)args.GameTime.ElapsedGameTime.TotalSeconds;
			_currentRenderer?.Update(args, new PlayerLocation(args.Camera.Position, 16f * _rot % 360f, 16f * _rot % 360f, 8f * _rot % 360f));
		}

		/// <inheritdoc />
		public override void DrawContext3D(IRenderArgs args, IGuiRenderer guiRenderer)
		{
			_currentRenderer?.Render(args, false);
		}
	}

	public class BlockModelExplorer : ModelExplorer
	{
		private BlockState[] _blockStates;
		private int           _index = 0;

		private GraphicsDevice GraphicsDevice { get; }
		private Alex           Alex           { get; }
		private World          World          { get; }

		public BlockModelExplorer(Alex alex, World world)
		{
			Alex           = alex;
			World          = world;
			GraphicsDevice = alex.GraphicsDevice;

			_blockStates = BlockFactory.AllBlockstates.Values.ToArray();
		}

		//private AlphaTestEffect _alphaEffect = null;
		private BasicEffect     _basicEffect = null;
		private Effect          _currentEffect;

		private ChunkData _data = null;
		private void SetVertices()
		{
			var b     = _blockStates[_index];
			var world = new ItemRenderingWorld(b.Block);
				
			var       old       = _data;
			ChunkData chunkData = new ChunkData(null, ChunkCoordinates.Zero);
			b.VariantMapper.Model.GetVertices(world, chunkData, _location.GetCoordinates3D(), _location, b);

			_data = chunkData;
			_data.ApplyChanges(Alex.GraphicsDevice, true);

			old?.Dispose();
			// _vertices = vertices.vertices;
			// _indices = vertices.indexes;
		}

		public override void Next()
		{
			if (_index + 1 >= _blockStates.Length)
			{
				_index = 0;
			}
			else
			{
				_index++;
			}

			SetVertices();
		}

		public override void Previous()
		{
			if (_index - 1 < 0)
			{
				_index = _blockStates.Length - 1;
			}
			else
			{
				_index--;
			}

			SetVertices();
		}

		public override void Skip()
		{
			int start        = _index;
			var currentState = _blockStates[start];

			for (int i = start; i < _blockStates.Length; i++)
			{
				var state = _blockStates[i];
				if (!string.Equals(currentState.Name, state.Name))
				{
					_index = i;
					SetVertices();
					break;
				}
			}
		}

		public override void SetLocation(PlayerLocation location)
		{
			// _rotation.Y = MathUtils.RadianToDegree(rotation.Y);
			//   _rotation.Z = MathUtils.RadianToDegree(rotation.Z);
			//  _rotation.X = MathUtils.RadianToDegree(rotation.X);
			_location = location;
		}

		private float _rot = 0f;
		/// <inheritdoc />
		public override void UpdateContext3D(IUpdateArgs args, IGuiRenderer guiRenderer)
		{
			_rot += (float)args.GameTime.ElapsedGameTime.TotalSeconds;
			
			if (_basicEffect == null)
			{
				_basicEffect                    = new BasicEffect(Alex.GraphicsDevice);
				_basicEffect.VertexColorEnabled = true;
				_basicEffect.TextureEnabled     = true;
				_basicEffect.LightingEnabled = false;
				_basicEffect.FogEnabled = false;
			}

			var offset = new Vector3(0.5f, 0.5f, 0.5f);
		 _basicEffect.Projection = args.Camera.ProjectionMatrix;
			 _basicEffect.View       = args.Camera.ViewMatrix;

			 _basicEffect.World = Matrix.CreateScale(1f / 16f)
			                                          * Matrix.CreateRotationY(MathUtils.ToRadians(18f * _rot % 360f))
			                                          * Matrix.CreateRotationX(MathUtils.ToRadians(8f * _rot % 360f));

			var block = _blockStates[_index];

			if (_index != _previousIndex)
			{
				//if (block.Block.Animated)
				//{
				//	_alphaEffect.Texture = Alex.Resources.Atlas.GetAtlas(0);
					//_basicEffect.Texture = Alex.Resources.Atlas.GetAtlas(0);
				//}
				//else
				//{
				//	_alphaEffect.Texture = Alex.Resources.Atlas.GetStillAtlas();
					_basicEffect.Texture = Alex.Resources.Atlas.GetStillAtlas();
				//}

				_previousIndex = _index;
			}

			_currentEffect =  _basicEffect;
		}

		/// <inheritdoc />
		public override void DrawContext3D(IRenderArgs args, IGuiRenderer guiRenderer)
		{
			if (_data == null)
				return;

			var data = _data;

			if (_currentEffect != null)
			{
				//context.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
				//context.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

				data.Draw(args.GraphicsDevice, RenderStage.OpaqueFullCube, _currentEffect);
				data.Draw(args.GraphicsDevice, RenderStage.Opaque, _currentEffect);
				data.Draw(args.GraphicsDevice, RenderStage.Transparent, _currentEffect);
				data.Draw(args.GraphicsDevice, RenderStage.Translucent, _currentEffect);
				data.Draw(args.GraphicsDevice, RenderStage.Animated, _currentEffect);
				data.Draw(args.GraphicsDevice, RenderStage.Liquid, _currentEffect);
			}
		}

		private static Vector3        _rotationCenter = Vector3.One / 2f;
		private        PlayerLocation _location       = new PlayerLocation(Vector3.Zero);
		private        int            _previousIndex  = -1;

		public override string GetDebugInfo()
		{
			var block = _blockStates[_index];

			StringBuilder sb = new StringBuilder();

			if (block != null)
			{
				sb.AppendLine($"{block.Name}");
				
				var dict = block.ToDictionary();
				foreach (var kv in dict)
				{
					sb.AppendLine($"{kv.Key}={kv.Value}");
				}
			}

			return sb.ToString();
		}
	}

	public abstract class ModelExplorer : IGuiContext3DDrawable
	{
		public abstract void   Next();
		public abstract void   Previous();
		public abstract void   Skip();
		//public abstract void   Render(GraphicsContext context, RenderArgs renderArgs);
	//	public abstract void   Update(UpdateArgs      args);
		public abstract string GetDebugInfo();

		public virtual void SetLocation(PlayerLocation location)
		{
		}

		/// <inheritdoc />
		public abstract void UpdateContext3D(IUpdateArgs args, IGuiRenderer guiRenderer);

		/// <inheritdoc />
		public abstract void DrawContext3D(IRenderArgs args, IGuiRenderer guiRenderer);
	}
}