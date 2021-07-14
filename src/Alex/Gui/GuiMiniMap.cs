using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.Common;
using Alex.Common.Graphics;
using Alex.Common.Gui.Graphics;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Gamestates;
using Alex.Graphics.Camera;
using Alex.Utils;
using Alex.Worlds;
using Alex.Worlds.Chunks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;


namespace Alex.Gui
{
    public class GuiMiniMap : RocketElement
    {
        private World World { get; }
        
        private ConcurrentDictionary<ChunkCoordinates, TextureContainer> _textureContainers =
            new ConcurrentDictionary<ChunkCoordinates, TextureContainer>();

        private       float _frameAccumulator = 0f;
        private       float _targetTime        = 1f / 10f;

        private Image _marker;
        public GuiMiniMap(World world)
        {
            World = world;
            //world.ChunkManager.

            Background = Color.White * 0.5f;
            ClipToBounds = true;
            
            AutoSizeMode = AutoSizeMode.None;
            Width = 128;
            Height = 128;
            Margin = new Thickness(10, 10);
            Anchor = Alignment.TopRight;
            
            World.ChunkManager.OnChunkAdded += OnChunkAdded;
            World.ChunkManager.OnChunkRemoved += OnChunkRemoved;
            World.ChunkManager.OnChunkUpdate += OnChunkUpdate;
            
            AddChild(_marker = new Image(AlexGuiTextures.MapMarkerWhite)
            {
                Anchor = Alignment.MiddleCenter,
                RotationOrigin = new Vector2(4, 4)
            });
        }

        private void OnChunkUpdate(object sender, ChunkUpdatedEventArgs e)
        {
            if (TryGetContainer(e.Position, out var container))
            {
                container.MarkDirty();
            }
        }

        private void OnChunkRemoved(object sender, ChunkRemovedEventArgs e)
        {
            RemoveContainer(e.Position);
        }

        private void OnChunkAdded(object sender, ChunkAddedEventArgs e)
        {
            var container = new TextureContainer(e.Position);
            if (TryAdd(e.Position, container))
            {
                container.MarkDirty();
            }
            else
            {
                if (TryGetContainer(e.Position, out var c))
                {
                    c.MarkDirty();
                }
            }
        }

        private bool TryAdd(ChunkCoordinates coordinates, TextureContainer container)
        {
            return _textureContainers.TryAdd(coordinates, container);
        }

        private bool TryGetContainer(ChunkCoordinates coordinates, out TextureContainer container)
        {
            if (!_textureContainers.TryGetValue(coordinates, out container)) return false;

            if (container.Invalidated)
            {
                RemoveContainer(coordinates);
                return false;
            }

            return true;
        }
        

        private void RemoveContainer(ChunkCoordinates coordinates)
        {
            if (_textureContainers.TryRemove(coordinates, out var container))
            {
                container.Dispose();
            }
        }

        private IEnumerable<TextureContainer> GetContainers(ChunkCoordinates center, int radius)
        {
            for (int x = center.X - radius; x < center.X + radius; x++)
            {
                for (int y = center.Z - radius; y < center.Z + radius; y++)
                {
                    if (TryGetContainer(new ChunkCoordinates(x, y), out var container))
                    {
                        yield return container;
                    }
                }
            }
        }
        
        private int _radius = 8;
        private ChunkCoordinates _playerPosition = ChunkCoordinates.Zero;
        private Vector2 _playerOffset = Vector2.Zero;
        
        private float _rotation = 0f;

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (!IsVisible)
                return;
            
            var frameTime = (float) gameTime.ElapsedGameTime.TotalSeconds; // / 50;
            _frameAccumulator += frameTime;

            if (_frameAccumulator < _targetTime)
                return;

            _frameAccumulator = 0;
            
            _rotation = (World.Player.KnownPosition.HeadYaw).ToRadians();
            _marker.Rotation = 180f - World.Player.KnownPosition.HeadYaw;
            
            Rotation = _rotation;
            var playerPos = World.Player.KnownPosition;
            _playerPosition = new ChunkCoordinates(playerPos);
            
            var scale = RenderBounds.Width / (_radius * 16f);
            
            var playerOffset =new Vector3(_playerPosition.X << 4, 0, _playerPosition.Z << 4) -  playerPos.ToVector3();
            _playerOffset = new Vector2(playerOffset.X, playerOffset.Z) * scale;
            
            var device = Alex.Instance.GraphicsDevice;

            using (new GraphicsContext(device))
            {
                foreach (var container in GetContainers(_playerPosition, _radius))
                {
                    if (container.IsDirty)
                    {
                        World.ChunkManager.TryGetChunk(container.Coordinates, out var chunk);
                        container.Update(World, chunk, device);
                    }
                }
            }
        }

        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            base.OnDraw(graphics, gameTime);
            
            if (!IsVisible)
                return;
            
            var center = _playerPosition;

          //  var halfRadius = _radius / 2;
            var tCenter = new Vector2(Width / 2f, Height / 2f);
            var renderPos = RenderBounds.Location.ToVector2();
            renderPos += _playerOffset;
            foreach (var container in GetContainers(center, _radius))
            {
                var texture = container.Texture;

                if (texture != null)
                {
                    var cc = container.Coordinates - center;
                    var texturePosition = tCenter + new Vector2(cc.X * 16, cc.Z * 16);
                    graphics.FillRectangle(new Rectangle((renderPos + texturePosition).ToPoint(), texture.Bounds.Size), (TextureSlice2D)texture);
                }
            }

            graphics.DrawRectangle(RenderBounds, Color.Black, 1);
            //  var rotationOrigin = new Vector2(renderTarget.Width / 2f, renderTarget.Height / 2f);
         //   var renderPos = new Vector2(RenderBounds.X + rotationOrigin.X, RenderBounds.Y + rotationOrigin.Y);
        //    var target = new Rectangle(renderPos.ToPoint(), new Point(renderTarget.Width, renderTarget.Height));
        //    graphics.SpriteBatch.Draw(renderTarget,
        //        target, new Rectangle(0,0, renderTarget.Width, renderTarget.Height), Color.White, _rotation, rotationOrigin, SpriteEffects.None, 0f);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                World.ChunkManager.OnChunkAdded -= OnChunkAdded;
                World.ChunkManager.OnChunkRemoved -= OnChunkRemoved;
                World.ChunkManager.OnChunkUpdate -= OnChunkUpdate;
                
                var elements = _textureContainers.ToArray();
                _textureContainers.Clear();

                foreach (var element in elements)
                {
                    element.Value?.Dispose();
                }
            }
        }

        private class TextureContainer : IDisposable
        {
            public bool IsDirty { get; private set; }
            public bool Invalidated { get; private set; } = false;
            
            public Texture2D Texture { get; private set; }

            public ChunkCoordinates Coordinates { get; }
            private Map _map;
            public TextureContainer(ChunkCoordinates coordinates)
            {
                Coordinates = coordinates;
                _map = new Map(16, 16);
            }

            private void Init(GraphicsDevice device)
            {
                if (Texture != null)
                    return;
                
                Texture = new Texture2D(device, 16, 16);
            }

            public void Update(World world, ChunkColumn target, GraphicsDevice device)
            {
                if (target == null)
                {
                    Invalidated = true;
                    return;
                }

                if (Texture == null)
                    Init(device);
                
                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        var height = target.GetHeight(x, z) - 1;
                        var surrounding = GetHeighestSurrounding(world, target, x, height, z);
                        var offset = 2;

                        if (surrounding > height)
                        {
                            var difference = surrounding - height;

                            if (difference <= 8)
                                offset = 1;
                            else
                                offset = 0;
                        }

                        var state = target.GetBlockState(x, height, z);
                        var blockMaterial = state?.Block?.BlockMaterial;

                        if (blockMaterial != null)
                        {
                            _map[x, z] = blockMaterial.MapColor.Index * 4 + offset;
                        }
                    }
                }

                Texture.SetData(_map.GetData());
                IsDirty = false;
            }

            private int GetHeighestSurrounding(World world, ChunkColumn chunk, int x, int y, int z)
            {
                var coords = new BlockCoordinates(x + (chunk.X * 16), y, z + (chunk.Z * 16));
                
                var h = Math.Max(0, world.GetHeight(coords + BlockCoordinates.West) - 1);
                h = Math.Max(h, world.GetHeight(coords + BlockCoordinates.East) - 1);
                h = Math.Max(h, world.GetHeight(coords + BlockCoordinates.North) - 1);
                h = Math.Max(h, world.GetHeight(coords + BlockCoordinates.South) - 1);

                return h;
            }

            public void MarkDirty()
            {
                IsDirty = true;
            }
            
            /// <inheritdoc />
            public void Dispose()
            {
                _map?.Dispose();
                _map = null;
               Texture?.Dispose();
               Texture = null;
            }
        }
    }
}
