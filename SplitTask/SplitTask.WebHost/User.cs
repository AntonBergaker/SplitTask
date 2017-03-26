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
        public string displayname;
        public List<TaskServer> connectedToServers;
        public string email;

        public User()
        {

        }
        public User ImportFromJson(JObject obj)
        {
            User user = new User();
            user.username = (string)obj["name"];
            user.displayname = (string)obj["displayname"];
            JArray array = (JArray)obj["servers"];

            return user;
        }
    }
}
