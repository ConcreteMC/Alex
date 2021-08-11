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

        private TextElement _north, _east, _south, _west;
        public MapRenderElement(WorldMap map)
        {
            _map = map;

            Background = Color.White * 0.5f;
            ClipToBounds = true;

            Width = 128;
            Height = 128;
            Anchor = Alignment.TopRight;
            Padding = Thickness.One;
            
            AddChild(_north = new TextElement("North")
            {
                Anchor = Alignment.TopCenter,
                FontStyle = FontStyle.DropShadow,
                IsVisible = false
            });
            
            AddChild(_east = new TextElement("East")
            {
                Anchor = Alignment.MiddleRight,
                FontStyle = FontStyle.DropShadow,
                IsVisible = false
            });
            
            AddChild(_south = new TextElement("South")
            {
                Anchor = Alignment.BottomCenter,
                FontStyle = FontStyle.DropShadow,
                IsVisible = false
            });
            
            AddChild(_west = new TextElement("West")
            {
                Anchor = Alignment.MiddleLeft,
                FontStyle = FontStyle.DropShadow,
                IsVisible = false
            });
        }

        private bool _showCompass = false;
        public bool ShowCompass
        {
            get
            {
                return _showCompass;
            }
            set
            {
                _showCompass = value;
                _north.IsVisible = value;
                _east.IsVisible = value;
                _south.IsVisible = value;
                _west.IsVisible = value;
            }
        }

        private Point _previousSize = new Point(128, 128);
        /// <inheritdoc />
        protected override void OnAfterMeasure()
        {
            base.OnAfterMeasure();
            
            var size = RenderBounds;

            if (_previousSize != size.Size)
            {
                var z = ZoomLevel.Maximum;

                var radiusBy2 = _radius * 2;
                var chunksX = size.Width / 16;
                var chunksZ = size.Height / 16;

                for (int zl = (int)ZoomLevel.Minimum; zl < (int)ZoomLevel.Maximum; zl++)
                {
                    var zoomScale = ((float)ZoomLevel.Maximum / (float)zl);

                    if (16 * zoomScale <= chunksX && 16 * zoomScale <= chunksZ)
                    {
                        ZoomLevel = (ZoomLevel)zl;

                        break;
                    }
                }

                _previousSize = size.Size;
            }
        }

        public void SetSize(double multiplier)
        {
            Width = (int)Math.Ceiling(128 * multiplier);
            Height = (int)Math.Ceiling(128 * multiplier);
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

            Rotation = _map.MapRotation;
            var centerPosition = _map.CenterPosition;
            var zoomScale = ((float) ZoomLevel.Maximum / (float) ZoomLevel);
            
            DrawMap(graphics, centerPosition, _radius, zoomScale, out var minY, out var maxY);
            DrawMarkers(graphics, centerPosition, _radius, zoomScale, minY, maxY);

            graphics.DrawRectangle(RenderBounds, Color.Black, 1);
        }

        private static readonly Vector2 MarkerRotationOrigin = new Vector2(4, 4);
        private void DrawMarkers(GuiSpriteBatch graphics, Vector3 centerPosition, int radius, float zoomScale, int minY, int maxY)
        {
            var center = new ChunkCoordinates(centerPosition);

            foreach (var icon in _map.GetMarkers(center, radius))
            {
                var position = GetRenderPosition(icon.Position, centerPosition, zoomScale);
                var value = icon.Marker.ToTexture();

                if (value.HasValue)
                {
                    var yDistance =centerPosition.Y - icon.Position.Y;
                    //For every block away from me, scale their map icon by 0.05
                    yDistance *= 0.05f;
                    yDistance = 1f - yDistance;

                    yDistance = Math.Clamp(yDistance, 0.05f, 1f);
                    
                    graphics.SpriteBatch.Draw(
                        value, position, value.Color.GetValueOrDefault(icon.Color),
                       ((icon.Rotation).ToRadians()) - Rotation, MarkerRotationOrigin, Vector2.One * zoomScale * (yDistance));
                }
            }
        }

        private static readonly Vector2 MapRotationOrigin = new Vector2(8, 8);
        private void DrawMap(GuiSpriteBatch graphics, Vector3 centerPosition, int radius, float zoomScale, out int minY, out int maxY)
        {
            minY = (int)centerPosition.Y;
            maxY = minY;
            
            var center = new ChunkCoordinates(centerPosition);

            foreach (var container in _map.GetContainers(center,  radius))
            {
                var texture = container.Texture;

                if (texture != null)
                {
                    var mapPosition = new Vector3(container.Coordinates.X * 16f, 0f, container.Coordinates.Z * 16f);
                    var position = GetRenderPosition(mapPosition, centerPosition, zoomScale);
                    graphics.SpriteBatch.Draw((TextureSlice2D) texture, position, Color.White, -Rotation, Vector2.Zero, (Vector2.One) * zoomScale);

                    minY = Math.Min(container.MinHeight, minY);
                    maxY = Math.Max(container.MaxHeight, maxY);
                }
            }
        }

        private Vector2 GetRenderPosition(Vector3 position, Vector3 centerPosition, float scale)
        {
            position = Vector3.Transform(position, Matrix.CreateTranslation(-centerPosition) * Matrix.CreateRotationY(Rotation) * Matrix.CreateTranslation(centerPosition));
            var distance = position - centerPosition;

            var tCenter = RenderBounds.Size.ToVector2() / 2f;
            var renderPos = RenderBounds.Location.ToVector2() + tCenter;

            renderPos += new Vector2(distance.X, distance.Z) * scale;

            return renderPos;;
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
