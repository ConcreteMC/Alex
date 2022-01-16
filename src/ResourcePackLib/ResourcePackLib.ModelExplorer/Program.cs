// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using NLog;
using NLog.Extensions.Hosting;
using NLog.Extensions.Logging;
using ResourcePackLib.ModelExplorer.Abstractions;
using ResourcePackLib.ModelExplorer.Configuration;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ResourcePackLib.ModelExplorer;

public static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ConfigureNLog(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));
        var logger = LogManager.GetCurrentClassLogger();
        try
        {
            logger.Info("Starting TruSaber");
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            // NLog: catch any exception and log it.
            logger.Error(ex, "Stopped program because of exception");
            throw;
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            LogManager.Shutdown();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseNLog()
            .ConfigureHostConfiguration(builder =>
            {
                builder.AddEnvironmentVariables("TruSaber_");
                builder.AddJsonFile("TruSaber.hostconfig.json", optional: true, reloadOnChange: false);
            })
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddCommandLine(args);
                builder.AddEnvironmentVariables("TruSaber_");
                builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            })
            .ConfigureLogging((context, builder) =>
            {
                builder
                    .ClearProviders()
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddNLog();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLogging();

                var opts = new GameOptions();
                hostContext.Configuration.Bind(opts);

                services.AddOptions<GameOptions>().Bind(hostContext.Configuration);
                services.AddSingleton<GameOptions>(opts);
                services.AddSingleton<IGame, ModelExplorerGame>();
                services.AddSingleton<Game>(sp => sp.GetRequiredService<IGame>().Game);
                services.AddHostedService<Worker>();
                Startup.RegisterServices(services);
            })
            .UseConsoleLifetime();

    private static void ConfigureNLog(string baseDir)
    {
        string loggerConfigFile = Path.Combine(baseDir, "NLog.config");

        string logsDir = Path.Combine(baseDir, "logs");
        if (!Directory.Exists(logsDir))
        {
            Directory.CreateDirectory(logsDir);
        }

        LogManager.ThrowConfigExceptions = false;
        LogManager.LoadConfiguration(loggerConfigFile);
//			LogManager.Configuration = new XmlLoggingConfiguration(loggerConfigFile);
        LogManager.Configuration.Variables["basedir"] = baseDir;
    }
}