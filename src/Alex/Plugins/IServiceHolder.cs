using Microsoft.Extensions.DependencyInjection;

namespace Alex.Plugins
{
    public interface IServiceHolder
    {
        void ConfigureServices(IServiceCollection serviceCollection);
    }
}