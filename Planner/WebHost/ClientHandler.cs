using Newtonsoft.Json.Linq;
using Planner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaskFunctions;

namespace WebHost
{
    class ClientHandler
    {
        TcpClient client;
        TaskCollection tasks;
        NetworkStream stream;
        Thread thread;
        public int ID;
        private readonly byte[] terminationBytes = new byte[] { 0x15, 0xba, 0xfc, 0x61 };

        public ClientHandler(int ID)
        {
            this.ID = ID;
        }

        public void StartClient(TcpClient client, TaskCollection tasks)
        {
            this.tasks = tasks;
            this.client = client;
            stream = client.GetStream();
            thread = new Thread(SendHandShake);
            thread.Start();
        }
        private void MainLoop()
        {
            while (true)
            {
                if (stream.DataAvailable)
                {
                    try
                    {
                        string message = Encoding.UTF8.GetString(RecieveData());
                        JObject obj = JObject.Parse(message);
                        HandleData(obj);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine("Closed Thread");
                        break;
                    }
                }
                else
                {
                    Thread.Sleep(10);
                    if (!client.Connected)
                    {
                        Console.WriteLine("Closed Thread");
                        break;
                    }
                }
            }
        }
        private void HandleData(JObject obj)
        {
            string type = (string)obj["type"];
            Planner.Task task;
            switch (type)
            {
                case "AddTask":
                    task = Planner.Task.Parse((JObject)obj["task"]);
                    string parentTask = (string)obj["parent"];
                    if (parentTask != null)
                    {
                        tasks.Add(task, parentTask);
                    }
                    else
                    {
                        tasks.Add(task);
                    }
                    Console.WriteLine("made a new task: " + task.title + "(" + task.ID + ")");
                    OnRecievedJson(obj, true);
                    break;
                case "RenameTask":
                    string taskID = (string)obj["ID"];
                    string newName = (string)obj["newName"];
                    tasks.Rename(taskID,newName);
                    Console.WriteLine("Renamed task: " + newName + "(" + taskID + ")");
                    OnRecievedJson(obj,true);
                    break;
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
        public void SendData(string data)
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
            int timeoutTimer = 0;

            do
            {
                if (stream.DataAvailable)
                {
                    bytesRead = stream.Read(recievedBytes, 0, recievedBytes.Length);

                    byteStream.Write(recievedBytes, 0, bytesRead);
                    if (byteStream.Length > 4)
                    {
                        byteStream.Position -= 4;
                        byteStream.Read(finalBytes, 0, 4);
                    }
                } else
                {
                    timeoutTimer += 2;
                    Thread.Sleep(2);
                    if (timeoutTimer > 10000)
                    { break; }
                }
            }
            while (!Enumerable.SequenceEqual(finalBytes, terminationBytes));

            fullPackage = byteStream.ToArray();
            Array.Resize(ref fullPackage, fullPackage.Length - 4);

            return fullPackage;
        }

        public event EventHandler<RecievedJsonEventArgs> RecievedJson;
        /// <summary>
        /// Called when recieved Json that should be sent to all clients.
        /// </summary>
        protected virtual void OnRecievedJson(JObject obj, bool sendToAll = true)
        {
            if (RecievedJson != null)
            {
                RecievedJsonEventArgs e = new RecievedJsonEventArgs();
                e.obj = obj;
                e.senderID = ID;
                e.sendToAll = sendToAll;
                RecievedJson(this, e);
            }
        }

    }
}
