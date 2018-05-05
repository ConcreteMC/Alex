using Alex.API.Data.Options;

namespace Alex.API.Services
{
    public interface IOptionsProvider
    {
        AlexOptions AlexOptions { get; }

        void Load();
        void Save();

        void ResetAllToDefault();
    }
}
