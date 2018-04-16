using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui.Elements.Controls;

namespace Alex.API.Gui.Elements
{
    public enum LabelPosition
    {
        AboveControl,
        LeftOrControl
    }

    public class GuiLabelledControlGroup : GuiStackContainer
    {

        private int _labelWidth = 80;

        public int LabelWidth
        {
            get => _labelWidth;
            set
            {
                _labelWidth = value;
                UpdateLayout();
            }
        }

        private LabelPosition _labelPosition = LabelPosition.AboveControl;

        public LabelPosition LabelPosition
        {
            get => _labelPosition;
            set
            {
                _labelPosition = value;
                UpdateLayout();
            }
        }

        public GuiLabelledControlGroup()
        {

        }

        protected override void OnUpdateLayout()
        {
            if (LabelPosition == LabelPosition.LeftOrControl)
            {
                ForEachChild(c =>
                {
                    if (c is GuiLabelledControlRow row)
                    {
                        row.Label.LayoutWidth = LabelWidth;

                        row.Element.LayoutOffsetY = 0;

                        row.Orientation = Orientation.Horizontal;
                        row.HorizontalContentAlignment = HorizontalAlignment.None;
                    }
                });
            }
            else if(LabelPosition == LabelPosition.AboveControl)
            {
                ForEachChild(c =>
                {
                    if (c is GuiLabelledControlRow row)
                    {
                        row.Label.LayoutWidth = LabelWidth;
                        
                        row.Orientation       = Orientation.Vertical;
                        row.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    }
                });
            }

            base.OnUpdateLayout();
        }

        public GuiSlider AppendSlider(string  label, double defaultValue, Action<double> valueUpdatedAction, double? minValue = null,
                                      double? maxValue = null, double? stepInterval = null)
        {
            var slider = AppendValuedControl<GuiSlider, double>(label, defaultValue, valueUpdatedAction);
            
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

        public GuiToggleButton AppendToggleButton(string label, bool defaultValue, Action<bool> valueUpdatedAction)
        {
            var button = AppendValuedControl<GuiToggleButton, bool>(label, defaultValue, valueUpdatedAction);
            return button;
        }

        public TControl AppendValuedControl<TControl, TValue>(string label, TValue defaultValue, Action<TValue> valueChangedAction) where TControl : IGuiElement, IValuedControl<TValue>, new()
        {
            var control = new TControl();

            control.Value = defaultValue;
            control.ValueChanged += (s, v) => valueChangedAction?.Invoke(v);

            return AppendControl<TControl>(label, control);
        }

        public TGuiElement AppendControl<TGuiElement>(string label, TGuiElement control) where TGuiElement : IGuiElement
        {
            var row = new GuiLabelledControlRow(label, control);
            AddChild(row);

            return control;
        }
        
        class GuiLabelledControlRow : GuiStackContainer
        {

            public GuiTextElement Label { get; set; }
            public IGuiElement Element { get; set; }
            
            public GuiLabelledControlRow(string label, IGuiElement element)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch;
                
                AddChild(Label = new GuiTextElement()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Text = label
                });

                element.HorizontalAlignment = HorizontalAlignment.Right;
                element.VerticalAlignment = VerticalAlignment.Top;

                AddChild(Element = element);
            }
        }
    }
}
