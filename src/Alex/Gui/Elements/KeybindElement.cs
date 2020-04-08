using System;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI;

namespace Alex.Gui.Elements
{
    public class KeybindElement : GuiControl, IValuedControl<Keys>
    {
        public static readonly Keys Unbound = (Keys) int.MaxValue;
        
        private GuiTextElement TextElement { get; }
        
        public Color BorderColor { get; set; } = Color.LightGray;
        public Thickness BorderThickness { get; set; } = Thickness.One;
        
        public KeybindElement(Keys key)
        {
            _value = key;
            
            BackgroundOverlay = Color.Black;
            
            TextElement = new GuiTextElement();
            TextElement.Anchor = Alignment.MiddleCenter;
            
            AddChild(TextElement);
            
            UpdateText(key);
        }

        private float _cursorAlpha = 1;
        private bool _prevFocused = false;
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (Focused)
            {
                TextElement.Text = "_";
            }
            else if (!Focused && _prevFocused)
            {
                UpdateText(_value);
            }

            _prevFocused = Focused;
        }

        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            base.OnDraw(graphics, gameTime);
            
            var bounds = RenderBounds;
            bounds.Inflate(1f, 1f);
            graphics.DrawRectangle(bounds, BorderColor, BorderThickness);
        }

        protected override bool OnKeyInput(char character, Keys key)
        {
            if (Focused)
            {
                Value = key;
                
                ClearFocus();
                return true;
            }

            return false;
        }

        private void UpdateText(Keys key)
        {
            if (key == Unbound)
            {
                TextElement.Text = $"{TextColor.Red}Unbound";
            }
            else
            {
                TextElement.Text = key.ToString().SplitPascalCase();
            }
        }
        
        public event EventHandler<Keys> ValueChanged;
        private Keys _value;

        public Keys Value
        {
            get { return _value; }
            set
            {
                var oldValue = _value;
                _value = value;
                
                //if (_value != oldValue)
                {
                    UpdateText(value);
                    
                    ValueChanged?.Invoke(this, _value);
                }
            }
        }

        public ValueFormatter<Keys> DisplayFormat { get; set; }
    }
}