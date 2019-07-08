using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
				.AddNamedPipeEndpoint<IGuiDebuggerService>(name: "guiDebugger",
														   pipeName: GuiDebuggerConstants.NamedPipeName)
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
				 .AddService<IGuiDebuggerService>(s => Alex.Instance.GuiDebugHelper);
			});

			return services;
		}

	}
}
