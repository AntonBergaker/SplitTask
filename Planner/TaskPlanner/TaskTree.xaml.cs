using Planner;
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
using System.Windows.Shapes;

namespace TaskPlanner
{
    /// <summary>
    /// Interaction logic for TaskTree.xaml
    /// </summary>
    public partial class TaskTree : UserControl
    {
        List<TaskTreeNode> visibleTasks;
        List<TaskTreeNode> tasks;
        public TaskTreeNode selectedNode;

        public bool HasSelection { get { return selectedNode != null; } }

        public TaskTree()
        {
            InitializeComponent();
            tasks = new List<TaskTreeNode>();
            visibleTasks = new List<TaskTreeNode>();
        }
        public void AddNode(Task task)
        {
            TaskTreeNode node = new TaskTreeNode(task,0);
            NodeAddToTree(node);
        }

        public void AddNode(Task task, string parentTask)
        {

        }

        private void NodeAddToTree(TaskTreeNode node)
        {
            stackPanel.Children.Add(node);
            node.MouseDown += Node_MouseDown;
            foreach (TaskTreeNode n in node.children)
            { NodeAddToTree(n); }
        }
        public void Select(TaskTreeNode node)
        {
            selectedNode = node;
            selectedNode.Select();
            OnSelectionChanged();
        }

        private void Node_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Select((TaskTreeNode)sender);
        }

        public void RenameTask(string taskID)
        {
            
        }

        public void ClearAll()
        {
            stackPanel.Children.Clear();
            foreach (TaskTreeNode t in visibleTasks)
            {
                stackPanel.Children.Add(t);
            }
        }

        public void RemoveNode(string taskID)
        {
            RemoveNode(tasks, taskID);
        }
        public void RemoveNode(List<TaskTreeNode> nodes, string nodeID)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                TaskTreeNode n = nodes[i];
                if (n.ID == nodeID)
                {
                    nodes.Remove(n);
                    i--;
                }
                else
                {
                    RemoveNode(n.children, nodeID);
                }
            }
        }


        #region events
        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskTree));
        public event RoutedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        private void OnSelectionChanged()
        {
            RoutedEventArgs e = new RoutedEventArgs(TaskTree.SelectionChangedEvent);
            RaiseEvent(e);
        }
        #endregion
    }
}
