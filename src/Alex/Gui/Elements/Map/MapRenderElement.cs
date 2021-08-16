using System;
using Alex.Common.Utils.Vectors;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NLog;
using RocketUI;
using RocketUI.Input;

namespace Alex.Gui.Elements.Map
{
    public class MapRenderElement : RocketControl
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MapRenderElement));
        private IMap _map;

        private static readonly byte MaxZoomLevel = (byte) ZoomLevel.Maximum;
        private static readonly byte MinZoomLevel = (byte) ZoomLevel.Minimum;

        public EventHandler<MapClickedEventArgs> OnClick;

        private ZoomLevel _zoomLevel = ZoomLevel.Default;
        public ZoomLevel ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                _zoomLevel = (ZoomLevel)Math.Clamp((byte)value, MinZoomLevel, MaxZoomLevel);
            }
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

        private int _radius = 1;
        public int Radius
        {
            get => _radius;
            set
            {
                _radius = value;
            }
        }
        
        public bool FixedRotation { get; set; } = true;

        private TextElement _north, _east, _south, _west;
        public MapRenderElement(IMap map)
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

        private static readonly Vector2 MarkerRotationOrigin = new Vector2(4, 4);
        private Point _previousSize = new Point(128, 128);
        /// <inheritdoc />
        protected override void OnAfterMeasure()
        {
            base.OnAfterMeasure();
            
            var size = RenderBounds;

            if (_previousSize != size.Size)
            {
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
        
        private Vector3 _mapOffset = Vector3.Zero;
        public Vector3 Center => _map.Center + (_mapOffset);
        public float ZoomScale => ((float) _zoomLevel) / (((float) ZoomLevel.Maximum) / 2f);
        public float CursorScale =>  ((float)ZoomLevel.Maximum / ((float)_zoomLevel * 2f));
        public Vector3 CursorWorldPosition => GetWorldPosition(_cursorPosition.ToVector2());
        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            base.OnDraw(graphics, gameTime);
            
            if (!IsVisible)
                return;
            
            if (!FixedRotation)
                Rotation = _map.Rotation;

            var centerPosition = Center;
            var zoomScale = ZoomScale;
            
            DrawMap(graphics, centerPosition, _radius, zoomScale);
            DrawMarkers(graphics, centerPosition, _radius, zoomScale);

            graphics.DrawRectangle(RenderBounds, Color.Black, 1);
        }
        
        private void DrawMarkers(GuiSpriteBatch graphics, Vector3 centerPosition, int radius, float zoomScale)
        {
            var center = new ChunkCoordinates(centerPosition);

            foreach (var icon in _map.GetMarkers(center, radius))
            {
                var position = GetRenderPosition(icon.Position, centerPosition, zoomScale);

                if (!RenderBounds.Contains(position))
                {
                    if (icon.AlwaysShown)
                    {
                        position = Vector2.Clamp(
                            position, RenderBounds.Location.ToVector2() + MarkerRotationOrigin,
                            (RenderBounds.Location + RenderBounds.Size).ToVector2() - MarkerRotationOrigin);
                    }
                    else
                    {
                        continue;
                    }
                }
                
                var value = icon.Marker.ToTexture();

                if (value.HasValue)
                {
                    graphics.SpriteBatch.Draw(
                        value, position, value.Color.GetValueOrDefault(icon.Color),
                       ((icon.Rotation).ToRadians()) - Rotation, MarkerRotationOrigin, Vector2.One * zoomScale );
                }
            }
        }
        
        private void DrawMap(GuiSpriteBatch graphics, Vector3 centerPosition, int radius, float zoomScale)
        {
            var center = new ChunkCoordinates(centerPosition);

            var texture = _map.GetTexture(graphics.Context.GraphicsDevice, centerPosition);

            if (texture == null)
                return;
            
            var position = GetRenderPosition(new Vector3(center.X * 16f, 0f, center.Z * 16f), centerPosition, zoomScale);
            
            graphics.SpriteBatch.Draw(
                (TextureSlice2D)texture, position,
                Color.White, -Rotation, new Vector2(texture.Width / 2f, texture.Height / 2f),
                (Vector2.One) * zoomScale);
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

        private bool _dragging = false;

        /// <inheritdoc />
        protected override void OnFocusDeactivate()
        {
            base.OnFocusDeactivate();
            _dragging = false;
        }

        /// <inheritdoc />
        protected override void OnCursorDown(Point cursorPosition)
        {
            base.OnCursorDown(cursorPosition);
            _dragging = true;
        }

        /// <inheritdoc />
        protected override void OnCursorUp(Point cursorPosition)
        {
            base.OnCursorUp(cursorPosition);
            _dragging = false;
        }

        private Point _cursorPosition = Point.Zero;
        /// <inheritdoc />
        protected override void OnCursorMove(Point cursorPosition, Point previousCursorPosition, bool isCursorDown)
        {
            _cursorPosition = cursorPosition;
            base.OnCursorMove(cursorPosition, previousCursorPosition, isCursorDown);

            if (!_dragging)
                return;

            var amount = (previousCursorPosition - cursorPosition).ToVector2() * (CursorScale);
            _mapOffset += new Vector3(amount.X, 0f, amount.Y);
            //_map.Move(amount);
        }

        private DateTime _lastLeftClick = DateTime.UtcNow;
        private DateTime _lastRightClick = DateTime.UtcNow;
        /// <inheritdoc />
        protected override void OnCursorPressed(Point cursorPosition, MouseButton button)
        {
            base.OnCursorPressed(cursorPosition, button);

            var now = DateTime.UtcNow;
            
            if (button == MouseButton.Left)
            {
                //Double click
                if ((now - _lastLeftClick).TotalMilliseconds <= 750)
                {
                    OnClick?.Invoke(
                        this,
                        new MapClickedEventArgs(
                            ClickEventType.DoubleClick, cursorPosition.ToVector2(),
                            GetWorldPosition(cursorPosition.ToVector2()), MouseButton.Left));
                }
                else //Single Click
                {
                    OnClick?.Invoke(
                        this,
                        new MapClickedEventArgs(
                            ClickEventType.SingleClick, cursorPosition.ToVector2(),
                            GetWorldPosition(cursorPosition.ToVector2()), MouseButton.Left));
                }

                _lastLeftClick = now;
            }
            else if (button == MouseButton.Right)
            {
                //Double click
                if ((now - _lastRightClick).TotalMilliseconds <= 750)
                {
                    OnClick?.Invoke(
                        this,
                        new MapClickedEventArgs(
                            ClickEventType.DoubleClick, cursorPosition.ToVector2(),
                            GetWorldPosition(cursorPosition.ToVector2()), MouseButton.Right));
                }
                else //Single Click
                {
                    OnClick?.Invoke(
                        this,
                        new MapClickedEventArgs(
                            ClickEventType.SingleClick, cursorPosition.ToVector2(),
                            GetWorldPosition(cursorPosition.ToVector2()), MouseButton.Right));
                }

                _lastRightClick = now;
            }
        }

        private Vector3 GetWorldPosition(Vector2 cursorPosition)
        {
            var tCenter = RenderBounds.Size.ToVector2() / 2f;

            var distanceFromCenter = cursorPosition - tCenter;
            distanceFromCenter *= (CursorScale);
            
            return Center + new Vector3(distanceFromCenter.X, 0f, distanceFromCenter.Y);
        }
        
        public void Reset()
        {
            _mapOffset = Vector3.Zero;
            ZoomLevel = ZoomLevel.Default;
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

    public class MapClickedEventArgs : EventArgs
    {
        public ClickEventType EventType { get; }
        public Vector2 CursorPosition { get; }
        public Vector3 WorldPosition { get; }
        public MouseButton MouseButton { get; }

        public MapClickedEventArgs(ClickEventType eventType, Vector2 cursorPosition, Vector3 worldPosition, MouseButton mouseButton)
        {
            EventType = eventType;
            CursorPosition = cursorPosition;
            WorldPosition = worldPosition;
            MouseButton = mouseButton;
        }
    }

    public enum ClickEventType
    {
        SingleClick,
        DoubleClick
    }
}
