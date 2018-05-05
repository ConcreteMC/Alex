using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RocketUI.Styling
{
    public class StyleSelector
    {
        private static readonly RegexOptions RegexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline;
        private static readonly Regex SelectorRegex = new Regex(@"\s*(?'parentSelectorType'>)?\s*(?'targetType'\w*)(?'className'\.\w+)\s*", RegexOptions);

        public string Selector { get; }
        
        private readonly StyleSelector _parentSelector;
        private readonly List<ISelectorConstraint> _constraints = new List<ISelectorConstraint>();

        private StyleSelector(StyleSelector parentSelector, Match selectorMatch)
        {
            _parentSelector = parentSelector;
            Selector = selectorMatch.Value;

            Initialise(selectorMatch);
        }

        private void Initialise(Match selectorMatch)
        {
            _constraints.Clear();
            
            if (_parentSelector == null)
            {
                var parentMatch = selectorMatch.Groups["parentSelectorType"];
                _constraints.Add(new ParentSelectorConstraint(_parentSelector, parentMatch.Success));
            }

            var targetType = selectorMatch.Groups["targetType"];
            if (targetType.Success)
            {
                if (StyleManager.TryResolveType(targetType.Value, out var type))
                {
                    _constraints.Add(new TypeSelectorConstraint(type));
                }
            }

            var className = selectorMatch.Groups["className"];
            if (className.Success)
            {
                _constraints.Add(new ClassNameSelectorConstaint(className.Value.Substring(1)));
            }
        }


        public bool IsMatch(IStyledElement element)
        {
            return _constraints.All(constraint => constraint.IsMatch(element));
        }
        
        public static StyleSelector FromString(string selectorString)
        {
            // First, split by parent separator
            var rawSelectors = SelectorRegex.Matches(selectorString);

            StyleSelector selector = null;
            foreach (Match rawSelector in rawSelectors)
            {
                selector = new StyleSelector(selector, rawSelector);
            }

            return selector;
        }
    }
}
