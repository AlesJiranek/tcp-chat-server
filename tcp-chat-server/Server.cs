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
        }


        /**
         * Starts server
         */ 
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

        /**
         * Stops server
         */ 
        public void Stop()
        {
            this.isRunning = false;
            this.serverSocket.Stop();

            // Close all clients sockets to terminate their threads
            foreach(TcpClient client in this.connectedClients.Values)
            {
                client.Close();
            }
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
                    connectedClients.Add(clientSocket.GetHashCode(), clientSocket);

                    Thread clientThread = new Thread(() => this.HandleClient(clientSocket));
                    clientThread.Start();
                }
                catch (SocketException e)
                {
                    // 10004 is fine - server socket was closed by Server.Stop() method
                    if (e.ErrorCode != 10004)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        
        /**
         * Handles connected client
         */ 
        public void HandleClient(TcpClient clientSocket)
        {
            String chatroomName;
            String username;

            Client client = new Client(clientSocket);
            Console.WriteLine("[" + DateTime.Now + "][" + client.GetSocket().GetHashCode() + "] New client has connected.");

            // Send list of available chatrooms to client
            client.SendMessage(Server.GenerateSystemMessage(this.chatRooms.Select(r => r.GetName()).ToList<String>()));

            try
            {
                // Get client's selected chatroom
                chatroomName = client.ReceiveMessage().Content.ToString();
                // Get client's username
                username = client.ReceiveMessage().Content.ToString();
            }
            catch (IOException)
            {
                Console.WriteLine("[" + DateTime.Now + "][" + client.GetSocket().GetHashCode() + "] Client has disconnected.");
                return;
            }

            // Check if room with selected name exists
            Room selectedRoom = this.chatRooms.Find(delegate(Room room)
            {
                return room.GetName() == chatroomName;
            });

            // Create new room if does not exists
            if (selectedRoom == null)
            {
                selectedRoom = new Room(chatroomName);
                this.chatRooms.Add(selectedRoom);
            }

            client.SetRoom(selectedRoom);
            selectedRoom.AddClient(client);

            // Check if user with same username is already connected to selected chatroom
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

            // Send last ten messages from history
            List<Message> lastTenMessages = DAOs.MessagesDAO.GetLastTenMessagesInRoom(client.GetRoom());

            foreach(Message m in lastTenMessages)
            {
                client.SendMessage(m);
            }

            try
            {
                // Handles clients chating
                client.Chat();
            }
            catch (IOException)
            {
                // Client was disconnected, remove him from room
                client.GetRoom().RemoveClient(client);
                // Remove client from list of connected users
                this.connectedClients.Remove(client.GetHashCode());
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


        /**
         * Shows list of available rooms
         */ 
        public void ListRooms()
        {
            Console.WriteLine("total " + this.chatRooms.Count());

            foreach(Room room in this.chatRooms)
            {
                Console.WriteLine(room.GetName() + "\t\t" + room.GetClients().Count() + " clients");
            }
        }
    }
}
