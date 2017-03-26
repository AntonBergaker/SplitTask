using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SplitTask.Common;
using System.Net;
using System.Net.Sockets;

namespace SplitTask.WebHost
{
    class TaskServer
    {
        TaskCollection tasks;
        List<ClientHandler> clients;
        List<User> users;

        public TaskServer(TaskCollection tasks)
        {
            this.tasks = tasks;
            clients = new List<ClientHandler>();
        }
        /// <summary>
        /// Adds a client to the TaskServer
        /// </summary>
        /// <param name="client"></param>
        /// <returns>Returns true if the client was properly added</returns>
        public bool AddClient(TcpClient client)
        {
            i
        }
    }
}
