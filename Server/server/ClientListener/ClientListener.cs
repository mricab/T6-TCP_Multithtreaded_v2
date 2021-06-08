using System;
using System.Net.Sockets;
using System.Threading;

namespace server
{

    public class ClientListener : IClientListener
    {
        //Properties
        private static bool Listen;
        private TcpListener Listener;
        private Thread ThreadListen;

        //Events
        public event EventHandler ClientListenerUp;
        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        //Methods
        public ClientListener(ref TcpListener Listener)
        {
            this.Listener = Listener;
        }

        public void Start()
        {
            Listen = true;
            ThreadListen = new Thread(new ThreadStart(Process));
            ThreadListen.Start();
            OnClientListenerUp();
        }

        public void Stop()
        {
            Listen = false;
            Listener.Stop();
            ThreadListen.Join(); // Blocks the calling thread until ThreadListen ends.
        }

        private void Process()
        {
            try
            {
                Listener.Start();
                Console.WriteLine("(Listener)\tListener up.");

                while (Listen)
                {
                    TcpClient client = Listener.AcceptTcpClient();
                    Console.WriteLine("(Listener)\tClient Accepted!");
                    OnClientConnected(client);
                }

            }
            catch (SocketException)
            {
                Console.WriteLine("(Listener)\tListener interrupted!");
            }
            catch (Exception e)
            {
                Console.WriteLine("(Listener)\tCan't accept conections.");
                Console.WriteLine(e.ToString());
            }
            finally
            {
                Listener.Stop();
            }
            Console.WriteLine("(Listener)\tListener down.");
        }

        // Dispachers
        protected virtual void OnClientConnected(TcpClient client)
        {
            if (ClientConnected != null)
            {
                ConnectionData connection = new ConnectionData(client);
                ClientConnected(this, new ClientConnectedEventArgs() { Connection = connection }) ;
            }
        }

        protected virtual void OnClientListenerUp()
        {
            if (ClientListenerUp != null)
            {
                ClientListenerUp(this, EventArgs.Empty);
            }
        }


    } // class ClientListener
}
