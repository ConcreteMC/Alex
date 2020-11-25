using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Alex.API.Blocks;
using Alex.API.Entities;
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

        public override void Cache(McResourcePack pack)
        {
            if (!Model.Textures.TryGetValue("layer0", out var t))
            {
                t = Model.Textures.FirstOrDefault(x => x.Value != null).Value;
            }

            if (t == null)
            {
                return;
            }

            List<VertexPositionColor> vertices = new List<VertexPositionColor>();
         
            if (pack.TryGetBitmap(t, out var rawTexture))
            {
                var texture = rawTexture.CloneAs<Rgba32>();

                float toolPosX = 0.0f;
                float toolPosY = 1.0f;
                float toolPosZ = (1f / 16f) * 7.5f;

                for (int y = 0; y < texture.Height; y++)
                {
                    for (int x = 0; x < texture.Width; x++)
                    {
                        var pixel = texture[x, y];
                        if (pixel.A == 0)
                        {
                            continue;
                        }

                        Color color = new Color(pixel.R, pixel.G, pixel.B, pixel.A);

                        ItemModelCube built =
                            new ItemModelCube(new Vector3(1f / texture.Width, 1f / texture.Height, 1f / 16f));
                        built.BuildCube(color);

                        var origin = new Vector3(
                            (toolPosX + (1f / texture.Width) * x),
                            toolPosY - (1f / texture.Height) * y,
                            toolPosZ
                        );

                        vertices.AddRange(Modify(built.Front, origin));
                        vertices.AddRange(Modify(built.Bottom, origin));
                        vertices.AddRange(Modify(built.Back, origin));
                        vertices.AddRange(Modify(built.Top, origin));
                        vertices.AddRange(Modify(built.Left, origin));
                        vertices.AddRange(Modify(built.Right, origin));
                    }
                }
            }

            Vertices = vertices.ToArray();
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
                Vertices = Vertices.Clone() as VertexPositionColor[],
           //     Indexes = Indexes.ToArray()
            };
        }
    }

    public class ItemModelRenderer<TVertice> : Model, IAttachable, IItemRenderer where TVertice : struct, IVertexType
    {
        public ResourcePackModelBase Model { get; }

        private DisplayPosition _displayPosition;// = DisplayPosition.Ground;

        public DisplayPosition DisplayPosition
        {
            get => _displayPosition;
            set
            {
                var oldDisplayPosition = _displayPosition;
                _displayPosition = value;

                if (oldDisplayPosition != _displayPosition)
                {
                    UpdateDisplay();
                }
            }
        }

        public DisplayElement ActiveDisplayItem { get; private set; } = DisplayElement.Default;

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
            
            ActiveDisplayItem = DisplayElement.Default;
        }

        protected TVertice[]  Vertices { get; set; } = null;
        private   BasicEffect Effect   { get; set; } = null;
        
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Translation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;

        private          VertexBuffer      Buffer { get; set; } = null;
        private readonly VertexDeclaration _declaration;

        protected Vector3 Offset { get; set; } = Vector3.Zero;
        
        public Color DiffuseColor { get; set; } = Color.White;
        
        public ItemModelRenderer(ResourcePackModelBase model, VertexDeclaration declaration)
        {
            Model = model;
            _declaration = declaration;
        }

        private Matrix _parentMatrix = Matrix.Identity;

        /// <inheritdoc />
        public bool ApplyHeadYaw { get; set; }

        /// <inheritdoc />
        public bool ApplyPitch { get; set; }

        public void Update(IUpdateArgs args, Matrix characterMatrix, Vector3 diffuseColor, PlayerLocation modelLocation)
        {
            _parentMatrix = characterMatrix;
            
            Translation = modelLocation.ToVector3();
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

        public virtual void Update(GraphicsDevice device, ICamera camera)
        {
            if (Effect == null)
            {
                var effect = new BasicEffect(device);
                InitEffect(effect);
                Effect = effect;
            }

            Effect.Projection = camera.ProjectionMatrix;
            Effect.View = camera.ViewMatrix;

            var activeDisplayItem = ActiveDisplayItem;

            var world = Matrix.Identity;

            var a = new Vector3(0.5f, 1.0f, 0.5f);

            if (activeDisplayItem != null)
            {
                var displayTrans = new Vector3(activeDisplayItem.Translation.X / 32f,
                    activeDisplayItem.Translation.Y / 32f,
                    activeDisplayItem.Translation.Z / 32f);
                
                world *= 
                    Matrix.CreateTranslation(-a) * 
                    Matrix.CreateScale(activeDisplayItem.Scale)
                    * Matrix.CreateTranslation(displayTrans.X, displayTrans.Y, displayTrans.Z)
                    * Matrix.CreateFromAxisAngle(Vector3.Right, MathUtils.ToRadians(activeDisplayItem.Rotation.X))
                    * Matrix.CreateFromAxisAngle(Vector3.Backward, MathUtils.ToRadians(activeDisplayItem.Rotation.Z))
                    * Matrix.CreateFromAxisAngle(Vector3.Up, MathUtils.ToRadians(activeDisplayItem.Rotation.Y))
                    * Matrix.CreateTranslation(a);
            }
            else
            {
                world *= Matrix.CreateScale(1f)
                         * Matrix.CreateFromAxisAngle(Vector3.Forward, MathHelper.TwoPi);
            }

            var offset = Offset;
            if ((_displayPosition & DisplayPosition.Gui) != 0)
            {
                offset = Vector3.Zero;
            }
            else if ((_displayPosition & ResourcePackLib.Json.Models.Items.DisplayPosition.FirstPerson) != 0)
            {
                offset += new Vector3(-2f / 16f, 0.5f, -6f / 16f);
                world *= Matrix.CreateRotationX(-MathF.PI / 5f);
            }
            else if ((_displayPosition & ResourcePackLib.Json.Models.Items.DisplayPosition.ThirdPerson) != 0)
            {
                offset += new Vector3(-2f / 16f, 0f, -3f / 16f);
                world *= Matrix.CreateRotationX(-MathF.PI / 4f);
            }

            if (offset != Vector3.Zero)
            {
                world *= Matrix.CreateTranslation(offset);
            }

            Effect.World = world * _parentMatrix;
            Effect.DiffuseColor = DiffuseColor.ToVector3();
            
            if (Buffer == null && Vertices != null)
            {
                var vertices = Vertices;
               
                if (vertices.Length == 0)
                {
                   // _canInit = false;
                }
                else
                {
                    var buffer = GpuResourceManager.GetBuffer(this, device, _declaration,
                        Vertices.Length, BufferUsage.WriteOnly);

                    buffer.SetData(vertices);

                    Buffer = buffer;
                }
            }
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

            var count = Vertices.Length;
            device.SetVertexBuffer(Buffer);

            count = Math.Min(count, Buffer.VertexCount);
            
            foreach (var a in Effect.CurrentTechnique.Passes)
            {
                a.Apply();

                //DrawLine(device, Vector3.Zero, Vector3.Up, Color.Green);
                //	DrawLine(device, Vector3.Zero, Vector3.Forward, Color.Blue);
                //	DrawLine(device, Vector3.Zero, Vector3.Right, Color.Red);
                
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, count / 3);
                //device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indexes, 0, Indexes.Length / 3);
            }
        }

        public virtual void Cache(McResourcePack pack)
        {
            //Buffer = GpuResourceManager.GetBuffer(this, )
        }

        public virtual IItemRenderer Clone()
        {
            return new ItemModelRenderer<TVertice>(Model, _declaration);
        }

        public string Name => "Item-Renderer";
    }
}
