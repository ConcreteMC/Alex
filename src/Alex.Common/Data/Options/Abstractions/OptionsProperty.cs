using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using NLog;

namespace Alex.Common.Data.Options
{
    public delegate void OptionsPropertyChangedDelegate<TProperty>(TProperty oldValue, TProperty newValue);
    public delegate TProperty OptionsPropertyValidator<TProperty>(TProperty currentValue, TProperty newValue);

    [Serializable]
    [JsonConverter(typeof(OptionsPropertyJsonConverter))]
    public class OptionsProperty<TProperty> : IOptionsProperty
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(IOptionsProperty));
        
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

        public OptionsProperty()
        {

        }

        internal OptionsProperty(TProperty defaultValue, OptionsPropertyValidator<TProperty> validator = null)
        {
            _defaultValue = defaultValue;
            _value = defaultValue;
            
            _validator = validator;
        }

        public void ResetToDefault()
        {
            Value = _defaultValue;
        }

        public object GetValue()
        {
            return Value;
        }

        public void SetValue(object obj)
        {
            if (obj == null)
            {
                _value = default(TProperty);
                return;
            }
            
            if (obj.GetType() == typeof(TProperty))
            {
                _value = (TProperty) obj;
            }
            else
            {
                Log.Warn($"Value is not of correct type, got {obj.GetType()} expected {typeof(TProperty)}");
            }
        }

        public Type PropertyType
        {
            get { return typeof(TProperty); }
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

    public class OptionsPropertyJsonConverter : JsonConverter<IOptionsProperty>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(OptionsPropertyJsonConverter));
        
        public override void WriteJson(JsonWriter writer, IOptionsProperty value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.GetValue());
        }

        public override IOptionsProperty ReadJson(JsonReader reader, Type objectType, IOptionsProperty existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (hasExistingValue)
            {
                var type = existingValue.PropertyType;

                existingValue.SetValue(serializer.Deserialize(reader, type));
                return existingValue;
            }
            
            
            var value = serializer.Deserialize(reader);
            var property = (IOptionsProperty) Activator.CreateInstance(objectType);
            property.SetValue(value);

            return property;
        }
    }
}