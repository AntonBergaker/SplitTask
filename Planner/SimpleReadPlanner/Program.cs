using Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using TaskFunctions;

namespace SimpleReadPlanner
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskCollection tasks = new TaskCollection();

            TaskClientHandler client = new TaskClientHandler(tasks);

            client.RecievedData += new EventHandler<RecievedDataEventArgs>(HandleData);

            client.Connect("127.0.0.1");

            Console.WriteLine("Recieved " + tasks.Count + " tasks");


            Console.ReadLine();
        }

        private static void HandleData(object sender, RecievedDataEventArgs e)
        {
            Console.WriteLine(e.textData);
        }
    }
}
