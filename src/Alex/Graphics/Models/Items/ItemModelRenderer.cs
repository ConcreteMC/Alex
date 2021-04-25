using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Alex.API.Blocks;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Entities;
using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = Microsoft.Xna.Framework.Color;
using MathF = System.MathF;

namespace Alex.Graphics.Models.Items
{
    public class ItemModelRenderer : ItemModelRenderer<VertexPositionColor>
    {
        public ItemModelRenderer(ResourcePackModelBase model) : base(model,
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
            
            if (!Model.Textures.TryGetValue("layer0", out var texture))
            {
                texture = Model.Textures.FirstOrDefault(x => x.Value != null).Value;
            }

            if (texture == null)
            {
                return false;
            }

            List<VertexPositionColor> vertices = new List<VertexPositionColor>();

            if (pack.TryGetBitmap(texture, out var bitmap))
            {
                if (Model.ParentName.Contains("handheld", StringComparison.InvariantCultureIgnoreCase))
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

                        this.Size = new Vector3(16f, 16f, 1f);
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

        public override IItemRenderer Clone()
        {
            return new ItemModelRenderer(Model)
            {
                Vertices = Vertices != null ? Vertices.Clone() as VertexPositionColor[] : null,
                Size = Size
           //     Indexes = Indexes.ToArray()
            };
        }
    }

    public class ItemModelRenderer<TVertice> : Model, IItemRenderer where TVertice : struct, IVertexType
    {
        public Vector3               Size  { get; set; } = Vector3.One;
        
        public ResourcePackModelBase Model { get; }

        private DisplayPosition _displayPosition = DisplayPosition.Undefined;

        public DisplayPosition DisplayPosition
        {
            get => _displayPosition;
            set
            {
                var oldDisplayPosition = _displayPosition;
                _displayPosition = value;

                //if (oldDisplayPosition != _displayPosition)
                {
                    UpdateDisplay();
                }
            }
        }

        public DisplayElement ActiveDisplayItem { get; set; } = DisplayElement.Default;

        private void UpdateDisplay()
        {
            try
            {
                if (Model.Display.TryGetValue(DisplayPositionHelper.ToString(_displayPosition), out var display))
                {
                    ActiveDisplayItem = display;

                    return;
                }
            }
            catch(ArgumentOutOfRangeException)
            {
                
            }
            
            //ActiveDisplayItem = DisplayElement.Default;
        }

        protected virtual TVertice[]  Vertices { get; set; } = null;
        private   BasicEffect Effect   { get; set; } = null;
        
        //public Vector3 Rotation { get; set; } = Vector3.Zero;
        //public Vector3 Translation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;

        private          VertexBuffer      Buffer { get; set; } = null;
        private readonly VertexDeclaration _declaration;

        public ItemModelRenderer(ResourcePackModelBase model, VertexDeclaration declaration)
        {
            Model = model;
            _declaration = declaration;
        }

        //private Matrix _parentMatrix = Matrix.Identity;
        public void Update(IUpdateArgs args, Vector3 parentScale)
        {
            // _parentMatrix = characterMatrix;
            
            if (Effect == null)
            {
                var effect = new BasicEffect(args.GraphicsDevice);
                InitEffect(effect);
                Effect = effect;
            }

            Effect.Projection = args.Camera.ProjectionMatrix;
            Effect.View = args.Camera.ViewMatrix;

            //Effect.DiffuseColor = diffuseColor;
            
            if (Buffer == null && Vertices != null)
            {
                var vertices = Vertices;
               
                if (vertices.Length == 0)
                {
                   // _canInit = false;
                }
                else
                {
                    var buffer = GpuResourceManager.GetBuffer(this, args.GraphicsDevice, _declaration,
                        Vertices.Length, BufferUsage.WriteOnly);

                    buffer.SetData(vertices);

                    Buffer = buffer;
                }
            }
         //   Rotation = knownPosition.ToRotationVector3();
        }

        /// <inheritdoc />
        public IAttached Parent { get; set; } = null;

        public int Render(IRenderArgs args, Microsoft.Xna.Framework.Graphics.Effect effect, Matrix characterMatrix)
        {
            if (Effect == null || Buffer == null || Buffer.VertexCount == 0)
                return 0;
            
                        var halfSize          = Size / 2f;
           // halfSize.Z = 0f;
           // halfSize.Y = 8f;
            var activeDisplayItem = ActiveDisplayItem;
            //   world.Right = -world.Right;

            var scale = activeDisplayItem.Scale * Scale;
            var translation = activeDisplayItem.Translation;
            var rotation = activeDisplayItem.Rotation;

            if (DisplayPosition.HasFlag(DisplayPosition.Gui))
            {
                if (this is ItemBlockModelRenderer)
                {
                    Effect.World = Matrix.CreateScale(activeDisplayItem.Scale)
                                   * MatrixHelper.CreateRotationDegrees(new Vector3(25f, 45f, 0f))
                                   * Matrix.CreateTranslation(translation)
                                   * Matrix.CreateTranslation(new Vector3(0f, 0.25f, 0f)) * characterMatrix;
                }
                else
                {
                    Effect.World = Matrix.CreateScale(1f/16f) * characterMatrix;
                }
            }
            else if (DisplayPosition.HasFlag(DisplayPosition.ThirdPerson))
            {
                if (rotation != Vector3.Zero)
                {
                    //r.Y += 12.5f;
                    rotation.Z -= 67.5f;
                    Effect.World = Matrix.CreateScale(scale)
                                   * Matrix.CreateTranslation(-halfSize)
                                   * Matrix.CreateRotationZ(MathUtils.ToRadians(-32.5f))
                                   * Matrix.CreateTranslation(halfSize)
                                   //* MCMatrix.CreateRotationDegrees(new Vector3(-67.5f, 180f, 0f))
                                   * MatrixHelper.CreateRotationDegrees(rotation * new Vector3(1f, -1f, -1f))
                                   * Matrix.CreateTranslation(
                                       new Vector3(translation.X + 6f, translation.Y + 3f,  translation.Z))
                                   * characterMatrix;
                }
                else
                {
                    Effect.World =  
                        Matrix.CreateScale(scale)
                        * MatrixHelper.CreateRotationDegrees(new Vector3(-67.5f, 0f, 0f))
                        * Matrix.CreateTranslation(
                           new Vector3(translation.X + 2f, Size.Y - translation.Y, translation.Z))
                        * characterMatrix;
                }
            }
            else  if (DisplayPosition.HasFlag(DisplayPosition.FirstPerson))
            {
                Effect.World = Matrix.CreateScale(scale)
                               * Matrix.CreateRotationY(MathUtils.ToRadians(180f))
                               * MatrixHelper.CreateRotationDegrees(rotation)
                               * Matrix.CreateTranslation(halfSize + translation)
                               * characterMatrix;
            }
            else 
            {
                
                    Effect.World = characterMatrix;
                
            }

            int drawCount = 0;
            var original = args.GraphicsDevice.RasterizerState;

            try
            {
                // device.RasterizerState = RasterizerState.CullCounterClockwise;
                var count = Vertices.Length;
                args.GraphicsDevice.SetVertexBuffer(Buffer);

                count = Math.Min(count, Buffer.VertexCount);

                foreach (var a in Effect.CurrentTechnique.Passes)
                {
                    a.Apply();

                  /*  DrawLine(args.GraphicsDevice, Vector3.Zero, Vector3.UnitY * 16f, Color.Green);
                    DrawLine(args.GraphicsDevice, Vector3.Zero, Vector3.UnitZ * 16f, Color.Blue);
                    DrawLine(args.GraphicsDevice, Vector3.Zero, Vector3.UnitX * 16f, Color.Red);*/

                    args.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, count / 3);
                    drawCount++;
                    //device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indexes, 0, Indexes.Length / 3);
                }
            }
            finally
            {
                args.GraphicsDevice.RasterizerState = original;

                if (args is AttachedRenderArgs at)
                {
                    args.GraphicsDevice.SetVertexBuffer(at.Buffer);
                }
            }

            return drawCount;
        }

        protected virtual void InitEffect(BasicEffect effect)
        {
            effect.VertexColorEnabled = true;
        }

        public static void DrawLine(GraphicsDevice device, Vector3 start, Vector3 end, Color color)
        {
            var vertices = new[] {new VertexPositionColor(start, color), new VertexPositionColor(end, color)};
            device.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }
        
        public virtual bool Cache(ResourceManager pack)
        {
            return false;
            //Buffer = GpuResourceManager.GetBuffer(this, )
        }

        public virtual IItemRenderer Clone()
        {
            return new ItemModelRenderer<TVertice>(Model, _declaration)
            {
                Size = this.Size
            };
        }

        public string Name => "Item-Renderer";

        /// <inheritdoc />
        public void AddChild(IAttached modelBone)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Remove(IAttached modelBone)
        {
            throw new NotImplementedException();
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
