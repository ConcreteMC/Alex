using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui;
using Alex.API.Gui.Elements.Controls;
using Alex.GameStates.Gui.Common;

namespace Alex.GameStates.Gui.MainMenu
{
    public class MultiplayerConnectState : GuiStateBase
    {

        public MultiplayerConnectState()
        {
            Title = "Connect to Server";
            Gui.AddChild(new GuiTextInput()
            {
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

    }
}
