using System;
using System.IO;

namespace Alex.API.Gui
{
    public interface IValuedControl<TValue> : IGuiControl
    {
        event EventHandler<TValue> ValueChanged;

        TValue Value { get; set; }

        ValueFormatter<TValue> DisplayFormat { get; set; }

    }

    public class ValueFormatter<TValue>
    {
        public static readonly string DefaultFormat = "{0}";
        public static readonly Func<TValue, string> DefaultFormatter = (v => v?.ToString() ?? string.Empty);
        
        private string _format = DefaultFormat;
        private Func<TValue, string> _formatter = DefaultFormatter;

        public string Format
        {
            get => _format ?? DefaultFormat;
            set => _format = value ?? DefaultFormat;
        }
        public Func<TValue, string> Formatter
        {
            get => _formatter ?? DefaultFormatter;
            set => _formatter = value ?? DefaultFormatter;
        }

        public ValueFormatter() { }
        public ValueFormatter(string format)
        {
            Format = format;
        }
        public ValueFormatter(Func<TValue, string> formatter)
        {
            Formatter = formatter;
        }

        public virtual string FormatValue(TValue value)
        {
            var valueStr = Formatter.Invoke(value);
            
            return string.Format(Format, valueStr);
        }
        
        public static implicit operator Func<TValue, string>(ValueFormatter<TValue> valueFormatter)
        {
            return valueFormatter.FormatValue;
        }
        
        public static implicit operator ValueFormatter<TValue>(Func<TValue, string> formatter)
        {
            return new ValueFormatter<TValue>(formatter);
        }
        
        public static implicit operator ValueFormatter<TValue>(string format)
        {
            return new ValueFormatter<TValue>(format);
        }
        
    }
}
