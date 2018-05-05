using System;
using Microsoft.Xna.Framework;
using RocketUI.Graphics;
using RocketUI.Graphics.Textures;
using RocketUI.Utilities;

namespace RocketUI.Elements.Controls
{
    public class Slider : Control, IValuedControl<double>
    {
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
                    proposedValue = MathUtils.RoundToNearestInterval(proposedValue, StepInterval);
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
        public int ThumbWidth { get; set; } = 10;

        public TextBlock Label { get; private set; }
        public string DisplayFormat { get; set; } = "{0:F2}";

        private double _thumbOffsetX;

        public Slider()
        {
            //Background = GuiTextures.ButtonDisabled;
            //ThumbBackground = GuiTextures.ButtonDefault;
            //ThumbHighlightBackground = GuiTextures.ButtonHover;
            
            //Background.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;
            //ThumbBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;
            //ThumbHighlightBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;

            MinWidth = 20;
            MinHeight = 20;

            MaxHeight = 22;
            MaxWidth  = 200;
            Padding = new Thickness(5, 5);
            Margin = new Thickness(2);

            
            //Background.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;

            AddChild(Label = new GuiAutoUpdatingTextElement(() => string.Format(DisplayFormat, Value))
            {
                Margin      =  Thickness.Zero,
                Anchor      = Anchor.MiddleCenter,
                Foreground   = Color.White
            });
        }

        protected override void OnInit()
        {
            base.OnInit();
            ThumbBackground.TryResolveTexture(Resources);
            ThumbHighlightBackground.TryResolveTexture(Resources);
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            var val = MathHelper.Clamp((float)Value, (float)MinValue, (float)MaxValue);
            val = MathUtils.RoundToNearestInterval(val, (float)StepInterval);


            var diff = MathUtils.RoundToNearestInterval(Math.Abs(MinValue - MaxValue), StepInterval);
            
            _thumbOffsetX = ((RenderBounds.Width - ThumbWidth) / (double) diff) * val;
        }

        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            base.OnDraw(graphics, gameTime);

            var bounds = new Rectangle((int) (RenderPosition.X + _thumbOffsetX), (int) RenderPosition.Y, ThumbWidth,
                                       RenderSize.Height);
            graphics.FillRectangle(bounds, Highlighted ? ThumbHighlightBackground : ThumbBackground);
        }

        private void SetValueFromCursor(Point relativePosition)
        {
            var percentageClicked = relativePosition.X / (float)RenderBounds.Width;

            var diff = Math.Abs(MinValue - MaxValue);
            Value = MinValue + diff * percentageClicked;
        }

        protected override void OnHighlightActivate()
        {
            base.OnHighlightActivate();

            Label.Foreground = Color.Yellow;
        }

        protected override void OnHighlightDeactivate()
        {
            base.OnHighlightDeactivate();

            Label.Foreground = Color.White;
        }


        protected override void OnCursorPressed(Point cursorPosition)
        {
            SetValueFromCursor(cursorPosition);
        }

        protected override void OnCursorMove(Point relativeNewPosition, Point relativeOldPosition, bool isCursorDown)
        {
            if (isCursorDown)
            {
                SetValueFromCursor(relativeNewPosition);
            }
        }
    }
}
