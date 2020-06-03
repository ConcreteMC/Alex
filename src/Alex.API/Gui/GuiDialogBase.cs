using Alex.API.Gui.Elements.Layout;
using RocketUI;

namespace Alex.API.Gui
{
    public abstract class GuiDialogBase : GuiScreen
    {

        public bool ShowBackdrop { get; set; }

        protected GuiContainer ContentContainer;


        protected GuiDialogBase()
        {
            AddChild(ContentContainer = new GuiContainer()
            {
                Anchor = Alignment.MiddleCenter
            });
        }
        public virtual void OnClose()
        {
            
        }
    }
}
