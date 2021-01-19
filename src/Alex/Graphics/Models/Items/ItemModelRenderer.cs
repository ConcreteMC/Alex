using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Alex.Api;
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

        private bool _cached = false;
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
                bitmap.Mutate(
                    x =>
                    {
                        x.Flip(FlipMode.Horizontal);
                        x.Rotate(RotateMode.Rotate90);
                    });
                try
                {
                    if (bitmap.TryGetSinglePixelSpan(out var pixels))
                    {
                        var pixelSize = new Vector3(bitmap.Width / 16f, bitmap.Height / 16f, 1f);

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
                                var   origin = new Vector3((x), y, 0f);

                                ItemModelCube built = new ItemModelCube(pixelSize, color);

                                vertices.AddRange(
                                    Modify(
                                        built.Front.Concat(built.Bottom).Concat(built.Back).Concat(built.Top)
                                           .Concat(built.Left).Concat(built.Right), origin));
                            }
                        }

                        this.Size = new Vector3(pixelSize.X * bitmap.Width, pixelSize.Z * bitmap.Height, 1f);
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

        protected TVertice[]  Vertices { get; set; } = null;
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
        public void Update(IUpdateArgs args, MCMatrix characterMatrix)
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

            var halfSize          = Size / 2f;
           // halfSize.Z = 0f;
           // halfSize.Y = 8f;
            var activeDisplayItem = ActiveDisplayItem;
            //   world.Right = -world.Right;

            if (DisplayPosition.HasFlag(DisplayPosition.ThirdPerson))
            {
                var t = activeDisplayItem.Translation;
                var r = activeDisplayItem.Rotation;

                if (r != Vector3.Zero)
                {
                    r.Y += 12.5f;
                    r.Z -= 67.5f;
                    Effect.World = MCMatrix.CreateScale(Scale * activeDisplayItem.Scale)
                                   * MCMatrix.CreateTranslation(-halfSize)
                                   * MCMatrix.CreateRotationZ(MathUtils.ToRadians(-22.5f))
                                   * MCMatrix.CreateTranslation(halfSize)
                                   //* MCMatrix.CreateRotationDegrees(new Vector3(-67.5f, 180f, 0f))
                                   * MCMatrix.CreateRotationDegrees(r * new Vector3(1f, -1f, -1f))
                                   * MCMatrix.CreateTranslation(
                                       new Vector3(t.X + 6f, t.Y + 4f,  t.Z))
                                   * characterMatrix;
                }
                else
                {
                    Effect.World =  
                        MCMatrix.CreateScale(Scale * activeDisplayItem.Scale)
                        * MCMatrix.CreateRotationDegrees(new Vector3(-67.5f, 0f, 0f))
                        * MCMatrix.CreateTranslation(
                           new Vector3(t.X + 2f, Size.Y - t.Y, t.Z))
                        * characterMatrix;
                }
            }
            else  if (DisplayPosition.HasFlag(DisplayPosition.FirstPerson))
            {
                Effect.World = MCMatrix.CreateScale(Scale * activeDisplayItem.Scale)
                               * MCMatrix.CreateRotationY(MathUtils.ToRadians(180f))
                               * MCMatrix.CreateRotationDegrees(activeDisplayItem.Rotation)
                               * MCMatrix.CreateTranslation(halfSize)
                               * MCMatrix.CreateTranslation(activeDisplayItem.Translation * new Vector3(1f, 1f, -1f))
                               * characterMatrix;
            }

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

        public void Render(IRenderArgs args, Microsoft.Xna.Framework.Graphics.Effect effect)
        {
            if (Effect == null || Buffer == null || Buffer.VertexCount == 0)
                return;

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
    }
}
