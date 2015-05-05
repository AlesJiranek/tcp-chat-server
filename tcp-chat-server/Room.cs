using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tcp_chat_server
{
    class Room
    {
        /** Room name */
        private String name;

        /** Clients connected to chat room*/
        private List<Client> clients;


        /**
         * Constructor
         */ 
        public Room(String name)
        {
            this.name = name;
            this.clients = new List<Client>();
        }


        /**
         * Get Name
         */ 
        public String GetName()
        {
            return this.name;
        }


        /**
         * Add client to chat room
         */ 
        public Room AddClient(Client client)
        {
            this.clients.Add(client);

            return this;
        }


        /**
         * Remove client from chat room
         */ 
        public Room RemoveClient(Client client)
        {
            this.clients.Remove(client);

            return this;
        }


        /**
         * Return all clients connected to chatroom
         */
        public List<Client> GetClients()
        {
            return this.clients;
        }


        /**
         * Broadcast message to users connected to chat room
         */ 
        public void BroadcastMessage(Message message)
        {
            foreach(Client client in this.clients)
            {
                client.SendMessage(message);
            }
        }
    }
}
