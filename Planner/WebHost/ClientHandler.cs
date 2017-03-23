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
using System.Security.Cryptography;

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
        RSACryptoServiceProvider RSA;
        ICryptoTransform encryptor;
        ICryptoTransform decryptor;

        public ClientHandler(int ID, RSACryptoServiceProvider RSA)
        {
            this.ID = ID;
            this.RSA = RSA;
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
                case "CheckTask":
                    taskID = (string)obj["ID"];
                    bool check = (bool)obj["check"];
                    tasks.Check(taskID, check);
                    Console.WriteLine("{0} the task: " + taskID, check ? "Checked" : "Unchecked");
                    OnRecievedJson(obj, true);
                    break;
            }
        }

        private void SendHandShake()
        {
            string message = RecieveUnencryptedDataString();
            if (message == "Ey waddup?")
            {
                Console.WriteLine("Recieved correct identifier: "+ '"'+message+'"');

                SendUnencryptedData(RSA.ExportCspBlob(false));


                byte[] keyAndIV = RSA.Decrypt(RecieveUnencryptedData(),false);
                byte[] key = keyAndIV.Take(32).ToArray();
                byte[] IV = keyAndIV.Skip(32).ToArray();


                RijndaelManaged RIJ = new RijndaelManaged();
                RIJ.IV = IV;
                RIJ.Key = key;

                decryptor = RIJ.CreateDecryptor(RIJ.Key, RIJ.IV);
                encryptor =  RIJ.CreateEncryptor(RIJ.Key,RIJ.IV);

                SendUnencryptedData(Encrypt(Encoding.UTF8.GetBytes(tasks.ExportString())));
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


        public void SendUnencryptedData(byte[] data)
        {
            byte[] sendBytes = data.Concat(terminationBytes).ToArray();
            stream.Write(sendBytes, 0, sendBytes.Length);
            stream.Flush();
        }
        public void SendData(string data)
        {
            byte[] sendBytes = Encrypt(Encoding.UTF8.GetBytes(data)).Concat(terminationBytes).ToArray();
            stream.Write(sendBytes, 0, sendBytes.Length);
            stream.Flush();
        }
        private string RecieveDataString()
        { return Encoding.UTF8.GetString(RecieveData()); }
        
        private string RecieveUnencryptedDataString()
        {
            return Encoding.UTF8.GetString(RecieveUnencryptedData());
        }

        private byte[] RecieveData()
        {
            return Decrypt(RecieveUnencryptedData());
        }

        private byte[] RecieveUnencryptedData()
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
                    { return new byte[0]; }
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
