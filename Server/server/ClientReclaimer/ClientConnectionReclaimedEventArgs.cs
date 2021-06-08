using System;

namespace server
{
    public class ClientConnectionReclaimedEventArgs : EventArgs
    {
        public int Key { get; set; }

    }
}
