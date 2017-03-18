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
using TaskFunctions;

namespace WebHost
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskCollection tasks = new TaskCollection();
            List<ClientHandler> clients = new List<ClientHandler>();
            string path = "tasks.tlf";
            if (File.Exists(path))
            {
                tasks.ImportFile(path);
                Console.WriteLine("Imported " + path + ". " + tasks.Count.ToString() + " tasks imported.");
            }

            WebServer server = new WebServer(tasks);
            server.Start(5171);

        }
    }
}
