using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using Alex.GuiDebugger.Models;

namespace Alex.GuiDebugger.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		private ObservableCollection<GuiElementItem> _elementTreeItems;

		public ObservableCollection<GuiElementItem> ElementTreeItems
		{
			get => _elementTreeItems;
			set
			{
				if (Equals(value, _elementTreeItems)) return;
				_elementTreeItems = value;
				OnPropertyChanged();
			}
		}
		
		public MainViewModel()
		{
			if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
			{
				ElementTreeItems = new ObservableCollection<GuiElementItem>();
				ElementTreeItems.Add(new GuiElementItem()
				{
					Type = "GuiScreen",
					Children =
					{
						new GuiElementItem()
						{
							Type = "GuiMultiStackContainer",
							Children =
							{
								new GuiElementItem()
								{
									Type = "GuiText"
								}
							}
						}
					}
				});
			}
		}

	}
}
