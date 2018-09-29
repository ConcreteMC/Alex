using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Alex.Launcher
{
    public static class Program
    {

        [STAThread]
        public static void Main(string[] args)
        {
            if (!args.Any())
            {
                // Run Launcher
                App.Main();
            }
            else
            {
                // Run Alex
                RunAlex(new string[] { });
            }
        }
        public static void RunAlex() => RunAlex(new string[0]);

        public static void RunAlex(string[] args)
        {
            try
            {
                var currentDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var alexDllPath = Path.Combine(currentDir, "Alex.dll");
                var alexArgs = string.Join(" ", args);
                
                var cmd = $"dotnet \"{alexDllPath}\" {alexArgs}";
                var processStartInfo = new ProcessStartInfo()
                {
                    FileName = "dotnet.exe",
                    Arguments = $"\"{alexDllPath}\" {alexArgs}",
                    UseShellExecute = false,
                    WorkingDirectory = currentDir,
                    CreateNoWindow = true,
                    
                    //WindowStyle = ProcessWindowStyle.Hidden
                };

                using(var alex = Process.Start(processStartInfo)) {
                if (alex != null && !alex.HasExited)
                {
                    if (Application.Current.MainWindow != null) Application.Current.MainWindow.Hide();
                }
                
                //MessageBox.Show($"Opened with pid {alex.Id}!");

                //Thread.Sleep(5 * 1000);

                //MessageBox.Show("Closed!");
                
                alex.WaitForExit();
                var main = Application.Current.MainWindow;
                if (main != null)
                {
                    main.Show();
                    main.Focus();
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
