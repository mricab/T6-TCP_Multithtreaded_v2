using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace server
{
    public class ClientHandler : IClientHandler
    {
        //Properties
        private static bool Handle;
        private int Id;
        private TcpClient Client;
        private Thread ClientThread;

        //Events
        public event EventHandler<ClientMessageReceivedEventArgs> ClientMessageReceived;
        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;

        //Methods
        public ClientHandler(int Id, TcpClient Client)
        {
            this.Id = Id;
            this.Client = Client;
        }

        public void Start()
        {
            Handle = true;
            ClientThread = new Thread(new ThreadStart(Process));
            ClientThread.Start();
        }

        public void Stop()
        {
            Handle = false;
            ClientThread.Join();
            //if (ClientThread != null && ClientThread.IsAlive) ClientThread.Join();
        }

        public bool Alive
        {
            get
            {
                return (ClientThread != null && ClientThread.IsAlive);
            }
        }

        private void Process()
        {
            string data = null;
            byte[] bytes; // Incoming data buffer.

            if (Client != null)
            {
                Console.WriteLine("(Handler#" + Id + ")\tStarting thread for client#" + Id + " from {0}:{1}.", ((IPEndPoint)Client.Client.RemoteEndPoint).Address, ((IPEndPoint)Client.Client.RemoteEndPoint).Port);
                Client.ReceiveTimeout = 10000; // 10s
                Client.SendTimeout = 10000; // 10s
                NetworkStream networkStream = Client.GetStream();

                while (Handle)
                {
                    bytes = new byte[Client.ReceiveBufferSize]; // 8192 Bytes
                    try
                    {
                        int BytesRead = networkStream.Read(bytes, 0, (int)Client.ReceiveBufferSize);

                        if (BytesRead > 0)
                        {
                            // Receiving
                            data = Encoding.ASCII.GetString(bytes, 0, BytesRead);
                            if (data != "keep") OnClientMessageReceived(data);
                            // Responding (Echoing)
                            byte[] sendBytes = Encoding.ASCII.GetBytes(data);
                            networkStream.Write(sendBytes, 0, sendBytes.Length);
                            // Quit
                            if (data == "quit")
                            {
                                Console.WriteLine("(Handler#" + Id + ")\tClient left.");
                                OnClientDisconnected();
                            }
                        }
                        else 
                        {
                            // Other end closed not responding (BytesRead == 0)
                            throw new System.Net.Sockets.SocketException();
                        }
                    }
                    catch (IOException)  // Timeout
                    {
                        Console.WriteLine("(Handler#" + Id + ")\tClient timed out!");
                        Handle = false;
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("(Handler#" + Id + ")\tConection broken!");
                        break;
                    }
                    Thread.Sleep(200); // 0.2s
                }

                networkStream.Close();
                Client.Close();
                Console.WriteLine("(Handler#" + Id + ")\tClient stopped.");
            }
        }

        // Dispachers
        protected virtual void OnClientMessageReceived(String message)
        {
            if (ClientMessageReceived != null)
            {
                ClientMessageReceived(this, new ClientMessageReceivedEventArgs() { Message = message, ClientId = Id });;
            }
        }

        protected virtual void OnClientDisconnected()
        {
            if (ClientDisconnected != null)
            {
                ConnectionData connection = new ConnectionData(this.Client, this.Id);
                ClientDisconnected(this, new ClientDisconnectedEventArgs() { Connection = connection});
            }
        }

    }
}
