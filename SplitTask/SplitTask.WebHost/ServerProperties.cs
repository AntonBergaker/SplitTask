using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using System.IO;
using IniParser.Parser;
using IniParser.Model;

namespace SplitTask.WebHost
{
    class ServerProperties
    {
        public string sqlAddress;
        public string sqlPassword;
        public string sqlUser;
        public string sqlDatabase;

        public ServerProperties(string path)
        {
            if (!File.Exists(path))
            {
                CreateServerProperties(path);
            }
            if (File.Exists(path))
            {
                FileIniDataParser ini = new FileIniDataParser();
                IniData data = ini.ReadFile(path);

                sqlAddress = data["SQL"]["address"];
                sqlUser = data["SQL"]["user"];
                sqlPassword = data["SQL"]["password"];
                sqlDatabase = data["SQL"]["database"];
                Console.WriteLine("Imported server settings");
            }
            else
            { Console.WriteLine("Could not create server settings"); }
        }
        private void CreateServerProperties(string path)
        {
            IniData data = new IniData();
            Console.WriteLine("No ini file detected, creating.");
            data["SQL"]["address"] = "127.0.0.1";
            data["SQL"]["user"] = "user";
            data["SQL"]["password"] = "password";
            data["SQL"]["database"] = "database";
            FileIniDataParser ini = new FileIniDataParser();
            ini.WriteFile(path,data);
        }
    }
}
