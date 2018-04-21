namespace Alex.API.Data.Options
{
    public delegate TProperty OptionsPropertyValidator<TProperty>(TProperty currentValue, TProperty newValue);
}