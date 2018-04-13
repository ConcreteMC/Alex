using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Gui.Rendering;

namespace Alex.API.Gui.Elements.Icons
{
    public class GuiConnectionPingIcon : GuiImage
    {
        public override int Width => 10;
        public override int Height => 8;

        private GuiTextures _offlineState = GuiTextures.ServerPing0;
        private GuiTextures[] _qualityStates = new[]
        {
            GuiTextures.ServerPing1,
            GuiTextures.ServerPing2,
            GuiTextures.ServerPing3,
            GuiTextures.ServerPing4,
            GuiTextures.ServerPing5,
        };

        private GuiTextures[] _connectingStates = new[]
        {
            GuiTextures.ServerPingPending1,
            GuiTextures.ServerPingPending2,
            GuiTextures.ServerPingPending3,
            GuiTextures.ServerPingPending4,
            GuiTextures.ServerPingPending5,
        };

        private int _width1;

        public GuiConnectionPingIcon() : base(GuiTextures.ServerPing0)
        {
        }
    }
}
