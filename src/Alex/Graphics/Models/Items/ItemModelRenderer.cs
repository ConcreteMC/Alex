using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Alex.Common.Graphics;
using Alex.Common.Utils;
using Alex.Graphics.Models.Entity;
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
        public ItemModelRenderer(ResourcePackModelBase resourcePackModel, VertexPositionColor[] vertices = null, Texture2D texture = null) : base(resourcePackModel,
            VertexPositionColor.VertexDeclaration, vertices, texture)
        {
           
        }

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

            if (pack.TryGetBitmap(texture, out var bitmap))
            {
                if (ResourcePackModel.ParentName.Path.Contains("handheld", StringComparison.InvariantCultureIgnoreCase))
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
                    if (bitmap.TryGetSinglePixelSpan(out var pixels))
                    {
                        var pixelSize = new Vector3( 16f / bitmap.Width, 16f / bitmap.Height, 1f);

                        for (int y = 0; y < bitmap.Height; y++)
                        {
                            for (int x = 0; x < bitmap.Width; x++)
                            {
                                var pixel = pixels[(bitmap.Width * (bitmap.Height - 1 - y)) + (x)];

                                if (pixel.A == 0)
                                {
                                    continue;
                                }

                                Color color  = new Color(pixel.R, pixel.G, pixel.B, pixel.A);
                                var   origin = new Vector3((x * pixelSize.X), y * pixelSize.Y, 0f);

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

        public override IItemRenderer CloneItemRenderer()
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
        }
    }

    public class ItemModelRenderer<TVertice> : ModelBase, IItemRenderer where TVertice : struct, IVertexType
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ItemModelRenderer));
        public Vector3               Size  { get; set; } = Vector3.One;
        
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

            if (root != null)
            {
                if (_displayPosition.HasFlag(DisplayPosition.Gui))
                {
                    root.BaseScale = Vector3.One / 16f;
                    root.BaseRotation = Vector3.Zero;//.Identity;
                    root.BasePosition = Vector3.Zero;
                }
                else if (displayPosition.HasFlag(DisplayPosition.Ground))
                {
                    root.BaseScale = displayElement.Scale * Scale;

                    root.BaseRotation =  new Vector3(
                        displayElement.Rotation.X, displayElement.Rotation.Y, displayElement.Rotation.Z);

                    root.BasePosition = new Vector3(
                        displayElement.Translation.X, displayElement.Translation.Y, displayElement.Translation.Z);
                }
                else
                {
                    root.BaseScale = new Vector3(
                        ActiveDisplayItem.Scale.X, ActiveDisplayItem.Scale.Y,
                        ActiveDisplayItem.Scale.Z); // ActiveDisplayItem.Scale;

                    if ((ResourcePackModel.Type & ModelType.Handheld) != 0)
                    {
                        root.BaseRotation = new Vector3(
                                                                                 ActiveDisplayItem.Rotation.X, -ActiveDisplayItem.Rotation.Y,
                                                                                 -ActiveDisplayItem.Rotation.Z)
                                                                             + new Vector3(-67.5f, -22.5f, 0f);

                        root.BasePosition = new Vector3(
                            (6f + ActiveDisplayItem.Translation.X), 6f + ActiveDisplayItem.Translation.Y,
                            -(14f + (ActiveDisplayItem.Translation.Z)));
                    }
                    else
                    {
                        root.BaseRotation =  new Vector3(
                                                                                  ActiveDisplayItem.Rotation.X, -ActiveDisplayItem.Rotation.Y,
                                                                                  -ActiveDisplayItem.Rotation.Z)
                                                                              + new Vector3(-67.5f, 0f, 0f);

                        root.BasePosition = new Vector3(
                            (-ActiveDisplayItem.Translation.X), 8f + ActiveDisplayItem.Translation.Y,
                            -(12f + ActiveDisplayItem.Translation.Z));
                    }
                }
            }
        }

        protected TVertice[] Vertices
        {
            get => _vertices;
            set
            {
                var previousValue = _vertices;
                _vertices = value;

                if (value != null && value.Length > 0)
                {
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

                if (value != null && Model != null)
                {
                    foreach (var mesh in Model.Meshes)
                    {
                        foreach (var part in mesh.MeshParts)
                        {
                            if (part.Effect == null)
                                part.Effect = value;
                        }
                    }
                }
            }
        }
        // public Vector3 Scale { get; set; } = Vector3.One;

        private readonly VertexDeclaration _declaration;

        public Model Model { get; set; } = null;

        /// <inheritdoc />
        public IHoldAttachment Parent { get; set; }

        protected Texture2D  _texture;
        public ItemModelRenderer(ResourcePackModelBase resourcePackModel, VertexDeclaration declaration, TVertice[] vertices = null, Texture2D texture = null)
        {
            Scale = 1f;
            ResourcePackModel = resourcePackModel;
            _declaration = declaration;

            if (texture != null)
            {
                _texture = texture;
            }

            Vertices = vertices;
            if (vertices != null)
            {
            //    Vertices = vertices;
               // InitializeModel(Alex.Instance.GraphicsDevice, vertices);
            }
            else
            {
              // InitCache();
            }
        }
        
        private TVertice[] _vertices = null;
        private BasicEffect _effect = null;

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
        private void InitializeModel(GraphicsDevice device, TVertice[] vertices)
        {
            if (_initalizedModel)
                return;
            
            if (vertices == null || vertices.Length == 0)
            {
                Log.Warn($"Could not initalize model, no vertices specified.");

                return;
            }
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
            meshPart.Effect = Effect;
            meshes.Add(mesh);
            rootBone.AddMesh(mesh);

            bones.Add(rootBone);

            Model model = new Model(bones, meshes);
            model.Root = rootBone;

            model.BuildHierarchy();
            Model = model;
            UpdateDisplay();
            
            ThreadPool.QueueUserWorkItem(
                (o) =>
                {
                    Effect = new BasicEffect(Alex.Instance.GraphicsDevice);

                    if (_texture != null)
                        Effect.Texture = _texture;

                    InitEffect(Effect);

                    foreach (var modelMesh in model.Meshes)
                    {
                        foreach (var subMesh in modelMesh.MeshParts)
                        {
                            subMesh.Effect = Effect;
                        }
                    }

                    meshPart.VertexBuffer = new VertexBuffer(
                        device, _declaration, vertices.Length, BufferUsage.WriteOnly);

                    meshPart.VertexBuffer.SetData(vertices);

                    meshPart.IndexBuffer = new IndexBuffer(
                        device, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);

                    meshPart.IndexBuffer.SetData(indices.ToArray());
                        
                    UpdateDisplay();
                });
            
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

            return Model.Draw(characterMatrix, args.Camera.ViewMatrix, args.Camera.ProjectionMatrix);
        }

        protected virtual void InitEffect(BasicEffect effect)
        {
            if (_texture != null)
                effect.Texture = _texture;
            
            effect.VertexColorEnabled = true;
        }

        protected void InitCache()
        {
            Cache(Alex.Instance.Resources);
        }
        
        public virtual bool Cache(ResourceManager pack)
        {
            return false;
            //Buffer = GpuResourceManager.GetBuffer(this, )
        }

        public virtual IItemRenderer CloneItemRenderer()
        {
            var renderer = new ItemModelRenderer<TVertice>(ResourcePackModel, _declaration, Vertices?.Clone() as TVertice[], _texture)
            {
                Size = this.Size,
                Scale = Scale,
                DisplayPosition = DisplayPosition,
                ActiveDisplayItem = ActiveDisplayItem.Clone()
            };

           // if (renderer._vertices == null || renderer._vertices.Length == 0)
            //    renderer.InitCache();
            
            return renderer;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var displayItem = ActiveDisplayItem;

            if (displayItem != null)
            {
                return $"Translation= {displayItem.Translation}, Rotation= {displayItem.Rotation} Scale= {displayItem.Scale}";
            }
            return base.ToString();
        }
    }
}
