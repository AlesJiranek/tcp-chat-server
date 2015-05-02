using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using tcp_chat_server.DAOs;

namespace tcp_chat_server
{
    class Client
    {
        /** Clients name  */
        private String name;

        /** Clients socket  */
        private TcpClient socket;

        /** Room, where the client is connected */
        private Room room;

        /** Clients stream reader */
        private StreamReader reader;

        /** Clients stream writer */
        private StreamWriter writer;


        /**
         * Constructor
         */
        public Client(TcpClient socket)
        {
            this.socket = socket;
            this.reader = new StreamReader(this.GetSocket().GetStream());
            this.writer = new StreamWriter(this.GetSocket().GetStream());
        }


        /**
         * Get Name
         */
        public String GetName()
        {
            return this.name;
        }


        /**
         * Set Name
         */
        public Client SetName(String name)
        {
            this.name = name;

            return this;
        }


        /**
         * Get Socket
         */
        public TcpClient GetSocket()
        {
            return this.socket;
        }


        /**
         * Set chat room
         */
        public Client SetRoom(Room room)
        {
            this.room = room;

            return this;
        }


        /**
         * Get chat room
         */
        public Room GetRoom()
        {
            return this.room;
        }


        /**
         * Send message to client
         */
        public void SendMessage(Message message)
        {
            Server.SendMessage(this.GetSocket(), message);
        }


        /**
         * Receives and deserializes message sent from client
         */
        public Message ReceiveMessage()
        {
            NetworkStream stream = this.GetSocket().GetStream();

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new DeserializationBinder();

            Message message = (Message)formatter.Deserialize(stream);

            return message;
        }

        
        /**
         * Handles messages sent by client
         */ 
        public void Chat()
        {
            while (true)
            {
                // Wait for incoming message from client
                Message message = ReceiveMessage();
                
                // Save message to history
                Task saveHistory = new Task(() => DAOs.MessagesDAO.Add(message, this.GetRoom()));
                saveHistory.Start();

                // Brodcast message to other clients in same room
                this.GetRoom().BroadcastMessage(message);
            }
        }
    }
}
