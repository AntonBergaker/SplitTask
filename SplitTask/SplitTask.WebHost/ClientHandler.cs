using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SplitTask.Common;
using System.Security.Cryptography;

namespace SplitTask.WebHost
{
    class ClientHandler
    {
        TcpClient client;
        TaskCollection tasks;
        NetworkStream stream;
        public User user;
        Thread thread;
        public int ID;
        private readonly byte[] terminationBytes = new byte[] { 0x15, 0xba, 0xfc, 0x61, 0xf1, 0x03 };
        RSACryptoServiceProvider RSA;
        Dictionary<string, User> usersByKey;
        Dictionary<string, User> usersByName;
        ICryptoTransform encryptor;
        ICryptoTransform decryptor;

        public ClientHandler(int ID, RSACryptoServiceProvider RSA, Dictionary<string,User> usersByKey, Dictionary<string,User> usersByName)
        {
            this.ID = ID;
            this.RSA = RSA;
            this.usersByKey = usersByKey;
            this.usersByName = usersByName;
        }

        public void StartClient(TcpClient client)
        {
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
            OnRecievedJson(obj);
        }

        private void SendHandShake()
        {

            byte[] recievedData = RSA.Decrypt(RecieveUnencryptedData(),false);
            byte[] key = recievedData.Take(32).ToArray();
            byte[] IV = recievedData.Skip(32).Take(16).ToArray();

            string password = Encoding.UTF8.GetString(recievedData.Skip(48).Take(32).ToArray());
            string username = Encoding.UTF8.GetString(recievedData.Skip(80).ToArray());

            if (usersByName.ContainsKey(username))
            {
                if (usersByName[username].password == password)
                {
                    user = usersByName[username];
                    Console.WriteLine("Logged in as "+username);
                    RijndaelManaged RIJ = new RijndaelManaged();
                    RIJ.IV = IV;
                    RIJ.Key = key;

                    decryptor = RIJ.CreateDecryptor(RIJ.Key, RIJ.IV);
                    encryptor =  RIJ.CreateEncryptor(RIJ.Key,RIJ.IV);

                    SendData("ey waddup?");

                    string[] servers = RecieveDataString().Split(',');

                    OnAuthenticated(servers);
                    MainLoop();
                }
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
            byte[] finalBytes = new byte[6];

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
                    if (byteStream.Length > 6)
                    {
                        byteStream.Position -= 6;
                        byteStream.Read(finalBytes, 0, 6);
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
            if (fullPackage.Length > 6)
            {
                Array.Resize(ref fullPackage, fullPackage.Length - 6);
            }

            return fullPackage;
        }

        public event EventHandler<AuthenticatedEventArgs> Authenticated;
        /// <summary>
        /// Called when recieved Json that should be sent to all clients.
        /// </summary>
        protected virtual void OnAuthenticated(string[] taskServers)
        {
            if (Authenticated != null)
            {
                AuthenticatedEventArgs e = new AuthenticatedEventArgs();
                e.servers = taskServers;
                Authenticated(this, e);
            }
        }

        public event EventHandler<RecievedJsonEventArgs> RecievedJson;
        /// <summary>
        /// Called when recieved Json
        /// </summary>
        protected virtual void OnRecievedJson(JObject obj)
        {
            if (RecievedJson != null)
            {
                RecievedJsonEventArgs e = new RecievedJsonEventArgs();
                e.obj = obj;
                e.senderID = ID;
                RecievedJson(this, e);
            }
        }
    }
    public class AuthenticatedEventArgs : EventArgs
    {
        public int senderID { get; set; }
        public string[] servers;
    }
}
