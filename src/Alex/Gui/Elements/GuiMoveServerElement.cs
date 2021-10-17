using Alex.Common.Gui.Elements;
using Alex.Gamestates.Multiplayer;
using NLog;
using RocketUI;
using System.Numerics;

namespace Alex.Gui.Elements
{
    public class GuiMoveServerElement : TextureElement
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MultiplayerConnectState));

        public AlexButton JoinServerButton { get; set; }
        public AlexButton MoveUpServerButton {  get; set; }
        public AlexButton MoveDownServerButton {  get; set;}
        public GuiMoveServerElement()
        {
            JoinServerButton = new AlexButton(">", new System.Action(
                () => Log.Info("You have pressed Join Server Button")),true)
            {
                //TranslationKey = "selectServer.select",
                Enabled = true,
                ParentElement = this,
                Anchor = Alignment.MiddleRight,
                Width = 10,
                Height = 10,
                MinHeight = 10,
                MinWidth = 10,
                MaxWidth = 10,
                MaxHeight = 10,
                Margin = Thickness.One,
            };

            MoveUpServerButton = new AlexButton("^", () => Log.Info("You pressed Up server button"), true)
            {
                //TranslationKey = "selectServer.up",
                Enabled = true,
                ParentElement = this,
                Anchor = Alignment.TopCenter,
                Width = 20,
                Height = 10,
                MinHeight = 10,
                MinWidth = 20,
                MaxWidth = 20,
                MaxHeight = 10,
                Margin = Thickness.One,
            };
            
            MoveDownServerButton = new AlexButton("v", () => Log.Info("You pressed Down server button"), true)
            {
                //TranslationKey = "selectServer.down",
                Enabled = true,
                ParentElement = this,
                Anchor = Alignment.BottomCenter,
                Width = 20,
                Height = 10,
                MinHeight = 10,
                MinWidth = 20,
                MaxWidth = 20,
                MaxHeight = 10,
                Margin = Thickness.One,
            };

            Children.Add(JoinServerButton);
            Children.Add(MoveUpServerButton);
            Children.Add(MoveDownServerButton);
        }

      
    }
}
