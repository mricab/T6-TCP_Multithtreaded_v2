using System;
namespace server
{
    public class ClientDisconnectedEventArgs : EventArgs
    {
        public ConnectionData Connection { get; set; }
    }
}
