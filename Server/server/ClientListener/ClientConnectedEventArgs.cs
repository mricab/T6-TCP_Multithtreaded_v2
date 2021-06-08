using System;
namespace server
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public ConnectionData Connection { get; set; }
    }
}