namespace RocketUI.Styling
{
    interface ISelectorConstraint
    {
        bool IsMatch(IStyledElement element);
    }
}