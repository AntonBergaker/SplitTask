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
        public static List<Task> ImportFromFile(string path)
        {
            JObject obj;

            try
            {
                obj = JObject.Parse(File.ReadAllText(path));
            }
            catch (JsonReaderException e)
            {
                obj = new JObject();
            }

            return TasklistFromJObject(obj);
        }

        public static List<Task> ImportFromText(string text)
        {
            JObject array;

            try
            {
                array = JObject.Parse(text);
            }
            catch (JsonReaderException e)
            {
                array = new JObject();
            }

            return TasklistFromJObject(array);
        }


        private static List<Task> TasklistFromJObject(JObject obj)
        {
            List<Task> list = new List<Task>();
            JArray array = (JArray)obj["tasks"];
            try
            {
                foreach (JObject nobj in array)
                {
                    Task task = Task.Parse(nobj);
                    list.Add(task);
                }
                return list;
            }
            catch (JsonReaderException e)
            {
                return list;
            }
            
        }
    }
}
