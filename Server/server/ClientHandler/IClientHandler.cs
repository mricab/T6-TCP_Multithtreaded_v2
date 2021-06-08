using System;
namespace server
{
    public class IClientHandler
    {
        event EventHandler<ClientMessageReceivedEventArgs> ClientMessageReceived; //Event and Delegate
        event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected; //Event and Delegate
    }
}
