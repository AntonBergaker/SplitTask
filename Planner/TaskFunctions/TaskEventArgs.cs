using Planner;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFunctions
{
    public class TaskEventArgs : EventArgs
    {
        public object originalSender;
        public Task task;
    }
    public class TaskRenamedEventArgs : TaskEventArgs
    {
        public string newName;
        public string oldName;
    }
    public class TaskCheckedEventArgs : TaskEventArgs
    {
        public bool check;
    }
    public class TaskDescriptionChangedEventArgs : TaskEventArgs
    {
        public string oldDescription;
        public string newDescription;
    }
    public class TaskAddedEventArgs : TaskEventArgs
    {
        public Task parentTask;
        public Task taskUnder;
    }
    public class TaskFolderChangedEventArgs : TaskEventArgs
    {
        public bool isFolder;
    }

}
