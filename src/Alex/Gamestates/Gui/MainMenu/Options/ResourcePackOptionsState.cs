using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui;
using Alex.API.Gui.Elements.Controls;
using Alex.GameStates.Gui.Common;
using Microsoft.Xna.Framework;

namespace Alex.GameStates.Gui.MainMenu.Options
{
    public class ResourcePackOptionsState : OptionsStateBase
    {
        protected readonly GuiSelectionList ListContainer;

        public ResourcePackOptionsState()
        {
            TitleTranslationKey = "resourcePack.title";
            
            AddGuiElement(ListContainer = new GuiSelectionList()
            {
                BackgroundOverlay = new Color(Color.Black, 0.65f),
                //Y = Header.Height,
                //Width = 322,
                Anchor      = Alignment.Fill,
                ChildAnchor = Alignment.TopFill,
            });
            ListContainer.SelectedItemChanged += HandleSelectedItemChanged;
            ListContainer.Margin              =  new Thickness(0, Header.Height, 0, Footer.Height);
        }

        private void HandleSelectedItemChanged(object sender, GuiSelectionListItem item)
        {


        }

    }
}
