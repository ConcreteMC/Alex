using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Alex.Launcher.Views
{
    public class MainWindow : Window
    {
        internal static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            Instance = this;

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            var playButton = this.FindControl<Button>("PlayButton");
            playButton.Click += PlayButtonOnClick;
        }

        private void PlayButtonOnClick(object sender, RoutedEventArgs e)
        {
            Program.RunAlex();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}