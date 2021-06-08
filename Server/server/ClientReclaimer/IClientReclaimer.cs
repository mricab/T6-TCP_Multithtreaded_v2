using System;
namespace server
{
    public class IClientReclaimer
    {
        event EventHandler ClientReclaimerUp; //Event and Delegate
        event EventHandler<ClientConnectionReclaimedEventArgs> ClientConnectionReclaimed; //Event and Delegate
    }
}
