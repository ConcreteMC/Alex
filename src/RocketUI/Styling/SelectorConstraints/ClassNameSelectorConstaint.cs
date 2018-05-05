using System;

namespace RocketUI.Styling
{
    internal class ClassNameSelectorConstaint : ISelectorConstraint
    {
        private readonly string _className;

        public ClassNameSelectorConstaint(string className)
        {
            _className = className;
        }

        public bool IsMatch(IStyledElement element)
        {
            return string.Equals(element.ClassName, _className, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}