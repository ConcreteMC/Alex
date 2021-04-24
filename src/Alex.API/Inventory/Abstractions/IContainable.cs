namespace Alex.API.Inventory
{
    public interface IContainable
    {
        bool Stackable { get; }

        int MaxStackSize { get; }
    }
}