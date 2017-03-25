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
using SplitTask.Common;

namespace TaskPlanner
{
    /// <summary>
    /// Interaction logic for TaskTreeNodeControl.xaml
    /// </summary>
    public partial class TaskTreeNode : UserControl
    {
        public Task task;
        public List<TaskTreeNode> children;
        public bool isExpanded;
        public int depth;
        public string ID;
        

        private SolidColorBrush defaultExpanderBrush;

        public TaskTreeNode(Task task, int depth)
        {
            InitializeComponent();
            this.task = task;
            this.depth = depth;
            this.ID = task.ID;

            var dispatcher = Application.Current.MainWindow.Dispatcher;

            task.TaskChecked += (sender, args) => dispatcher.BeginInvoke(
                new Action(() => { Task_TaskChecked(sender, args); }));
            task.TaskRenamed += (sender, args) => dispatcher.BeginInvoke(
                new Action(() => { Task_TaskRenamed(sender, args); }));
            task.TaskFolderChanged += (sender, args) => dispatcher.BeginInvoke(
                new Action(() => { Task_TaskFolderChanged(sender, args); }));

            isExpanded = true;
            defaultExpanderBrush = (SolidColorBrush)expanderArrow.Foreground;

            children = new List<TaskTreeNode>();
            foreach (Task t in task.subtasks)
            { children.Add(new TaskTreeNode(t,depth+1)); }
            Refresh();
        }

        private void Task_TaskFolderChanged(object sender, TaskFolderChangedEventArgs e)
        {
            if (e.originalSender != this)
            { Refresh(); }
        }

        private void Task_TaskRenamed(object sender, TaskRenamedEventArgs e)
        {
            if (e.originalSender != this)
            { Refresh(); }
        }

        private void Task_TaskChecked(object sender, TaskCheckedEventArgs e)
        {
            if (e.originalSender != this)
            { Refresh(); }
        }

        public void Refresh()
        {
            textBox.Text = task.name;
            checkBox.IsChecked = task.isCompleted;
            offsetGrid.Margin = new Thickness(depth * 30, 0, 0, 0);
            ExpanderRefresh();
            FolderRefresh();
        }
        public void Rename()
        {
            //No idea why just a simple .Focus() doesn't work.
            textBox.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                textBox.Focus();
            }));
        }

        private void FolderRefresh()
        {
            if (task.isFolder)
            {
                folderIcon.IsEnabled = true;
                folderIcon.Visibility = Visibility.Visible;
                checkBox.IsEnabled = false;
                checkBox.Visibility = Visibility.Hidden;
            }
            else
            {
                folderIcon.IsEnabled = false;
                folderIcon.Visibility = Visibility.Hidden;
                checkBox.IsEnabled = true;
                checkBox.Visibility = Visibility.Visible;
            }
        }

        public void ExpanderRefresh()
        {
            if (children.Count == 0)
            { expanderArrow.Foreground = null; }
            else
            { expanderArrow.Foreground = defaultExpanderBrush; }
            if (isExpanded)
            { expanderArrow.RenderTransform = new RotateTransform(45); }
            else
            { expanderArrow.RenderTransform = new RotateTransform(0); }
        }

        public void Select()
        {
            background.Fill = new SolidColorBrush(Colors.LightGray);
        }
        public void Deselect()
        {
            background.Fill = new SolidColorBrush(Colors.Transparent);
        }

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            OnSelectionChanged();
        }
        private void grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnSelectionChanged();
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            textBox.Focus();
            textBox.SelectAll();
            e.Handled = true;
        }
        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            task.Rename(textBox.Text,this);
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            OnCheckUpdated();
        }
        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            OnCheckUpdated();
        }

        private void checkBox_Click(object sender, RoutedEventArgs e)
        {
            OnCheckUpdated();
        }
        private void OnCheckUpdated()
        {
            task.Check((bool)checkBox.IsChecked,this);
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //Unfocusing was tricker than it looked
                var scope = FocusManager.GetFocusScope(textBox); // elem is the UIElement to unfocus
                FocusManager.SetFocusedElement(scope, null); // remove logical focus
                Keyboard.ClearFocus(); // remove keyboard focus
            }
        }

        private void expanderWrapper_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ExpanderRefresh();
            //Let the tasktree update its expand setting so it can create the necessary child nodes
            OnExpandUpdated(!isExpanded);
        }


        #region SelectionChanged
        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TaskTreeNode));
        public event RoutedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        private void OnSelectionChanged()
        {
            RoutedEventArgs e = new RoutedEventArgs(TaskTreeNode.SelectionChangedEvent);
            RaiseEvent(e);
        }
        #endregion
        #region ExpandUpdated
        public delegate void ExpandUpdatedEventHandler(object sender, ExpandUpdatedEventArgs e);
        public static readonly RoutedEvent ExpandUpdatedEvent = EventManager.RegisterRoutedEvent(
            "ExpandUpdated", RoutingStrategy.Bubble, typeof(ExpandUpdatedEventHandler), typeof(TaskTreeNode));
        public event ExpandUpdatedEventHandler ExpandUpdated
        {
            add { AddHandler(ExpandUpdatedEvent, value); }
            remove { RemoveHandler(ExpandUpdatedEvent, value); }
        }

        private void OnExpandUpdated(bool expanded)
        {
            //Only pass the event if it was changed
            if (isExpanded != expanded)
            {
                ExpandUpdatedEventArgs e = new ExpandUpdatedEventArgs(TaskTreeNode.ExpandUpdatedEvent);
                e.task = task;
                e.expanded = expanded;
                RaiseEvent(e);
            }
        }
        #endregion

    }

    public class ExpandUpdatedEventArgs : RoutedEventArgs
    {
        public Task task;
        public bool expanded;
        public ExpandUpdatedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
    }
}
