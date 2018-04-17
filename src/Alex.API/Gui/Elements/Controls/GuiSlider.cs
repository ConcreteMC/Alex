using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiSlider : GuiControl, IValuedControl<double>
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
        
        public GuiTextures?   DefaultThumbBackgroundTexture { get; set; } = GuiTextures.ButtonDefault;
        public TextureSlice2D ThumbBackground               { get; set; }
        public GuiTextures?   DefaultThumbHighlightBackgroundTexture { get; set; } = GuiTextures.ButtonHover;
        public TextureSlice2D ThumbHighlightBackground               { get; set; }
        public int ThumbWidth { get; set; } = 10;

        public GuiTextElement Label { get; private set; }
        public string DisplayFormat { get; set; } = "{0:F2}";

        private double _thumbOffsetX;

        public GuiSlider()
        {
            MinWidth = 64;
            MinHeight = 22;
            HighlightOutlineThickness = Thickness.Zero;
            
            DefaultBackgroundTexture = GuiTextures.ButtonDisabled;
            BackgroundRepeatMode = TextureRepeatMode.NoScaleCenterSlice;

            AddChild(Label = new GuiAutoUpdatingTextElement(() => string.Format(DisplayFormat, Value))
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
            
            if (DefaultThumbBackgroundTexture.HasValue && ThumbBackground == null)
            {
                ThumbBackground = renderer.GetTexture(DefaultThumbBackgroundTexture.Value);
            }
            
            if (DefaultThumbHighlightBackgroundTexture.HasValue && ThumbHighlightBackground == null)
            {
                ThumbHighlightBackground = renderer.GetTexture(DefaultThumbHighlightBackgroundTexture.Value);
            }
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            var val = Math.Min(MaxValue, Math.Max(MinValue, Value));
            val = MathHelpers.RoundToNearestInterval(val, StepInterval);


            var diff = MathHelpers.RoundToNearestInterval(Math.Abs(MinValue - MaxValue), StepInterval);
            
            _thumbOffsetX = ((RenderSize.X - ThumbWidth) / (double) diff) * val;
        }

        protected override void OnDraw(GuiRenderArgs args)
        {
            base.OnDraw(args);

            args.Draw(IsHighlighted ? ThumbHighlightBackground : ThumbBackground, new Rectangle((int)(RenderPosition.X + _thumbOffsetX), (int)RenderPosition.Y, ThumbWidth, RenderSize.Y), TextureRepeatMode.NoScaleCenterSlice);
        }

        private void SetValueFromCursor(Vector2 relativePosition)
        {
            var percentageClicked = relativePosition.X / (float)RenderSize.X;

            var diff = Math.Abs(MinValue - MaxValue);
            Value = MinValue + diff * percentageClicked;
        }


        protected override void OnClick(Vector2 relativePosition)
        {
            SetValueFromCursor(relativePosition);
        }

        protected override void OnCursorMove(Vector2 relativeNewPosition, Vector2 relativeOldPosition, bool isCursorDown)
        {
            if (isCursorDown)
            {
                SetValueFromCursor(relativeNewPosition);
            }
        }
    }
}
