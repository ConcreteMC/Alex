using System;
using Alex.API.Events;
using Alex.API.Events.World;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;
using MiNET;
using RocketUI;

namespace Alex.Gui.Elements
{
    public class TipPopupComponent : GuiContainer
    {
        private GuiTextElement Tip { get; set; }
        private GuiTextElement Popup { get; set; }
        
        private DateTime TipHideTime { get; set; } = DateTime.UtcNow;
        private DateTime PopupHideTime { get; set; } = DateTime.UtcNow;

        private bool DoUpdate { get; set; } = false;

        public TipPopupComponent()
        {
            Tip = new GuiTextElement()
            {
                Anchor = Alignment.BottomCenter,
                Margin = new Thickness(0, 0, 0, 5)
            };
            
            Popup = new GuiTextElement()
            {
                Anchor = Alignment.BottomCenter
            };
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);

            Tip.Font = renderer.Font;
            Popup.Font = renderer.Font;
            
            AddChild(Tip);
            AddChild(Popup);
        }

        [EventHandler]
        public void OnChatMessage(ChatMessageReceivedEvent e)
        {
            if (e.IsChat())
                return;

            switch (e.Type)
            {
                case MessageType.Popup:
                    Popup.Text = e.ChatObject.RawMessage;
                    PopupHideTime = DateTime.UtcNow + TimeSpan.FromSeconds(3);
                    Popup.IsVisible = true;
                    
                    break;
                case MessageType.Tip:
                    Tip.Text = e.ChatObject.RawMessage;
                    TipHideTime = DateTime.UtcNow + TimeSpan.FromSeconds(3);
                    Tip.IsVisible = true;
                    
                    break;
            }
            
            DoUpdate = true;
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (!DoUpdate)
                return;

            if (DateTime.UtcNow >= PopupHideTime && Popup.IsVisible)
            {
                Popup.IsVisible = false;
            }
            
            if (DateTime.UtcNow >= TipHideTime && Tip.IsVisible)
            {
                Tip.IsVisible = false;
            }

            if (!Tip.IsVisible && !Popup.IsVisible)
                DoUpdate = false;
        }
    }
}