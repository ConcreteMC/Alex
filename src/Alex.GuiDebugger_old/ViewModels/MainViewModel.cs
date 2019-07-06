using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using Alex.GuiDebugger.Common;
using Alex.GuiDebugger.Models;
using Catel.MVVM;

namespace Alex.GuiDebugger.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		private ObservableCollection<GuiElementInfo> _elementTreeItems;
		private ObservableCollection<PropertyGridItem> _selectedGuiElementPropertyInfos;

		public ObservableCollection<GuiElementInfo> ElementTreeItems { get; set; }

		public ObservableCollection<PropertyGridItem> SelectedGuiElementPropertyInfos { get; set; }

		public MainViewModel()
		{
			ElementTreeItems = new ObservableCollection<GuiElementInfo>();
			SelectedGuiElementPropertyInfos = new ObservableCollection<PropertyGridItem>();
		}

	}
}
