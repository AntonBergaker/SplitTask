using Planner;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Linq;

namespace TaskFunctions
{
    class TaskClientHandler
    {
        private TaskCollection tasks;
        private IPAddress address;
        private TcpClient client;
        private NetworkStream stream;
        private bool handShaken;
        private readonly byte[] terminationBytes = new byte[] { 0x15, 0xba, 0xfc, 0x61 };

        public TaskClientHandler(TaskCollection tasks)
        {
            this.tasks = tasks;
            handShaken = false;
        }
        public TaskClientHandler(TaskCollection tasks, string address)
        {
            this.tasks = tasks;
            handShaken = false;
            Connect(address);
        }

        public void Connect(string address)
        {
            client = new TcpClient();
            this.address = IPAddress.Parse(address);
            client.Connect(this.address, 5171);
            stream = client.GetStream();

            Thread mainThread = new Thread(Handshake);
            mainThread.Start();
            
        }
        private void MainLoop()
        {
            while (true)
            {
                string message = Encoding.UTF8.GetString(RecieveData());
            }
        }


        private void Handshake()
        {
            SendData("Ey waddup?");
            string message = RecieveDataString();
            tasks.ImportText(message);
            OnRecievedTasks();
            MainLoop();
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
            while (!Enumerable.SequenceEqual(finalBytes,terminationBytes));

            fullPackage = byteStream.ToArray();
            Array.Resize(ref fullPackage, fullPackage.Length - 4);

            RecievedDataEventArgs e = new RecievedDataEventArgs();
            e.byteData = fullPackage;
            OnRecievedData(e);
            return fullPackage;
        }

        public event EventHandler<RecievedDataEventArgs> RecievedData;
        protected virtual void OnRecievedData(RecievedDataEventArgs e)
        {
            if (RecievedData != null)
            {
                RecievedData(this, e);
            }
        }
        public event EventHandler<RecievedTasksEventArgs> RecievedTasks;
        protected virtual void OnRecievedTasks()
        {
            if (RecievedTasks != null)
            {
                RecievedTasksEventArgs e = new RecievedTasksEventArgs();
                e.tasks = tasks;
                RecievedTasks(this, e);
            }
        }
    }
    class RecievedDataEventArgs : EventArgs
    {
        public string textData { get { return Encoding.UTF8.GetString(byteData); } }
        public byte[] byteData { get; set; }
    }
    class RecievedTasksEventArgs : EventArgs
    {
        public TaskCollection tasks { get; set; }
    }
}
