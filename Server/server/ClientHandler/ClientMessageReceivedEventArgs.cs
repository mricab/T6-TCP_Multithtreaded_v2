using System;
namespace server
{
    public class ClientMessageReceivedEventArgs : EventArgs
    {
        public String Message { get; set; }
        public int ClientId { get; set; }
    }
}
