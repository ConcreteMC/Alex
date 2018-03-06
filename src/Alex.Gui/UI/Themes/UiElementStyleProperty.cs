namespace Alex.Graphics.UI.Themes
{
    public interface IUiElementStyleProperty
    {
        int Priority { get; }

        object Value { get; set; }

        bool HasValue { get; }
    }

    public class UiElementStyleProperty<TProperty> : IUiElementStyleProperty
    {
        public int Priority { get; }

        object IUiElementStyleProperty.Value
        {
            get => Value;
            set => Value = (TProperty) value;
        }

        public TProperty Value { get; set; }

        public bool HasValue => Value != null;

        public UiElementStyleProperty() : this(default(TProperty), -1)
        {

        }

        public UiElementStyleProperty(TProperty value) : this(value, -1)
        {

        }

        private UiElementStyleProperty(TProperty value, int priority)
        {
            Value = value;
            Priority = priority;
        }

        public static implicit operator UiElementStyleProperty<TProperty>(TProperty value)
        {
            return new UiElementStyleProperty<TProperty>(value);
        }

        public static implicit operator TProperty(UiElementStyleProperty<TProperty> value)
        {
            return value.Value;
        }

        public override string ToString()
        {
            return string.Format("<{0}>({1})", typeof(TProperty).Name, HasValue ? Value.ToString() : "NULL");
        }
    }
}
