using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Alex.GuiDebugger.Common;
using Alex.GuiDebugger.Common.Services;
using Catel.IoC;
using EasyPipes;
using JKang.IpcServiceFramework;

namespace Alex.GuiDebugger.Services
{
    public class AlexPipeService : IAlexPipeService
    {
        public IGuiDebuggerService GuiDebuggerService { get; }

        private readonly IpcServiceClient<IGuiDebuggerService> _ipcServiceClient;

        public AlexPipeService()
        {
            _ipcServiceClient = new IpcServiceClientBuilder<IGuiDebuggerService>()
                                .UseNamedPipe(GuiDebuggerConstants.NamedPipeName)
                                .UseTcp(IPAddress.Loopback, GuiDebuggerConstants.TcpEndpointPort)
                                .Build();


            GuiDebuggerService = new GuiDebuggerServiceProxy(_ipcServiceClient);
        }

    }

    internal class GuiDebuggerServiceProxy : IGuiDebuggerService
    {
        private readonly IpcServiceClient<IGuiDebuggerService> _client;

        internal GuiDebuggerServiceProxy(IpcServiceClient<IGuiDebuggerService> client)
        {
            _client = client;
        }

        public Guid? TryGetElementUnderCursor()
        {
            return _client.InvokeAsync(x => x.TryGetElementUnderCursor()).GetAwaiter().GetResult();
        }

        public void HighlightGuiElement(Guid id)
        {
            _client.InvokeAsync(x => x.HighlightGuiElement(id)).Wait();
        }

        public void DisableHighlight()
        {
            _client.InvokeAsync(x => x.DisableHighlight()).Wait();
        }

        public GuiElementInfo[] GetAllGuiElementInfos()
        {
            return _client.InvokeAsync(x => x.GetAllGuiElementInfos()).Result;
        }

        public GuiElementPropertyInfo[] GetElementPropertyInfos(Guid id)
        {
            return _client.InvokeAsync(x => x.GetElementPropertyInfos(id)).Result;
        }

        public bool SetElementPropertyValue(Guid id, string propertyName, string propertyValue)
        {
            return _client.InvokeAsync(x => x.SetElementPropertyValue(id, propertyName, propertyValue)).Result;
        }

        public void EnableUIDebugging()
        {
            _client.InvokeAsync(x => x.EnableUIDebugging()).Wait();
        }

        public bool IsUIDebuggingEnabled()
        {
            return _client.InvokeAsync(x => x.IsUIDebuggingEnabled()).GetAwaiter().GetResult();
        }
    }
}
