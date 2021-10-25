namespace Alex.Common.Services
{
    public interface IDataProvider<out TDataType>
    {
        TDataType Data { get; }
        
        void Load();
        void Save();
    }
}
