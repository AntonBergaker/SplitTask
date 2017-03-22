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
using TaskFunctions;

namespace TaskPlanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Random randomGenerator = new Random();
        TaskCollection tasks = new TaskCollection();
        TaskWebClient webClient;
        Task sideWindowTask;
        Control[] sideWindowControls;

        public MainWindow()
        {
            InitializeComponent();

            var dispatcher = System.Windows.Application.Current.MainWindow.Dispatcher;

            Panel parentPanel = (Panel)menuFile.Parent;
            parentPanel.Children.Remove(menuFile);
            expander.Content = menuFile;

            sideWindowControls = new Control[] { textBoxTaskName, textBoxTaskDescription, datePickerDue};
            SideWindowEnable(false);

            webClient = new TaskWebClient(tasks);
            webClient.RecievedTasks += (sender, args) => dispatcher.BeginInvoke(
                new Action(() => { HandleTasksRecieve(sender, args); }));
            webClient.RecievedTask += (sender, args) => dispatcher.BeginInvoke(
                new Action(() => { HandleTaskRecieve(sender, args); }));
            webClient.RenamedTask += (sender, args) => dispatcher.BeginInvoke(
                new Action(() => { HandleTaskRename(sender, args); }));
            webClient.CheckedTask += (sender, args) => dispatcher.BeginInvoke(
                new Action(() => { HandleTaskCheck(sender, args); }));
            webClient.Connect("185.16.95.101");

            gridSplitter.DragDelta += SplitterNameDragDelta;
        }

        private void taskTree_CheckUpdated(object sender, CheckUpdatedEventArgs e)
        {
            webClient.TaskCheck(e.task.ID,e.check);
            tasks.Check(e.task.ID,e.check);
        }

        private void taskTree_TextUpdated(object sender, TextUpdatedEventArgs e)
        {
            webClient.TaskRename(e.task,e.newName);
            tasks.Rename(e.task, e.newName);
            if (sideWindowTask.ID == e.task.ID)
            { SideWindowUpdate(e.task); }
        }

        private void taskTree_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (taskTree.selectedNodes.Count == 1)
            {
                SideWindowUpdate(taskTree.selectedNodes[0].task);
                buttonNewSubtask.IsEnabled = true;
            }
            else
            {
                SideWindowClear();
                buttonNewSubtask.IsEnabled = false;
            }
        }

        private void SplitterNameDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double width = mainGrid.ColumnDefinitions[0].ActualWidth + e.HorizontalChange;
            if (width > 10)
            {
                mainGrid.ColumnDefinitions[0].Width = new GridLength(mainGrid.ColumnDefinitions[0].ActualWidth + e.HorizontalChange);
            }
        }


        private void buttonNewTask_Click(object sender, RoutedEventArgs e)
        {
            Task task = new Task("");
            task.chooseID(randomGenerator);
            tasks.Add(task);
            webClient.TaskAdd(task);
            taskTree.AddNode(task);
            taskTree.RenameTask(task.ID);
            e.Handled = true;
        }

        private void buttonNewSubtask_Click(object sender, RoutedEventArgs e)
        {
            if (taskTree.HasSelection)
            {
                Task task = new Task("");
                Task parentTask = taskTree.selectedNodes[0].task;
                task.chooseID(randomGenerator);
                tasks.Add(task,parentTask);
                webClient.TaskAdd(task,parentTask);
                taskTree.AddNode(task,parentTask.ID);
                taskTree.RenameTask(task.ID);
            }
        }

        private void PopulateList()
        {
            taskTree.ClearAll();
            foreach (Task t in tasks.tasks)
            {
                taskTree.AddNode(t);
            }

        }

        #region WebClient Event Handlers

        private void HandleTasksRecieve(object sender, RecievedTasksEventArgs e)
        {
            PopulateList();
        }
        private void HandleTaskCheck(object sender, CheckedTaskEventArgs e)
        {
            string id = e.taskID;
            taskTree.RefreshNode(id);
        }

        private void HandleTaskRecieve(object sender, RecievedTaskEventArgs e)
        {
            Task task = e.task;
            string parent = e.parentTask;
            if (parent == null)
            { taskTree.AddNode(task);}
            else
            { taskTree.AddNode(task, parent); }
            
        }
        private void HandleTaskRename(object sender, RenamedTaskEventArgs e)
        {
            string id = e.taskID;
            taskTree.RefreshNode(id);
        }

        #endregion

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Tasklist Files|*.tlf";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tasks.ImportFile(dialog.FileName);
                PopulateList();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            webClient.RequestClose();
        }

        private void SideWindowClear()
        {
            textBoxTaskName.Text = "";
            textBoxTaskDescription.Text = "";
            datePickerDue.SelectedDate = null;
            SideWindowEnable(false);
        }
        private void SideWindowUpdate(Task task)
        {
            sideWindowTask = task;
            textBoxTaskName.Text = task.title;
            textBoxTaskDescription.Text = task.description;
            datePickerDue.SelectedDate = task.timeDue;
            SideWindowEnable(true);
        }
        private void SideWindowEnable(bool enabled)
        {
            foreach (Control c in sideWindowControls)
             { c.IsEnabled = enabled; }
        }
    }
}
