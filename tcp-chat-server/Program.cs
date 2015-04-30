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
        private static Dictionary<String, String> commands = new Dictionary<String, String>();

        static void Main(string[] args)
        {

            String ip   = "127.0.0.1";
            int port    = 7777;

            initCommands();
            //print info in console
            printHeader();

            IPAddress ipAddress = IPAddress.Parse(ip);
            Server server = new Server(ipAddress, port);

            if (!server.Start()) {
                Console.WriteLine("Server cannot be started.");
                return; 
            }

            Console.WriteLine("Server is listening on port " + port);           
           
            String command;
            while( (command = Console.ReadLine()) != "exit")
            {
                switch (command)
                {
                    case "help": listCommands(); break;
                    default: Console.WriteLine("Invalid command. Type \"help\" to list all available commands."); break;
                }
            }

            server.Stop();
            Console.ReadKey();
        }

        /**
         * Prints informational text to console
         */ 
        static void printHeader()
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("ChatServer :: Ales Jiranek :: (c) 2015");
            Console.WriteLine("Server is starting up.");
            Console.WriteLine("Type \"help\" to see all available commands,\n     \"exit\" to quit ");
            Console.WriteLine("=================================================");
        }


        /**
         * Prints all available commands
         */ 
        static void listCommands()
        {
            foreach(var command in commands)
            {
                Console.WriteLine(command.Key + " - " + command.Value);
            }
        }


        /**
         * Initialises dictionary of available commands 
         */
        static void initCommands()
        {
            commands.Add("help","Shows list of all available commands.");
            commands.Add("exit","Exits chat server.");
        }
    }
}
