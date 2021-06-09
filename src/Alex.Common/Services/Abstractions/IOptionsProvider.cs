using Alex.Common.Data.Options;

namespace Alex.Common.Services
{
    public interface IOptionsProvider
    {
        AlexOptions AlexOptions { get; }

        void Load();
        void Save();

        void ResetAllToDefault();
    }
}
