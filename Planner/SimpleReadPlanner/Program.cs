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

namespace SimpleReadPlanner
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskCollection tasks = new TaskCollection();

            TcpClient client = new TcpClient("127.0.0.1",5171);

            byte[] fullPackage;

            using (NetworkStream stream = client.GetStream())
            {
                byte[] recievedBytes = new byte[1024];
                MemoryStream byteStream = new MemoryStream();

                // Incoming message may be larger than the buffer size.
                int bytesRead = 0;

                do
                {
                    bytesRead = stream.Read(recievedBytes, 0, recievedBytes.Length);

                    byteStream.Write(recievedBytes, 0, bytesRead);
                    System.Threading.Thread.Sleep(1);
                }
                while (stream.DataAvailable);

                fullPackage = byteStream.ToArray();
                string data = Encoding.UTF8.GetString(fullPackage);
                Console.WriteLine(data);
                tasks.ImportText(data);
                Console.WriteLine(data.Count(f => f == '\n').ToString() + " lines read. " + tasks.Count.ToString() + " tasks imported.");
                Console.ReadLine();
            }
        }
    }
}
