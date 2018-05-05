using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RocketUI.Styling
{
    public class Style
    {
        public StyleSelector Selector { get; }

        private readonly IReadOnlyDictionary<string, object> _styledProperties;

        public Style(StyleSelector selector, IDictionary<string, object> properties)
        {
            Selector = selector;
            _styledProperties = new ReadOnlyDictionary<string, object>(properties);
        }

        public bool IsMatch(IStyledElement element)
        {
            return Selector.IsMatch(element);
        }

        public void Apply(IStyledElement element)
        {
            if (!IsMatch(element)) return;

            var type = element.GetType();
            foreach (var styledProperty in _styledProperties)
            {
                if (StyledProperty.TryGetProperty(type, styledProperty.Key, out var prop))
                {
                    prop.SetValue(element, styledProperty.Value);
                }
            }
        }

    }
}
