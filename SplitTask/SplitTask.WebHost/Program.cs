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
using System.Security.Cryptography;
using SplitTask.Common;

namespace SplitTask.WebHost
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskCollection tasks = new TaskCollection();
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

            string path = "server_credentials.blob";
            if (File.Exists(path))
            {
                byte[] blob = File.ReadAllBytes(path);
                RSA.ImportCspBlob(blob);
            }
            else
            {
                byte[] blob = RSA.ExportCspBlob(true);
                File.WriteAllBytes(path,blob);
            }

            WebServer server = new WebServer(RSA);
            server.Start(5171);

        }
    }
}
