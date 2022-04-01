using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.Common.Graphics;
using Alex.Common.Utils.Collections;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using SixLabors.ImageSharp.Processing;
using Color = Microsoft.Xna.Framework.Color;

namespace Alex.Graphics.Models.Items
{
	public class ItemModelRenderer : ItemModelRenderer<VertexPositionColor>
	{
		public ItemModelRenderer(ResourcePackModelBase resourcePackModel, VertexPositionColor[] vertices = null) : base(
			resourcePackModel, VertexPositionColor.VertexDeclaration, vertices) { }

		/// <inheritdoc />
		private bool _cached = false;

		public override bool Cache(ResourceManager pack)
		{
			if (_cached) return true;

			if (!ResourcePackModel.Textures.TryGetValue("layer0", out var texture))
			{
				texture = ResourcePackModel.Textures.FirstOrDefault(x => x.Value != null).Value;
			}

			if (texture == null)
			{
				return false;
			}

			_cached = true;
			List<VertexPositionColor> vertices = new List<VertexPositionColor>();

			//if (pack.TryGetBitmap(texture, out var bitmap))

			if (pack.TryGetBitmap(texture, out var bitmap))
			{
				var parentName = ResourcePackModel?.ParentName?.Path;

				if (parentName != null && parentName.Contains("handheld", StringComparison.InvariantCultureIgnoreCase))
				{
					bitmap.Mutate(
						x =>
						{
							x.Flip(FlipMode.Horizontal);
							x.Rotate(RotateMode.Rotate90);
						});
				}

				try
				{
					if (bitmap.DangerousTryGetSinglePixelMemory(out var pixelMemory))
					{
						var pixels = pixelMemory.Span;
						var pixelSize = new Vector3(16f / bitmap.Width, 16f / bitmap.Height, 1f);

						for (int y = 0; y < bitmap.Height; y++)
						{
							for (int x = 0; x < bitmap.Width; x++)
							{
								var pixel = pixels[(bitmap.Width * (bitmap.Height - 1 - y)) + (x)];

								if (pixel.A == 0)
								{
									continue;
								}

								Color color = new Color(pixel.R, pixel.G, pixel.B, pixel.A);
								var origin = new Vector3((x * pixelSize.X), y * pixelSize.Y, 0f);

								ItemModelCube built = new ItemModelCube(pixelSize, color);

								vertices.AddRange(
									Modify(
										built.Front.Concat(built.Bottom).Concat(built.Back).Concat(built.Top)
										   .Concat(built.Left).Concat(built.Right), origin));
							}
						}

						this.Size = new Vector3(bitmap.Width * pixelSize.X, bitmap.Height * pixelSize.Y, pixelSize.Z);
					}
				}
				finally
				{
					bitmap.Dispose();
				}
			}

			Vertices = vertices.ToArray();

			return true;
		}

		private IEnumerable<VertexPositionColor> Modify(IEnumerable<VertexPositionColor> vertices, Vector3 offset)
		{
			foreach (var vertex in vertices)
			{
				var vertexPositionColor = vertex;
				vertexPositionColor.Position += offset;

				yield return vertexPositionColor;
			}
		}

		/*public override IItemRenderer CloneItemRenderer()
		{
		    var renderer = new ItemModelRenderer(ResourcePackModel, Vertices?.Select(
		        x => new VertexPositionColor(
		            new Vector3(x.Position.X, x.Position.Y, x.Position.Z), new Color(x.Color.PackedValue))).ToArray(), _texture)
		    {
		        Size = Size,
		        Scale = Scale,
		        DisplayPosition = DisplayPosition,
		        ActiveDisplayItem = ActiveDisplayItem.Clone()
		    };
		    
		   // if (renderer.Vertices == null || renderer.Vertices.Length == 0)
		   //     renderer.InitCache();

		    return renderer;
		}*/
	}

	public class ItemModelRenderer<TVertice> : ModelBase, IItemRenderer, IItemRendererHolder
		where TVertice : struct, IVertexType
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ItemModelRenderer));
		public Vector3 Size { get; set; } = Vector3.One;

		public ResourcePackModelBase ResourcePackModel { get; }

		private DisplayPosition _displayPosition = DisplayPosition.ThirdPersonRightHand;

		public DisplayPosition DisplayPosition
		{
			get => _displayPosition;
			set
			{
				_displayPosition = value;
				UpdateDisplay();
			}
		}

		public DisplayElement ActiveDisplayItem { get; set; } = DisplayElement.Default;

		private void UpdateDisplay()
		{
			if (Model == null) return;

			if (_displayPosition.TryGetString(out string displayPosStr))
			{
				if (ResourcePackModel.Display.TryGetValue(displayPosStr, out var display))
				{
					ActiveDisplayItem = display;
					UpdateDisplayInfo(_displayPosition, ActiveDisplayItem);
				}
				else
				{
					// Log.Warn($"Invalid displayposition (str): {displayPosStr}");
				}
			}
			else
			{
				// Log.Warn($"Invalid displayposition (enum): {_displayPosition}");
			}

			//ActiveDisplayItem = DisplayElement.Default;
		}

		protected virtual void UpdateDisplayInfo(DisplayPosition displayPosition, DisplayElement displayElement)
		{
			var root = Model?.Root;

			if (root == null) return;

			if (displayPosition.HasFlag(DisplayPosition.Gui))
			{
				root.BaseScale = Vector3.One / 16f;
				root.BaseRotation = Vector3.Zero; //.Identity;
				root.BasePosition = Vector3.Zero;
			}
			else if (displayPosition.HasFlag(DisplayPosition.Ground))
			{
				root.BaseScale = displayElement.Scale * Scale;

				root.BaseRotation = new Vector3(
					displayElement.Rotation.X, displayElement.Rotation.Y, displayElement.Rotation.Z);

				root.BasePosition = new Vector3(
					displayElement.Translation.X, displayElement.Translation.Y, displayElement.Translation.Z);
			}
			/*else if (displayPosition.HasFlag(DisplayPosition.FirstPerson))
			{
			    root.BaseScale = new Vector3(
			        displayElement.Scale.X, 
			        displayElement.Scale.Y,
			        displayElement.Scale.Z);
			    
			    root.BaseRotation =  new Vector3(
			                             displayElement.Rotation.X, 
			                             displayElement.Rotation.Y, 
			                             displayElement.Rotation.Z);

			    root.BasePosition = new Vector3(
			        displayElement.Translation.X, 
			        8f + displayElement.Translation.Y,
			        displayElement.Translation.Z);
			}*/
			else
			{
				root.BaseScale = new Vector3(displayElement.Scale.X, displayElement.Scale.Y, displayElement.Scale.Z);

				if ((ResourcePackModel.Type & ModelType.Handheld) != 0)
				{
					root.BaseRotation = new Vector3(
						                    displayElement.Rotation.X, -displayElement.Rotation.Y,
						                    -displayElement.Rotation.Z)
					                    + new Vector3(-67.5f, -22.5f, 0f);

					root.BasePosition = new Vector3(
						6f + displayElement.Translation.X, 6f + displayElement.Translation.Y,
						2f + displayElement.Translation.Z);
				}
				else
				{
					root.BaseRotation = new Vector3(
						                    displayElement.Rotation.X, -displayElement.Rotation.Y,
						                    -displayElement.Rotation.Z)
					                    + new Vector3(-67.5f, 0f, 0f);

					root.BasePosition = new Vector3(
						displayElement.Translation.X, 8f + displayElement.Translation.Y, displayElement.Translation.Z);
				}
			}
		}

		protected TVertice[] Vertices
		{
			set
			{
				if (value != null && value.Length > 0)
				{
					_isCached = true;
					InitializeModel(Alex.Instance.GraphicsDevice, value);
				}
			}
		}

		protected BasicEffect Effect
		{
			get => _effect;
			set
			{
				_effect = value;

				/*if (value != null && Model != null)
				{
				    foreach (var mesh in Model.Meshes)
				    {
				        foreach (var part in mesh.MeshParts)
				        {
				            if (part.Effect == null)
				                part.Effect = value;
				        }
				    }
				}*/
			}
		}
		// public Vector3 Scale { get; set; } = Vector3.One;

		private VertexDeclaration _declaration;

		public IModel Model
		{
			get => _model;
			set
			{
				var oldValue = _model;
				_model = value;

				foreach (var instance in _instances)
				{
					if (instance is RendererInstance rendererInstance)
						rendererInstance.OnModelChanged(oldValue, value);
				}
			}
		}

		//protected Texture2D  _texture;
		protected ThreadSafeList<IItemRenderer> _instances = new ThreadSafeList<IItemRenderer>();
		private BasicEffect _effect = null;

		public ItemModelRenderer(ResourcePackModelBase resourcePackModel,
			VertexDeclaration declaration,
			TVertice[] vertices = null)
		{
			Scale = 1f;
			ResourcePackModel = resourcePackModel;
			_declaration = declaration;

			Vertices = vertices;
		}

		public void Update(IUpdateArgs args)
		{
			if (Effect == null)
			{
				return;
			}

			Effect.Projection = args.Camera.ProjectionMatrix;
			Effect.View = args.Camera.ViewMatrix;
		}

		private bool _initalizedModel = false;

		private SemaphoreSlim _initSemaphore = new SemaphoreSlim(1);

		private void InitializeModel(GraphicsDevice device, TVertice[] vertices)
		{
			if (_initalizedModel)
				return;

			if (vertices == null || vertices.Length == 0)
			{
				Log.Warn($"Could not initalize model, no vertices specified.");

				return;
			}

			var declaration = _declaration;

			if (declaration == null)
				return;

			if (!_initSemaphore.Wait(0))
				return;


			_initalizedModel = true;
			List<ModelBone> bones = new List<ModelBone>();
			List<ModelMesh> meshes = new List<ModelMesh>();
			List<ModelMeshPart> meshParts = new List<ModelMeshPart>();

			var rootBone = new ModelBone() { Name = "ItemRoot" };

			var meshPart = new ModelMeshPart() { StartIndex = 0, NumVertices = vertices.Length, VertexOffset = 0 };

			List<short> indices = new List<short>();

			for (int i = 0; i < vertices.Length; i++)
				indices.Add((short)i);

			meshPart.PrimitiveCount = (indices.Count) / 3;
			meshParts.Add(meshPart);

			ModelMesh mesh = new ModelMesh(device, meshParts) { Name = "Item" };
			//meshPart.Effect = Effect;
			meshes.Add(mesh);
			rootBone.AddMesh(mesh);

			bones.Add(rootBone);

			Model model = new Model(bones, meshes);
			model.Root = rootBone;

			model.BuildHierarchy();
			UpdateDisplay();

			void Finish()
			{
				try
				{
					var effect = new BasicEffect(Alex.Instance.GraphicsDevice);
					_effect = effect;

					foreach (var m in model.Meshes)
					{
						var mps = m.MeshParts;

						if (mps == null)
							continue;

						foreach (var mp in mps)
						{
							mp.Effect = effect;
						}
					}

					InitEffect(effect);

					var vertexBuffer = new VertexBuffer(device, declaration, vertices.Length, BufferUsage.WriteOnly);

					vertexBuffer.SetData(vertices);
					meshPart.VertexBuffer = vertexBuffer;

					var indexBuffer = new IndexBuffer(
						device, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);

					indexBuffer.SetData(indices.ToArray());
					meshPart.IndexBuffer = indexBuffer;
					meshPart.Effect = effect;

					Model = model;
					UpdateDisplay();
				}
				finally
				{
					_initSemaphore.Release();
				}
			}


			Finish();


			//  var vertices = Vertices;
		}

		private Action _setupForRendering = null;

		public int Render(IRenderArgs args, Matrix characterMatrix)
		{
			if (_setupForRendering != null)
			{
				_setupForRendering?.Invoke();
				_setupForRendering = null;
			}

			if (Effect == null || Model == null)
				return 0;

			return Model.Draw(characterMatrix, args.Camera.ViewMatrix, args.Camera.ProjectionMatrix, null);
		}

		public int Render(IRenderArgs args, Matrix characterMatrix, Matrix[] matrices)
		{
			if (_setupForRendering != null)
			{
				_setupForRendering?.Invoke();
				_setupForRendering = null;
			}

			if (Effect == null || Model == null)
				return 0;

			return Model.Draw(characterMatrix, args.Camera.ViewMatrix, args.Camera.ProjectionMatrix, matrices);
		}

		protected virtual void InitEffect(BasicEffect effect)
		{
			if (effect == null)
				return;

			/*if (_texture != null)
			    effect.Texture = _texture;*/

			effect.VertexColorEnabled = true;
		}

		public virtual bool Cache(ResourceManager pack)
		{
			return false;
			//Buffer = GpuResourceManager.GetBuffer(this, )
		}

		public virtual IItemRenderer CloneItemRenderer()
		{
			var newInstance = new RendererInstance(this);
			_instances.Add(newInstance);

			return newInstance;
		}

		public void RemoveInstance(IItemRenderer instance)
		{
			_instances.Remove(instance);
		}

		protected bool _isCached = false;
		private IModel _model = null;

		public void TryCache(ResourceManager resourceManager)
		{
			if (!_isCached)
			{
				if (Cache(resourceManager))
				{
					_isCached = true;
				}
			}
		}

		public class RendererInstance : ModelMatrixHolder, IItemRenderer
		{
			private IItemRendererHolder _parent;

			/// <inheritdoc />
			public override IModel Model
			{
				get => _parent?.Model;
				set { }
			}

			public RendererInstance(IItemRendererHolder parent)
			{
				_parent = parent;
				OnModelChanged(null, parent.Model);
			}

			/// <inheritdoc />
			protected override void ModelChanged(Model newModel)
			{
				base.ModelChanged(newModel);
				IsMatricesDirty = true;
			}

			private DisplayPosition _displayPosition1;

			/// <inheritdoc />
			public int Render(IRenderArgs args, Matrix characterMatrix)
			{
				var parent = _parent;

				var model = Model;

				if (parent == null || model == null)
					return 0;

				var originalParentDisplayPosition = parent.DisplayPosition;
				var originalParentScale = parent.Scale;

				parent.DisplayPosition = DisplayPosition;
				int amount = 0;

				var matrices = GetTransforms();

				if (matrices != null)
				{
					amount = parent.Render(args, characterMatrix, matrices);
				}


				parent.Scale = originalParentScale;
				parent.DisplayPosition = originalParentDisplayPosition;

				return amount;
			}

			/// <inheritdoc />
			public override void Update(IUpdateArgs args)
			{
				base.Update(args);
				var parent = _parent;
				var model = Model;

				/*if (parent != null && model != null && _transformsDirty)
				{
				    var originalParentDisplayPosition = parent.DisplayPosition;
				    var originalParentScale = parent.Scale;

				    parent.DisplayPosition = DisplayPosition;
				    
				    base.Update(args);
				    
				    parent.Scale = originalParentScale;
				    parent.DisplayPosition = originalParentDisplayPosition;
				    _transformsDirty = false;
				}*/

				_parent?.Update(args);
			}

			private void Dispose(bool disposing)
			{
				_parent?.RemoveInstance(this);
				_parent = null;
			}

			/// <inheritdoc />
			public override void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			/// <inheritdoc />
			public ResourcePackModelBase ResourcePackModel => _parent.ResourcePackModel;

			/// <inheritdoc />
			public DisplayPosition DisplayPosition
			{
				get => _displayPosition1;
				set
				{
					_displayPosition1 = value;
					IsMatricesDirty = true;
				}
			}

			/// <inheritdoc />
			public bool Cache(ResourceManager pack)
			{
				return true;
			}

			/// <inheritdoc />
			public IItemRenderer CloneItemRenderer()
			{
				return _parent.CloneItemRenderer();
			}

			~RendererInstance()
			{
				Dispose(false);
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			var displayItem = ActiveDisplayItem;

			if (displayItem != null)
			{
				return
					$"Translation= {displayItem.Translation}, Rotation= {displayItem.Rotation} Scale= {displayItem.Scale}";
			}

			return base.ToString();
		}

		protected virtual void OnDispose(bool disposing) { }

		/// <inheritdoc />
		public void Dispose()
		{
			if (_instances.Count == 0)
			{
				OnDispose(true);

				Model?.Dispose();
				Model = null;

				/*var t = _texture;

				if (t != null && t.Tag != AtlasGenerator.Tag)
				{
				    t.Dispose();
				}

				_texture = null;*/

				_effect?.Dispose();
				_effect = null;
				_declaration?.Dispose();
				_declaration = null;
			}
			else
			{
				Log.Warn($"Tried to dispose of ModelRendererer with {_instances.Count} remaining instances!");
			}
		}
	}

	public interface IItemRendererHolder : IItemRenderer
	{
		float Scale { get; set; }

		void RemoveInstance(IItemRenderer rendererInstance);

		int Render(IRenderArgs args, Matrix characterMatrix, Matrix[] matrices);
	}
}