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
        public static void Export(List<Task> tasks, string path)
        {
            List<JObject> list = new List<JObject>();
            for (int i = 0; i < tasks.Count; i++)
            {
                Task t = tasks[i];
                JObject obj = CreateJObjectFromTask(t);
                list.Add(obj);
            }

            File.WriteAllText(path,new JArray(list).ToString());
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
