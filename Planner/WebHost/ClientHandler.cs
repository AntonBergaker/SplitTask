using Planner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebHost
{
    class ClientHandler
    {
        TcpClient client;
        TaskCollection tasks;
        NetworkStream stream;
        private readonly byte[] terminationBytes = new byte[] { 0x15, 0xba, 0xfc, 0x61 };

        public void StartClient(TcpClient client, TaskCollection tasks)
        {
            this.tasks = tasks;
            this.client = client;
            stream = client.GetStream();
            Thread thread = new Thread(SendHandShake);
            thread.Start();
        }
        private void MainLoop()
        {
            while (true)
            {
                string message = Encoding.UTF8.GetString(RecieveData());
                Console.WriteLine(message);
            }
        }

        private void SendHandShake()
        {
            string message = RecieveDataString();
            if (message == "Ey waddup?")
            {
                Console.WriteLine("Recieved correct identifier: "+ '"'+message+'"');
                SendData(tasks.ExportString());
                MainLoop();
            }
        }
        private void SendData(string data)
        {
            byte[] sendBytes = Encoding.UTF8.GetBytes(data).Concat(terminationBytes).ToArray();
            stream.Write(sendBytes, 0, sendBytes.Length);
            stream.Flush();
        }
        private string RecieveDataString()
        { return Encoding.UTF8.GetString(RecieveData()); }

        private byte[] RecieveData()
        {
            byte[] fullPackage;

            byte[] finalBytes = new byte[4];

            byte[] recievedBytes = new byte[1024];
            MemoryStream byteStream = new MemoryStream();
            int bytesRead = 0;

            do
            {
                bytesRead = stream.Read(recievedBytes, 0, recievedBytes.Length);

                byteStream.Write(recievedBytes, 0, bytesRead);
                if (byteStream.Length > 4)
                {
                    byteStream.Position -= 4;
                    byteStream.Read(finalBytes, 0, 4);
                }
            }
            while (!Enumerable.SequenceEqual(finalBytes, terminationBytes));

            fullPackage = byteStream.ToArray();
            Array.Resize(ref fullPackage, fullPackage.Length - 4);

            return fullPackage;
        }
    }
}
