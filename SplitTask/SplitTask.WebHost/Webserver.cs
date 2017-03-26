using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using SplitTask;
using System.Threading;
using SplitTask.Common;
using System.Security.Cryptography;

namespace SplitTask.WebHost
{
    class WebServer
    {
        List<ClientHandler> clients = new List<ClientHandler>();
        TcpListener serverSocket;
        TaskCollection tasks;
        RSACryptoServiceProvider RSA;

        public WebServer(RSACryptoServiceProvider RSA)
        {
            this.RSA = RSA;
        }

        public void Start(int port)
        {
            serverSocket = new TcpListener(IPAddress.Any, port);
            TcpClient clientSocket;

            int clientCount = 0;

            try
            {
                serverSocket.Start();
            }
            catch (Exception ex)
            { Console.WriteLine(ex.ToString()); }

            while (true)
            {
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Client Connected");
                ClientHandler client = new ClientHandler(clientCount, RSA);
                client.RecievedJson += new EventHandler<RecievedJsonEventArgs>(HandleJson);
                clientCount++;

                clients.Add(client);
                client.StartClient(clientSocket, tasks);
            }

            clientSocket.Close();
            serverSocket.Stop();
        }

        /// <summary>
        /// Sends incomming JSON to all other clients
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleJson(object sender, RecievedJsonEventArgs e)
        {
            int senderID = e.senderID;
            foreach (ClientHandler c in clients)
            {
                if (c.ID != senderID)
                {
                    c.SendData(e.obj.ToString());
                }
            }
        }
    }
}
