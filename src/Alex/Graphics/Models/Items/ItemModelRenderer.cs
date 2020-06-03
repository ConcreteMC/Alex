using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Alex.API.Blocks;
using Alex.API.Blocks.State;
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
        public ItemModelRenderer(ResourcePackModelBase model, McResourcePack resourcePack) : base(model, resourcePack,
            VertexPositionColor.VertexDeclaration)
        {
            if (model.ParentName != "item/handheld")
            {
                Offset = new Vector3(0f, -0.5f, 0f);
            }
        }

        public override void Cache(McResourcePack pack)
        {
            string t = string.Empty;
            if (!Model.Textures.TryGetValue("layer0", out t))
            {
                t = Model.Textures.FirstOrDefault(x => x.Value != null).Value;
            }

            if (t == default)
            {
                return;
            }

            List<VertexPositionColor> vertices = new List<VertexPositionColor>();
            List<short> indexes = new List<short>();

            if (pack.TryGetBitmap(t, out var rawTexture))
            {
                var texture = rawTexture.CloneAs<Rgba32>();

                int i = 0;
                float toolPosX = 0.0f;
                float toolPosY = 1.0f;
                float toolPosZ = (1f / 16f) * 7.5f;
                int verticesPerTool = texture.Width * texture.Height * 36;


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

                        vertices = ModifyCubeIndexes(vertices, ref built.Front, origin);
                        vertices = ModifyCubeIndexes(vertices, ref built.Back, origin);
                        vertices = ModifyCubeIndexes(vertices, ref built.Top, origin);
                        vertices = ModifyCubeIndexes(vertices, ref built.Bottom, origin);
                        vertices = ModifyCubeIndexes(vertices, ref built.Left, origin);
                        vertices = ModifyCubeIndexes(vertices, ref built.Right, origin);

                        var indices = built.Front.indexes
                            .Concat(built.Back.indexes)
                            .Concat(built.Top.indexes)
                            .Concat(built.Bottom.indexes)
                            .Concat(built.Left.indexes)
                            .Concat(built.Right.indexes)
                            .ToArray();

                        indexes.AddRange(indices);
                    }
                }
            }

            Vertices = vertices.ToArray();

            for (var index = 0; index < Vertices.Length; index++)
            {
                var vertice = Vertices[index];
                Vertices[index] = vertice;
            }

            Indexes = indexes.ToArray();
        }

        private List<VertexPositionColor> ModifyCubeIndexes(List<VertexPositionColor> vertices,
            ref (VertexPositionColor[] vertices, short[] indexes) data, Vector3 offset)
        {
            var startIndex = (short) vertices.Count;
            foreach (var vertice in data.vertices)
            {
                var vertex = vertice;
                vertex.Position += offset;
                vertices.Add(vertex);
            }

            //vertices.AddRange(data.vertices);

            for (int i = 0; i < data.indexes.Length; i++)
            {
                data.indexes[i] += startIndex;
            }

            return vertices;
        }

        public override IItemRenderer Clone()
        {
            return new ItemModelRenderer(Model, null)
            {
                Vertices = Vertices.ToArray(),
                Indexes = Indexes.ToArray()
            };
        }
    }

    public class ItemModelRenderer<TVertice> : Model, IAttachable, IItemRenderer where TVertice : struct, IVertexType
    {
        public ResourcePackModelBase Model { get; }

        private DisplayPosition _displayPosition;

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

        public DisplayElement ActiveDisplayItem { get; private set; }

        private void UpdateDisplay()
        {
            if (Model.Display.TryGetValue(DisplayPositionHelper.ToString(_displayPosition), out var display))
            {
                ActiveDisplayItem = display;
            }
            else
            {
                ActiveDisplayItem = null;
            }
        }

        public TVertice[] Vertices { get; set; } = null;
        public short[] Indexes { get; set; } = null;

        protected BasicEffect Effect { get; set; } = null;
        
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Translation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;

        private VertexBuffer Buffer { get; set; } = null;
        private IndexBuffer IndexBuffer { get; set; } = null;
        private VertexDeclaration _declaration;

        protected Vector3 Offset { get; set; } = Vector3.Zero;
        
        public ItemModelRenderer(ResourcePackModelBase model, McResourcePack resourcePack, VertexDeclaration declaration)
        {
            Model = model;
            _declaration = declaration;
        }

        protected Matrix ParentMatrix = Matrix.Identity;

        public long VertexCount => Vertices?.Length ?? 0;
        
        public void Update(Matrix matrix, PlayerLocation knownPosition)
        {
            ParentMatrix = matrix;
            
            Translation = knownPosition.ToVector3();
         //   Rotation = knownPosition.ToRotationVector3();
        }

        public void Render(IRenderArgs args)
        {
            Render(args.GraphicsDevice);
        }

        private bool _canInit = true;

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

            var scale = Scale;
            var rotation = Rotation;
            var trans = Translation;

            var world = Matrix.Identity;

            var a = new Vector3(0.5f, 0.5f, 0.5f);
            if (Model.GuiLight.HasValue && Model.GuiLight == GuiLight.Side)
            {
              //  world *= Matrix.CreateTranslation(-a)
           //              * Matrix.CreateFromYawPitchRoll(45f, -45f, 0f)
             //            * Matrix.CreateTranslation(a);
            }

           /* world *= //Matrix.CreateTranslation(-a)
                Matrix.CreateRotationX(MathUtils.ToRadians(Rotation.Z))
                * Matrix.CreateRotationY(MathUtils.ToRadians(Rotation.Y))
                * Matrix.CreateRotationZ(MathUtils.ToRadians(-Rotation.X));*/
                    // * Matrix.CreateTranslation(a);

                   // Matrix.cr
                    
            if (activeDisplayItem != null)
            {
               /* world *= Matrix.CreateTranslation(-a) * Matrix.CreateScale(activeDisplayItem.Scale)
                                                      * Matrix.CreateTranslation(activeDisplayItem.Translation.X / 32f,
                                                          activeDisplayItem.Translation.Y / 16f,
                                                          activeDisplayItem.Translation.Z / 32f)
                                                      * Matrix.CreateRotationX(
                                                          MathUtils.ToRadians(activeDisplayItem.Rotation.X))
                                                      * Matrix.CreateRotationZ(
                                                          MathUtils.ToRadians(activeDisplayItem.Rotation.Z))
                                                      * Matrix.CreateRotationY(
                                                          MathUtils.ToRadians(activeDisplayItem.Rotation.Y)) *
                                                      Matrix.CreateTranslation(a);*/
               
               var displayTrans = new Vector3(activeDisplayItem.Translation.X / 32f,
                   activeDisplayItem.Translation.Y / 16f,
                   activeDisplayItem.Translation.Z / 32f);
               
               world *= Matrix.CreateTranslation(-a) * Matrix.CreateScale(activeDisplayItem.Scale)
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
                offset = new Vector3(-2f / 16f, 11.5f / 16f, -6f / 16f);
                
                world *= Matrix.CreateRotationX(-MathF.PI / 5f);
                world *= Matrix.CreateRotationZ(-1f / 16f);
               // world *= Matrix.CreateTranslation(-2f / 16f, 11.5f / 16f, -6f / 16f);
            }
            else if ((_displayPosition & ResourcePackLib.Json.Models.Items.DisplayPosition.ThirdPerson) != 0)
            {
                offset += new Vector3(-2f / 16f, 8f / 16f, -3f / 16f);
                world *= Matrix.CreateRotationX(-MathF.PI / 4f);
                //     world *= Matrix.CreateRotationX(MathF.PI / 4f);
              //  world *= Matrix.CreateTranslation(-2f / 16f, 8f / 16f, -3f / 16f);
            }

            if (offset != Vector3.Zero)
            {
                world *= Matrix.CreateTranslation(offset);
            }

            if ((_displayPosition & ResourcePackLib.Json.Models.Items.DisplayPosition.FirstPerson) == 0) //Not in first person
            {
                var pivot = offset;

                world *= Matrix.CreateTranslation(pivot)
                         * Matrix.CreateRotationX(MathUtils.ToRadians(-Rotation.X))
                         * Matrix.CreateRotationZ(MathUtils.ToRadians(-Rotation.Z))
                         * Matrix.CreateRotationY(MathUtils.ToRadians(Rotation.Y)) * Matrix.CreateTranslation(-pivot);
            }

            /*ParentMatrix = Matrix.Identity *
                           Matrix.CreateScale(scale) *
                           Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y)) *
                           Matrix.CreateTranslation(trans);*/


            if (Model.GuiLight.HasValue && Model.GuiLight == GuiLight.Side)
            { 
                //var o = new Vector3(0.5f, 0.5f, 0.5f);

             /*   world *= Matrix.CreateTranslation(-o)
                            * Matrix.CreateFromAxisAngle(Vector3.Right, MathUtils.ToRadians(25f))
                            * Matrix.CreateFromAxisAngle(Vector3.Up, MathUtils.ToRadians(270f))
                            * Matrix.CreateTranslation(o);*/
            }
            
            Effect.World = world * ParentMatrix;

            if (Buffer == null && Vertices != null && Indexes != null && _canInit)
            {
                var vertices = Vertices;
                var indexes = Indexes;

                if (vertices.Length == 0 || indexes.Length == 0)
                {
                    _canInit = false;
                }
                else
                {
                    var buffer = GpuResourceManager.GetBuffer(this, device, _declaration,
                        Vertices.Length, BufferUsage.WriteOnly);
                    var indexBuffer = GpuResourceManager.GetIndexBuffer(this, device, IndexElementSize.SixteenBits,
                        Indexes.Length, BufferUsage.WriteOnly);

                    buffer.SetData(vertices);
                    indexBuffer.SetData(indexes);

                    Buffer = buffer;
                    IndexBuffer = indexBuffer;
                }
            }
        }


        private void DrawLine(GraphicsDevice device, Vector3 start, Vector3 end, Color color)
        {
            var vertices = new[] {new VertexPositionColor(start, color), new VertexPositionColor(end, color)};
            device.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }

        public void Render(GraphicsDevice device)
        {
            if (Effect == null || Buffer == null || Buffer.VertexCount == 0)
                return;

            foreach (var a in Effect.CurrentTechnique.Passes)
            {
                a.Apply();

                //DrawLine(device, Vector3.Zero, Vector3.Up, Color.Green);
                //	DrawLine(device, Vector3.Zero, Vector3.Forward, Color.Blue);
                //	DrawLine(device, Vector3.Zero, Vector3.Right, Color.Red);

                device.Indices = IndexBuffer;
                device.SetVertexBuffer(Buffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexBuffer.IndexCount / 3);
                //device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indexes, 0, Indexes.Length / 3);
            }
        }

        public virtual void Cache(McResourcePack pack)
        {
            //Buffer = GpuResourceManager.GetBuffer(this, )
        }

        public virtual IItemRenderer Clone()
        {
            return new ItemModelRenderer<TVertice>(Model, null, _declaration);
        }
    }
}
