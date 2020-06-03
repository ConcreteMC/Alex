using System;
using Alex.API.Data.Options;
using Alex.API.GameStates;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Services;
using Alex.Gamestates.Common;
using Alex.Gui;
using Alex.Gui.Elements;
using Microsoft.Xna.Framework;
using NLog;
using RocketUI;

namespace Alex.Gamestates.MainMenu.Options
{
    public class OptionsStateBase : GuiMenuStateBase
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(OptionsStateBase));
        
        protected AlexOptions Options => _optionsProvider.AlexOptions;

        private readonly IOptionsProvider _optionsProvider;

        private GuiPanoramaSkyBox _skyBox;
        protected GuiBackButton BackButton { get; }
        public OptionsStateBase(GuiPanoramaSkyBox skyBox)
        {
            _skyBox = skyBox;
            _optionsProvider = GetService<IOptionsProvider>();

            var footerRow = Footer.AddRow(BackButton = new GuiBackButton()
            {
                TranslationKey = "gui.done",
                Anchor = Alignment.TopFill,
				Modern = false
            });

            Footer.ChildAnchor = Alignment.MiddleCenter;

            Body.ChildAnchor = Alignment.MiddleCenter;
            
            Background = new GuiTexture2D(_skyBox, TextureRepeatMode.Stretch);
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
            
            _optionsProvider.Load();
            base.OnShow();
        }
        
        protected override void OnHide()
        {
            _optionsProvider.Save();

            base.OnHide();
        }

        protected override void OnUnload()
        {
            _optionsProvider.Save();
            
            base.OnUnload();
        }
        
        private TGameState Construct<TGameState>() where TGameState : class, IGameState
        {
            TGameState state = null;
            foreach (var constructor in (typeof(TGameState).GetConstructors()))
            {
                bool canConstruct = true;
                object[] passedParameters = new object[0];
                var objparams = constructor.GetParameters();
                
                passedParameters = new object[objparams.Length];

                for (var index = 0; index < objparams.Length; index++)
                {
                    var param = objparams[index];
                    if (param.ParameterType == typeof(GuiPanoramaSkyBox))
                    {
                        passedParameters[index] = _skyBox;
                    }
                    else
                    {
                        canConstruct = false;
                        break;
                    }
                }

                if (canConstruct)
                {
                    state = (TGameState) constructor.Invoke(passedParameters);
                    break;
                }
            }

            return state;
        }

        protected GuiButton CreateLinkButton<TGameState>(string translationKey) where TGameState : class, IGameState
        {
            var state = Construct<TGameState>();
            
            if (state == null)
                throw new Exception($"Can not create linkbutton with type {typeof(TGameState)}");
            
            return new GuiButton(() =>
            {
                Alex.GameStateManager.SetActiveState(state);
                state.ParentState = ParentState;
            })
            {
                TranslationKey = translationKey,
	            Modern = false
            };
        }

        protected GuiSlider CreateSlider(Func<double, string> formatter, Func<AlexOptions, OptionsProperty<int>> optionsAccessor,
            int? minValue = null, int? maxValue = null, int? stepInterval = null)
            => CreateSlider(new ValueFormatter<double>(formatter), optionsAccessor, minValue, maxValue, stepInterval);

        protected GuiSlider CreateSlider(string label, Func<AlexOptions, OptionsProperty<int>> optionsAccessor,
            int? minValue = null, int? maxValue = null, int? stepInterval = null)
            => CreateSlider(new ValueFormatter<double>(label), optionsAccessor, minValue, maxValue, stepInterval);
        
        protected GuiSlider CreateSlider(ValueFormatter<double> label, Func<AlexOptions, OptionsProperty<int>> optionsAccessor, int? minValue = null, int? maxValue = null, int? stepInterval = null)
        {
            var slider = CreateValuedControl<GuiSlider, double, int>(label, optionsAccessor);
            
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

        protected GuiSlider CreateSlider(Func<double, string> formatter, Func<AlexOptions, OptionsProperty<double>> optionsAccessor,
            double? minValue = null, double? maxValue = null, double? stepInterval = null)
            => CreateSlider(new ValueFormatter<double>(formatter), optionsAccessor, minValue, maxValue, stepInterval);


        protected GuiSlider CreateSlider(string label, Func<AlexOptions, OptionsProperty<double>> optionsAccessor,
            double? minValue = null, double? maxValue = null, double? stepInterval = null)
            => CreateSlider(new ValueFormatter<double>(label), optionsAccessor, minValue, maxValue, stepInterval);
        
        protected GuiSlider CreateSlider(ValueFormatter<double> valueFormatter, Func<AlexOptions, OptionsProperty<double>> optionsAccessor, double? minValue = null, double? maxValue = null, double? stepInterval = null)
        {
            var slider = CreateValuedControl<GuiSlider, double>(valueFormatter, optionsAccessor);
            
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

        protected GuiEnumSwitchButton<TEnum> CreateSwitch<TEnum>(string displayFormat, Func<AlexOptions, OptionsProperty<TEnum>> optionsAccessor) where TEnum : Enum
        {
            var @switch = CreateValuedControl<GuiEnumSwitchButton<TEnum>, TEnum>(displayFormat, optionsAccessor);// {Modern = false, DisplayFormat = displayFormat};
            @switch.Modern = false;
            return @switch;
        }

        protected GuiToggleButton CreateToggle(string displayFormat,
            Func<AlexOptions, OptionsProperty<bool>> optionsAccessor)
        {
            var sw = CreateValuedControl<GuiToggleButton, bool>(displayFormat, optionsAccessor);
            sw.Modern = false;
            
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

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
            _skyBox.Update(gameTime);
        }

        protected override void OnDraw(IRenderArgs args)
        {
            if (_skyBox != null)
            {
                if (!_skyBox.Loaded)
                {
                    _skyBox.Load(Alex.GuiRenderer);
                }
                
                _skyBox.Draw(args);
            }

            if (Alex.InGame)
            {
                ParentState.Draw(args);
            }

            base.OnDraw(args);
        }
    }
}
