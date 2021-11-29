using System;
using System.Collections.Generic;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI;
using RocketUI.Input;

namespace Alex.Gui.Elements
{
    public class KeybindElement : RocketControl, IValuedControl<Keys[]>
    {
        public static readonly Keys[] Unbound = new Keys[] { (Keys)int.MaxValue };
        
        private TextElement TextElement { get; }
        private TextElement TimerText { get; }
        
        public Color BorderColor { get; set; } = Color.LightGray;
        public Thickness BorderThickness { get; set; } = Thickness.One;
        
        public InputCommand InputCommand { get; }
        public KeybindElement(InputCommand inputCommand, params Keys[] key)
        {
            InputCommand = inputCommand;
            _value = key;
            
            BackgroundOverlay = Color.Black;
            
            TextElement = new TextElement();
            TextElement.Anchor = Alignment.MiddleCenter;
            
            TimerText = new TextElement();
            TimerText.Anchor = Alignment.MiddleRight;
            TimerText.IsVisible = false;
            
            AddChild(TextElement);
            AddChild(TimerText);
            
            UpdateText(key);
        }

        private float _cursorAlpha = 1f;
        private List<Keys> _tempBinding = new List<Keys>();
        private TimeSpan _timer = TimeSpan.Zero;

        private void FocusLost()
        {
            TimerText.IsVisible = false;
            TextElement.TextOpacity = 1f;
            var newValue = _tempBinding.ToArray();
            _tempBinding.Clear();
            _timer = TimeSpan.Zero;
            
            if (newValue.Length > 0)
            {
                Value = newValue;
            }
            else if (Value != Unbound)
            {
                Value = Unbound;
            }
            
            UpdateText(_value);
        }
        
        private bool _wasFocused = false;
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            bool focused = Focused;

            if (focused && !_wasFocused)
            {
                _tempBinding.Clear();
                _timer = TimeSpan.FromSeconds(5);
                TextElement.Text = "_";
                TimerText.IsVisible = true;
            }
            else if (!focused && _wasFocused)
            {
                FocusLost();
            }

            if (_timer > TimeSpan.Zero)
            {
                _timer -= gameTime.ElapsedGameTime;
                TimerText.Text = $"{_timer:%s}";
                
                TextElement.TextOpacity = 1f - ((float)gameTime.TotalGameTime.TotalSeconds % 0.75f);
                
                if (_timer <= TimeSpan.Zero)
                {
                    FocusLost();
                }
            }

            _wasFocused = focused;
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
            /*if (Focused && key == Keys.Escape)
            {
                if (_tempBinding.Count == 0)
                {
                    _timer = TimeSpan.Zero;
                    UpdateText(_value);
                    return true;
                }   
            }*/
            if (_timer <= TimeSpan.Zero)
            {
                return true;
            }
            
            _timer = TimeSpan.FromSeconds(5);
            _tempBinding.Add(key);
            UpdateText(_tempBinding);
           // if (Focused)
           // {
                //Value = key;
                
                //ClearFocus();
               // return true;
           // }

            return true;
        }

        private void UpdateText(ICollection<Keys> key)
        {
            if (key == Unbound)
            {
                TextElement.Text = $"{TextColor.Red}Unbound";
            }
            else
            {
                TextElement.Text = string.Join(" + ", key);// key.ToString().SplitPascalCase();
            }
        }
        
        public event EventHandler<Keys[]> ValueChanged;
        private Keys[] _value;

        public Keys[] Value
        {
            get { return _value; }
            set
            {
                var oldValue = _value;
                _value = value;
                
                UpdateText(value);

                if (_value != oldValue)
                {
                    ValueChanged?.Invoke(this, _value);
                }
            }
        }

        public ValueFormatter<Keys[]> DisplayFormat { get; set; }
    }
}