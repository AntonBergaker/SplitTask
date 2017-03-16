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
            TcpClient clientSocket;
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
        NetworkStream stream;

        public void startClient(TcpClient inClientSocket, string clineNo, TaskCollection tasks)
        {
            this.tasks = tasks;
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            stream = clientSocket.GetStream();
            Thread ctThread = new Thread(SendHandShake);
            ctThread.Start();
        }
        private void SendHandShake()
        {
            SendData(tasks.ExportString());
        }
        private void SendData(string data)
        {
            byte[] sendBytes = Encoding.UTF8.GetBytes(data);
            stream.Write(sendBytes, 0, sendBytes.Length);
            stream.Flush();
        }
        private byte[] RecieveData()
        {
            byte[] recievedBytes = new byte[1024];
            MemoryStream byteStream = new MemoryStream();
            int bytesRead = 0;

            do
            {
                bytesRead = stream.Read(recievedBytes, 0, recievedBytes.Length);

                byteStream.Write(recievedBytes, 0, bytesRead);
                Thread.Sleep(1);
            }
            while (stream.DataAvailable);

            return byteStream.ToArray();
        }
    }
}
