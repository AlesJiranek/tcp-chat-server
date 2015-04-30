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
        String name;

        /** Clients connected to chat room*/
        Hashtable clients;


        /**
         * Constructor
         */ 
        public Room(String name)
        {
            this.name = name;
            this.clients = new Hashtable();
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
            this.clients.Add(client.GetHashCode(), client);

            return this;
        }


        /**
         * Remove client from chat room
         */ 
        public Room RemoveClient(Client client)
        {
            this.clients.Remove(client.GetHashCode());

            return this;
        }


        /**
         * Broadcast message to users connected to chat room
         */ 
        public void BroadcastMessage(String message, Client sender)
        {
            foreach(Client client in this.clients.Values)
            {
                Server.sendMessage(client.getSocket(), message);
            }
        }
    }
}
