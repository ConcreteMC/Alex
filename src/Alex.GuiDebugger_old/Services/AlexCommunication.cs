using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using Alex.GuiDebugger.Common;

namespace Alex.GuiDebugger.Services
{
	public class AlexCommunication
	{
		private NamedPipeServerStream _namedPipeServerStream;

		public AlexCommunication()
		{
			_namedPipeServerStream = new NamedPipeServerStream(GuiDebuggerConstants.NamedPipeName, PipeDirection.InOut);
			
			_namedPipeServerStream.BeginWaitForConnection(OnClientConnect, null);
		}

		private void OnClientConnect(IAsyncResult ar)
		{
			_namedPipeServerStream.EndWaitForConnection(ar);
		}
	}
}
