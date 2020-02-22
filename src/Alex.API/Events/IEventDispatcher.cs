namespace Alex.API.Events
{
    public interface IEventDispatcher
    {
        void RegisterEvents<T>(T obj) where T : class;
        void UnregisterEvents<T>(T obj) where T : class;
        void DispatchEvent(Event e);
    }
}