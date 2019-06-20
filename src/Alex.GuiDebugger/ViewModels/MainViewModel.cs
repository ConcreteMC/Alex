using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using Alex.GuiDebugger.Common;
using Alex.GuiDebugger.Models;

namespace Alex.GuiDebugger.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		private ObservableCollection<GuiElementInfo> _elementTreeItems;
		private ObservableCollection<PropertyGridItem> _selectedGuiElementPropertyInfos;

		public ObservableCollection<GuiElementInfo> ElementTreeItems
		{
			get => _elementTreeItems;
			set
			{
				if (Equals(value, _elementTreeItems)) return;
				_elementTreeItems = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<PropertyGridItem> SelectedGuiElementPropertyInfos
		{
			get => _selectedGuiElementPropertyInfos;
			set
			{
				if (Equals(value, _selectedGuiElementPropertyInfos)) return;
				_selectedGuiElementPropertyInfos = value;
				OnPropertyChanged();
			}
		}

		public MainViewModel()
		{
			ElementTreeItems = new ObservableCollection<GuiElementInfo>();
			SelectedGuiElementPropertyInfos = new ObservableCollection<PropertyGridItem>();
			//if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
			//{
			//	ElementTreeItems = new ObservableCollection<GuiElementInfo>();
			//	ElementTreeItems.Add(new GuiElementInfo()
			//	{
			//		ElementType = "GuiScreen",
			//		ChildElements = 
			//		{
			//			new GuiElementInfo()
			//			{
			//				ElementType = "GuiMultiStackContainer",
			//				ChildElements = 
			//				{
			//					new GuiElementInfo()
			//					{
			//						ElementType = "GuiText"
			//					}
			//				}
			//			}
			//		}
			//	});
			//}
		}

	}
}
