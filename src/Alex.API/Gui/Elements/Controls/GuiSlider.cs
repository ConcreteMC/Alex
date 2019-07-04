using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiSlider : GuiControl, IValuedControl<double>
    {
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

        public int ThumbWidth
        {
            get => _thumbWidth;
            set { _thumbWidth = value; }
        }

        public GuiTextElement Label { get; private set; }
        public string DisplayFormat { get; set; } = "{0:F2}";

        private double _thumbOffsetX;
        private int _thumbWidth = 10;

        public GuiSlider()
        {
            Background = GuiTextures.ButtonDisabled;
            ThumbBackground = GuiTextures.ButtonDefault;
            ThumbHighlightBackground = GuiTextures.ButtonHover;
            
            Background.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;
            ThumbBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;
            ThumbHighlightBackground.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;


            MinWidth = 20;
            MinHeight = 20;

            MaxHeight = 22;
            MaxWidth  = 200;
            Padding = new Thickness(5, 5);
            Margin = new Thickness(2);

            
           // Background.RepeatMode = TextureRepeatMode.NoScaleCenterSlice;

            AddChild(Label = new GuiAutoUpdatingTextElement(() => string.Format(DisplayFormat, Value))
            {
                Margin      =  Thickness.Zero,
                Anchor      = Alignment.MiddleCenter,
                TextColor   = TextColor.White,
                FontStyle   = FontStyle.DropShadow,
                Enabled = false
            });
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
            ThumbBackground.TryResolveTexture(renderer);
            ThumbHighlightBackground.TryResolveTexture(renderer);
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

            Label.TextColor = TextColor.Yellow;
        }

        protected override void OnHighlightDeactivate()
        {
            base.OnHighlightDeactivate();

            Label.TextColor = TextColor.White;
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
