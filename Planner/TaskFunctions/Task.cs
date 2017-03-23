using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TaskFunctions;

namespace Planner
{
    public class Task
    {
        public List<Task> subtasks = new List<Task>();

        public string ID {
            get { return id; }
        }

        public int Count {
            get { return subtasks.Count; }
        }
        public string name = "";
        public string description = "";
        public bool isFolder = false;
        public bool isCompleted = false;
        public DateTime? timeCreated;
        public DateTime? timeCompleted;
        public DateTime? timeDue;
        private string id = "";

        public Task(string name)
        {
            this.name = name;
            timeCreated = DateTime.Now;
        }
        public Task()
        {
            timeCreated = DateTime.Now;
        }
        public Task(string name, string ID)
        {
            this.name = name;
            id = ID;
            timeCreated = DateTime.Now;
        }
        public JObject ToJObject()
        {
            JObject obj = new JObject();
            obj.Add("name", name);

            List<JObject> sublist = new List<JObject>();
            foreach (Task t in subtasks)
            { sublist.Add(t.ToJObject()); }

            JArray array = new JArray(sublist);
            obj.Add("subtasks", array);
            obj.Add("done", isCompleted);
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
            task.isCompleted = TryReadValue("done", obj, false);
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
        
        public void Rename(string newName, object sender)
        {
            //If it was changed, raise events
            if (newName != name)
            {
                string oldName = name;
                name = newName;
                OnRenamed(newName,oldName,sender);
            }
        }
        public void Check(bool check, object sender)
        {
            if (check != isCompleted)
            {
                isCompleted = check;
                if (check)
                { timeCompleted = DateTime.Now; }
                OnChecked(check,sender);
            }
        }

        public override string ToString()
        {
            return name + '(' +id + ')';
        }


        public event EventHandler<TaskRenamedEventArgs> TaskRenamed;
        protected virtual void OnRenamed(string newName, string oldName, object sender)
        {
            if (TaskRenamed != null)
            {
                TaskRenamedEventArgs e = new TaskRenamedEventArgs();
                e.newName = newName;
                e.oldName = oldName;
                e.originalSender = sender;
                e.task = this;
                TaskRenamed(this, e);
            }
        }
        public event EventHandler<TaskCheckedEventArgs> TaskChecked;
        protected virtual void OnChecked(bool check, object sender)
        {
            if (TaskChecked != null)
            {
                TaskCheckedEventArgs e = new TaskCheckedEventArgs();
                e.check = check;
                e.originalSender = sender;
                e.task = this;
                TaskChecked(this, e);
            }
        }


    }
}
