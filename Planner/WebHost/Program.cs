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

            TcpListener serverSocket = new TcpListener(IPAddress.Any,5171);
            TcpClient clientSocket;

            try
            {
                serverSocket.Start();
            } catch (Exception ex)
            { Console.WriteLine(ex.ToString()); }

            while (true)
            {
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Client Connected");
                ClientHandler client = new ClientHandler();
                client.StartClient(clientSocket, tasks);
            }

            clientSocket.Close();
            serverSocket.Stop();
        }
    }
}
