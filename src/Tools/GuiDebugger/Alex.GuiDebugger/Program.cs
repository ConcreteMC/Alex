using System;
using System.Collections.ObjectModel;
using Alex.GuiDebugger.Factories;
using Alex.GuiDebugger.Models;
using Avalonia;
using Avalonia.Logging.Serilog;
using Alex.GuiDebugger.ViewModels;
using Alex.GuiDebugger.Views;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Serializer;

namespace Alex.GuiDebugger
{
    class Program
    {
        private static void Print(Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            if (ex.InnerException != null)
            {
                Print(ex.InnerException);
            }
        }

        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                var   serializer = new DockJsonSerializer(typeof(ObservableCollection<>));
                var   vm         = new MainWindowViewModel();
                var   factory    = new DefaultDockFactory(new DockData());
                IDock layout     = null;

                //string path = serializer.GetBasePath("Layout.json");
                //if (serializer.Exists(path))
                //{
                //    layout = serializer.Load<RootDock>(path);
                //}

                BuildAvaloniaApp().Start<MainWindow>(() =>
                {
                    vm.Factory = factory;
                    vm.Layout  = layout ?? vm.Factory.CreateLayout();
                    vm.Factory.InitLayout(vm.Layout);
                    return vm;
                });

                if (vm.Layout is IDock dock)
                {
                    dock.Close();
                }

                //serializer.Save(path, vm.Layout);
            }
            catch (Exception ex)
            {
                Print(ex);
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UseReactiveUI()
                .UseDataGrid()
                .UsePlatformDetect()
                .LogToDebug();
    }
}
