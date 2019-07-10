namespace Alex.API.Services
{
    public interface IDataProvider<TDataType>
    {
        TDataType Data { get; }
        
        void Load();
        void Save(TDataType data);
    }
}
