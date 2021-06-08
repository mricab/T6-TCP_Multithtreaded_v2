using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;

namespace client
{
    public class ServerHandler : IServerHandler
    {
        // Properties
        private static bool Handle;
        private TcpClient Client;
        private Thread ClientThread;
        private bool quit = false;

        // CancellableReadLine CancellationToken
        CancellationTokenSource source = new CancellationTokenSource();
        CancellationToken ReadLineToken;

        // Events
        public event EventHandler UserQuitted;

        // Methods
        public ServerHandler(TcpClient Client)
        {
            this.Client = Client;
        }

        public void Start()
        {
            Handle = true;
            ReadLineToken = source.Token;
            ClientThread = new Thread(new ThreadStart(Process));
            ClientThread.Start();
        }

        public void Stop()
        {
            Handle = false;
            source.Cancel();
        }

        private void Process()
        {
            Console.WriteLine("(Handler)\tHandler up.");

            String DataToSend = "";
            NetworkStream networkStream = Client.GetStream();
            Console.WriteLine();

            while (Handle && DataToSend != "quit")
            {
                try
                {
                    // Sending message
                    byte[] sendBytes;
                    bool receive = GetInput(out sendBytes);
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                    if (!receive) break;

                    // Receiving response
                    byte[] Bytes = new byte[Client.ReceiveBufferSize];
                    int BytesRead = networkStream.Read(Bytes, 0, (int)Client.ReceiveBufferSize);
                    GetResponse(Bytes, BytesRead);

                    // Quitting
                    if (quit)
                    {
                        Console.WriteLine();
                        OnUserQuitted();
                    }
                }
                catch (IOException) // Timeout
                {
                    Console.WriteLine("\n(Handler)\tServer timed out!");
                    break;
                }
                catch (SocketException)
                {
                    Console.WriteLine("\n(Handler)\tConection broken!");
                    break;
                }
            }

            networkStream.Close();
            Client.Close();
            Console.WriteLine("(Handler)\tHandler stopped.");
        }

        private void GetResponse(byte[] Bytes, int BytesRead)
        {
            if (BytesRead > 0)
            {
                string response = Encoding.ASCII.GetString(Bytes, 0, BytesRead);
                if (response != "keep") Console.WriteLine("(Handler)\tServer: {0}", response);
            }
            else
            {
                // Other end not responding (BytesRead == 0)
                throw new System.Net.Sockets.SocketException();
            }
        }

        private bool GetInput(out byte[] Bytes)
        {
            Bytes = new byte[0];
            String Data = String.Empty;
            Console.Write("(Handler)\tMessage> ");
            Data = ProgramHelperFunctions.CancellableReadLine(ReadLineToken);
            while (Handle && (Data.Length == 0 || Data == "keep"))
            {
                Console.Write("(Handler)\tInvalid input. ");
                Console.Write("(Handler)\tMessage> ");
                Data = Console.ReadLine();
            }
            Bytes = Encoding.ASCII.GetBytes(Data);
            if (Data == "quit")
            {
                quit = true;
            }
            if (Data.Length == 0)
            {
                return false;
            }
            return true;
        }

        // Dispachers
        protected virtual void OnUserQuitted()
        {
            if(UserQuitted!=null)
            {
                UserQuitted(this, EventArgs.Empty);
            }
        }

    }
}
