using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Alex.API.Data.Options
{
    public delegate void OptionsPropertyChangedDelegate<TProperty>(TProperty oldValue, TProperty newValue);
    public delegate TProperty OptionsPropertyValidator<TProperty>(TProperty currentValue, TProperty newValue);

    [Serializable]
    [JsonConverter(typeof(OptionsPropertyJsonConverter<>))]
    public class OptionsProperty<TProperty> : IOptionsProperty
    {
        public event OptionsPropertyChangedDelegate<TProperty> ValueChanged;

        private readonly TProperty _defaultValue;
        private TProperty _value;
        public TProperty Value
        {
            get => _value;
            set
            {
                var oldValue = _value;
                var newValue = value;

                if (_validator != null)
                {
                    newValue = _validator.Invoke(oldValue, newValue);
                }

                _value = newValue;
                ValueChanged?.Invoke(oldValue, newValue);
            }
        }

        private readonly OptionsPropertyValidator<TProperty> _validator;

        internal OptionsProperty(TProperty defaultValue, OptionsPropertyValidator<TProperty> validator = null)
        {
            _defaultValue = defaultValue;
            _validator = validator;
        }

        public void ResetToDefault()
        {
            Value = _defaultValue;
        }
        
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {

        }
        [OnSerializing]
        private void OnSerialized(StreamingContext context)
        {

        }



        public static implicit operator TProperty(OptionsProperty<TProperty> optionsProperty)
        {
            return optionsProperty.Value;
        }

        public OptionsPropertyAccessor<TProperty> Bind(OptionsPropertyChangedDelegate<TProperty> listenerDelegate)
        {
            var accessor = new OptionsPropertyAccessor<TProperty>(this, listenerDelegate);
            ValueChanged += accessor.Invoke;

            return accessor;
        }
        internal void Unbind(OptionsPropertyAccessor<TProperty> accessor)
        {
            ValueChanged -= accessor.Invoke;
        }

    }

    public class OptionsPropertyJsonConverter<TProperty> : JsonConverter<OptionsProperty<TProperty>> where TProperty : class
    {
        public override void WriteJson(JsonWriter writer, OptionsProperty<TProperty> value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Value);
        }

        public override OptionsProperty<TProperty> ReadJson(JsonReader reader, Type objectType, OptionsProperty<TProperty> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = serializer.Deserialize(reader, typeof(TProperty)) as TProperty;

            if (hasExistingValue)
            {
                existingValue.Value = value;
            }

            return new OptionsProperty<TProperty>(value);
        }
    }
}