using System.Collections.Generic;
using System.Linq;
using Alex.Blocks.State;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Items;
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

        public ItemBlockModelRenderer(BlockState block, ResourcePackModelBase resourcePackModel, Texture2D texture) : base(resourcePackModel,
            VertexPositionColorTexture.VertexDeclaration)
        {
            _blockState = block;
            _texture = texture;
            
            Scale = 16f;
            Size = new Vector3(16f, 16f, 16f);

            float biggestDimensions = 0;
            foreach (var bb in block.Block.GetBoundingBoxes(Vector3.Zero))
            {
                var dimension = bb.GetDimensions();
                var len = dimension.Length();
                if (len > biggestDimensions)
                {
                    biggestDimensions = len;
                    Size = dimension;
                }
            }
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

            var rawVertices = chunkData.BuildVertices();
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
            
            effect.Texture = _texture;
        }

        /// <inheritdoc />
        protected override void UpdateDisplayInfo(DisplayPosition displayPosition, DisplayElement displayElement)
        {
            var root = Model?.Root;

            if (root != null)
            {
                if (displayPosition.HasFlag(DisplayPosition.Gui))
                {
                    root.Size = Size;
                    root.BaseScale = displayElement.Scale;
                    root.BaseRotation = new Vector3(25f, 45f, 0f);
                    root.BasePosition = new Vector3(displayElement.Translation.X, displayElement.Translation.Y + 0.25f, displayElement.Translation.Z);
                }
                else if (displayPosition.HasFlag(DisplayPosition.ThirdPerson))
                {
                    root.Size = Size;
                    root.BaseScale = displayElement.Scale * Scale;
                    root.BaseRotation = new Vector3(-67.5f, 0f, 0f);
                    root.BasePosition = new Vector3(displayElement.Translation.X + 2f, displayElement.Translation.Y +8f, displayElement.Translation.Z - 2f);
                }
                else if (displayPosition.HasFlag(DisplayPosition.FirstPerson))
                {
                    root.Size = Size;
                    root.BaseScale = displayElement.Scale * (Scale / 2f);
                    root.BaseRotation = new Vector3(-67.5f, 0f, 0f) + new Vector3(displayElement.Rotation.X, displayElement.Rotation.Y, displayElement.Rotation.Z);
                    root.BasePosition = new Vector3(displayElement.Translation.X + 4f, displayElement.Translation.Y + 18f, displayElement.Translation.Z - 2f);
                }
                else if (displayPosition.HasFlag(DisplayPosition.Ground))
                {
                    root.Size = null;
                    root.BaseScale = displayElement.Scale * Scale;
                    root.BaseRotation = new Vector3(displayElement.Rotation.X, displayElement.Rotation.Y, displayElement.Rotation.Z);
                    root.BasePosition = new Vector3(displayElement.Translation.X, displayElement.Translation.Y, displayElement.Translation.Z);
                }
                else
                {
                    base.UpdateDisplayInfo(displayPosition, displayElement);
                }
            }
        }

        /// <inheritdoc />
       /* protected override Matrix GetWorldMatrix(DisplayElement activeDisplayItem)
        {
            if ((DisplayPosition & DisplayPosition.Ground) != 0)
            {
                return Matrix.CreateScale(activeDisplayItem.Scale * Scale)
                       * MatrixHelper.CreateRotationDegrees(activeDisplayItem.Rotation)
                       * Matrix.CreateTranslation(activeDisplayItem.Translation);
            }
            
            if ((DisplayPosition & DisplayPosition.FirstPerson) != 0)
            {
                var translate = activeDisplayItem.Translation;
                return Matrix.CreateScale(activeDisplayItem.Scale * (Scale / 2f))
                       * MatrixHelper.CreateRotationDegrees(new Vector3(-67.5f, 0f, 0f))
                       * MatrixHelper.CreateRotationDegrees(activeDisplayItem.Rotation)
                       * Matrix.CreateTranslation(new Vector3(translate.X + 4f, translate.Y + 18f, translate.Z - 2f));
            }
            
            if ((DisplayPosition & DisplayPosition.ThirdPerson) != 0)
            {
                var translate = activeDisplayItem.Translation;
                return Matrix.CreateScale(activeDisplayItem.Scale * Scale)
                       * MatrixHelper.CreateRotationDegrees(new Vector3(-67.5f, 0f, 0f))
                       * MatrixHelper.CreateRotationDegrees(activeDisplayItem.Rotation)
                       * Matrix.CreateTranslation(new Vector3(translate.X + 2f, translate.Y + (8f), translate.Z - 2f));
            }
            
            if ((DisplayPosition & DisplayPosition.Gui) != 0)
            {
                return Matrix.CreateScale(activeDisplayItem.Scale)
                       * MatrixHelper.CreateRotationDegrees(new Vector3(25f, 45f, 0f))
                       * Matrix.CreateTranslation(activeDisplayItem.Translation) 
                       * Matrix.CreateTranslation(new Vector3(0f, 0.25f, 0f));
            }

            return base.GetWorldMatrix(activeDisplayItem);
        }*/

        public override IItemRenderer CloneItemRenderer()
        {
            return new ItemBlockModelRenderer(_blockState, ResourcePackModel, _texture)
            {
                Vertices = (VertexPositionColorTexture[]) Vertices.Clone(),
                Size = Size,
                Scale = Scale
            };
        }
    }
}