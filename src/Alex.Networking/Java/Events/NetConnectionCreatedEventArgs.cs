#region Imports

using System;

#endregion

namespace Alex.Networking.Java.Events
{
	public class NetConnectionCreatedEventArgs : EventArgs
	{
		public NetConnection Connection { get; }
		internal NetConnectionCreatedEventArgs(NetConnection connection)
		{
			Connection = connection;
		}
	}
}
