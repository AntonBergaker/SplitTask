using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Planner;
using System.Windows.Shapes;

namespace TaskPlanner
{
    /// <summary>
    /// Interaction logic for TaskTreeNodeControl.xaml
    /// </summary>
    public partial class TaskTreeNode : UserControl
    {
        public Task task;
        public List<TaskTreeNode> children;
        public bool isCollapsed;
        public int depth;
        public string ID;

        public TaskTreeNode(Task task, int depth)
        {
            InitializeComponent();
            this.task = task;
            this.depth = depth;
            this.ID = task.ID;
            textBox.Text = task.title;
            checkBox.IsChecked = task.isCompleted;
            offsetGrid.Margin = new Thickness(depth*30,0,0,0);
            children = new List<TaskTreeNode>();
            foreach (Task t in task.subtasks)
            { children.Add(new TaskTreeNode(t,depth+1)); }
        }
        public void Select()
        {
            background.Fill = new SolidColorBrush(Colors.LightGray);
        }
    }
}
