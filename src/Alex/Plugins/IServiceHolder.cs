using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Alex.Plugins
{
	public interface IServiceHolder
	{
		void ConfigureServices(Container serviceCollection);
	}
}