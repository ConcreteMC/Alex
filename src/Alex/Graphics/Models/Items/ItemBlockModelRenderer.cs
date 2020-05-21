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
    public class ItemBlockModelRenderer : ItemModelRenderer<BlockShaderVertex>
    {
        private BlockState _block;
        private ResourceManager _resource;

        public ItemBlockModelRenderer(BlockState block, ResourcePackModelBase model, McResourcePack resourcePack,
            ResourceManager resourceManager) : base(model, resourcePack,
            BlockShaderVertex.VertexDeclaration)
        {
            _block = block;
            _resource = resourceManager;

            Offset = new Vector3(0f, -0.5f, 0f);
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

        public override IItemRenderer Clone()
        {
            return new ItemBlockModelRenderer(_block, Model, null, _resource)
            {
                Vertices = (BlockShaderVertex[]) Vertices.Clone(),
                Indexes = (short[]) Indexes.Clone()
            };
        }
    }
}