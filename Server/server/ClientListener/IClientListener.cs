using System;
namespace server
{
    public interface  IClientListener
    {
        event EventHandler ClientListenerUp; //Event and Delegate
        event EventHandler<ClientConnectedEventArgs> ClientConnected; //Event and Delegate
    }
}
