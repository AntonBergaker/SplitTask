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
        private Dictionary<string, TaskServer> taskServers;
        private RSACryptoServiceProvider RSA;
        private MySqlConnection SQLconnection;
        private ServerProperties serverProperties;

        public WebServer(RSACryptoServiceProvider RSA)
        {
            this.RSA = RSA;
            taskServers = new Dictionary<string, TaskServer>();

            serverProperties = new ServerProperties("settings.ini");

            StartSQL();
        }

        private void StartSQL()
        {
            string connectionString = "server="+serverProperties.sqlAddress+";uid="+serverProperties.sqlUser+";pwd="+serverProperties.sqlPassword+";database="+serverProperties.sqlDatabase+";";
            SQLconnection = new MySqlConnection(connectionString);
            SQLconnection.Open();
            Console.WriteLine("Connected to mySQL");
            string sql = "SHOW TABLES LIKE 'users'";
            MySqlCommand command = new MySqlCommand(sql, SQLconnection);
            bool tablesExist = true;

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read() == false)
                {
                    tablesExist = false;
                }
            }
            if (!tablesExist)
            {
                command = new MySqlCommand(File.ReadAllText("SQLscripts/create_tables.sql"),SQLconnection);
                command.ExecuteNonQuery();
                Console.WriteLine("Created tables");
            }
            SQLconnection.Close();
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
                ClientHandler client = new ClientHandler(clientCount, RSA, SQLconnection);
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
