using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.API.Gui
{
    public interface IGuiScreen : IGuiElement3D
    {

        void RegisterElement(IGuiElement3D element);
        void UnregisterElement(IGuiElement3D element);

    }
}
