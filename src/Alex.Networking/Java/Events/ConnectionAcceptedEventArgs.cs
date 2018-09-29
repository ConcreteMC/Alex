#region Imports

using System;

#endregion

namespace Alex.Networking.Java.Events
{
    public class ConnectionAcceptedEventArgs : EventArgs
    {
        public NetConnection Connection { get; }
        internal ConnectionAcceptedEventArgs(NetConnection connection)
        {
            Connection = connection;
        }
    }
}
