using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SplitTask.Common
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
                JObject nObj = t.ToJObject();
                list.Add(nObj);
            }
            obj.Add("type", "TaskList");
            obj.Add("tasks", new JArray(list));

            return obj;
        }
    }
}
