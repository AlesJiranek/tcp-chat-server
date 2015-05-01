using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

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


        /**
         * Listens for new clients connections
         */
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
                catch (SocketException e)
                {
                    // 10004 is fine - server socket was closed by Stop() method
                    if (e.ErrorCode != 10004)
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
            String chatroomName;
            String username;

            Client client = new Client(clientSocket);
            Console.WriteLine("[" + DateTime.Now + "][" + client.GetSocket().GetHashCode() + "] New client has connected.");

            // Send list of available chatrooms
            client.SendMessage(Server.GenerateSystemMessage(this.chatRooms.Select(r => r.GetName()).ToList<String>()));

            try
            {
                chatroomName = client.ReceiveMessage().Content.ToString();
                username = client.ReceiveMessage().Content.ToString();
            }
            catch (IOException)
            {
                Console.WriteLine("[" + DateTime.Now + "][" + client.GetSocket().GetHashCode() + "] Client has disconnected.");
                return;
            }


            Room selectedRoom = this.chatRooms.Find(delegate(Room room)
            {
                return room.GetName() == chatroomName;
            });

            if (selectedRoom == null)
            {
                selectedRoom = new Room(chatroomName);
            }

            client.SetRoom(selectedRoom);
            selectedRoom.AddClient(client);

            // Check if user with same username is already connected
            var existingUsername = selectedRoom.GetClients().OfType<Client>().Where(c => c.GetName() == username);
            if (existingUsername.Count() > 0)
            {
                // add number to username, becase user with same username is already in same chatroom
                client.SetName(username + "(" + (existingUsername.Count() + 1) + ")");
            }
            else
            {
                client.SetName(username);
            }

            Console.WriteLine("[" + DateTime.Now + "][" + client.GetSocket().GetHashCode() + "] " + client.GetName() + " joined room " + client.GetRoom().GetName());

            // Send list of names of connected clients
            client.GetRoom().BroadcastMessage(Server.GenerateSystemMessage("Connected Users"));
            client.GetRoom().BroadcastMessage(Server.GenerateSystemMessage(client.GetRoom().GetClients().Select(c => c.GetName()).ToList<String>()));

            try
            {
                client.Chat();
            }
            catch (IOException)
            {
                client.GetRoom().RemoveClient(client);
                Console.WriteLine("[" + DateTime.Now + "][" + client.GetSocket().GetHashCode() + "] " + client.GetName() + " left room " + client.GetRoom().GetName());
                Console.WriteLine("[" + DateTime.Now + "][" + client.GetSocket().GetHashCode() + "] Client has disconnected.");
            }
        }


        /**
         * Generates system message with given content
         */
        public static Message GenerateSystemMessage(Object content)
        {
            Message message = new Message();
            message.Type = Message.MessageType.system;
            message.Username = "System";
            message.Content = content;
            message.Timestamp = DateTime.Now;

            return message;
        }


        /**
         * Serializes and sends message to given tcp client
         */
        public static void SendMessage(TcpClient client, Message message)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new DeserializationBinder();
            formatter.Serialize(client.GetStream(), message);
        }
    }
}
