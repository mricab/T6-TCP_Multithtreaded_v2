using System;

namespace client
{
    class MainClass
    {

        private static int ServerPort;
        private static Client client;

        public static void Main(String[] args)
        {
            if (args.Length > 0 && ProgramHelperFunctions.GetPort(args[0], out ServerPort))
            {
                client = new Client(ServerPort);
                client.Start();
            }
            else
            {
                Console.WriteLine("Server port expected as first and only argument (Invalid port or no port supplied).");
            }

        } // Main()

    } //MainClass()
}
