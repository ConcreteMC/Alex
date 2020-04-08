using System;

namespace Alex.API.Data.Options
{
    public interface IOptionsProperty
    {
        void ResetToDefault();

        object GetValue();
        void SetValue(object obj);
        
        Type PropertyType { get; }
    }
}