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
using Newtonsoft.Json.Linq;

namespace TaskFunctions
{
    class TaskWebClient
    {
        private TaskCollection tasks;
        private IPAddress address;
        private TcpClient client;
        private NetworkStream stream;
        private bool handShaken;
        private readonly byte[] terminationBytes = new byte[] { 0x15, 0xba, 0xfc, 0x61 };

        public TaskWebClient(TaskCollection tasks)
        {
            this.tasks = tasks;
            handShaken = false;
        }
        public TaskWebClient(TaskCollection tasks, string address)
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
        public void TaskAdd(Task task, Task parentTask = null)
        {
            //tasks.Add(task,parentTask);
            JObject obj = new JObject();
            obj.Add("type", "AddTask");
            if (parentTask != null)
            {
                obj.Add("parent", parentTask.ID);
            }
            else
            { obj.Add("parent", null); }
            obj.Add("task", task.ToJObject());
            SendData(obj.ToString());
        }
        public void TaskRename(Task task, string newName)
        {
            TaskRename(task.ID, newName);
        }
        public void TaskRename(string taskID, string newName)
        {
            JObject obj = new JObject();
            obj.Add("type", "RenameTask");
            obj.Add("ID", taskID);
            obj.Add("newName", newName);
            SendData(obj.ToString());
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
                        break;
                    }
                }
                else
                {
                    Thread.Sleep(10);
                    if (!client.Connected)
                    {
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
                    task = Task.Parse((JObject)obj["task"]);
                    tasks.Add(task);
                    Console.WriteLine("made a new task: " + task.title + "(" + task.ID + ")");
                    OnRecievedTask(task);
                    break;
                case "RenameTask":
                    string taskID = (string)obj["ID"];
                    string newName = (string)obj["newName"];
                    tasks.Rename(taskID, newName);
                    Console.WriteLine("Renamed task: " + newName + "(" + taskID + ")");
                    OnRenamedTask(taskID, newName);
                    break;
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
        public event EventHandler<RecievedTaskEventArgs> RecievedTask;
        protected virtual void OnRecievedTask(Task task)
        {
            if (RecievedTask != null)
            {
                RecievedTaskEventArgs e = new RecievedTaskEventArgs();
                e.task = task;
                RecievedTask(this, e);
            }
        }
        public event EventHandler<RenamedTaskEventArgs> RenamedTask;
        protected virtual void OnRenamedTask(string taskID, string newName)
        {
            if (RenamedTask != null)
            {
                RenamedTaskEventArgs e = new RenamedTaskEventArgs();
                e.taskID = taskID;
                e.newName = newName;
                RenamedTask(this, e);
            }
        }
    }
}
