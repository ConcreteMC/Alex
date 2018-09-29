#region Imports

using System;

#endregion

namespace Alex.Networking.Java.Events
{
    public sealed class IncomingConnectionEventArgs : EventArgs
    {
        internal IncomingConnection Connection { get; }
        public bool ConnectionAccepted { get; private set; }
        internal IncomingConnectionEventArgs(IncomingConnection connection)
        {
            Connection = connection;
            ConnectionAccepted = false;
        }

        public void Allow()
        {
            ConnectionAccepted = true;
        }

        public void Deny()
        {
            ConnectionAccepted = false;
        }
    }
}
