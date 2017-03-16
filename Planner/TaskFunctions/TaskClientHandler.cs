using Planner;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace TaskFunctions
{
    class TaskClientHandler
    {
        private TaskCollection tasks;
        private IPAddress address;
        private TcpClient client;
        private NetworkStream stream;
        private bool handShaken;

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
            if (Handshake())
            {
                Thread mainThread = new Thread(MainLoop);
                mainThread.Start();
            }
            
        }
        private void MainLoop()
        {
            while (true)
            {
                RecieveData();
            }
        }


        private bool Handshake()
        {
            SendData("Ey waddup?");
            byte[] response = RecieveData();
            string message = Encoding.UTF8.GetString(response);
            tasks.ImportText(message);
            return true;
        }
        private void SendData(string data)
        {            
            byte[] sendBytes = Encoding.UTF8.GetBytes(data);
            stream.Write(sendBytes, 0, sendBytes.Length);
            stream.Flush();
        }
        private byte[] RecieveData()
        {
            byte[] fullPackage;

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

            fullPackage = byteStream.ToArray();
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
    }
    public class RecievedDataEventArgs : EventArgs
    {
        public string textData { get { return Encoding.UTF8.GetString(byteData); } }
        public byte[] byteData { get; set; }
    }
}
