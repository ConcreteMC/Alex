#region Imports

using System;

#endregion

namespace Alex.Networking.Java.Events
{
	public class ConnectionConfirmedEventArgs : EventArgs
	{
		public NetConnection Connection { get; }
		internal ConnectionConfirmedEventArgs(NetConnection connection)
		{
			Connection = connection;
		}
	}
}
