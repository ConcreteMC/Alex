using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.API;
using Alex.API.Data.Options;
using Alex.API.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Gamestates;
using Alex.Worlds.Chunks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Worlds
{
	public class ChunkManager : IChunkManager, IDisposable
	{
		private readonly static RenderStage[]   RenderStages = ((RenderStage[]) Enum.GetValues(typeof(RenderStage)));
		
		private                 GraphicsDevice  Graphics  { get; }
		private                 ResourceManager Resources { get; }
		private                 World           World     { get; }
		
		private AlexOptions                                         Options        { get; }
		private ConcurrentDictionary<ChunkCoordinates, ChunkColumn> Chunks         { get; }
		
		public  RenderingShaders                                    DefaultShaders { get; set; }
		public ChunkManager(IServiceProvider serviceProvider, GraphicsDevice graphics, World world)
		{
			Graphics = graphics;
			World = world;
			
			Options = serviceProvider.GetRequiredService<IOptionsProvider>().AlexOptions;
			Resources = serviceProvider.GetRequiredService<ResourceManager>();
			
			var stillAtlas = Resources.Atlas.GetStillAtlas();
	        
			var fogStart = 0;
			DefaultShaders = new RenderingShaders(Graphics);
			DefaultShaders.SetTextures(stillAtlas);
			DefaultShaders.SetAnimatedTextures(Resources.Atlas.GetAtlas(0));
			
			RenderDistance = Options.VideoOptions.RenderDistance;
			
			Chunks = new ConcurrentDictionary<ChunkCoordinates, ChunkColumn>();
		}

		private long _threadsRunning = 0;

		
		public int RenderDistance { get; set; } = 0;
		
		public int ConcurrentChunkUpdates => (int) _threadsRunning;
		public int EnqueuedChunkUpdates   => UpdateQueue.Count;
		
		/// <inheritdoc />
		public int ChunkCount => Chunks.Count;

		/// <inheritdoc />
		public long Vertices { get; private set; }

		/// <inheritdoc />
		public int RenderedChunks { get; private set; }

		public bool FogEnabled
		{
			get { return DefaultShaders.FogEnabled; }
			set
			{
				DefaultShaders.FogEnabled = value;
			}
		}

		public Vector3 FogColor
		{
			get { return DefaultShaders.FogColor; }
			set
			{
				DefaultShaders.FogColor = value;
				//  LightShaders.FogColor = value;
			}
		}

		public float FogDistance
		{
			get { return DefaultShaders.FogDistance; }
			set
			{
				DefaultShaders.FogDistance = value;
				//  LightShaders.FogDistance = value;
			}
		}

		public Vector3 AmbientLightColor
		{
			get { return DefaultShaders.AmbientLightColor; }
			set
			{
				DefaultShaders.AmbientLightColor = value;
			}
		}

		public float BrightnessModifier
		{
			get
			{
				return DefaultShaders.BrightnessModifier;
			}
			set
			{
				DefaultShaders.BrightnessModifier = value;
			}
		}

		private ConcurrentQueue<ChunkCoordinates> UpdateQueue { get; set; } = new ConcurrentQueue<ChunkCoordinates>();
		/// <inheritdoc />
		public void Start()
		{
			ThreadPool.QueueUserWorkItem(
				o =>
				{
					Thread.CurrentThread.Name = "Chunk Management";

					SpinWait sw = new SpinWait();
					while (true)
					{
						if (Interlocked.Read(ref _threadsRunning) >= Options.VideoOptions.ChunkThreads)
						{
							sw.SpinOnce();
							continue;
						}

						ChunkCoordinates coordinates;
						if (!UpdateQueue.TryDequeue(out coordinates))
						{
							sw.SpinOnce();
							continue;
						}

						if (TryGetChunk(coordinates, out var chunk))
						{
							Interlocked.Increment(ref _threadsRunning);
							
							ThreadPool.QueueUserWorkItem(
								oo =>
								{
									try
									{
										if (!Monitor.TryEnter(chunk.UpdateLock, 0))
											return;

										try
										{
											chunk.BuildBuffer(Graphics, World);
										}
										finally
										{
											Monitor.Exit(chunk.UpdateLock);
										}
									}
									finally
									{
										Interlocked.Decrement(ref _threadsRunning);
									}
								});
						}
					}
				});
			// ChunkManagementThread.Start();
		}

		/// <inheritdoc />
		public void AddChunk(ChunkColumn chunk, ChunkCoordinates position, bool doUpdates = false)
		{
			Chunks.AddOrUpdate(
				position, coordinates => chunk, (coordinates, column) =>
				{
					if (!ReferenceEquals(column, chunk))
					{
						column.Dispose();
					}

					return chunk;
				});
			
			UpdateQueue.Enqueue(position);
		}

		/// <inheritdoc />
		public void RemoveChunk(ChunkCoordinates position, bool dispose = true)
		{
			if (Chunks.TryRemove(position, out var column))
			{
				if (dispose)
					column.Dispose();
			}
		}

		/// <inheritdoc />
		public bool TryGetChunk(ChunkCoordinates coordinates, out ChunkColumn chunk)
		{
			return Chunks.TryGetValue(coordinates, out chunk);
		}

		/// <inheritdoc />
		public KeyValuePair<ChunkCoordinates, ChunkColumn>[] GetAllChunks()
		{
			return Chunks.ToArray();
		}

		/// <inheritdoc />
		public void ClearChunks()
		{
			var chunks = Chunks.ToArray();
			Chunks.Clear();

			foreach (var chunk in chunks)
			{
				chunk.Value.Dispose();
			}
		}

		public void ScheduleChunkUpdate(ChunkCoordinates position, ScheduleType type, bool prioritize = false)
		{
			UpdateQueue.Enqueue(position);
		}
		
		#region  Drawing

		private static readonly SamplerState RenderSampler = new SamplerState()
		{
			Filter = TextureFilter.PointMipLinear,
			AddressU = TextureAddressMode.Wrap,
			AddressV = TextureAddressMode.Wrap,
			MipMapLevelOfDetailBias = -1f,
			MaxMipLevel = Alex.MipMapLevel,
			// MaxMipLevel = 0,
			FilterMode = TextureFilterMode.Comparison,
			AddressW = TextureAddressMode.Wrap,
			MaxAnisotropy = 16,
			// ComparisonFunction = 
		};
	    
		private static readonly BlendState LightMapBS = new BlendState()
		{
			ColorSourceBlend = Blend.One,
			ColorDestinationBlend = Blend.One,
			ColorBlendFunction = BlendFunction.Add,
			AlphaSourceBlend = Blend.One,
			AlphaDestinationBlend = Blend.One,
			AlphaBlendFunction = BlendFunction.Add
		};
	    
		private static DepthStencilState DepthStencilState { get; } = new DepthStencilState()
		{
			DepthBufferEnable = true,
			DepthBufferFunction = CompareFunction.Less,
			DepthBufferWriteEnable = true
		};

		private static RasterizerState RasterizerState = new RasterizerState()
		{
			//DepthBias = 0.0001f,
			CullMode = CullMode.CullClockwiseFace,
			FillMode = FillMode.Solid,
			//DepthClipEnable = true,
			//ScissorTestEnable = true
		};

		private static BlendState TranslucentBlendState { get; } = new BlendState()
		{
			AlphaSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.SourceAlpha,
			AlphaDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.InverseSourceAlpha,
			ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.InverseSourceAlpha,
			ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.SourceAlpha,
			//ColorBlendFunction = BlendFunction.Add,
			IndependentBlendEnable = true,
			//AlphaBlendFunction = BlendFunction.Add,
			// ColorBlendFunction = BlendFunction.Add
		};
		
		public void Draw(IRenderArgs args, bool depthMapOnly, params RenderStage[] stages)
		{
			var view       = args.Camera.ViewMatrix;
			var projection = args.Camera.ProjectionMatrix;

			DefaultShaders.UpdateMatrix(view, projection);
			//	LightShaders.UpdateMatrix(view, projection);

			//DepthEffect.View = view;
			//DepthEffect.Projection = projection;

			var device = args.GraphicsDevice;

			var originalSamplerState = device.SamplerStates[0];

			device.SamplerStates[0] = RenderSampler;

			RasterizerState originalState = device.RasterizerState;
			args.GraphicsDevice.RasterizerState = RasterizerState;

			/*bool usingWireFrames = UseWireFrames;

			if (usingWireFrames)
			{
				originalState = device.RasterizerState;
				RasterizerState rasterizerState = originalState.Copy();
				rasterizerState.FillMode = FillMode.WireFrame;
				device.RasterizerState = rasterizerState;
			}*/

			device.DepthStencilState = DepthStencilState;


			// device.DepthStencilState = DepthStencilState.DepthRead;
			device.BlendState = BlendState.AlphaBlend;

			DrawStaged(
				args, out int chunksRendered, out int verticesRendered, null,
				stages.Length > 0 ? stages : RenderStages);

			Vertices = verticesRendered;
			RenderedChunks = chunksRendered;
			//IndexBufferSize = 0;


			//if (usingWireFrames)
			device.RasterizerState = originalState;

			device.SamplerStates[0] = originalSamplerState;
		}
		
		private bool IsWithinView(ChunkCoordinates chunk, BoundingFrustum frustum, float y)
		{
			var chunkPos = new Vector3(chunk.X * ChunkColumn.ChunkWidth, 0, chunk.Z * ChunkColumn.ChunkDepth);
			return frustum.Intersects(new Microsoft.Xna.Framework.BoundingBox(chunkPos,
				chunkPos + new Vector3(ChunkColumn.ChunkWidth, y + 10,
					ChunkColumn.ChunkDepth)));

		}
		
		private void DrawStaged(IRenderArgs args, out int chunksRendered, out int drawnVertices,
			Effect forceEffect = null, params RenderStage[] stages)
		{
			var originalBlendState = args.GraphicsDevice.BlendState;

			if (stages == null || stages.Length == 0)
				stages = RenderStages;

			var tempVertices    = 0;
			int tempChunks      = 0;
			var indexBufferSize = 0;

			List<ChunkData> chunkDatas = new List<ChunkData>();

			foreach (var chunk in Chunks.ToArray())
			{
				if (!chunk.Value.ChunkData.Ready)
					continue;
				
				if (IsWithinView(chunk.Key, args.Camera.BoundingFrustum, args.Camera.Position.Y))
				{
					 chunkDatas.Add(chunk.Value.ChunkData);
				}
			}
			
			ChunkData[] chunks = chunkDatas.ToArray();

			foreach (var stage in stages)
			{
				args.GraphicsDevice.BlendState = originalBlendState;
			    
				RenderingShaders shaders = DefaultShaders;
			    
				bool   setDepth = false;
				Effect effect   = forceEffect;
				if (forceEffect == null)
				{
					switch (stage)
					{
						case RenderStage.OpaqueFullCube:
							effect = shaders.TransparentEffect;
							break;
						case RenderStage.Opaque:
							effect = shaders.TransparentEffect;
							break;
						case RenderStage.Transparent:
							effect = shaders.TransparentEffect;
							break;
						case RenderStage.Translucent:
							args.GraphicsDevice.BlendState = TranslucentBlendState;
							effect = shaders.TranslucentEffect;
							break;
						case RenderStage.Animated:
							effect = shaders.AnimatedEffect;
							break;
						case RenderStage.Liquid:
						case RenderStage.AnimatedTranslucent:
							effect = shaders.AnimatedEffect;
						    
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}


				tempVertices += DrawChunks(args.GraphicsDevice, chunks, effect, stage);
			}

			tempChunks = chunks.Count(x => x != null && x.RenderStages != null && x.RenderStages.Count > 0);

			chunksRendered = tempChunks;
			drawnVertices = tempVertices;

			args.GraphicsDevice.BlendState = originalBlendState;
		}
		
		private int DrawChunks(GraphicsDevice device, ChunkData[] chunks, Effect effect, RenderStage stage)
		{
			int verticeCount = 0;
			for (var index = 0; index < chunks.Length; index++)
			{
				var chunk = chunks[index];
				if (chunk == null) continue;
			    
				if (chunk.RenderStages.TryGetValue(stage, out var renderStage))
				{ 
					verticeCount += renderStage.Render(device, effect);
				}
			}

			return verticeCount;
		}
		
		#endregion

		/// <inheritdoc />
		public void Dispose()
		{
			//Graphics?.Dispose();
		}

		int   _currentFrame = 0;
		int   _framerate    = 12;     // Animate at 12 frames per second
		float _timer        = 0.0f;
		public void Update(IUpdateArgs args)
		{
			_timer += (float)args.GameTime.ElapsedGameTime.TotalSeconds;
			if (_timer >= (1.0f / _framerate ))
			{
				_timer -= 1.0f / _framerate ;
				_currentFrame = (_currentFrame + 1) % Resources.Atlas.GetFrameCount();
				
				DefaultShaders.SetAnimatedTextures(Resources.Atlas.GetAtlas(_currentFrame));
			}
		}
	}
}