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
using Newtonsoft.Json.Linq;

namespace SplitTask.WebHost
{
    class Program
    {
        static void Main(string[] args)
        {
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

            string private_cred_path = "private_server_credentials.blob";
            string public_cred_path = "public_server_credentials.blob";

            if (File.Exists(private_cred_path))
            {
                byte[] blob = File.ReadAllBytes(private_cred_path);
                RSA.ImportCspBlob(blob);
                Console.WriteLine("Imported server credentials from "+private_cred_path);
            }
            else
            {
                Console.WriteLine("Could not find server credentials!");
                byte[] blob = RSA.ExportCspBlob(true);
                File.WriteAllBytes(private_cred_path,blob);
                blob = RSA.ExportCspBlob(false);
                File.WriteAllBytes(public_cred_path, blob);
                Console.WriteLine("Creating new server credentials at "+private_cred_path+"\nDo not lose these or all clients will have to update their connection");
            }
           

            WebServer server = new WebServer(RSA);
            server.Start(5171);
        }


    }
}
