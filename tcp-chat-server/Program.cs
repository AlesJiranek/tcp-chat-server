using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;


namespace tcp_chat_server
{
    class Program
    {
        static void Main(string[] args)
        {
            String ip   = "127.0.0.1";
            int port    = 7777;

            IPAddress ipAddress = IPAddress.Parse(ip);

            Server server = new Server(ipAddress, port);

            if (!server.Start()) {
                Console.WriteLine("Server cannot be started.");
                return;
            }

            Console.WriteLine("Server is listening on port " + port);
            server.Listen();
           
        }
    }
}
