using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Alex.Common.Utils.Vectors;
using Alex.Entities;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui.Elements.Map
{
    public class MapRenderElement : RocketElement
    {
        private World _world;

        private       float _frameAccumulator = 0f;
        private       float _targetTime        = 1f / 10f;
        
        public MapRenderElement(World world)
        {
            _world = world;

            Background = Color.White * 0.5f;
            ClipToBounds = true;

            Width = 128;
            Height = 128;
            Anchor = Alignment.TopRight;
        }
        
        public void SetSize(double multiplier)
        {
            Width = (int)Math.Ceiling(128 * multiplier);
            Height = (int)Math.Ceiling(128 * multiplier);
        }
        
        private float _rotation = 0f;

        public float Scale { get; set; }= 1f;
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
            
            _rotation = (_world.Player.KnownPosition.HeadYaw).ToRadians();
            Rotation = _rotation;
        }
        
        //public Vector3 Center { get; private set; } = Vector3.Zero;
        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            base.OnDraw(graphics, gameTime);
            
            if (!IsVisible)
                return;

            var playerPos = _world.Camera.Position;
            var center = new ChunkCoordinates(playerPos);
            var renderPos = RenderBounds.Location.ToVector2();
            
            
            var playerOffset =new Vector3(center.X << 4, 0, center.Z << 4) -  playerPos;

            var cameraOffset = new Vector2(playerOffset.X, playerOffset.Z);
            
            renderPos += cameraOffset;
            
            var tCenter = RenderBounds.Size.ToVector2() / 2f;
            //var renderPos = RenderBounds.Location.ToVector2();

            var radius = _world.ChunkManager.RenderDistance;
           // var scale = RenderBounds.Width / (radius * 16f);

          //  var scaledInstance = new Vector2(16f, 16f) * scale;
            foreach (var container in _world.Map.GetContainers(center,  radius))
            {
                var texture = container.Texture;

                if (texture != null)
                {
                    var cc = container.Coordinates - center;
                    var texturePosition = tCenter + new Vector2(cc.X * 16f, cc.Z * 16f);

                    graphics.SpriteBatch.Draw((TextureSlice2D) texture, renderPos + texturePosition);
                
                }
            }

            renderPos = RenderBounds.Location.ToVector2() + tCenter;
            var rotationOrigin = new Vector2(4, 4);
            foreach (var icon in _world.Map.GetMarkers(center, radius))
            {
                var relativePosition = icon.Position - playerPos;
                var position = new Vector2(relativePosition.X, relativePosition.Z);
                //mapIcon.Center = center;
                //   var position = mapIcon.WorldPosition - playerPos;
                var value = icon.Marker.ToTexture();

                if (value.HasValue)
                {
                  //  if (value.Texture != null)
                    {
                        graphics.SpriteBatch.Draw(
                            value,  (renderPos + position), value.Color.GetValueOrDefault(Color.White), icon.Rotation.ToRadians(),
                            rotationOrigin, Vector2.One);
                    }
                }
            }
            // _world.Map.Render(graphics, new Rectangle(renderPos.ToPoint(), RenderBounds.Size), center, _world.ChunkManager.RenderDistance);

            graphics.DrawRectangle(RenderBounds, Color.Black, 1);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            { 
                //_icons.CollectionChanged -= IconsOnCollectionChanged;
               // _icons.Clear();
               // _icons = null;
                
                _world = null;
            }
        }
    }
}
