using System;

namespace Alex.Gamestates.Common
{
    public class GuiCallbackStateBase<TResponse> : GuiMenuStateBase
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
