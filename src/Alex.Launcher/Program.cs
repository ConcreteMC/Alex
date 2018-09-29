using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Alex.Launcher.ViewModels;
using Alex.Launcher.Views;
using Avalonia;
using Avalonia.Logging.Serilog;

namespace Alex.Launcher
{
    class Program
    {

        static void Main(string[] args)
        {
            if (!args.Any())
            {
                BuildAvaloniaApp().Start<MainWindow>(() => new MainWindowViewModel());
            }
            else
            {
                // Run Alex
                RunAlex(new string[] { });
            }
        }

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
                                                        .UsePlatformDetect()
                                                        .UseReactiveUI()
                                                        .LogToDebug();
        public static void RunAlex() => RunAlex(new string[0]);

        public static void RunAlex(string[] args)
        {
            try
            {
                var currentDir  = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var alexDllPath = Path.Combine(currentDir, "Alex.dll");
                var alexArgs    = string.Join(" ", args);
                
                var cmd = $"dotnet \"{alexDllPath}\" {alexArgs}";
                var processStartInfo = new ProcessStartInfo()
                {
                    FileName         = "dotnet.exe",
                    Arguments        = $"\"{alexDllPath}\" {alexArgs}",
                    UseShellExecute  = false,
                    WorkingDirectory = currentDir,
                    CreateNoWindow   = true,
                    
                    //WindowStyle = ProcessWindowStyle.Hidden
                };

                using(var alex = Process.Start(processStartInfo)) {
                    if (alex != null && !alex.HasExited)
                    {
                        MainWindow.Instance?.Hide();
                    }
                
                    //MessageBox.Show($"Opened with pid {alex.Id}!");

                    //Thread.Sleep(5 * 1000);

                    //MessageBox.Show("Closed!");
                
                    alex.WaitForExit();
                    if (MainWindow.Instance != null)
                    {
                        MainWindow.Instance.Show();
                        MainWindow.Instance.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
