using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public DateTime timeCreated;
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

        public void chooseID(Random randomGenerator)
        {
            byte[] randomValue = new byte[33];
            randomGenerator.NextBytes(randomValue);
            id = Convert.ToBase64String(randomValue).Replace("/", "-");
        }

        public override string ToString()
        {
            return title;
        }
    }
}
