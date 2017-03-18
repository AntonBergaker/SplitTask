using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planner
{
    class TaskTreeNode
    {
        public Task task;
        public List<TaskTreeNode> children;
        public string ID;
        public int x = 0;
        public int y = 0;

        public TaskTreeNode(Task task)
        {
            this.task = task;
            this.ID = task.ID;
            children = new List<TaskTreeNode>();
        }
    }
}
