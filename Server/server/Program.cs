using System;
using System.Threading;

namespace server
{
    class MainClass
    {
        // Properties
        private static int ListeningPort;
        private static Server server;

        // Methods
        public static void Main(String[] args)
        {
            if (args.Length > 0 && ProgramHelperFunctions.GetPort(args[0], out ListeningPort))
            {
                server = new Server(ListeningPort);
                server.Start();
                Thread.Sleep(60000);
                server.Stop();
            }
            else
            {
                Console.WriteLine("Server port expected as first and only argument (Invalid port or no port supplied).");
            }
        }
    }
}
