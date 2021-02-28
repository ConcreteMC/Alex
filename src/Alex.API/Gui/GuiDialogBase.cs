using Alex.API.Gui.Elements.Layout;
using RocketUI;

namespace Alex.API.Gui
{
    public abstract class DialogBase : Screen
    {

        public bool ShowBackdrop { get; set; }

        protected Container ContentContainer;


        protected DialogBase()
        {
            AddChild(ContentContainer = new Container()
            {
                Anchor = Alignment.MiddleCenter
            });
        }
        public virtual void OnClose()
        {
            
        }
    }
}
