using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFunctions;

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
        public void Check(string taskID, bool isChecked, object sender)
        {
            if (IDDictionary.ContainsKey(taskID))
            {
                IDDictionary[taskID].Check(isChecked,sender);
            }
        }
        public void Check(string taskID, bool isChecked)
        {
            Check(taskID, isChecked, this);
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

        public void Add(Task task, object sender = null)
        {
            tasks.Add(task);
            if (!IDDictionary.ContainsKey(task.ID))
            {
                IDDictionary.Add(task.ID, task);
                OnTaskAdded(task, null, null, sender);
            }
        }
        public void Add(Task newTask, Task parentTask, object sender = null)
        {
            if (parentTask != null)
            {
                parentTask.subtasks.Add(newTask);
                IDDictionary.Add(newTask.ID, newTask);
                OnTaskAdded(newTask, parentTask, null, sender);
            }
        }
        public void Add(Task newTask, string parentTaskID)
        {
            if (IDDictionary.ContainsKey(parentTaskID))
            {
                Add(newTask, IDDictionary[parentTaskID]);
            }
        }
        public void DescriptionChange(string ID, string newDescription, object sender)
        {
            if (IDDictionary.ContainsKey(ID))
            {
                IDDictionary[ID].DescriptionChange(newDescription, sender);
            }
        }
        public void FolderChange(string ID, bool isFolder, object sender)
        {
            if (IDDictionary.ContainsKey(ID))
            {
                IDDictionary[ID].FolderChange(isFolder,sender);
            }
        }

        public void Rename(string ID, string name, object sender)
        {
            if (IDDictionary.ContainsKey(ID))
            {
                IDDictionary[ID].Rename(name, sender);
            }
        }
        public void Rename(string ID, string name)
        {
            Rename(ID, name, this);
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

        public event EventHandler<TaskAddedEventArgs> TaskAdded;
        private void OnTaskAdded(Task task, Task taskParent, Task taskUnder,object sender)
        {
            if (TaskAdded != null)
            {
                TaskAddedEventArgs e = new TaskAddedEventArgs();
                e.task = task;
                e.taskUnder = taskUnder;
                e.parentTask = taskParent;
                e.originalSender = sender;
                TaskAdded(this, e);
            }
        }
        
    }
}