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
using Alex.GuiDebugger.Common.Services;
using Alex.GuiDebugger.Models;
using Alex.GuiDebugger.ViewModels;
using Catel.IoC;

namespace Alex.GuiDebugger.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private MainViewModel _mainViewModel;

		public MainWindow()
		{
			InitializeComponent();
			DataContext = _mainViewModel = new MainViewModel();

			PropertyGridItem.ValueChanged -= PropertyGridItemOnValueChanged;
			PropertyGridItem.ValueChanged += PropertyGridItemOnValueChanged;
		}

		private IGuiDebuggerService GuiDebuggerService
		{
			get => this.GetDependencyResolver().Resolve<IGuiDebuggerService>();
		}

		private void PropertyGridItemOnValueChanged(object sender, PropertyGridItemValueChangedEventArgs e)
		{
			GuiDebuggerService.SetElementPropertyValue(e.ElementId, e.PropertyName, e.NewValue);
		}

		private void OnRefreshButtonClick(object sender, RoutedEventArgs e)
		{
			var items = GuiDebuggerService.GetAllGuiElementInfos();
			_mainViewModel.ElementTreeItems.Clear();

			foreach (var item in items)
			{
				_mainViewModel.ElementTreeItems.Add(item);
			}
		}

		private void OnRefreshPropertiesButtonClick(object sender, RoutedEventArgs e)
		{
			if (_elementTreeView.SelectedItem is GuiElementInfo elementInfo)
			{
				var items = GuiDebuggerService.GetElementPropertyInfos(elementInfo.Id);
				_mainViewModel.SelectedGuiElementPropertyInfos.Clear();

				foreach (var item in items)
				{
					_mainViewModel.SelectedGuiElementPropertyInfos.Add(new PropertyGridItem(elementInfo.Id, item.Name, item.StringValue));
				}
			}
			else
			{
				_mainViewModel.SelectedGuiElementPropertyInfos.Clear();
			}
		}

		private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			var item = e.NewValue as GuiElementInfo;
			if (item == null)
			{
				GuiDebuggerService.DisableHighlight();
				_mainViewModel.SelectedGuiElementPropertyInfos.Clear();
			}
			else
			{
				GuiDebuggerService.HighlightGuiElement(item.Id);
				_mainViewModel.SelectedGuiElementPropertyInfos.Clear();

				var infos = GuiDebuggerService.GetElementPropertyInfos(item.Id);
				foreach (var info in infos)
				{
					_mainViewModel.SelectedGuiElementPropertyInfos.Add(new PropertyGridItem(item.Id, info.Name, info.StringValue));
				}
			}
		}

	}
}