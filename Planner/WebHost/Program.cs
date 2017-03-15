using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Planner;
using System.Threading;

namespace WebHost
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskCollection tasks = new TaskCollection();
            string path = "tasks.tlf";
            if (File.Exists(path))
            {
                tasks.ImportFile(path);
                Console.WriteLine("Imported " + path + ". " + tasks.Count.ToString() + " tasks imported.");
            }

            IPAddress adress = IPAddress.Parse("127.0.0.1");
            TcpListener serverSocket = new TcpListener(adress,5171);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            serverSocket.Start();

            while (true)
            {
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Client Connected");
                HandleClient client = new HandleClient();
                client.startClient(clientSocket, Convert.ToString(counter), tasks);
            }

            clientSocket.Close();
            serverSocket.Stop();
        }
    }

    //Class to handle each client request
    class HandleClient
    {
        TcpClient clientSocket;
        string clNo;
        TaskCollection tasks;

        public void startClient(TcpClient inClientSocket, string clineNo, TaskCollection tasks)
        {
            this.tasks = tasks;
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            Thread ctThread = new Thread(SendHandShake);
            ctThread.Start();
        }
        private void SendHandShake()
        {
            byte[] bytesFrom = new byte[10025];
            byte[] sendBytes = null;

            try
            {
                NetworkStream networkStream = clientSocket.GetStream();

                sendBytes = Encoding.UTF8.GetBytes(tasks.ExportString());
                networkStream.Write(sendBytes, 0, sendBytes.Length);
                networkStream.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
