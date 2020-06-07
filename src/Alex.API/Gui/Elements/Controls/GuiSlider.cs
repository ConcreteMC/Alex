using System;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using NLog;
using RocketUI;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiSlider : GuiControl, IValuedControl<double>
    {
		public static readonly ValueFormatter<double> DefaultDisplayFormat = "{0:0.#}";
        
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(GuiSlider));
        public event EventHandler<double> ValueChanged; 

        public double MinValue { get; set; } = 0.0d;
        public double MaxValue { get; set; } = 100.0d;
        private double _value = 0.0d;

        public double Value
        {
            get => _value;
            set
            {
                var proposedValue = value;
                if (StepInterval != 0d)
                {
                    proposedValue = MathHelpers.RoundToNearestInterval(proposedValue, StepInterval);
                }

                if (proposedValue != _value)
                {
                    _value = proposedValue;
                    ValueChanged?.Invoke(this, _value);
                }
            }
        }

        public double StepInterval { get; set; } = 1.0d;

        public GuiTexture2D ThumbBackground;
        public GuiTexture2D ThumbHighlightBackground;
        public GuiTexture2D ThumbDisabledBackground;

        public int ThumbWidth
        {
            get => _thumbWidth;
            set { _thumbWidth = value; }
        }

        public GuiTextElement Label { get; private set; }
        public ValueFormatter<double> DisplayFormat { get; set; } = DefaultDisplayFormat;

        private double _thumbOffsetX;
        private int _thumbWidth = 10;

        [Obsolete("Use DisplayFormat property instead.", true)]
        public Func<double, string> ValueFormatter { get; set; } = null;
        
        private TextColor _foregroundColor = TextColor.White;
        private TextColor _originalForegroundColor = TextColor.White;
        
        public TextColor ForegroundColor
        {
            get
            {
                return _foregroundColor;
            }
            set
            {
                _originalForegroundColor = value;
                FixForegroundColor(value);
            }
        }

        void FixForegroundColor(TextColor color)
        {
            _foregroundColor = color;
            var c = color.ForegroundColor;

            if (!Enabled)
            {
                c = c.Darken(0.5f);
            }
            
            _foregroundColor = new TextColor(color.Code, color.Name);
            _foregroundColor.BackgroundColor = color.BackgroundColor;
            _foregroundColor.ForegroundColor = c;
        }

        public GuiSlider()
        {
            Background = GuiTextures.ButtonDisabled;
            ThumbBackground = GuiTextures.ButtonDefault;
            ThumbHighlightBackground = GuiTextures.ButtonHover;
            ThumbDisabledBackground = GuiTextures.ButtonDisabled;
            
            Background.RepeatMode = TextureRepeatMode.Stretch;
            ThumbBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;
            ThumbHighlightBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;
            ThumbDisabledBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;

            MinWidth = 20;
            MinHeight = 20;

            MaxHeight = 22;
           // MaxWidth  = 200;
            //Padding = new Thickness(5, 5);
            Margin = new Thickness(2);
            Height = 20;

            // Background.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;

            AddChild(Label = new GuiAutoUpdatingTextElement(() => DisplayFormat?.FormatValue(Value) ?? string.Empty)
            {
                Margin      =  Thickness.Zero,
                Anchor      = Alignment.MiddleCenter,
                TextColor   = _foregroundColor,
                FontStyle   = FontStyle.DropShadow,
                Enabled = false,
                CanFocus = false
            });
        }

        protected override void OnEnabledChanged()
        {
            base.OnEnabledChanged();

            FixForegroundColor(_originalForegroundColor);
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
            
            FixForegroundColor(_originalForegroundColor);
            
            ThumbBackground.TryResolveTexture(renderer);
            ThumbHighlightBackground.TryResolveTexture(renderer);
            ThumbDisabledBackground.TryResolveTexture(renderer);
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            var val = MathHelper.Clamp((float)Value, (float)MinValue, (float)MaxValue);
            val = MathHelpers.RoundToNearestInterval(val, (float)StepInterval);


            var diff = MathHelpers.RoundToNearestInterval(Math.Abs(MinValue - MaxValue), StepInterval);
            
            _thumbOffsetX = ((RenderBounds.Width - ThumbWidth) / (double) diff) * (val - MinValue);
        }

        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            base.OnDraw(graphics, gameTime);

            var bounds = new Rectangle((int) (RenderPosition.X + _thumbOffsetX), (int) RenderPosition.Y, ThumbWidth,
                                       RenderSize.Height);
            
            graphics.FillRectangle(bounds, Enabled ? (Highlighted ? ThumbHighlightBackground : ThumbBackground) : ThumbDisabledBackground);
        }

        private void SetValueFromCursor(Point relativePosition)
        {
            var halfThumb = _thumbWidth / 2f;

            float percentageClicked = 0f;
            if (relativePosition.X <= halfThumb)
                percentageClicked = 0f;
            else if (relativePosition.X >= (RenderBounds.Width - halfThumb))
                percentageClicked = 1f;
            else 
                percentageClicked = (relativePosition.X - halfThumb) / (float)(RenderBounds.Width - _thumbWidth);

            var diff = Math.Abs(MinValue - MaxValue);
            Value = MinValue + (diff * percentageClicked);
        }

        protected override void OnHighlightActivate()
        {
            base.OnHighlightActivate();

            Label.TextColor = TextColor.Yellow;
        }

        protected override void OnHighlightDeactivate()
        {
            base.OnHighlightDeactivate();

            Label.TextColor = TextColor.White;
        }
        
        protected override void OnCursorPressed(Point cursorPosition, MouseButton button)
        {
            SetValueFromCursor(cursorPosition);
        }

        protected override void OnCursorDown(Point cursorPosition)
        {
            if (Focused)
                _cursorDown = true;
            
            base.OnCursorDown(cursorPosition);
        }

        protected override void OnCursorUp(Point cursorPosition)
        {
            _cursorDown = false;
            base.OnCursorUp(cursorPosition);
        }

        private bool _cursorDown = false;
        protected override void OnCursorLeave(Point cursorPosition)
        {
            _cursorDown = false;
            base.OnCursorLeave(cursorPosition);
        }

        protected override void OnCursorEnter(Point cursorPosition)
        {
            base.OnCursorEnter(cursorPosition);
        }

        protected override void OnCursorMove(Point relativeNewPosition, Point relativeOldPosition, bool isCursorDown)
        {
            if (!isCursorDown && _cursorDown) _cursorDown = false;
            
            if (isCursorDown && _cursorDown)
            {
                SetValueFromCursor(relativeNewPosition);
            }
        }
    }
}
