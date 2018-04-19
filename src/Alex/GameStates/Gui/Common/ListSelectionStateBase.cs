using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Utils;
using Alex.GameStates.Gui.Elements;
using Microsoft.Xna.Framework;

namespace Alex.GameStates.Gui.Common
{
    public class ListSelectionStateBase<TGuiListItemContainer> : GuiStateBase where TGuiListItemContainer : GuiContainer
    {
        protected TGuiListItemContainer[] Items => _items.ToArray();
        private List<TGuiListItemContainer> _items { get; } = new List<TGuiListItemContainer>();

        private readonly GuiStackContainer _listContainer;

        public ListSelectionStateBase() : base()
        {
	        Gui.AddChild(_listContainer = new GuiStackContainer()
            {
				BackgroundOverlayColor = new Color(Color.Black, 0.35f),
                //Y = Header.Height,
                //Width = 322,
	            Anchor = Alignment.Fill,
				ChildAnchor = Alignment.TopCenter,
            });
			_listContainer.Margin = new Thickness(0, Header.Height, 0, Footer.Height);
        }

	    protected override void OnUpdate(GameTime gameTime)
	    {
		    base.OnUpdate(gameTime);
	    }

	    public void AddItem(TGuiListItemContainer item)
        {
            _items.Add(item);
            _listContainer.AddChild(item);
        }
        
        public void RemoveItem(TGuiListItemContainer item)
        {
            _listContainer.RemoveChild(item);
            _items.Remove(item);
        }
    }
}
