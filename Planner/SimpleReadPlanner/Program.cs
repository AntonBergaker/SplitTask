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

            TaskWebClient client = new TaskWebClient(tasks);

            client.RecievedData += new EventHandler<RecievedDataEventArgs>(HandleData);
            client.RecievedTasks += new EventHandler<RecievedTasksEventArgs>(HandleTasks);
    
            client.Connect("185.16.95.101");


            Console.ReadLine();
        }

        private static void HandleData(object sender, RecievedDataEventArgs e)
        {
            Console.WriteLine(e.textData);
        }
        private static void HandleTasks(object sender, RecievedTasksEventArgs e)
        {
            Console.WriteLine("Recieved " + e.tasks.Count + " tasks");
        }
    }
}
