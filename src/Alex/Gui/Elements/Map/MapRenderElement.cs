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
        private WorldMap _map;

        private       float _frameAccumulator = 0f;
        private       float _targetTime        = 1f / 10f;

        private static readonly byte MaxZoomLevel = (byte) ZoomLevel.Maximum;
        private static readonly byte MinZoomLevel = (byte) ZoomLevel.Minimum;
        
        private ZoomLevel _zoomLevel = ZoomLevel.Default;
        public ZoomLevel ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                _zoomLevel = (ZoomLevel)Math.Clamp((byte)value, MinZoomLevel, MaxZoomLevel);
            }
        }

        public MapRenderElement(WorldMap map)
        {
            _map = map;

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
            
           // _rotation = (_world.Player.KnownPosition.HeadYaw).ToRadians();
            //Rotation = _rotation;
        }

        private int _radius = 1;
        public int Radius
        {
            get => _radius;
            set
            {
                _radius = value;
            }
        }

        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            base.OnDraw(graphics, gameTime);
            
            if (!IsVisible)
                return;
            
            var centerPosition = _map.CenterPosition;
            var zoomScale = ((float) ZoomLevel.Maximum / (float) ZoomLevel);
            
            DrawMap(graphics, centerPosition, _radius, zoomScale);
            DrawMarkers(graphics, centerPosition, _radius, zoomScale);

            graphics.DrawRectangle(RenderBounds, Color.Black, 1);
        }

        private static readonly Vector2 MarkerRotationOrigin = new Vector2(4, 4);
        private void DrawMarkers(GuiSpriteBatch graphics, Vector3 centerPosition, int radius, float zoomScale)
        {
            var center = new ChunkCoordinates(centerPosition);

            foreach (var icon in _map.GetMarkers(center, radius))
            {
                var position = GetRenderPosition(icon.Position, centerPosition, zoomScale);
                var value = icon.Marker.ToTexture();

                if (value.HasValue)
                {
                    graphics.SpriteBatch.Draw(
                        value, position, value.Color.GetValueOrDefault(icon.Color),
                        icon.Rotation.ToRadians(), MarkerRotationOrigin, Vector2.One * zoomScale);
                }
            }
        }

        private void DrawMap(GuiSpriteBatch graphics, Vector3 centerPosition, int radius, float zoomScale)
        {
            var center = new ChunkCoordinates(centerPosition);

            foreach (var container in _map.GetContainers(center,  radius))
            {
                var texture = container.Texture;

                if (texture != null)
                {
                    var position = GetRenderPosition(new Vector3(container.Coordinates.X * 16f, 0f, container.Coordinates.Z * 16f), centerPosition, zoomScale);
                    graphics.SpriteBatch.Draw((TextureSlice2D) texture, position, Color.White, 0f, Vector2.Zero, Vector2.One * zoomScale);
                }
            }
        }

        private Vector2 GetRenderPosition(Vector3 position, Vector3 centerPosition, float scale)
        {
            var distance = position - centerPosition;

            var tCenter = RenderBounds.Size.ToVector2() / 2f;
            var renderPos = RenderBounds.Location.ToVector2() + tCenter;

            renderPos += new Vector2(distance.X, distance.Z) * scale;
            
            return renderPos;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _map = null;
            }
        }
    }
}
