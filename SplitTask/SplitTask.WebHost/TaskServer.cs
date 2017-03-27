using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SplitTask.Common;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Newtonsoft.Json.Linq;

namespace SplitTask.WebHost
{
    class TaskServer
    {
        public string ID;
        public TaskCollection tasks;
        public List<ClientHandler> clients;
        public List<User> users;

        public TaskServer(string ID)
        {
            this.ID = ID;
            clients = new List<ClientHandler>();
            tasks = new TaskCollection();
            if (File.Exists(ID+".tlf"))
            {
                tasks.ImportFile(ID + ".tlf");
            }
        }


        /// <summary>
        /// Adds a clienthandler to the TaskServer. Only add if the client is already authorized
        /// </summary>
        /// <param name="client"></param>
        /// <returns>Returns true if the client was properly added</returns>
        public bool AddClient(ClientHandler client)
        {
            Console.WriteLine(client.user + " joined "+ ID);
            clients.Add(client);
            client.SendData(tasks.ExportString());
            client.RecievedJson += Client_RecievedJson;
            return true;
        }

        private void Client_RecievedJson(object sender, RecievedJsonEventArgs e)
        {
            HandleData(e.obj, e.senderID);
        }

        /// <summary>
        /// Sends incomming JSON to all other clients
        /// </summary>
        private void ForwardToClients(JObject obj,int senderID)
        {
            string data = obj.ToString();
            foreach (ClientHandler c in clients)
            {
                if (c.ID != senderID)
                {
                    c.SendData(data);
                }
            }
        }

        /// <summary>
        /// Adds a user so that the user can in the future connect to this TaskServer
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Returns true if the addition was successful</returns>
        public bool AddUser(User user)
        {
            users.Add(user);
            return true;
        }


        private void HandleData(JObject obj, int senderID)
        {
            string type = (string)obj["type"];
            Common.Task task;
            switch (type)
            {
                case "AddTask":
                    task = Common.Task.Parse((JObject)obj["task"]);
                    string parentTask = (string)obj["parent"];
                    if (parentTask != null)
                    {
                        tasks.Add(task, parentTask);
                    }
                    else
                    {
                        tasks.Add(task);
                    }
                    Console.WriteLine("made a new task: " + task.name + "(" + task.ID + ")");
                    ForwardToClients(obj, senderID);
                    break;
                case "RenameTask":
                    string taskID = (string)obj["ID"];
                    string newName = (string)obj["newName"];
                    tasks.Rename(taskID, newName);
                    Console.WriteLine("Renamed task: " + newName + "(" + taskID + ")");
                    ForwardToClients(obj, senderID);
                    break;
                case "CheckTask":
                    taskID = (string)obj["ID"];
                    bool check = (bool)obj["check"];
                    tasks.Check(taskID, check, this);
                    Console.WriteLine("{0} the task: " + taskID, check ? "Checked" : "Unchecked");
                    ForwardToClients(obj, senderID);
                    break;
                case "DescriptionChange":
                    taskID = (string)obj["ID"];
                    string description = (string)obj["description"];
                    tasks.DescriptionChange(taskID, description, this);
                    Console.WriteLine("Added the description \"" + description + "\" to " + taskID);
                    ForwardToClients(obj, senderID);
                    break;
                case "FolderChange":
                    taskID = (string)obj["ID"];
                    bool isFolder = (bool)obj["isFolder"];
                    tasks.FolderChange(taskID, isFolder, this);
                    Console.WriteLine("Set the task " + taskID + " to a {0}", isFolder ? "folder" : "task");
                    ForwardToClients(obj, senderID);
                    break;
            }
        }
    }
}
