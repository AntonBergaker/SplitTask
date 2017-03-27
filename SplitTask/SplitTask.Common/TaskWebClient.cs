using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace SplitTask.Common
{
    class TaskWebClient
    {

        private TaskCollection tasks;
        private IPAddress address;
        private TcpClient client;
        private NetworkStream stream;
        private bool handShaken;
        private bool running;
        private byte[] streamKeys;
        private ICryptoTransform decryptor;
        private ICryptoTransform encryptor;
        private readonly byte[] terminationBytes = new byte[] { 0x15, 0xba, 0xfc, 0x61, 0xf1, 0x03 };

        public TaskWebClient(TaskCollection tasks)
        {
            this.tasks = tasks;
            tasks.TaskAdded += Tasks_TaskAdded;
            handShaken = false;
            running = true;
            AddEventsToTasks(tasks.tasks);
        }

        public TaskWebClient(TaskCollection tasks, string address)
        {
            this.tasks = tasks;
            handShaken = false;
            running = true;
            AddEventsToTasks(tasks.tasks);
            Connect(address);
        }
        private void AddEventsToTasks(List<Task> tasks)
        {
            foreach (Task t in tasks)
            {
                AddEventsToTask(t);
                AddEventsToTasks(t.subtasks);
            }
        }

        private void AddEventsToTask(Task task)
        {
            task.TaskChecked += Task_TaskChecked;
            task.TaskRenamed += Task_TaskRenamed;
            task.TaskFolderChanged += Task_TaskFolderChanged;
            task.TaskDescriptionChanged += Task_TaskDescriptionChanged;
        }

        private void Task_TaskFolderChanged(object sender, TaskFolderChangedEventArgs e)
        {
            if (e.originalSender != this)
            {
                TaskFolderChange(e.task.ID, e.isFolder);
            }
        }

        private void Task_TaskDescriptionChanged(object sender, TaskDescriptionChangedEventArgs e)
        {
            if (e.originalSender != this)
            {
                TaskDescriptionChange(e.task.ID, e.newDescription);
            }
        }

        private void Task_TaskRenamed(object sender, TaskRenamedEventArgs e)
        {
            if (e.originalSender != this)
            {
                TaskRename(e.task.ID,e.newName);
            }
        }

        private void Task_TaskChecked(object sender, TaskCheckedEventArgs e)
        {
            if (e.originalSender != this)
            {
                TaskCheck(e.task.ID, e.check);
            }
        }

        private void Tasks_TaskAdded(object sender, TaskAddedEventArgs e)
        {
            AddEventsToTask(e.task);
        }

        public void Connect(string address)
        {
            client = new TcpClient();
            this.address = IPAddress.Parse(address);
            Thread mainThread = new Thread(ConnectToClient);
            mainThread.Start();
        }
        private void ConnectToClient()
        {
            while (running)
            {
                try
                {
                    client.Connect(address, 5171);
                    stream = client.GetStream();

                    Handshake();
                }
                catch (Exception ex)
                {
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }


        public void TaskAdd(Task task, Task parentTask = null)
        {
            if (handShaken)
            {
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
        }
        public void TaskRename(Task task, string newName)
        {
            TaskRename(task.ID, newName);
        }
        public void TaskRename(string taskID, string newName)
        {
            if (handShaken)
            {
                JObject obj = new JObject();
                obj.Add("type", "RenameTask");
                obj.Add("ID", taskID);
                obj.Add("newName", newName);
                SendData(obj.ToString());
            }
        }
        public void TaskCheck(string taskID, bool check)
        {
            if (handShaken)
            {
                JObject obj = new JObject();
                obj.Add("type", "CheckTask");
                obj.Add("ID", taskID);
                obj.Add("check", check);
                SendData(obj.ToString());
            }
        }
        public void TaskDescriptionChange(string taskID, string newDescription)
        {
            if (handShaken)
            {
                JObject obj = new JObject();
                obj.Add("type", "DescriptionChange");
                obj.Add("ID", taskID);
                obj.Add("description", newDescription);
                SendData(obj.ToString());
            }
        }
        public void TaskFolderChange(string taskID, bool isFolder)
        {
            if (handShaken)
            {
                JObject obj = new JObject();
                obj.Add("type", "FolderChange");
                obj.Add("ID", taskID);
                obj.Add("isFolder", isFolder);
                SendData(obj.ToString());
            }
        }

        private void MainLoop()
        {
            while (running)
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
                        Console.WriteLine(ex);
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
            string taskID;
            Task task;
            switch (type)
            {
                case "AddTask":
                    task = Task.Parse((JObject)obj["task"]);
                    string parentTask = (string)obj["parent"];
                    if (parentTask != null)
                    {
                        tasks.Add(task, parentTask);
                    }
                    else
                    {
                        tasks.Add(task);
                    }
                    Console.WriteLine("Made a new task: " + task.name + "(" + task.ID + ")");
                    OnRecievedTask(task, parentTask);
                    break;
                case "RenameTask":
                    taskID = (string)obj["ID"];
                    string newName = (string)obj["newName"];
                    tasks.Rename(taskID, newName,this);
                    Console.WriteLine("Renamed task: " + newName + "(" + taskID + ")");
                    break;
                case "CheckTask":
                    taskID = (string)obj["ID"];
                    bool check = (bool)obj["check"];
                    tasks.Check(taskID, check, this);
                    Console.WriteLine("{0} the task: " + taskID, check ? "Checked" : "Unchecked");
                    break;
                case "DescriptionChange":
                    taskID = (string)obj["ID"];
                    string description = (string)obj["description"];
                    tasks.DescriptionChange(taskID, description, this);
                    Console.WriteLine("Added the description \"" + description + "\" to "+taskID);
                    break;
                case "FolderChange":
                    taskID = (string)obj["ID"];
                    bool isFolder = (bool)obj["isFolder"];
                    tasks.FolderChange(taskID, isFolder, this);
                    Console.WriteLine("Set the task " + taskID + "to a {0}", isFolder ? " folder" : "task");
                    break;
            }
        }
        public void RequestClose()
        {
            running = false;
        }

        private void Handshake()
        {
            byte[] blob = File.ReadAllBytes("public_server_credentials.blob");

            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSA.ImportCspBlob(blob);

            RijndaelManaged RIJ = new RijndaelManaged();
            RIJ.GenerateKey();
            RIJ.GenerateIV();
            decryptor = RIJ.CreateDecryptor(RIJ.Key,RIJ.IV);
            encryptor = RIJ.CreateEncryptor(RIJ.Key, RIJ.IV);

            byte[] username = Encoding.UTF8.GetBytes("anton");
            byte[] password = Encoding.UTF8.GetBytes("TCEMvdMIGP8i0grlgL3ZlB4bc22K0bmc");

            streamKeys = RIJ.Key.Concat(RIJ.IV).Concat(password).Concat(username).ToArray();

            SendUnencryptedData(RSA.Encrypt(streamKeys, false));

            byte[] message = Decrypt(RecieveUnencryptedData());

            string text = Encoding.UTF8.GetString(message);
            if (text == "ey waddup?")
            {
                SendData("L0RuPtEwlxokJfyBcwPiRAOiPJL73MBF6a1OpXPNHp3YH");

                text = RecieveDataString();
                tasks.ImportText(text);
                AddEventsToTasks(tasks.tasks);
                OnRecievedTasks();
                handShaken = true;
                MainLoop();
            }
        }

        private byte[] Decrypt(byte[] data)
        {
            byte[] decrypted;
            // Create the streams used for decryption. 
            using (MemoryStream msDecrypt = new MemoryStream(data))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        // Read the decrypted bytes from the decrypting stream 
                        // and place them in a string.
                        decrypted = Encoding.UTF8.GetBytes(srDecrypt.ReadToEnd());
                    }
                }
            }

            return decrypted;
        }
        private byte[] Encrypt(byte[] data)
        {
            byte[] encrypted;
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {

                        //Write all data to the stream.
                        swEncrypt.Write(Encoding.UTF8.GetString(data));
                    }
                    encrypted = msEncrypt.ToArray();
                }


                // Return the encrypted bytes from the memory stream. 
                return encrypted;
            }
        }


        private void SendUnencryptedData(string text)
        {
            SendUnencryptedData(Encoding.UTF8.GetBytes(text));
        }

        private void SendUnencryptedData(byte[] data)
        {
            byte[] sendBytes = data.Concat(terminationBytes).ToArray();
            stream.Write(sendBytes, 0, sendBytes.Length);
            stream.Flush();
        }

        private void SendData(string data)
        {
            byte[] sendBytes = Encrypt(Encoding.UTF8.GetBytes(data)).Concat(terminationBytes).ToArray();
            stream.Write(sendBytes, 0, sendBytes.Length);
            stream.Flush();
        }
        private string RecieveDataString()
        { return Encoding.UTF8.GetString(RecieveData()); }

        private byte[] RecieveData()
        {
            return Decrypt(RecieveUnencryptedData());
        }

        private byte[] RecieveUnencryptedData()
        {
            byte[] fullPackage;

            byte[] finalBytes = new byte[6];

            byte[] recievedBytes = new byte[1024];
            MemoryStream byteStream = new MemoryStream();
            int bytesRead = 0;

            do
            {
                bytesRead = stream.Read(recievedBytes, 0, recievedBytes.Length);

                byteStream.Write(recievedBytes, 0, bytesRead);
                if (byteStream.Length > 6)
                {
                    byteStream.Position -= 6;
                    byteStream.Read(finalBytes, 0, 6);
                }
            }
            while (!Enumerable.SequenceEqual(finalBytes,terminationBytes));

            fullPackage = byteStream.ToArray();
            if (fullPackage.Length > 6)
            {
                Array.Resize(ref fullPackage, fullPackage.Length - 6);
            }

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
        protected virtual void OnRecievedTask(Task task, string parentTask)
        {
            if (RecievedTask != null)
            {
                RecievedTaskEventArgs e = new RecievedTaskEventArgs();
                e.task = task;
                e.parentTask = parentTask;
                RecievedTask(this, e);
            }
        }
    }
}
