using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tcp_chat_server
{
    class Server
    {
        private IPAddress   ipAdress;
        private int         port;
        private Boolean     isRunning = false;
        private TcpListener serverSocket;
        private Hashtable   connectedClients;

        /**
         * Constructor
         * 
         * @param IPAdress ip - address, where server listens for connections
         */
        public Server(IPAddress ip, int port) {
            this.ipAdress = ip;
            this.port = port;
            this.connectedClients = new Hashtable();
        }

        public bool Start() {
            this.serverSocket = new TcpListener(this.ipAdress, this.port);

            try {
                this.serverSocket.Start();
                this.isRunning = true;
            }
            catch (SocketException e) {
                this.isRunning = false;
                Console.WriteLine("Unable initialize server socket");
                Console.WriteLine("Error: " + e.Message);
            }

            return isRunning;
        }

        public void Listen() {
            while(this.isRunning){
                TcpClient clientSocket = serverSocket.AcceptTcpClient();
                this.connectedClients.Add(clientSocket.GetHashCode(),clientSocket);
                Console.WriteLine("New Connection!");
            }
        }
    }
}
