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
        private string _title;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                if(_headerTitle != null)
                    _headerTitle.Text = value;
            }
        }

        private GuiContainer _header;
        private GuiTextElement _headerTitle;
        private GuiBeaconButton _headerBackButton;

        protected TGuiListItemContainer[] Items => _items.ToArray();
        private List<TGuiListItemContainer> _items { get; } = new List<TGuiListItemContainer>();

        private readonly GuiStackContainer _listContainer;

        public ListSelectionStateBase() : base()
        {
            Gui.AddChild(_header = new GuiContainer()
            {
                Height = 42,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            });

            _header.AddChild(_headerBackButton = new GuiBackButton()
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
            });

            _header.AddChild(_headerTitle = new GuiTextElement()
            {
                Text = Title,
                TextColor = TextColor.White,

                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
            });

            Gui.AddChild(_listContainer = new GuiStackContainer()
            {
                BackgroundOverlayColor = new Color(Color.Black, 0.5f),
                LayoutOffsetY = 42,
                Width = 192,
                VerticalAlignment = VerticalAlignment.Top,
                //HorizontalAlignment = HorizontalAlignment.Stretch,
                
                //VerticalContentAlignment = VerticalAlignment.Top,
                //HorizontalContentAlignment = HorizontalAlignment.Stretch,
            });
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
