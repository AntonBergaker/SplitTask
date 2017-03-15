using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Planner
{
    static class Exporter
    {
        public static void ExportToFile(List<Task> tasks, string path)
        {
            JObject array = JObjectFromTasklist(tasks);
            File.WriteAllText(path,array.ToString());
        }
        public static string ExportToString(List<Task> tasks)
        {
            JObject array = JObjectFromTasklist(tasks);
            return array.ToString();
        }

        private static JObject JObjectFromTasklist(List<Task> tasks)
        {
            JObject obj = new JObject();
            List<JObject> list = new List<JObject>();
            for (int i = 0; i < tasks.Count; i++)
            {
                Task t = tasks[i];
                JObject nObj = CreateJObjectFromTask(t);
                list.Add(nObj);
            }

            obj.Add("tasks", new JArray(list));

            return obj;
        }

        private static JObject CreateJObjectFromTask(Task task)
        {
            JObject obj = new JObject();
            obj.Add("name", task.title);

            List<JObject> sublist = new List<JObject>();
            foreach (Task t in task.subtasks)
            { sublist.Add(CreateJObjectFromTask(t)); }

            JArray array = new JArray(sublist);
            obj.Add("subtasks", array);
            obj.Add("description", task.description);
            obj.Add("created", task.timeCreated);
            obj.Add("folder", task.isFolder);
            obj.Add("ID", task.ID);

            return obj;
        }
    }
}
