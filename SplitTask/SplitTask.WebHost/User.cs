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
        public string password;

        public User()
        {

        }
        public void GenerateID(Random randomGenerator)
        {
            byte[] randomValue = new byte[33];
            randomGenerator.NextBytes(randomValue);
            id = "U" + Convert.ToBase64String(randomValue).Replace("/", "-");
        }

        public static User ImportFromJson(JObject obj)
        {
            User user = new User();
            user.username = (string)obj["username"];
            user.id = (string)obj["id"];
            user.displayname = (string)obj["displayname"];
            user.connectedToLists = obj["lists"].ToObject<string[]>();
            user.email = (string)obj["email"];
            user.password = (string)obj["password"];

            return user;
        }
    }
}
