using Microsoft.Xna.Framework;
using RocketUI.Input;
using RocketUI.Input.Listeners;

namespace Alex.API.Input
{
    public class AlexMouseInputListenerFactory : AlexInputListenerFactoryBase<MouseInputListener>
    {
        protected override MouseInputListener CreateInstance(PlayerIndex playerIndex)
            => new MouseInputListener(playerIndex);

        protected override void RegisterMaps(MouseInputListener l)
        {
            l.ClearMap();
            
            l.RegisterMap(AlexInputCommand.LeftClick, MouseButton.Left);
            l.RegisterMap(AlexInputCommand.RightClick, MouseButton.Right);
            l.RegisterMap(AlexInputCommand.MiddleClick, MouseButton.Middle);
            //l.RegisterMap(InputCommand.HotBarSelectPrevious, MouseButton.ScrollDown);
            // l.RegisterMap(InputCommand.HotBarSelectNext, MouseButton.ScrollUp);
        }
    }
}