using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Data.Options;
using Alex.API.GameStates;
using Alex.API.Gui;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Services;
using Alex.GameStates.Gui.Common;
using Alex.Gui.Elements;

namespace Alex.GameStates.Gui.MainMenu.Options
{
    public class OptionsStateBase : GuiMenuStateBase
    {
        protected AlexOptions Options => _optionsProvider.AlexOptions;

        private readonly IOptionsProvider _optionsProvider;

        public OptionsStateBase()
        {
            _optionsProvider = GetService<IOptionsProvider>();

            Footer.AddChild(new GuiBackButton()
            {
                TranslationKey = "gui.done",
                Anchor = Alignment.TopFill
            });
        }

        protected override void OnShow()
        {
            _optionsProvider.Load();
            base.OnShow();
        }
        protected override void OnHide()
        {
            _optionsProvider.Save();

            base.OnHide();
        }

        protected GuiButton CreateLinkButton<TGameState>(string translationKey) where TGameState : class,IGameState, new()
        {
            return new GuiButton(() => Alex.GameStateManager.SetActiveState<TGameState>())
            {
                TranslationKey = translationKey
            };
        }
        protected GuiSlider CreateSlider(string label, Func<AlexOptions, OptionsProperty<int>> optionsAccessor, int? minValue = null, int? maxValue = null, int? stepInterval = null)
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
        protected GuiSlider CreateSlider(string label, Func<AlexOptions, OptionsProperty<double>> optionsAccessor, double? minValue = null, double? maxValue = null, double? stepInterval = null)
        {
            var slider = CreateValuedControl<GuiSlider, double>(label, optionsAccessor);
            
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
        protected TControl CreateValuedControl<TControl, TValue>(string label, Func<AlexOptions, OptionsProperty<TValue>> propertyAccessor)
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
        protected TControl CreateValuedControl<TControl, TValue, TPropertyValue>(string label, Func<AlexOptions, OptionsProperty<TPropertyValue>> propertyAccessor) 
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
    }
}
