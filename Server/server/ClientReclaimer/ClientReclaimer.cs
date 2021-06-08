using System;
using System.Collections.Generic;
using System.Threading;

namespace server
{
    public class ClientReclaimer : IClientReclaimer
    {
        //Properties
        private static bool Reclaim = false;
        private Dictionary<int, ClientData> Clients;
        private List<int> ClientsToReclaim;
        private Thread ThreadReclaim;

        //Events
        public event EventHandler ClientReclaimerUp;
        public event EventHandler<ClientConnectionReclaimedEventArgs> ClientConnectionReclaimed;

        //Methods
        public ClientReclaimer(ref Dictionary<int, ClientData> Clients)
        {
            this.Clients = Clients;
        }

        public void Start()
        {
            Reclaim = true;
            ThreadReclaim = new Thread(new ThreadStart(Process));
            ThreadReclaim.Start();
            OnClientReclaimerUp();
        }

        public void Stop()
        {
            Reclaim = false;
            ThreadReclaim.Join(); // Blocks the calling thread until ThreadReclaim ends.
        }

        public void Process()
        {
            Console.WriteLine("(Reclaimer)\tReclaimer up.");

            while (Reclaim)
            {
                ClientsToReclaim = new List<int>();

                foreach (KeyValuePair<int, ClientData> Client in Clients)
                {
                    if(!Client.Value.Handler.Alive)
                    {
                        ClientsToReclaim.Add(Client.Key);
                    }
                }
                foreach(int key in ClientsToReclaim)
                {
                    Console.WriteLine("(Reclaimer)\tClaiming client#{0}.", key);
                    OnClientConnectionReclaimed(key);
                }

                Thread.Sleep(2000); // 2s
            }
            Console.WriteLine("(Reclaimer)\tReclaimer down.");
        }

        // Dispachers
        protected virtual void OnClientConnectionReclaimed(int key)
        {
            if (ClientConnectionReclaimed != null)
            {
                ClientConnectionReclaimed(this, new ClientConnectionReclaimedEventArgs() { Key = key });
            }
        }

        protected virtual void OnClientReclaimerUp()
        {
            if (ClientReclaimerUp != null)
            {
                ClientReclaimerUp(this, EventArgs.Empty);
            }
        }
    }
}
