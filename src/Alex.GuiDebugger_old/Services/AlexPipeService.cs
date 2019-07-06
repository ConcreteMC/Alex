using System;
using System.Collections.Generic;
using System.Text;
using Alex.GuiDebugger.Common;
using Alex.GuiDebugger.Common.Services;
using Catel.IoC;
using EasyPipes;

namespace Alex.GuiDebugger.Services
{
    public class AlexPipeService : IAlexPipeService
    {
        private Client _server;
        
        public IGuiDebuggerService GuiDebuggerService { get; }

        public AlexPipeService()
        {
            _server = new Client(GuiDebuggerConstants.NamedPipeName);

            GuiDebuggerService = _server.GetServiceProxy<IGuiDebuggerService>();
        }

    }
}
