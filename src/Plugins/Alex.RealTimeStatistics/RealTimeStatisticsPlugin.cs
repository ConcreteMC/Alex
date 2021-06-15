using System;
using Alex.Plugins;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MiNET.Plugins.Attributes;
using VueCliMiddleware;

namespace Alex.RealTimeStatistics
{
    [PluginInfo(Name = "Real Time Statistics", Description = "Provides a Real Time Statistics web-interface for Alex", Author = "Kenny van Vulpen")]
    public class RealTimeStatisticsPlugin : Plugin
    {
        /*public static void Main(string[] args)
        {
            if (!CommandLine.Arguments.TryGetOptions(args, true, out string mode, out ushort port, out bool https)) return;

            if (mode == "kill") {
                Console.WriteLine($"Killing process serving port {port}...");
                PidUtils.KillPort(port, true, true);
                return;
            }

            CreateHostBuilder(args).Build().Run();
        }*/


        private IHost _host;

        public RealTimeStatisticsPlugin()
        {
            _host = CreateHostBuilder(new string[0]).Build();
        }
        /// <inheritdoc />
        public override void Enabled()
        {
            _host.StartAsync().Wait();
        }

        /// <inheritdoc />
        public override void Disabled()
        {
            _host.StopAsync().Wait();
        }
        
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
