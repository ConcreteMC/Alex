using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using RocketUI.Styling;

namespace RocketUI.Elements
{
    public partial class VisualElement : IStyledElement
    {
        StyledElementPropertyCollection IStyledElement.StyledProperties { get; } = new StyledElementPropertyCollection();

        public string ClassName { get; set; }
        
        public StyleSheet Styles { get; set; }

        public bool IsStyleDirty { get; protected set; } = true;
        
        public void InvalidateStyle()
        {
            InvalidateStyle(this);
        }
        public void InvalidateStyle(IStyledElement sender)
        {
            IsStyleDirty = true;

            ForEachChild<IStyledElement>(c =>
            {
                if (c != sender)
                {
                    c.InvalidateStyle(this);
                }
            });

            if (ParentElement != sender)
            {
                (ParentElement as IStyledElement)?.InvalidateStyle(this);
            }
        }

        private void InitialiseStyledProperties()
        {
            var type = GetType();

            foreach (var member in type.GetProperties(BindingFlags.Instance))
            {
                var attr = member.GetCustomAttribute<StyledPropertyAttribute>();
                if (attr != null)
                {
                    var prop = StyledProperty.Register(member.Name, member.DeclaringType, member.PropertyType);
                    prop.GetOrCreateInstance(this);
                }
            }
        }

        private void UpdateStyle()
        {
            if (!IsStyleDirty) return;

        }
        protected object GetValue(StyledProperty property, Action callbackAction = null)
        {
            var val = property.GetValue(this);
            callbackAction?.Invoke();
            return val;
        }

        protected void SetValue(StyledProperty property, object value, Action callbackAction = null)
        {
            property.SetValue(this, value);
            callbackAction?.Invoke();
        }
        
        protected object GetValue(Action callbackAction = null, [CallerMemberName] string propertyName = null)
        {
            object value = null;

            if (StyledProperty.TryGetProperty(GetType(), propertyName, out var property))
            {
                value = property.GetValue(this);
            }

            callbackAction?.Invoke();
            OnPropertyChanged(propertyName);
            return value;
        }

        protected void SetValue(object value, Action callbackAction = null, [CallerMemberName] string propertyName = null)
        {
            if (StyledProperty.TryGetProperty(GetType(), propertyName, out var property))
            {
                property.SetValue(this, value);
            }

            callbackAction?.Invoke();
        }
    }
}
