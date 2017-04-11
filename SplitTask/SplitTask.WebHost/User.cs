using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplitTask.WebHost
{
    class User
    {
        public string username;
        public string id;
        public string displayname;
        public string[] connectedToLists;
        public string email;

        public User()
        {

        }

        public User(string username, string id, string displayname, string email)
        {
            this.id = id;
            this.displayname = displayname;
            this.username = username;
            this.email = email;
        }

        public User(JObject obj)
        {
            username = (string)obj["username"];
            id = (string)obj["id"];
            displayname = (string)obj["displayname"];
            connectedToLists = obj["lists"].ToObject<string[]>();
            email = (string)obj["email"];
        }
        public void GenerateID(Random randomGenerator)
        {
            byte[] randomValue = new byte[33];
            randomGenerator.NextBytes(randomValue);
            id = "U" + Convert.ToBase64String(randomValue).Replace("/", "-");
        }


    }
}
