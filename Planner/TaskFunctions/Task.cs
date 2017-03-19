using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Planner
{
    class Task
    {
        public List<Task> subtasks = new List<Task>();

        public string ID {
            get { return id; }
        }

        public int Count {
            get { return subtasks.Count; }
        }
        public string title = "";
        public string description = "";
        public bool isFolder = false;
        public Task parent = null;
        public DateTime? timeCreated;
        public DateTime? timeCompleted;
        public DateTime? timeDue;
        private string id = "";

        public Task(string name)
        {
            title = name;
            timeCreated = DateTime.Now;
        }
        public Task()
        {
            timeCreated = DateTime.Now;
        }
        public Task(string name, string ID)
        {
            title = name;
            id = ID;
            timeCreated = DateTime.Now;
        }
        public JObject ToJObject()
        {
            JObject obj = new JObject();
            obj.Add("name", title);

            List<JObject> sublist = new List<JObject>();
            foreach (Task t in subtasks)
            { sublist.Add(t.ToJObject()); }

            JArray array = new JArray(sublist);
            obj.Add("subtasks", array);
            obj.Add("description", description);
            obj.Add("created", timeCreated);
            obj.Add("folder", isFolder);
            obj.Add("ID", ID);

            return obj;
        }
        public static Task Parse(JObject obj)
        {
            string name = TryReadValue("name", obj, "Unknown Task Name");
            string id = TryReadValue("ID", obj, "ID-Is-Lost-You-Should-Panic-Right-Now-Friend");
            Task task = new Task(name, id);
            JArray array = TryReadValue("subtasks",obj,new JArray());
            foreach (JObject j in array)
            { task.subtasks.Add(Parse(j)); }

            task.description = TryReadValue("description",obj,"");
            task.isFolder = TryReadValue("folder", obj , false);
            task.timeCreated = TryReadValue<DateTime?>("created", obj,null);
            task.timeDue = TryReadValue<DateTime?>("due", obj, null);
            task.timeCompleted = TryReadValue<DateTime?>("completed", obj, null);
            return task;
        }

        private static T TryReadValue<T>(string key, JObject obj, T defaultValue)
        {
            JToken token = obj[key];
            if (token == null)
            { return defaultValue; }
            return (T)Convert.ChangeType(token, typeof(T));
        }

        public void chooseID(Random randomGenerator)
        {
            byte[] randomValue = new byte[33];
            randomGenerator.NextBytes(randomValue);
            id = Convert.ToBase64String(randomValue).Replace("/", "-");
        }

        public override string ToString()
        {
            return title + '(' +id + ')';
        }
    }
}
