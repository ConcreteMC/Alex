using System;
using Alex.Common.Data;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui.Elements.Hud
{
    public class TipPopupComponent : Container, IChatRecipient
    {
        private TextElement Tip { get; set; }
        private TextElement Popup { get; set; }
        
        private DateTime TipHideTime { get; set; } = DateTime.UtcNow;
        private DateTime PopupHideTime { get; set; } = DateTime.UtcNow;

        private bool DoUpdate { get; set; } = false;

        private static readonly Thickness TipMargin = new Thickness(0, 0, 0, 16);
        public TipPopupComponent()
        {
            Tip = new TextElement(true)
            {
                Anchor = Alignment.BottomCenter,
                BackgroundOverlay = Color.Black * 0.5f,
                Margin = TipMargin
            };
            
            Popup = new TextElement(true)
            {
                Anchor = Alignment.BottomCenter,
                BackgroundOverlay = Color.Black * 0.5f
               // Margin = new Thickness(0, 0, 0, -12)
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

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (!DoUpdate)
                return;

            if (DateTime.UtcNow >= PopupHideTime && Popup.IsVisible)
            {
                Popup.IsVisible = false;
                UpdateMargins();
            }
            
            if (DateTime.UtcNow >= TipHideTime && Tip.IsVisible)
            {
                Tip.IsVisible = false;
                UpdateMargins();
            }

            if (!Tip.IsVisible && !Popup.IsVisible)
                DoUpdate = false;
        }

        private void UpdateMargins()
        {
            if (Popup.IsVisible)
            {
                Tip.Margin = TipMargin;
            }
            else
            {
                Tip.Margin = Thickness.Zero;
            }
        }

        /// <inheritdoc />
        public void AddMessage(string message, MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Popup:
                    Popup.Text = message;
                    PopupHideTime = DateTime.UtcNow + TimeSpan.FromSeconds(3);
                    Popup.IsVisible = true;
                    
                    break;
                case MessageType.Tip:
                    Tip.Text = message;
                    TipHideTime = DateTime.UtcNow + TimeSpan.FromSeconds(3);
                    Tip.IsVisible = true;

                    break;
            }

            DoUpdate = true;
            UpdateMargins();
        }
    }
}