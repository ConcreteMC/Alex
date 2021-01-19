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
        private BlockState _blockState;
        private Texture2D  _texture;

        public ItemBlockModelRenderer(BlockState block, ResourcePackModelBase model, Texture2D texture) : base(model,
            VertexPositionColorTexture.VertexDeclaration)
        {
            _blockState = block;
            _texture = texture;
            Scale = new Vector3(8f, 8f, 8f);
          //  Size = new Vector3(16f, 16f, 16f);
            //Offset = new Vector3(0f, -0.5f, 0f);
            //  Translation = -Vector3.Forward * 8f;
        }

        public override bool Cache(ResourceManager pack)
        {
            if (Vertices != null)
                return true;
            
            ChunkData chunkData = new ChunkData(ChunkCoordinates.Zero);
            
            _blockState.Model.GetVertices(new ItemRenderingWorld(_blockState.Block), chunkData, BlockCoordinates.Zero, Vector3.Zero, _blockState.Block);

           // var max         = _block.Model.GetBoundingBoxes(Vector3.Zero).Max(x => x.Max - x.Min);
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

           // if (_animated)
            {
                effect.Texture = _texture;// _resource.Atlas.GetAtlas(0);
            }
            //else
            {
                //effect.Texture = _resource.Atlas.GetStillAtlas();
            }
        }

        public override IItemRenderer Clone()
        {
            return new ItemBlockModelRenderer(_blockState, Model, _texture)
            {
                Vertices = (VertexPositionColorTexture[]) Vertices.Clone(),
                Size = Size
            };
        }
    }
}