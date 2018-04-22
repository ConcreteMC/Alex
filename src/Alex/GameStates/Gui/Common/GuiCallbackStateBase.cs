using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using System;

namespace Alex.GameStates.Gui.Common
{
    public class GuiCallbackStateBase<TResponse> : GuiStateBase
    {
        private readonly Action<TResponse> _callbackAction;

        public GuiCallbackStateBase(Action<TResponse> callbackAction) : base()
        {
            _callbackAction = callbackAction;
        }

        protected void InvokeCallback(TResponse response)
        {
            Alex.GameStateManager.Back();
            _callbackAction?.Invoke(response);
        }
    }
}
