using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planner
{
    public class TaskTreeNode
    {
        public string name;
        public bool check;
        public List<TaskTreeNode> children;
        public readonly string ID;
        public int x = 0;
        public int y = 0;

        public TaskTreeNode(string name, string ID)
        {
            this.name = name;
            this.ID = ID;
            children = new List<TaskTreeNode>();
        }
    }
}
