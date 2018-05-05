using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace Alex.API.Data.Options
{
    public class OptionsBase : IOptionsProperty, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected readonly List<IOptionsProperty> Properties = new List<IOptionsProperty>();

        protected OptionsBase()
        {

        }

        protected OptionsProperty<float> DefineRangedProperty(float defaultValue, float minValue, float maxValue)
        {
            return DefineProperty(defaultValue, (value, newValue) => MathHelper.Clamp(newValue, minValue, maxValue));
        }
        protected OptionsProperty<int> DefineRangedProperty(int defaultValue, int minValue, int maxValue)
        {
            return DefineProperty(defaultValue, (value, newValue) => MathHelper.Clamp(newValue, minValue, maxValue));
        }
        protected OptionsProperty<TProperty> DefineProperty<TProperty>(TProperty defaultValue, OptionsPropertyValidator<TProperty> validator = null)
        {
            var property = new OptionsProperty<TProperty>(defaultValue, validator);
            Properties.Add(property);
            return property;
        }

        protected TOptions DefineBranch<TOptions>() where TOptions : OptionsBase, new()
        {
            var opt = new TOptions();
            Properties.Add(opt);
            return opt;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        #region IOptionsProperty Implementation
        
        public void ResetToDefault()
        {
            foreach (var property in Properties.ToArray())
            {
                property.ResetToDefault();
            }
        }
        
        #endregion
    }
}