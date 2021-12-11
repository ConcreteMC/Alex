using System;
using System.Collections.Generic;
using Alex.Common.Data.Options;
using Alex.Common.GameStates;
using Alex.Common.Graphics;
using Alex.Common.Gui.Elements;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gamestates.Common;
using Alex.Gui;
using Alex.Gui.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using NLog;
using RocketUI;

namespace Alex.Gamestates.MainMenu.Options
{
    public class OptionsStateBase : GuiMenuStateBase
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(OptionsStateBase));
        
        protected AlexOptions Options => OptionsProvider.AlexOptions;

        private IOptionsProvider OptionsProvider => GetService<IOptionsProvider>();

        private GuiPanoramaSkyBox _skyBox;
        protected GuiBackButton BackButton { get; }
        private TextElement                  Description       { get; set; }
        protected Dictionary<IGuiControl, string> Descriptions { get; } = new Dictionary<IGuiControl, string>();
        public OptionsStateBase(GuiPanoramaSkyBox skyBox)
        {
            _skyBox = skyBox;
            //_optionsProvider = GetService<IOptionsProvider>();

            var footerRow = Footer.AddRow(BackButton = new GuiBackButton()
            {
                TranslationKey = "gui.done",
                Anchor = Alignment.TopFill,
            }.ApplyModernStyle(false));

            Footer.ChildAnchor = Alignment.MiddleCenter;

            Body.ChildAnchor = Alignment.MiddleCenter;
            
            if (_skyBox != null)
                Background = new GuiTexture2D(_skyBox, TextureRepeatMode.Stretch);
            
            Description = new TextElement()
            {
                Anchor = Alignment.MiddleLeft, Margin = new Thickness(5, 15, 5, 5), MinHeight = 80
            };
        }

        protected override void OnShow()
        {
            if (Alex.InGame)
            {
                Background = null;
                BackgroundOverlay = new Color(Color.Black, 0.65f);
            }
            else
            {
                Background = new GuiTexture2D(_skyBox, TextureRepeatMode.Stretch);
                //BackgroundOverlay = null;
            }
            
            OptionsProvider.Load();
            base.OnShow();
        }
        
        protected override void OnHide()
        {
            OptionsProvider.Save();

            base.OnHide();
        }

        private bool _initialized = false;
        /// <inheritdoc />
        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
            Initialize(renderer);
            
            if (!_descriptionsAdded)
            {
                var row = AddGuiRow(Description);
                row.ChildAnchor = Alignment.MiddleLeft;

                _descriptionsAdded = true;
            }
            
            _initialized = true;
        }

        protected virtual void Initialize(IGuiRenderer renderer)
        {
            
        }

        protected Button CreateLinkButton<TGameState>(string translationKey, string fallback = null) where TGameState : class, IGameState
        {
           // var state = Construct<TGameState>();
            
           // if (state == null)
           //     throw new Exception($"Can not create linkbutton with type {typeof(TGameState)}");
            
            return new AlexButton(() =>
            {
              //  state.ParentState = ParentState;
                Alex.GameStateManager.SetActiveState<TGameState>(true, false);
              //  Alex.GameStateManager.SetActiveState(state, true);
            })
            {
                Text = fallback ?? translationKey,
                TranslationKey = translationKey,
            }.ApplyModernStyle(false);
        }

        protected Slider CreateSlider(Func<double, string> formatter, Func<AlexOptions, OptionsProperty<int>> optionsAccessor,
            int? minValue = null, int? maxValue = null, int? stepInterval = null)
            => CreateSlider(new ValueFormatter<double>(formatter), optionsAccessor, minValue, maxValue, stepInterval);

        protected Slider CreateSlider(string label, Func<AlexOptions, OptionsProperty<int>> optionsAccessor,
            int? minValue = null, int? maxValue = null, int? stepInterval = null)
            => CreateSlider(new ValueFormatter<double>(label), optionsAccessor, minValue, maxValue, stepInterval);
        
        protected Slider CreateSlider(ValueFormatter<double> label, Func<AlexOptions, OptionsProperty<int>> optionsAccessor, int? minValue = null, int? maxValue = null, int? stepInterval = null)
        {
            var slider = CreateValuedControl<Slider, double, int>(label, optionsAccessor);
            slider.ApplyStyle();
            if (minValue.HasValue)
            {
                slider.MinValue = minValue.Value;
            }

            if (maxValue.HasValue)
            {
                slider.MaxValue = maxValue.Value;
            }

            if (stepInterval.HasValue)
            {
                slider.StepInterval = stepInterval.Value;
            }

            return slider;
        }

        protected Slider CreateSlider(Func<double, string> formatter, Func<AlexOptions, OptionsProperty<double>> optionsAccessor,
            double? minValue = null, double? maxValue = null, double? stepInterval = null)
            => CreateSlider(new ValueFormatter<double>(formatter), optionsAccessor, minValue, maxValue, stepInterval);


        protected Slider CreateSlider(string label, Func<AlexOptions, OptionsProperty<double>> optionsAccessor,
            double? minValue = null, double? maxValue = null, double? stepInterval = null)
            => CreateSlider(new ValueFormatter<double>(label), optionsAccessor, minValue, maxValue, stepInterval);
        
        protected Slider CreateSlider(ValueFormatter<double> valueFormatter, Func<AlexOptions, OptionsProperty<double>> optionsAccessor, double? minValue = null, double? maxValue = null, double? stepInterval = null)
        {
            var slider = CreateValuedControl<Slider, double>(valueFormatter, optionsAccessor);
            slider.ApplyStyle();
            
            if (minValue.HasValue)
            {
                slider.MinValue = minValue.Value;
            }

            if (maxValue.HasValue)
            {
                slider.MaxValue = maxValue.Value;
            }

            if (stepInterval.HasValue)
            {
                slider.StepInterval = stepInterval.Value;
            }

            slider.Value = optionsAccessor(Options).Value;

            return slider;
        }
        
        protected Slider<TEnum> CreateSlider<TEnum>(Func<TEnum, string> formatter, Func<AlexOptions, OptionsProperty<TEnum>> optionsAccessor,
            TEnum minValue, TEnum maxValue) where TEnum : Enum
            => CreateSlider<TEnum>(new ValueFormatter<TEnum>(formatter), optionsAccessor, minValue, maxValue);


        protected Slider<TEnum> CreateSlider<TEnum>(string label,
            Func<AlexOptions, OptionsProperty<TEnum>> optionsAccessor,
            TEnum minValue,
            TEnum maxValue) where TEnum : Enum => CreateSlider<TEnum>(
            new ValueFormatter<TEnum>(label), optionsAccessor, minValue, maxValue);
        
        protected Slider<TEnum> CreateSlider<TEnum>(ValueFormatter<TEnum> valueFormatter, Func<AlexOptions, OptionsProperty<TEnum>> optionsAccessor, TEnum minValue, TEnum maxValue) where TEnum : Enum
        {
            var slider = CreateValuedControl<Slider<TEnum>, TEnum>(valueFormatter, optionsAccessor);
            slider.ApplyStyle<Slider<TEnum>, TEnum>();
            
            slider.MinValue = minValue;
            slider.MaxValue = maxValue;

           // slider.StepInterval = ;

            slider.Value = optionsAccessor(Options).Value;

            return slider;
        }
        

        protected EnumSwitchButton<TEnum> CreateSwitch<TEnum>(string displayFormat, Func<AlexOptions, OptionsProperty<TEnum>> optionsAccessor) where TEnum : Enum
        {
            var @switch = CreateValuedControl<EnumSwitchButton<TEnum>, TEnum>(displayFormat, optionsAccessor);// {Modern = false, DisplayFormat = displayFormat};
            @switch.ApplyModernStyle(false);
            return @switch;
        }

        protected ToggleButton CreateToggle(string displayFormat,
            Func<AlexOptions, OptionsProperty<bool>> optionsAccessor)
        {
            var sw = CreateValuedControl<ToggleButton, bool>(displayFormat, optionsAccessor);
            sw.ApplyModernStyle(false);
            
            return sw;
        }
        
        protected TControl CreateValuedControl<TControl, TValue>(ValueFormatter<TValue> label, Func<AlexOptions, OptionsProperty<TValue>> propertyAccessor)
            where TControl : IGuiControl, IValuedControl<TValue>, new()
            where TValue : IConvertible
        {
            var property = propertyAccessor(Options);

            var control = new TControl();

            control.DisplayFormat =  label;
            control.Value         =  property.Value;
            control.ValueChanged  += (s, v) => property.Value = v;

            return control;
        }
        
        protected TControl CreateValuedControl<TControl, TValue, TPropertyValue>(ValueFormatter<TValue> label, Func<AlexOptions, OptionsProperty<TPropertyValue>> propertyAccessor) 
            where TControl : IGuiControl, IValuedControl<TValue>, new() 
            where TValue : IConvertible 
            where TPropertyValue : IConvertible 
        {
            var property = propertyAccessor(Options);

            var control = new TControl();

            control.DisplayFormat = label;
            control.Value         = (TValue) Convert.ChangeType(property.Value, typeof(TValue));
            control.ValueChanged += (s, v) => property.Value = (TPropertyValue) Convert.ChangeType(v, typeof(TPropertyValue));

            return control;
        }

        private IGuiControl _focusedControl = null;
        private static string DefaultDescription = $"Hover over any setting to get a description.\n\n";

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
            
            if (!Alex.InGame && _skyBox != null)
            {
               // _skyBox?.Update(gameTime);
            }

            var highlighted = Alex.GuiManager.FocusManager.HighlightedElement;
            if (_focusedControl != highlighted)
            {
                _focusedControl = highlighted;

                if (highlighted != null)
                {
                    if (Descriptions.TryGetValue(highlighted, out var description))
                    {
                        Description.Text = description;
                    }
                    else
                    {
                        Description.Text = DefaultDescription;
                    }
                }
                else
                {
                    Description.Text = DefaultDescription;
                }
            }
        }

        private bool _descriptionsAdded = false;
        protected override void OnDraw(IRenderArgs args)
        {
            if (!Alex.InGame && _skyBox != null)
            {
                _skyBox.Draw(args);
            }

            base.OnDraw(args);
        }
        
        protected void AddDescription(IGuiControl control, string title, string line1, string line2 = "")
        {
            Descriptions.Add(control,  $"{TextColor.Bold}{title}:{TextColor.Reset}\n{line1}\n{line2}");
        }
    }
}
