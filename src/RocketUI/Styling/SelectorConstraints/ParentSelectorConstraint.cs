namespace RocketUI.Styling
{
    internal class ParentSelectorConstraint : ISelectorConstraint
    {
        private readonly bool _isDirectOnly;
        private readonly StyleSelector _parentSelector;
        public ParentSelectorConstraint(StyleSelector parentSelector, bool isDirectOnly)
        {
            _parentSelector = parentSelector;
            _isDirectOnly = isDirectOnly;
        }

        public bool IsMatch(IStyledElement element)
        {
            if (element.ParentElement == null) 
                return false;

            if (_isDirectOnly)
            {
                if (element.ParentElement is IStyledElement styledParent)
                {
                    return _parentSelector.IsMatch(styledParent);
                }
            }
            else
            {
                if (element.TryFindParentOfType<IStyledElement>(parentElement => _parentSelector.IsMatch(element), out var matchedParentElement))
                {
                    return true;
                }
            }


            return false;
        }
    }
}