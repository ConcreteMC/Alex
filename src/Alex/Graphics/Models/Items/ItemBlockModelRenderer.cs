using System.Linq;
using Alex.API.Graphics;
using Alex.Blocks.State;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Models;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Items
{
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
                Vertices = (VertexPositionNormalTextureColor[]) Vertices.Clone(),
                Indexes = (short[]) Indexes.Clone()
            };
        }
    }
}