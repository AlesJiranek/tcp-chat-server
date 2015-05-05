using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;


namespace tcp_chat_server
{
    class Program
    {
        private static Dictionary<String, String> commands = new Dictionary<String, String>();

        /**
         * Main program function
         */
        static void Main(string[] args)
        {
            IPAddress ipAddress;
            int port;

            if (!IPAddress.TryParse(ConfigurationManager.AppSettings["ip"], out ipAddress))
            {
                Console.WriteLine("Invalid ip address in config file.");
                Console.ReadKey();
                return;
            }

            if(!int.TryParse(ConfigurationManager.AppSettings["port"], out port))
            {
                Console.WriteLine("Invalid port in config file.");
                return;
            }

            InitCommands();
            //print info in console
            PrintHeader();

            Server server = new Server(ipAddress, port);

            if (!server.Start())
            {
                Console.WriteLine("Server cannot be started.");
                return;
            }

            Console.WriteLine("Server is listening on port " + port);

            Console.Write("Server:$ ");
            String command;
            while ((command = Console.ReadLine()) != "exit")
            {
                switch (command)
                {
                    case "help": ListCommands(); break;
                    case "rooms": server.ListRooms(); break;
                    default: Console.WriteLine("Invalid command. Type \"help\" to list all available commands."); break;
                }
                Console.Write("Server:$ ");
            }

            server.Stop();
        }


        /**
         * Prints informational text to console
         */
        static void PrintHeader()
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
        static void ListCommands()
        {
            foreach (var command in commands)
            {
                Console.WriteLine(command.Key + "\t" + command.Value);
            }
        }


        /**
         * Initialises dictionary of available commands 
         */
        static void InitCommands()
        {
            commands.Add("help", "Shows list of all available commands.");
            commands.Add("exit", "Exits chat server.");
            commands.Add("rooms", "Shows list of all created chat rooms.");
        }
    }
}
