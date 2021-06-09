using System;
using System.Collections.Generic;
using Alex.Gui;
using RocketUI;

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
		public bool Visible { get; set; } = true;
		public MenuItem(MenuType type = MenuType.Button)
		{
			Type = type;
			Title = null;
			Children = new List<MenuItem>();
		}

		internal RocketElement[] BuildMenu(IMenuHolder menuHolder, BuildMode buildMode)
		{
			List<RocketElement> elements = new List<RocketElement>();

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
					StackMenuItem me = new StackMenuItem();
					me.ApplyModernStyle(true);
					
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

					if (Visible)
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