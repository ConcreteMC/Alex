namespace Alex.API.Data.Options
{
    public delegate void OptionsPropertyChangedDelegate<TProperty>(TProperty oldValue, TProperty newValue);
}