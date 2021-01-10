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
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SixLabors.ImageSharp.PixelFormats;
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
                try
                {
                    //  var texture = rawTexture.CloneAs<Rgba32>();

                   // float toolPosX = 0.0f;
                  //  float toolPosY = 0f; //1.0f;
                   // float toolPosZ = 0f;//(1f / 16f) * 7.5f;

                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            var pixel = bitmap[x, y];

                            if (pixel.A == 0)
                            {
                                continue;
                            }

                            Color color = new Color(pixel.R, pixel.G, pixel.B, pixel.A);
                            var origin = new Vector3(
                                (x), (bitmap.Height - y), 0f);
                            
                            ItemModelCube built = new ItemModelCube(new Vector3(bitmap.Width / 16f, bitmap.Height / 16f, 1f));

                            built.BuildCube(color);

                            vertices.AddRange(
                                Modify(
                                    built.Front.Concat(built.Bottom).Concat(built.Back).Concat(built.Top)
                                       .Concat(built.Left).Concat(built.Right), origin));
                            //vertices.AddRange(built.Front);
                           // vertices.AddRange(built.Bottom);
                           // vertices.AddRange(built.Back);
                            //vertices.AddRange(built.Top);
                           // vertices.AddRange(built.Left);
                           // vertices.AddRange(built.Right);
                        }
                    }

                    this.Size = new Vector3(bitmap.Width, bitmap.Height, 1f);
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
        
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Translation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;

        private          VertexBuffer      Buffer { get; set; } = null;
        private readonly VertexDeclaration _declaration;

        public ItemModelRenderer(ResourcePackModelBase model, VertexDeclaration declaration)
        {
            Model = model;
            _declaration = declaration;
        }

        //private Matrix _parentMatrix = Matrix.Identity;
        public void Update(IUpdateArgs args, MCMatrix characterMatrix, Vector3 diffuseColor)
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

            var activeDisplayItem = ActiveDisplayItem;

            var halfSize = Size / 2f;
            // var forward = 

            Effect.World = MCMatrix.CreateTranslation(-halfSize)
                           * MCMatrix.CreateRotationDegrees(activeDisplayItem.Rotation)
                           * MCMatrix.CreateTranslation(halfSize) 
                           * MCMatrix.CreateScale(activeDisplayItem.Scale)
                           * MCMatrix.CreateRotationDegrees(activeDisplayItem.Translation)
                           * characterMatrix;
            
            Effect.DiffuseColor = diffuseColor;
            
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

        public void Render(IRenderArgs args, bool mock, out int vertices)
        {
            Render(args.GraphicsDevice);

            vertices = 0;
        }

        protected virtual void InitEffect(BasicEffect effect)
        {
            effect.VertexColorEnabled = true;
        }

        private void DrawLine(GraphicsDevice device, Vector3 start, Vector3 end, Color color)
        {
            var vertices = new[] {new VertexPositionColor(start, color), new VertexPositionColor(end, color)};
            device.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        private void Render(GraphicsDevice device)
        {
            if (Effect == null || Buffer == null || Buffer.VertexCount == 0)
                return;

            var original = device.RasterizerState;

            try
            {
               // device.RasterizerState = RasterizerState.CullCounterClockwise;
                var count = Vertices.Length;
                device.SetVertexBuffer(Buffer);

                count = Math.Min(count, Buffer.VertexCount);

                foreach (var a in Effect.CurrentTechnique.Passes)
                {
                    a.Apply();

                    DrawLine(device, Vector3.Zero, Vector3.UnitY * 16f, Color.Green);
                    DrawLine(device, Vector3.Zero, Vector3.UnitZ * 16f, Color.Blue);
                    DrawLine(device, Vector3.Zero, Vector3.UnitX * 16f, Color.Red);

                    device.DrawPrimitives(PrimitiveType.TriangleList, 0, count / 3);
                    //device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indexes, 0, Indexes.Length / 3);
                }
            }
            finally
            {
                device.RasterizerState = original;
            }
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
    }
}
