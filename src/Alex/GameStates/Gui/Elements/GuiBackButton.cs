using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui.Elements.Controls;

namespace Alex.GameStates.Gui.Elements
{
    public class GuiBackButton : GuiBeaconButton
    {
        public GuiBackButton() : base("Back", () => Alex.Instance.GameStateManager.Back())
        {

        }
    }
}
