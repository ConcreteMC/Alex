using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.Gui
{
    public class UiRoot : UiContainer
    {

        public UiRoot(int? width, int? height) : base(width, height) { }
        public UiRoot() : this(null, null) { }


    }
}
