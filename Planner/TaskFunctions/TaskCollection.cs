using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planner
{
    class TaskCollection
    {
        public List<Task> tasks;
        public Dictionary<string, Task> IDDictionary;

        public TaskCollection()
        {
            IDDictionary = new Dictionary<string, Task>();
            tasks = new List<Task>();
        }
        public TaskCollection(List<Task> tasks)
        {
            IDDictionary = new Dictionary<string, Task>();
            this.tasks = tasks;
        }
        public void Remove(string taskID)
        {
            if (IDDictionary.ContainsKey(taskID))
            { Remove(IDDictionary[taskID]); }
        }

        public void Remove(Task task)
        {
            Remove(tasks, task);
        }
        public void Remove(List<Task> nodes, Task task)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Task t = nodes[i];
                if (t == task)
                {
                    nodes.Remove(t);
                    i--;
                }
                else
                {
                    Remove(t.subtasks, task);
                }
            }
        }
        public int Count
        { get { return tasks.Count; } }

        public void Add(Task task)
        {
            tasks.Add(task);
            if (!IDDictionary.ContainsKey(task.ID))
            { IDDictionary.Add(task.ID, task); }
        }
        public void Add(Task newTask, Task parentTask)
        {
            if (parentTask != null)
            {
                parentTask.subtasks.Add(newTask);
                IDDictionary.Add(newTask.ID, newTask);
            }
        }
        public void Rename(string ID, string name)
        {
            if (IDDictionary.ContainsKey(ID))
            {
                Rename(IDDictionary[ID], name);
            }
        }
        public void Rename(Task task, string name)
        {
            task.title = name;
        }
        public void ExportFile(string path)
        {
            Exporter.ExportToFile(tasks,path);
        }
        public string ExportString()
        {
            return Exporter.ExportToString(tasks);
        }
        public void ImportFile(string path)
        {
            tasks = Importer.ImportFromFile(path);
            UpdateIDDictionary();
        }
        public void ImportText(string text)
        {
            tasks = Importer.ImportFromText(text);
            UpdateIDDictionary();
        }
        private void UpdateIDDictionary()
        {
            IDDictionary.Clear();
            IDDictionaryAddFromList(tasks);
        }
        private void IDDictionaryAddFromList(List<Task> tasks)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                Task t = tasks[i];
                if (!IDDictionary.ContainsKey(t.ID))
                { IDDictionary.Add(t.ID,t); }
                IDDictionaryAddFromList(t.subtasks);
            }
        }
        
    }
}
