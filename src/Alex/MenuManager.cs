using System;
using System.Collections.Generic;
using Alex.API.Gui;
using Alex.API.Gui.Elements.Controls;

namespace Alex
{
	public enum GameMenu
	{
		MainMenu,
		Single
	}

	public interface IMenuHolder
	{
		public void ShowMenu(MenuItem menu);

		public bool GoBack();
	}

	public enum MenuType
	{
		Menu,
		SubMenu,
		Button
	}

	public enum BuildMode
	{
		Parent,
		Children
	}
	
	public class MenuItem
	{
		public string Title { get; set; }
		public bool IsTranslatable { get; set; } = false;
		
		public List<MenuItem> Children { get; set; }
		
		public EventHandler<MenuItemClickedEventArgs> OnClick;
		
		public MenuType Type { get; }
		public MenuItem(MenuType type = MenuType.Button)
		{
			Type = type;
			Title = null;
			Children = new List<MenuItem>();
		}

		internal IGuiElement[] BuildMenu(IMenuHolder menuHolder, BuildMode buildMode)
		{
			List<IGuiElement> elements = new List<IGuiElement>();

			if (buildMode == BuildMode.Children || Type == MenuType.Menu)
			{
				if (Children.Count > 0)
				{
					foreach (var child in Children)
					{
						elements.AddRange(child.BuildMenu(menuHolder, BuildMode.Parent));
					}
				}
			}
			else if (buildMode == BuildMode.Parent)
			{
				if (Type == MenuType.SubMenu || Type == MenuType.Button)
				{
					GuiStackMenuItem me = new GuiStackMenuItem();
					me.Text = Title;

					if (IsTranslatable)
					{
						me.TranslationKey = Title;
					}

					me.Action = () =>
					{
						if (Type == MenuType.SubMenu)
						{
							menuHolder.ShowMenu(this);
						}
						else
						{
							OnClick?.Invoke(this, new MenuItemClickedEventArgs());
						}
					};

					elements.Add(me);
				}
			}

			return elements.ToArray();
		}
	}

	public class MenuItemClickedEventArgs : EventArgs
	{
		public MenuItemClickedEventArgs()
		{
			
		}
	}
}