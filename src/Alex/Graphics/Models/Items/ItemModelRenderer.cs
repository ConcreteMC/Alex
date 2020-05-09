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
using Alex.Blocks.State;
using Alex.Entities;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp.PixelFormats;
using Color = Microsoft.Xna.Framework.Color;
using MathF = System.MathF;

namespace Alex.Graphics.Models.Items
{
    public interface IItemRenderer : IAttachable
    {
        ResourcePackModelBase Model { get; }

        Vector3 Rotation { get; set; }
        Vector3 Translation { get; set; }
        Vector3 Scale { get; set; }
        DisplayPosition DisplayPosition { get; set; }
        DisplayElement ActiveDisplayItem { get; }

        void Update(GraphicsDevice device, ICamera camera);

        void Cache(McResourcePack pack);

        IItemRenderer Clone();
    }

    public class ItemBlockModelRenderer : ItemModelRenderer<VertexPositionNormalTextureColor>
    {
        private BlockState _block;
        private ResourceManager _resource;

        public ItemBlockModelRenderer(BlockState block, ResourcePackModelBase model, McResourcePack resourcePack,
            ResourceManager resourceManager) : base(model, resourcePack,
            VertexPositionNormalTextureColor.VertexDeclaration)
        {
            _block = block;
            _resource = resourceManager;

          //  Translation = -Vector3.Forward * 8f;
        }

        public override void Cache(McResourcePack pack)
        {
            if (Vertices != null)
                return;

            var data = _block.Model.GetVertices(new ItemRenderingWorld(_block.Block), Vector3.Zero, _block.Block);
            Vertices = data.vertices;
            Indexes = data.indexes.Select(x => (short) x).ToArray();
        }

        protected override void InitEffect(BasicEffect effect)
        {
            base.InitEffect(effect);
            effect.TextureEnabled = true;

            if (_block.Block.Animated)
            {
                effect.Texture = _resource.Atlas.GetAtlas(0);
            }
            else
            {
                effect.Texture = _resource.Atlas.GetStillAtlas();
            }
        }

        public override void Update(GraphicsDevice device, ICamera camera)
        {
            base.Update(device, camera);
        }

        //
        // public override void Update(GraphicsDevice device, ICamera camera)
        // {
        //     if (Effect == null)
        //     {
        //         Effect = new BasicEffect(device);
        //         Effect.VertexColorEnabled = true;
        //         Effect.TextureEnabled = true;
        //
        //         if (_block.Block.Animated)
        //         {
        //             Effect.Texture = _resource.Atlas.GetAtlas(0);
        //         }
        //         else
        //         {
        //             Effect.Texture = _resource.Atlas.GetStillAtlas();
        //         }
        //     }
        //
        //     Effect.Projection = camera.ProjectionMatrix;
        //     Effect.View = camera.ViewMatrix;
        //
        //     var scale = Scale;
        //
        //     Effect.World = Matrix.CreateScale(scale)
        //                    * Matrix.CreateTranslation(Translation)
        //                    * Matrix.CreateFromAxisAngle(Vector3.Up, MathUtils.ToRadians(Rotation.Y))
        //                    * Matrix.CreateFromAxisAngle(Vector3.Right, MathUtils.ToRadians(Rotation.X))
        //                    * Matrix.CreateFromAxisAngle(Vector3.Forward, MathHelper.TwoPi - MathUtils.ToRadians(Rotation.Z))
        //                    // * Matrix.CreateRotationY(Rotation.Y - MathHelper.PiOver4)
        //                    // * Matrix.CreateRotationX(Rotation.X + MathHelper.PiOver4)
        //                    // * Matrix.CreateRotationZ(Rotation.Z)
        //                    * ParentMatrix;
        //
        //     base.Update(device, camera);
        // }

        public override IItemRenderer Clone()
        {
            return new ItemBlockModelRenderer(_block, Model, null, _resource)
            {
                
            };
        }
    }

    public class ItemModelRenderer : ItemModelRenderer<VertexPositionColor>
    {
        public ItemModelRenderer(ResourcePackModelBase model, McResourcePack resourcePack) : base(model, resourcePack,
            VertexPositionColor.VertexDeclaration)
        {
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

        public ItemModelRenderer(ResourcePackModelBase model, McResourcePack resourcePack, VertexDeclaration declaration)
        {
            Model = model;
            _declaration = declaration;
        }

        protected Matrix ParentMatrix = Matrix.Identity;

        public long VertexCount => Vertices?.Length ?? 0;
        
        public void Update(PlayerLocation knownPosition)
        {
            Translation = knownPosition.ToVector3();
            Rotation = knownPosition.ToRotationVector3();
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

            // Effect.World = 
            // 	Matrix.CreateTranslation(new Vector3(-trans.X, -trans.Y, trans.Z)) *
            // 	Matrix.CreateScale(scale) * 
            //                Matrix.CreateFromAxisAngle(Vector3.Forward, MathUtils.ToRadians(360f - Rotation.Z)) *
            //                Matrix.CreateFromAxisAngle(Vector3.Up, MathUtils.ToRadians(Rotation.Y)) *
            //                Matrix.CreateFromAxisAngle(Vector3.Right, MathUtils.ToRadians(Rotation.X)) *
            //                Matrix.CreateTranslation(
            // 	               new Vector3(trans.X, trans.Y, -trans.Z)) *
            // 				Matrix.CreateTranslation(0f, 0.62f, 0f)
            //                * ParentMatrix;

            var world = Matrix.Identity;

            if (Model.GuiLight.HasValue && Model.GuiLight == GuiLight.Side)
            {
                world *= Matrix.CreateTranslation(-new Vector3(0.5f, 0.5f, 0.5f))
                         * Matrix.CreateFromYawPitchRoll(45f, 45f, 0f)
                         * Matrix.CreateTranslation(new Vector3(0.5f, 0.5f, 0.5f));
            }

            if (activeDisplayItem != null)
            {
             //   var t = activeDisplayItem.Translation * new Vector3(1f, 1f, -1f) * (1 / 16f);
             world *= Matrix.CreateScale(activeDisplayItem.Scale)
                      * Matrix.CreateTranslation(activeDisplayItem.Translation.X / 32f,
                          activeDisplayItem.Translation.Y / 32f, activeDisplayItem.Translation.Z / 32f)
                      * Matrix.CreateRotationX(MathUtils.ToRadians(activeDisplayItem.Rotation.X))
                      * Matrix.CreateRotationZ(MathUtils.ToRadians(activeDisplayItem.Rotation.Z))
                      * Matrix.CreateRotationY(MathF.PI - MathUtils.ToRadians(activeDisplayItem.Rotation.Y));
                //* Matrix.CreateRotationY(MathF.PI)
                      /*   * Matrix.CreateTranslation(t)
                         * Matrix.CreateFromAxisAngle(Vector3.Up, MathUtils.ToRadians(activeDisplayItem.Rotation.Y))
                         * Matrix.CreateFromAxisAngle(Vector3.Right, MathUtils.ToRadians(activeDisplayItem.Rotation.X))
                         * Matrix.CreateFromAxisAngle(Vector3.Forward,
                             MathHelper.TwoPi - MathUtils.ToRadians(activeDisplayItem.Rotation.Z))*/
                    ;
            }
            else
            {
                world *= Matrix.CreateScale(1f)
                         //* Matrix.CreateTranslation(t)
                         // * Matrix.CreateFromAxisAngle(Vector3.Up, MathUtils.ToRadians(rotation.Y))
                         //* Matrix.CreateFromAxisAngle(Vector3.Right, MathUtils.ToRadians(activeDisplayItem.Rotation.X))
                         * Matrix.CreateFromAxisAngle(Vector3.Forward, MathHelper.TwoPi)
                    ;
            }

            // if(!_displayPosition.HasFlag(ResourcePackLib.Json.Models.Items.DisplayPosition.Gui))
            //     world *= Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);

            {
                // HACKS
                if (//_displayPosition.HasFlag(DisplayPosition.ThirdPerson) ||
                    (_displayPosition & ResourcePackLib.Json.Models.Items.DisplayPosition.FirstPerson) != 0)
                {
                    world *= Matrix.CreateTranslation(0f, 12f/16f, 4f/16f);
                }else if ((_displayPosition & ResourcePackLib.Json.Models.Items.DisplayPosition.ThirdPerson) != 0)
                {
                    world *= Matrix.CreateRotationX(MathF.PI / 4f);
                    world *= Matrix.CreateRotationX(MathF.PI / 4f);
                    world *= Matrix.CreateTranslation(1/16f, 11f/16f, -4f/16f);
                }
            }

            //world *= Matrix.CreateTranslation(trans);
            //                           * Matrix.CreateTranslation(-trans)
            // * Matrix.CreateFromAxisAngle(Vector3.Up, MathUtils.ToRadians(Rotation.Y) - (MathHelper.PiOver4*3))
            // * Matrix.CreateFromAxisAngle(Vector3.Right, MathUtils.ToRadians(Rotation.X) + MathHelper.PiOver4)
            // * Matrix.CreateFromAxisAngle(Vector3.Forward, MathHelper.TwoPi - MathUtils.ToRadians(Rotation.Z))
            // * Matrix.CreateRotationY(Rotation.Y - (MathHelper.PiOver4*3))
            // * Matrix.CreateRotationX(Rotation.X + MathHelper.PiOver4)
            // * Matrix.CreateRotationZ(MathHelper.TwoPi - Rotation.Z)

            ParentMatrix = Matrix.Identity *
                           Matrix.CreateScale(scale) *
                           //  Matrix.CreateRotationX(MathHelper.ToRadians(rotation.Y - 180f)) *
                         //  Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Y - 180f)) *
                           Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y - 180f)) *
                           Matrix.CreateTranslation(trans);
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
