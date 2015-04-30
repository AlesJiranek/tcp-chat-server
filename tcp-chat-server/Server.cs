using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace tcp_chat_server
{
    class Server
    {
        private IPAddress ipAdress;
        private int port;
        private Boolean isRunning = false;
        private TcpListener serverSocket;
        private Hashtable connectedClients;
        private Thread listeningThread;
        private List<Room> chatRooms;

        /**
         * Constructor
         * 
         * @param IPAdress ip - address, where server listens for connections
         */
        public Server(IPAddress ip, int port)
        {
            this.ipAdress = ip;
            this.port = port;
            this.connectedClients = new Hashtable();
            this.chatRooms = new List<Room>();
            this.chatRooms.Add(new Room("První místnost"));
            this.chatRooms.Add(new Room("Druhá místnost"));
            this.chatRooms.Add(new Room("Třetí místnost"));
        }

        public bool Start()
        {
            this.serverSocket = new TcpListener(this.ipAdress, this.port);

            try
            {
                this.serverSocket.Start();
                this.isRunning = true;
                this.listeningThread = new Thread(this.ListenForNewConnections);
                this.listeningThread.Start();
            }
            catch (SocketException e)
            {
                this.isRunning = false;
                Console.WriteLine("Unable initialize server socket");
                Console.WriteLine("Error: " + e.Message);
            }

            return isRunning;
        }

        public void Stop()
        {
            this.isRunning = false;
            this.serverSocket.Stop();
        }

        public void ListenForNewConnections()
        {
            while (this.isRunning)
            {
                try
                {
                    TcpClient clientSocket = serverSocket.AcceptTcpClient();

                    Thread clientThread = new Thread(() => this.HandleClient(clientSocket));
                    clientThread.Start();
                }
                catch(SocketException e)
                {
                    // 10004 is fine - server socket was closed by Stop() method
                    if(e.ErrorCode != 10004)
                    {
                        Console.WriteLine(e.Message);
                    }
                }     
            }
        }


        public static void sendMessage(TcpClient target, String message)
        {
            NetworkStream stream = target.GetStream();
            StreamWriter writer = new StreamWriter(stream);

            writer.WriteLine(message);
            writer.Flush();
        }


        public void HandleClient(TcpClient clientSocket)
        {            
            this.sendListOfRooms(clientSocket);
            Room room = this.getClientsChatRoom(clientSocket);
            String name = this.getClientsName(clientSocket);

            Client client = new Client(name, clientSocket);
            client.setRoom(room);
            Console.WriteLine(client.getName() + " joined room " + client.getRoom().GetName());


            NetworkStream stream = clientSocket.GetStream();
            StreamReader reader = new StreamReader(stream);
            String message;

            try
            {
                while (this.isRunning && !String.IsNullOrEmpty((message = reader.ReadLine())))
                {
                    Console.WriteLine(message);
                }
            }
            catch(IOException e)
            {
                Console.WriteLine("Client was disconnected");
            }
        }


        public void sendListOfRooms(TcpClient client)
        {
            StringBuilder message = new StringBuilder();

            foreach(Room room in this.chatRooms)
            {
                message.Append(room.GetName());
                message.Append(";");   
            }

            message.Remove(message.Length-1, 1);

            Server.sendMessage(client, message.ToString());
        }

        public Room getClientsChatRoom(TcpClient clientSocket)
        {
            NetworkStream stream = clientSocket.GetStream();
            StreamReader reader = new StreamReader(stream);

            String chatroomName = reader.ReadLine();

            Room selectedRoom = this.chatRooms.Find( delegate(Room room){
                return room.GetName() == chatroomName;
            });

            if (selectedRoom == null)
            {
                selectedRoom = new Room(chatroomName);
            }

            return selectedRoom;
        }

        public String getClientsName(TcpClient clientSocket)
        {
            NetworkStream stream = clientSocket.GetStream();
            StreamReader reader = new StreamReader(stream);

            return reader.ReadLine();
        }
    }
}
