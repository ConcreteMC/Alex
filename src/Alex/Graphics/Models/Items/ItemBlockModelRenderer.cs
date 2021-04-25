using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.Utils.Vectors;
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
            
            Scale = new Vector3(16f);
            Size = new Vector3(16f, 16f, 16f);
            //Offset = new Vector3(0f, -0.5f, 0f);
            //  Translation = -Vector3.Forward * 8f;
        }

        
        /// <inheritdoc />
        protected override VertexPositionColorTexture[] Vertices
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
        private VertexPositionColorTexture[] _vertices = null;

        public override bool Cache(ResourceManager pack)
        {
            if (_cached)
                return true;

            _cached = true;

            var world = new ItemRenderingWorld(_blockState.Block);

            ChunkData chunkData = new ChunkData(0,0);

            _blockState?.VariantMapper.Model.GetVertices(
                world, chunkData, BlockCoordinates.Zero, _blockState);

            // var max         = _block.Model.GetBoundingBoxes(Vector3.Zero).Max(x => x.Max - x.Min);
            var rawVertices = chunkData.BuildVertices(world);

            //  while (count > 0 && count % 3 != 0)
            // {
            //      count--;
            // }

            var scale = Vector2.One / _texture.Bounds.Size.ToVector2();

            Vertices = rawVertices.Select(
                    x =>
                    {
                        return new VertexPositionColorTexture(
                            x.Position, x.Color, new Vector2(x.TexCoords.X, x.TexCoords.Y) * scale);
                    })
               .ToArray();

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
               // Vertices = (VertexPositionColorTexture[]) Vertices.Clone(),
                Size = Size
            };
        }
    }
}