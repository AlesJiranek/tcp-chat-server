using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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


        /**
         * Constructor
         */ 
        public Client(String name, TcpClient socket)
        {
            this.name = name;
            this.socket = socket;
        }


        /**
         * Get Name
         */ 
        public String getName()
        {
            return this.name;
        }


        /**
         * Get Socket
         */ 
        public TcpClient getSocket()
        {
            return this.socket;
        }


        /**
         * Set chat room
         */ 
        public Client setRoom(Room room)
        {
            this.room = room;

            return this;
        }


        /**
         * Get chat room
         */ 
        public Room getRoom()
        {
            return this.room;
        }
    }
}
