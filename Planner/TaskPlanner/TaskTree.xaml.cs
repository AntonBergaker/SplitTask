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
        public List<TaskTreeNode> selectedNodes;
        Dictionary<string,TaskTreeNode> nodeDictionary;

        public bool HasSelection { get { return selectedNodes.Count > 0; } }

        public TaskTree()
        {
            InitializeComponent();
            tasks = new List<TaskTreeNode>();
            visibleTasks = new List<TaskTreeNode>();
            selectedNodes = new List<TaskTreeNode>();
            nodeDictionary = new Dictionary<string, TaskTreeNode>();
        }
        public void AddNode(Task task)
        {
            TaskTreeNode node = new TaskTreeNode(task,0);
            NodeAddToTree(node);
        }

        public void AddNode(Task task, string parentTask)
        {
            TaskTreeNode parentNode = nodeDictionary[parentTask];
            TaskTreeNode node = new TaskTreeNode(task, parentNode.depth+1);
            parentNode.children.Add(node);
            parentNode.ExpanderRefresh();
            NodeAddToTree(node,parentNode);
        }
        private void NodeAddToTree(TaskTreeNode node, TaskTreeNode parentNode)
        {
            int index = stackPanel.Children.IndexOf(parentNode)+parentNode.children.Count;
            stackPanel.Children.Insert(index,node);
            nodeDictionary[node.ID] = node;
            AddEvents(node);
            foreach (TaskTreeNode n in node.children)
            { NodeAddToTree(n, node); }
        }

        private void NodeAddToTree(TaskTreeNode node)
        {
            stackPanel.Children.Add(node);
            nodeDictionary[node.ID] = node;
            AddEvents(node);
            foreach (TaskTreeNode n in node.children)
            { NodeAddToTree(n); }
        }
        public void Select(TaskTreeNode node)
        {
            foreach (TaskTreeNode n in selectedNodes)
            { n.Deselect(); }
            selectedNodes.Clear();
            selectedNodes.Add(node);
            node.Select();
            OnSelectionChanged();
        }
        private void AddEvents(TaskTreeNode node)
        {
            node.SelectionChanged += Node_SelectionChanged;
            node.TextUpdated += Node_TextUpdated;
            node.CheckUpdated += Node_CheckUpdated;
        }

        private void Node_CheckUpdated(object sender, CheckUpdatedEventArgs e)
        {
            OnCheckUpdated(e);
        }

        private void Node_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Select((TaskTreeNode)sender);
        }
        private void Node_TextUpdated(object sender, TextUpdatedEventArgs e)
        {
            OnTextUpdated(e);
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



        #region SelectionChanged
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
        #region TextUpdated
        public delegate void TextUpdatedEventHandler(object sender, TextUpdatedEventArgs e);
        public static readonly RoutedEvent TextUpdatedEvent = EventManager.RegisterRoutedEvent(
            "TextUpdated", RoutingStrategy.Bubble, typeof(TextUpdatedEventHandler), typeof(TaskTree));
        public event TextUpdatedEventHandler TextUpdated
        {
            add { AddHandler(TextUpdatedEvent, value); }
            remove { RemoveHandler(TextUpdatedEvent, value); }
        }

        private void OnTextUpdated(TextUpdatedEventArgs e)
        {
            TextUpdatedEventArgs ex = new TextUpdatedEventArgs(TaskTree.TextUpdatedEvent);
            ex.newName = e.newName;
            ex.task = e.task;
            RaiseEvent(ex);
        }
        #endregion
        #region CheckUpdated
        public delegate void CheckUpdatedEventHandler(object sender, CheckUpdatedEventArgs e);
        public static readonly RoutedEvent CheckUpdatedEvent = EventManager.RegisterRoutedEvent(
            "CheckUpdated", RoutingStrategy.Bubble, typeof(CheckUpdatedEventHandler), typeof(TaskTree));
        public event CheckUpdatedEventHandler CheckUpdated
        {
            add { AddHandler(CheckUpdatedEvent, value); }
            remove { RemoveHandler(CheckUpdatedEvent, value); }
        }

        private void OnCheckUpdated(CheckUpdatedEventArgs e)
        {
            CheckUpdatedEventArgs ev = new CheckUpdatedEventArgs(TaskTree.CheckUpdatedEvent);
            ev.task = e.task;
            ev.check = e.check;
            RaiseEvent(ev);
        }
        #endregion
    }
}
