using System;

namespace RocketUI.Styling
{
    internal class TypeSelectorConstraint : ISelectorConstraint
    {
        private readonly Type _type;
        public TypeSelectorConstraint(Type type)
        {
            _type = type;
        }

        public bool IsMatch(IStyledElement element)
        {
            return (_type.IsInstanceOfType(element));
        }
    }
}