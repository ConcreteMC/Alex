#region Imports

using System;

#endregion

namespace Alex.Networking.Java.Events
{
	public sealed class ConnectionClosedEventArgs : EventArgs
	{
		public NetConnection Connection { get; }
		public bool Graceful { get; }

		internal ConnectionClosedEventArgs(NetConnection connection, bool requested)
		{
			Connection = connection;
			Graceful = requested;
		}
	}
}