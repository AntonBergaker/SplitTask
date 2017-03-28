using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using SplitTask;
using System.Threading;
using SplitTask.Common;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using MySql.Data.MySqlClient;

namespace SplitTask.WebHost
{
    class WebServer
    {
        private List<ClientHandler> clients = new List<ClientHandler>();
        private TcpListener serverSocket;
        private Dictionary<string, User> usersByKey;
        private Dictionary<string, User> usersByName;
        private Dictionary<string, TaskServer> taskServers;
        private RSACryptoServiceProvider RSA;
        private MySqlConnection SQLconnection;

        public WebServer(RSACryptoServiceProvider RSA)
        {
            this.RSA = RSA;
            usersByKey = new Dictionary<string, User>();
            usersByName = new Dictionary<string, User>();
            taskServers = new Dictionary<string, TaskServer>();

            StartSQL();
            ImportUsers();
            
        }

        private void StartSQL()
        {
            string connectionString = "";
            SQLconnection = new MySqlConnection(connectionString);
            SQLconnection.Open();
            string sql = "SELECT * FROM users";
            MySqlCommand command = new MySqlCommand(sql, SQLconnection);
            MySqlDataReader reader = command.ExecuteReader();
            using (reader)
            {
                reader.Read();
                reader.Read();
                Console.WriteLine(reader.GetString(0));
            }
        }

        private void ImportUsers()
        {
            string path = "users.json";
            if (File.Exists(path))
            {
                JArray array = JArray.Parse(File.ReadAllText(path));
                foreach (JToken t in array)
                {
                    JObject obj = (JObject)t;
                    User user = User.ImportFromJson(obj);
                    usersByKey.Add(user.id,user);
                    usersByName.Add(user.username, user);
                }
                Console.WriteLine("Imported users from "+path);
            }
            else
            { Console.WriteLine("No users found"); }
        }

        public void Start(int port)
        {
            serverSocket = new TcpListener(IPAddress.Any, port);

            try
            {
                serverSocket.Start();
                MainLoop();
            }
            catch (Exception ex)
            { Console.WriteLine(ex.ToString()); }

            serverSocket.Stop();
        }

        private void MainLoop()
        {
            TcpClient clientSocket;
            int clientCount = 0;
            //Look for new clients
            while (true)
            {
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Client Connected");
                ClientHandler client = new ClientHandler(clientCount, RSA, usersByKey, usersByName);
                clientCount++;

                client.Authenticated += Client_Authenticated;

                clients.Add(client);
                client.StartClient(clientSocket);
            }
        }

        private void Client_Authenticated(object sender, AuthenticatedEventArgs e)
        {
            string[] servers = e.servers;
            foreach (string s in servers)
            {
                if (!taskServers.ContainsKey(s))
                {
                    TaskServer server = new TaskServer(s);
                    taskServers.Add(s,server);
                }

                taskServers[s].AddClient((ClientHandler)sender);
            }
        }
    }
}
