using System;
using RocketUI.Elements.Controls;
using RocketUI.Elements.Layout;

namespace RocketUI.Elements
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
                InvalidateLayout();
            }
        }

        private LabelPosition _labelPosition = LabelPosition.AboveControl;

        public LabelPosition LabelPosition
        {
            get => _labelPosition;
            set
            {
                _labelPosition = value;
                InvalidateLayout();
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
                        //row.Label.LayoutWidth = LabelWidth;

                        //row.Element.LayoutOffsetY = 0;

                        row.Orientation = Orientation.Horizontal;
                        //row.HorizontalContentAlignment = HorizontalAlignment.None;
                    }
                });
            }
            else if(LabelPosition == LabelPosition.AboveControl)
            {
                ForEachChild(c =>
                {
                    if (c is GuiLabelledControlRow row)
                    {
                        //row.Label.LayoutWidth = LabelWidth;
                        
                        row.Orientation       = Orientation.Vertical;
                        //row.HorizontalContentAlignment = HorizontalAlignment.FillParent;
                    }
                });
            }

            base.OnUpdateLayout();
        }

        public Slider AppendSlider(string  label, double defaultValue, Action<double> valueUpdatedAction, double? minValue = null,
                                      double? maxValue = null, double? stepInterval = null)
        {
            var slider = AppendValuedControl<Slider, double>(label, defaultValue, valueUpdatedAction);
            
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

        public ToggleButton AppendToggleButton(string label, bool defaultValue, Action<bool> valueUpdatedAction)
        {
            var button = AppendValuedControl<ToggleButton, bool>(label, defaultValue, valueUpdatedAction);
            return button;
        }

        public TControl AppendValuedControl<TControl, TValue>(string label, TValue defaultValue, Action<TValue> valueChangedAction) where TControl : IVisualElement, IValuedControl<TValue>, new()
        {
            var control = new TControl();

            control.Value = defaultValue;
            control.ValueChanged += (s, v) => valueChangedAction?.Invoke(v);

            return AppendControl<TControl>(label, control);
        }

        public TGuiElement AppendControl<TGuiElement>(string label, TGuiElement control) where TGuiElement : IVisualElement
        {
            var row = new GuiLabelledControlRow(label, control);
            AddChild(row);

            return control;
        }
        
        class GuiLabelledControlRow : GuiStackContainer
        {

            public TextBlock Label { get; set; }
            public IVisualElement Element { get; set; }
            
            public GuiLabelledControlRow(string label, IVisualElement element)
            {
                Anchor = RocketUI.Anchor.FillX;
                
                AddChild(Label = new TextBlock()
                {
                    Anchor = RocketUI.Anchor.TopLeft,
                    Text = label
                });

                element.Anchor = RocketUI.Anchor.TopRight;

                AddChild(Element = element);
            }
        }
    }
}
