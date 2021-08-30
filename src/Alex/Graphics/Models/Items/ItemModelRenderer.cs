using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Alex.Blocks.Minecraft;
using Alex.Common.Graphics;
using Alex.Common.Graphics.GpuResources;
using Alex.Common.Utils;
using Alex.Entities;
using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Utils;
using FmodAudio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = Microsoft.Xna.Framework.Color;
using MathF = System.MathF;
using ModelBone = Alex.Graphics.Models.Entity.ModelBone;
using ModelMesh = Alex.Graphics.Models.Entity.ModelMesh;
using ModelMeshPart = Alex.Graphics.Models.Entity.ModelMeshPart;

namespace Alex.Graphics.Models.Items
{
    public class ItemModelRenderer : ItemModelRenderer<VertexPositionColor>
    {
        public ItemModelRenderer(ResourcePackModelBase resourcePackModel) : base(resourcePackModel,
            VertexPositionColor.VertexDeclaration)
        {
           
        }

        /// <inheritdoc />
        protected override VertexPositionColor[] Vertices
        {
            get
            {
                if (_vertices == null)
                {
                    if (!_cached)
                    {
                        Cache(Alex.Instance.Resources);
                    }
                }

                return _vertices;
            }
            set => _vertices = value;
        }

        private bool _cached = false;
        private VertexPositionColor[] _vertices;

        public override bool Cache(ResourceManager pack)
        {
            if (_cached)
                return true;

            _cached = true;
            
            if (!ResourcePackModel.Textures.TryGetValue("layer0", out var texture))
            {
                texture = ResourcePackModel.Textures.FirstOrDefault(x => x.Value != null).Value;
            }

            if (texture == null)
            {
                return false;
            }

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
            return new ItemModelRenderer(ResourcePackModel)
            {
                Vertices = Vertices != null ? Vertices = Vertices.Select(
                    x => new VertexPositionColor(
                        new Vector3(x.Position.X, x.Position.Y, x.Position.Z), new Color(x.Color.PackedValue))).ToArray() : null,
                Size = Size,
                Scale = Scale,
                DisplayPosition = DisplayPosition,
                ActiveDisplayItem = ActiveDisplayItem.Clone()
            };
        }
    }

    public class ItemModelRenderer<TVertice> : Model, IItemRenderer where TVertice : struct, IVertexType
    {
        public Vector3               Size  { get; set; } = Vector3.One;
        
        public ResourcePackModelBase ResourcePackModel { get; }

        private DisplayPosition _displayPosition = DisplayPosition.Undefined;

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
            try
            {
                if (ResourcePackModel.Display.TryGetValue(DisplayPositionHelper.ToString(_displayPosition), out var display))
                {
                    ActiveDisplayItem = display;
                }
                
                UpdateDisplayInfo(_displayPosition, ActiveDisplayItem);
                
            }
            catch(ArgumentOutOfRangeException)
            {
                
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
                        root.BaseRotation = Vector3.Zero;
                        root.BasePosition = Vector3.Zero;
                    }
                    else if (displayPosition.HasFlag(DisplayPosition.Ground))
                    {
                        root.BaseScale = displayElement.Scale * Scale;
                        root.BaseRotation = new Vector3(displayElement.Rotation.X, displayElement.Rotation.Y, displayElement.Rotation.Z);
                        root.BasePosition = new Vector3(displayElement.Translation.X, displayElement.Translation.Y, displayElement.Translation.Z);
                    }
                    else
                    {
                        root.BaseScale = new Vector3(
                            ActiveDisplayItem.Scale.X, ActiveDisplayItem.Scale.Y, ActiveDisplayItem.Scale.Z);// ActiveDisplayItem.Scale;

                        if ((ResourcePackModel.Type & ModelType.Handheld) != 0)
                        {
                            root.BaseRotation = new Vector3(
                                ActiveDisplayItem.Rotation.X, -ActiveDisplayItem.Rotation.Y,
                                -ActiveDisplayItem.Rotation.Z) + new Vector3(-67.5f, -22.5f, 0f);

                            root.BasePosition = new Vector3(
                                (6f + ActiveDisplayItem.Translation.X), 6f + ActiveDisplayItem.Translation.Y,
                                -(16f + (ActiveDisplayItem.Translation.Z)));
                        }
                        else
                        {
                            root.BaseRotation = new Vector3(
                                ActiveDisplayItem.Rotation.X, -ActiveDisplayItem.Rotation.Y,
                                -ActiveDisplayItem.Rotation.Z) + new Vector3(-67.5f, 0f, 0f);

                            root.BasePosition = new Vector3(
                                (-ActiveDisplayItem.Translation.X), 8f + ActiveDisplayItem.Translation.Y,
                                -(12f +ActiveDisplayItem.Translation.Z));
                        }
                    }
                }
        }
        
        protected virtual TVertice[]  Vertices { get; set; } = null;
        protected   BasicEffect Effect   { get; set; } = null;
       // public Vector3 Scale { get; set; } = Vector3.One;
        
        private readonly VertexDeclaration _declaration;

        public Entity.Model Model { get; set; } = null;
        public ItemModelRenderer(ResourcePackModelBase resourcePackModel, VertexDeclaration declaration)
        {
            Scale = 1f;
            ResourcePackModel = resourcePackModel;
            _declaration = declaration;
        }
        
        public void Update(IUpdateArgs args)
        {
            if (Effect == null)
            {
                var effect = new BasicEffect(args.GraphicsDevice);
                InitEffect(effect);
                Effect = effect;
            }

            Effect.Projection = args.Camera.ProjectionMatrix;
            Effect.View = args.Camera.ViewMatrix;
            
            if (Model == null && Vertices != null)
            {
                var vertices = Vertices;
                
                var buffer = new VertexBuffer(
                    args.GraphicsDevice, _declaration, vertices.Length, BufferUsage.WriteOnly);
                buffer.SetData(vertices);

                List<short> indices = new List<short>();
                for(int i = 0; i < vertices.Length; i++)
                    indices.Add((short)i);

                var indexBuffer = new IndexBuffer(
                    Alex.Instance.GraphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);
                indexBuffer.SetData(indices.ToArray());
                
                List<ModelBone> bones = new List<ModelBone>();
                List<ModelMesh> meshes = new List<ModelMesh>();
                List<ModelMeshPart> meshParts = new List<ModelMeshPart>();

                var rootBone = new ModelBone
                {
                    Name = "ItemRoot"
                };

                var meshPart = new ModelMeshPart()
                {
                    StartIndex = 0,
                    PrimitiveCount = (indices.Count) / 3,
                    NumVertices = vertices.Length,
                    
                    VertexBuffer = buffer,
                    IndexBuffer = indexBuffer,
                    VertexOffset = 0
                };
                meshParts.Add(meshPart);

                ModelMesh mesh = new ModelMesh(args.GraphicsDevice, meshParts)
                {
                    Name = "Item"
                };
                meshPart.Effect = Effect;
                meshes.Add(mesh);
                rootBone.AddMesh(mesh);
                
                bones.Add(rootBone);

                Entity.Model model = new Entity.Model(bones, meshes);
                model.Root = rootBone;
                
                model.BuildHierarchy();

                Model = model;
                UpdateDisplay();
            }
        }

        /// <inheritdoc />
        public IHoldAttachment Parent { get; set; } = null;

        public int Render(IRenderArgs args, Matrix characterMatrix)
        {
            if (Effect == null || Model == null)
                return 0;

            return Model.Draw(characterMatrix, args.Camera.ViewMatrix, args.Camera.ProjectionMatrix);
        }

        protected virtual void InitEffect(BasicEffect effect)
        {
            effect.VertexColorEnabled = true;
        }

        public virtual bool Cache(ResourceManager pack)
        {
            return false;
            //Buffer = GpuResourceManager.GetBuffer(this, )
        }

        public virtual IItemRenderer CloneItemRenderer()
        {
            return new ItemModelRenderer<TVertice>(ResourcePackModel, _declaration)
            {
                Size = this.Size,
                Vertices = Vertices != null ? Vertices.Clone() as TVertice[] : null,
                Scale = Scale,
                DisplayPosition = DisplayPosition,
                ActiveDisplayItem = ActiveDisplayItem.Clone()
            };
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
