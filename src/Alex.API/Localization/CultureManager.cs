using System.Collections.Concurrent;
using System.Globalization;

namespace Alex.API.Localization
{
    public class CultureManager
    {
        private ConcurrentDictionary<CultureInfo, CultureLanguage> Languages { get; }
        public CultureManager()
        {
            Languages = new ConcurrentDictionary<CultureInfo, CultureLanguage>();
        }

        public CultureLanguage GetOrCreateCulture(CultureInfo culture)
        {
            return Languages.GetOrAdd(culture, info => new CultureLanguage());
        }
    }
}
