using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;

namespace Planner
{
    static class Importer
    {
        public static List<Task> Import(string path)
        {
            List<Task> list = new List<Task>();

            try
            {
                JArray array = JArray.Parse(File.ReadAllText(path));

                foreach (JObject obj in array)
                {
                    Task task = CreateTaskFromJObject(obj);
                    list.Add(task);
                }
                return list;
            } catch(JsonReaderException e)
            {
                return list;
            }
        }

        private static Task CreateTaskFromJObject(JObject obj)
        {
            string name = (string)obj["name"];
            string id = (string)obj["ID"];
            Task task = new Task(name,id);
            JArray array = (JArray)obj["subtasks"];
            foreach (JObject j in array)
            { task.subtasks.Add(CreateTaskFromJObject(j)); }

            task.description = (string)obj.GetValue("description");
            task.isFolder = (bool)obj.GetValue("folder");
            task.timeCreated = (DateTime)obj.GetValue("created");
            return task;
        }
    }
}
