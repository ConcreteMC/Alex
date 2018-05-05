using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using TB.ComponentModel;

namespace RocketUI.Styling
{
    public class StyledPropertyChangedArgs : PropertyChangedEventArgs
    {
        public StyledProperty StyledProperty { get; }

        internal StyledPropertyChangedArgs(StyledProperty property) : base(property.Name)
        {
            StyledProperty = property;
        }
    }
    public delegate void StyledPropertyChangedEventHandler(object sender, StyledPropertyChangedArgs e);
    
    public class StyledProperty
    {
        public string Name { get; }

        public Type OwnerType { get; }
        public Type ValueType { get; }

        public PropertyInfo PropertyInfo { get; }
        public EventInfo ChangedEventInfo { get; }

        public object RawDefaultValue { get; protected set; }
        
        internal StyledProperty(string name, Type ownerType, Type valueType, object defaultValue)
        {
            Name            = name;
            ValueType       = valueType;
            OwnerType       = ownerType;
            RawDefaultValue = defaultValue;
            PropertyInfo = ResolvePropertyInfo(ownerType, name, valueType);
            ChangedEventInfo = ResolveEventInfo(ownerType, name + "Changed");
        }

        internal StyledProperty(PropertyInfo propertyInfo, EventInfo eventInfo) 
        {
            PropertyInfo = propertyInfo;
            ChangedEventInfo = eventInfo;

            Name = propertyInfo.Name;
            OwnerType = propertyInfo.DeclaringType;
            ValueType = propertyInfo.PropertyType;
        }

        public object GetValue(IStyledElement owner)
        {
            return GetOrCreateInstance(owner).GetValue();
        }
        public void SetValue(IStyledElement owner, object value)
        {
            if(owner == null) throw new ArgumentNullException(nameof(owner));
            if(!OwnerType.IsInstanceOfType(owner)) throw new ArgumentException($"Owner object ({owner.GetType()}) is not of type {OwnerType.FullName}");

            if (value == null)
            {
                value = RawDefaultValue;
            }

            if (!ValueType.IsInstanceOfType(value))
            {
                if (!UniversalTypeConverter.TryConvert(value, ValueType, out value))
                {
                    throw new
                        ArgumentException($"Value object ({value.GetType()}) is not of type {ValueType.FullName} and no valid type converter could be found.");
                }
            }

            GetOrCreateInstance(owner).SetValue(value);
        }

        internal StyledPropertyInstance GetOrCreateInstance(IStyledElement owner)
        {
            if (!owner.StyledProperties.TryGetValue(Name, out var instance))
            {
                instance                     = new StyledPropertyInstance(owner, this);

                owner.StyledProperties[Name] = instance;
            }

            return instance;
        }

        internal static bool TryGetProperty(Type type, string name, out StyledProperty property)
        {
            var key = new Tuple<Type, string>(type, name);
            if (!AllProperties.TryGetValue(key, out property))
            {
                var baseType = type.BaseType;
                if (baseType == null || baseType == type) return false;

                return TryGetProperty(baseType, name, out property);
            }

            return true;
        }

        private static PropertyInfo ResolvePropertyInfo(Type type, string propertyName, Type propertyType)
        {
            return type.GetProperty(propertyName, propertyType);
        }

        private static EventInfo ResolveEventInfo(Type type, string eventName)
        {
            return type.GetEvent(eventName, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private static readonly Dictionary<Tuple<Type, string>, StyledProperty> AllProperties = new Dictionary<Tuple<Type, string>, StyledProperty>();

        public static StyledProperty Register(string propertyName, Type ownerType, Type valueType, object defaultValue = null)
        {
            var key = new Tuple<Type, string>(ownerType, propertyName);
            if (!AllProperties.TryGetValue(key, out var property))
            {
                property = new StyledProperty(propertyName, ownerType, valueType, defaultValue);
                AllProperties[key] = property;
            }

            return property;
        }
    }

    public sealed class StyledPropertyInstance
    {
        public StyledProperty Property { get; }

        private readonly StyledPropertyChangedEventHandler _changedEvent;
        private IStyledElement _owner;
        private object _value;
        
        internal StyledPropertyInstance(IStyledElement owner, StyledProperty property)
        {
            _owner = owner;
            Property = property;
            _value = property.RawDefaultValue;
            _changedEvent = (StyledPropertyChangedEventHandler) property.ChangedEventInfo.GetRaiseMethod().CreateDelegate(typeof(StyledPropertyChangedEventHandler), owner);
        }

        public object GetValue()
        {
            return _value ?? Property.RawDefaultValue;
        }

        public void SetValue(object value)
        {
            if (value == null)
            {
                value = Property.RawDefaultValue;
            }

            if (!Property.ValueType.IsInstanceOfType(value))
            {
                if (!UniversalTypeConverter.TryConvert(value, Property.ValueType, out value))
                {
                    throw new
                        ArgumentException($"Value object ({value.GetType()}) is not of type {Property.ValueType.FullName} and no valid type converter could be found.");
                }
            }

            _value = value;

            _changedEvent?.Invoke(_owner, new StyledPropertyChangedArgs(Property));
        }
    }
}
