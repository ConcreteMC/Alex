using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Common;
using Alex.Common.Graphics;
using Alex.Common.Gui.Graphics;
using Alex.Common.Utils.Vectors;
using Alex.Gamestates;
using Alex.Graphics.Camera;
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
        
        private Dictionary<ChunkCoordinates, TextureContainer> _textureContainers =
            new Dictionary<ChunkCoordinates, TextureContainer>();

        private       float _frameAccumulator = 0f;
        private       float _targetTime        = 1f / 10f;

        // private RenderTarget2D _renderTarget;
        
        private object _containerLock = new object();
        private SpriteBatch _spriteBatch;
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

            _spriteBatch = new SpriteBatch(Alex.Instance.GraphicsDevice) {Name = "Minimap Spritebatch"};
            AddChild(_marker = new Image(AlexGuiTextures.MapMarkerWhite)
            {
                Anchor = Alignment.MiddleCenter,
                RotationOrigin = new Vector2(4, 4)
            });
        }

        /// <inheritdoc />
        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
            //    _renderTarget = new RenderTarget2D(GuiManager.GraphicsDevice, Width, Height);
            //  _renderTarget.ContentLost += RenderTargetOnContentLost;
            //    Background.Texture = (TextureSlice2D) _renderTarget;
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
            lock (_containerLock)
            {
                return _textureContainers.TryAdd(coordinates, container);
            }
        }   
        
        private bool TryGetContainer(ChunkCoordinates coordinates, out TextureContainer container)
        {
            lock (_containerLock)
            {
                if (_textureContainers.TryGetValue(coordinates, out container))
                {
                    if (container.Invalidated)
                    {
                        container.Dispose();
                        _textureContainers.Remove(coordinates);
                        container = default;

                        return false;
                    }

                    return true;
                }

                return false;
            }
        }

        private void RemoveContainer(ChunkCoordinates coordinates)
        {
            if (TryGetContainer(coordinates, out var container))
            {
                lock (_containerLock)
                {
                    _textureContainers.Remove(coordinates);
                }
                
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
                        container.Update(chunk, device, _spriteBatch);
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
                var cc = container.Coordinates - center;
                var texturePosition = tCenter + new Vector2(cc.X * 16, cc.Z * 16);

                var texture = container.Texture;

                if (texture != null)
                    graphics.FillRectangle(new Rectangle((renderPos + texturePosition).ToPoint(), texture.Bounds.Size), (TextureSlice2D)texture);
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
                lock (_containerLock)
                {
                    var elements = _textureContainers.ToArray();
                    _textureContainers.Clear();

                    foreach (var element in elements)
                    {
                        element.Value?.Dispose();
                    }
                }
            }
        }

        private class TextureContainer : IDisposable
        {
            public bool IsDirty { get; private set; }
            public bool Invalidated { get; private set; } = false;
            
            public RenderTarget2D Texture { get; private set; }

            public ChunkCoordinates Coordinates { get; }
            public TextureContainer(ChunkCoordinates coordinates)
            {
                Coordinates = coordinates;
            }

            private void Init(GraphicsDevice device)
            {
                if (Texture != null)
                    return;
                
                Texture = new RenderTarget2D(device, 16, 16, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                Texture.ContentLost += TextureOnContentLost;
            }

            public void Update(ChunkColumn target, GraphicsDevice device, SpriteBatch spriteBatch)
            {
                if (target == null)
                {
                    Invalidated = true;
                    return;
                }

                if (Texture == null)
                    Init(device);
                
                using (device.PushRenderTarget(Texture))
                {
                    spriteBatch.Begin(
                        SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap,
                        DepthStencilState.Default, RasterizerState.CullCounterClockwise);

                    for (int x = 0; x < 16; x++)
                    {
                        for (int z = 0; z < 16; z++)
                        {
                            var height = target.GetHeight(x, z);
                            var state = target.GetBlockState(x, height - 1, z);

                            var color = state?.Block?.BlockMaterial?.MapColor?.GetMapColor(2) ?? Color.Black;
                            spriteBatch.FillRectangle(new Rectangle(x, z, 1, 1), color);
                        }
                    }

                    spriteBatch.End();
                }

                IsDirty = false;
            }

            private void TextureOnContentLost(object sender, EventArgs e)
            {
                MarkDirty();
            }

            public void MarkDirty()
            {
                IsDirty = true;
            }
            
            /// <inheritdoc />
            public void Dispose()
            {
               Texture?.Dispose();
               Texture = null;
            }
        }
    }
}
