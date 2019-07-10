using System.Net;
using System.Threading;
using Alex.GuiDebugger.Common;
using Alex.GuiDebugger.Common.Services;
using JKang.IpcServiceFramework;
using Microsoft.Extensions.DependencyInjection;

namespace Alex.Services
{
	public class AlexIpcService
	{

		private IServiceCollection ServiceCollection;

		private IIpcServiceHost _host;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public AlexIpcService()
		{
			ServiceCollection = ConfigureServices(new ServiceCollection());

			_host = new IpcServiceHostBuilder(ServiceCollection.BuildServiceProvider())
				.AddNamedPipeEndpoint<IGuiDebuggerService>(name: "guiDebuggerNamedPipeEndpoint",
														   pipeName: GuiDebuggerConstants.NamedPipeName)
				.AddTcpEndpoint<IGuiDebuggerService>(name: "guiDebuggerTcpEndpoint", ipEndpoint: IPAddress.Loopback, port: GuiDebuggerConstants.TcpEndpointPort)
				.Build();

		}

		public void Start()
		{
			_host.RunAsync(_cancellationTokenSource.Token);
		}

		public void Stop()
		{
			_cancellationTokenSource.Cancel();
		}

		private static IServiceCollection ConfigureServices(IServiceCollection services)
		{
			services.AddIpc(x =>
			{
				x.AddNamedPipe(options => { options.ThreadCount = 2; })
				 .AddService<IGuiDebuggerService, GuiDebuggerService>();
			});

			return services;
		}

	}
}
