using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Alex.GuiDebugger.Views
{
	public class MainView : UserControl
	{
		public MainView()
		{
			this.InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
