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
using MySql.Data.MySqlClient;

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
        private readonly byte[] connectionBytes = new byte[] { 0x8a, 0xe1, 0x44, 0x72, 0xe1, 0xf3 };
        private readonly byte[] setupBytes = new byte[] { 0x12, 0x43, 0x1a, 0x71, 0x30, 0x10 };
        private readonly byte[] hasKeyBytes = new byte[] { 0x74, 0xf0, 0x4d, 0x5e, 0x7d, 0xce };
        RSACryptoServiceProvider RSA;
        MySqlConnection SQL;
        ICryptoTransform encryptor;
        ICryptoTransform decryptor;

        public ClientHandler(int ID, RSACryptoServiceProvider RSA, MySqlConnection SQLConnection)
        {
            this.ID = ID;
            this.RSA = RSA;
            this.SQL = SQLConnection;
        }

        public void StartClient(TcpClient client)
        {
            this.client = client;
            stream = client.GetStream();
            thread = new Thread(RecieveHandShake);
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

        private void RecieveHandShake()
        {
            //Find out what the client wants to do depending on the first 6 bytes
            byte[] recievedData = RecieveUnencryptedData();
            if (recievedData.Length > 6)
            {
                byte[] header = recievedData.Take(6).ToArray();
                byte[] payload = recievedData.Skip(6).ToArray();
                //Very random headers to avoid making connections with random data
                if (header.SequenceEqual(connectionBytes))
                {
                    ConnectClient(payload);
                }
                else if (header.SequenceEqual(setupBytes))
                {
                    SetupClient(payload);
                }
                else
                { Disconnect(); }
            }
        }

        private void Disconnect()
        {
            client.Close();
        }

        private void SetupClient(byte[] recievedData)
        {
            if (recievedData.Length >= 128)
            {
                byte[] sensitiveData = RSA.Decrypt(recievedData.Take(128).ToArray(), false);
                //If the client sent bytes that are properly decoded
                if (sensitiveData.Take(6).ToArray().SequenceEqual(hasKeyBytes))
                {
                    byte[] key = sensitiveData.Skip(6).Take(32).ToArray();
                    byte[] IV = sensitiveData.Skip(38).Take(16).ToArray();
                }
                else
                {
                    //TODO send back to client that their keys are incorrect
                }
            }
        }


        private void ConnectClient(byte[] recievedData)
        {
            if (recievedData.Length > 129)
            {
                //First 128 bytes get decoded with RSA. The following information gets decoded with RIJ.
                byte[] sensitiveData = RSA.Decrypt(recievedData.Take(128).ToArray(),false);
                byte[] key = sensitiveData.Take(32).ToArray();
                byte[] IV = sensitiveData.Skip(32).Take(16).ToArray();

                //First 64 bytes of password
                byte[] passwordPart1 = sensitiveData.Skip(48).Take(64).ToArray();

                RijndaelManaged RIJ = new RijndaelManaged();
                RIJ.IV = IV;
                RIJ.Key = key;

                decryptor = RIJ.CreateDecryptor(RIJ.Key, RIJ.IV);
                encryptor = RIJ.CreateEncryptor(RIJ.Key, RIJ.IV);

                Console.WriteLine("Recieved Password");

                //Last 64 bytes of password
                byte[] passwordAndUsername = Decrypt(recievedData.Skip(128).ToArray());

                byte[] passwordPart2 = passwordAndUsername.Take(64).ToArray();

                byte[] password = passwordPart1.Concat(passwordPart2).ToArray();

                //The username is at the end of the bytestream
                string username = Encoding.UTF8.GetString(passwordAndUsername.Skip(64).ToArray());

                bool correctPassword = false;
                string userID = "";
                string email = "";
                string displayname = "";

                SQL.Open();
                MySqlCommand command = new MySqlCommand("SELECT PasswordHash,userID,DisplayName,Email from users where DisplayName=@param_val_1", SQL);
                command.Parameters.AddWithValue("@param_val_1", username);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        byte[] storedPassword = new byte[64];
                        reader.GetBytes(0, 0, storedPassword, 0, 64);
                        if (password.SequenceEqual(storedPassword))
                        {
                            correctPassword = true;
                            userID = reader.GetInt32(1).ToString();
                            displayname = reader.GetString(2);
                            email = reader.GetString(3);
                        }
                        else
                        { Console.WriteLine("A user calling himself " + username + " tried to log in, but the password was incorrect"); }
                    }
                    else
                    { Console.WriteLine("A user calling himself "+username+" tried to log in, but no such user existed in the database."); }
                }

                if (correctPassword)
                {
                    user = new User(username,userID,displayname,email);

                    Console.WriteLine("Logged in " + username);

                    SendData("ey waddup?");

                    string[] servers = RecieveDataString().Split(',');

                    OnAuthenticated(servers);
                    MainLoop();
                }

                SQL.Close();
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
                    timeoutTimer -= 2;
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
