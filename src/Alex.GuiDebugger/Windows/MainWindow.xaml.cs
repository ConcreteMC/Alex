using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Alex.GuiDebugger.Common;
using Alex.GuiDebugger.Models;
using Alex.GuiDebugger.ViewModels;

namespace Alex.GuiDebugger.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private MainViewModel _mainViewModel;

		public MainWindow()
		{
			InitializeComponent();
			DataContext = _mainViewModel = new MainViewModel();
		}

		private void OnRefreshButtonClick(object sender, RoutedEventArgs e)
		{
			var items = App.GuiDebuggerService.GetAllGuiElementInfos();
			_mainViewModel.ElementTreeItems.Clear();

			foreach (var item in items)
			{
				_mainViewModel.ElementTreeItems.Add(item);
			}
		}

		private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			var item = e.NewValue as GuiElementInfo;
			if (item == null)
			{
				App.GuiDebuggerService.DisableHighlight();
			}
			else
			{
				App.GuiDebuggerService.HighlightGuiElement(item.Id);
			}
		}
	}
}
