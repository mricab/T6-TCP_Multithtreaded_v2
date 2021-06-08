using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace server
{
    public class Server
    {
        // Properties
        private Dictionary<int, ClientData> Clients;
        private TcpListener ListenerSocket;
        private static ClientListener Listener;
        private static ClientReclaimer Reclaimer;
        private int Connections;
        private int Disconnections;
        private bool ListenerUp;
        private bool ReclaimerUp;

        // Methods
        public Server(int ListeningPort)
        {
            try
            {
                Clients = new Dictionary<int, ClientData>();
                Reclaimer = new ClientReclaimer(ref Clients);
                ListenerSocket = new TcpListener(IPAddress.Loopback, ListeningPort);
                Listener = new ClientListener(ref ListenerSocket);
                Connections = 0;
                Disconnections = 0;
                ListenerUp = false;
                ReclaimerUp = false;
            }
            catch(Exception e)
            {
                Console.WriteLine("(Server)\tServer couldn't be started.");
                Console.WriteLine(e.ToString());
            }
        }

        public void Start()
        {
            Console.WriteLine("\n(Server)\tStarting server on port {0}.", ((IPEndPoint)ListenerSocket.LocalEndpoint).Port );
            // Events subscriptions
            Listener.ClientListenerUp += OnClientListenerUp;
            Listener.ClientConnected += OnClientConnected;
            Reclaimer.ClientReclaimerUp += OnClientReclaimerUp;
            Reclaimer.ClientConnectionReclaimed += OnClientConnectionReclaimed;
            // Server main processes
            Reclaimer.Start(); 
            Listener.Start();
            // Closing message
            Thread.Sleep(1000); // 1s
            if(ListenerUp && ReclaimerUp) Console.WriteLine("(Server)\tServer up.");
        }

        public void Stop()
        {
            Console.WriteLine("(Server)\tStopping server.");
            Listener.Stop();
            Reclaimer.Stop();
            CloseClients(); // Closing all remaining connections
            Console.WriteLine("(Server)\tServer down.");
        }

        public void Restart()
        {
            Console.WriteLine("(Server)\tRestarting server on port {0}.", ((IPEndPoint)ListenerSocket.LocalEndpoint).Port);
            Console.WriteLine("(Server)\tServer up.");
        }

        public void Pause()
        {
            Console.WriteLine("(Server)\tPausing server.");
            Console.WriteLine("(Server)\tServer paused.");
        }

        private void StartClient(int key)
        {
            ClientData clientData;
            Clients.TryGetValue(Connections, out clientData);
            clientData.Handler.ClientMessageReceived += OnClientMessageReceived; //Event Subscription
            clientData.Handler.ClientDisconnected += OnClientDisconnected; //Event Subscription
            clientData.Handler.Start();
        }

        private void StopClient(int key)
        {
            ClientData clientData;
            Clients.TryGetValue(Connections, out clientData);
            clientData.Handler.Stop();
        }

        private void CloseClients()
        {
            List<int> ClientsToRemove = new List<int>();

            Console.WriteLine("(Server)\tRemoving remaining clients.");
            foreach (KeyValuePair<int, ClientData> Client in Clients)
            {
                Client.Value.Handler.Stop();
                ClientsToRemove.Add(Client.Key);
            }
            foreach (int key in ClientsToRemove)
            {
                Console.WriteLine("(Server)\tRemoving client#{0}.", key);
            }
            Console.WriteLine("(Server)\tNo clients left.");
        }

        private void OnClientListenerUp(object source, EventArgs e)
        {
            ListenerUp = true;
        }

        private void OnClientReclaimerUp(object source, EventArgs e)
        {
            ReclaimerUp = true;
        }

        private void OnClientConnected(object source, ClientConnectedEventArgs e)
        {
            ++Connections;
            Console.WriteLine("(Server)\tAdding client#{0}.", Connections);
            ClientData clientData = new ClientData(e.Connection.Client, new ClientHandler(Connections, e.Connection.Client));
            Clients.Add(Connections, clientData);
            StartClient(Connections);
        }

        private void OnClientDisconnected(object source, ClientDisconnectedEventArgs e)
        {
            ++Disconnections;
            Console.WriteLine("(Server)\tRemoving client#{0}.", e.Connection.Id);
            StopClient(e.Connection.Id.Value);
            Clients.Remove(e.Connection.Id.Value);
        }

        private void OnClientMessageReceived(object source, ClientMessageReceivedEventArgs e)
        {
            Console.WriteLine("(Server)\tClient#{0}: {1}", e.ClientId, e.Message);
        }

        private void OnClientConnectionReclaimed(object source, ClientConnectionReclaimedEventArgs e)
        {
            ++Disconnections;
            Console.WriteLine("(Server)\tRemoving client#{0} due to timeout or broken connection.", e.Key);
            // No need to StopClient(), ClientHandler stops itself.
            Clients.Remove(e.Key);
        }
    }
}
