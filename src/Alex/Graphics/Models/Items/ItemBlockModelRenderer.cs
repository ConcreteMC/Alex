using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Blocks.State;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Models;
using Alex.Worlds;
using Alex.Worlds.Chunks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Items
{
    public class ItemBlockModelRenderer : ItemModelRenderer<VertexPositionColorTexture>
    {
        private bool            _animated = false;
        private BlockState      _block;
        private ResourceManager _resource;

        public ItemBlockModelRenderer(BlockState block, ResourcePackModelBase model,
            ResourceManager resourceManager) : base(model,
            VertexPositionColorTexture.VertexDeclaration)
        {
            _block = block;
            _resource = resourceManager;
            _animated = _block.Block.Animated;
            
            Offset = new Vector3(0f, -0.5f, 0f);
            //  Translation = -Vector3.Forward * 8f;
        }

        private ItemBlockModelRenderer(bool animated, ResourcePackModelBase model, ResourceManager resourceManager) : base(model, VertexPositionColorTexture.VertexDeclaration)
        {
            _animated = animated;
            _resource = resourceManager;
            Offset = new Vector3(0f, -0.5f, 0f);
        }
        
        public override bool Cache(ResourceManager pack)
        {
            if (Vertices != null)
                return true;
            
            ChunkData chunkData = new ChunkData();
            _block.Model.GetVertices(new ItemRenderingWorld(_block.Block), chunkData, BlockCoordinates.Zero, Vector3.Zero, _block.Block);
            
            var rawVertices = chunkData.Vertices;
            int count       = rawVertices.Length;

            while (count > 0 && count % 3 != 0)
            {
                count--;
            }
            
            Vertices = rawVertices.Take(count).Select(
                x => new VertexPositionColorTexture(x.Position, x.Color, x.TexCoords)).ToArray();

            chunkData.Dispose();

            return true;
        }

        protected override void InitEffect(BasicEffect effect)
        {
            base.InitEffect(effect);
            effect.TextureEnabled = true;

            if (_animated)
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
            return new ItemBlockModelRenderer(_block, Model, _resource)
            {
                Vertices = (VertexPositionColorTexture[]) Vertices.Clone()
            };
        }
    }
}