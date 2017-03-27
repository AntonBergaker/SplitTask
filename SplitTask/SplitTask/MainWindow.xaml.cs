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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Random randomGenerator = new Random();
        TaskCollection tasks = new TaskCollection();
        TaskWebClient webClient;
        Task sideWindowTask;
        Control[] sideWindowControls;

        EventHandler<TaskRenamedEventArgs> eventHandlerRenamed;
        EventHandler<TaskDescriptionChangedEventArgs> eventHandlerDescriptionChanged;
        EventHandler<TaskDateChangedEventArgs> eventHandlerDateDueChanged;

        public MainWindow()
        {
            InitializeComponent();

            var dispatcher = System.Windows.Application.Current.MainWindow.Dispatcher;

            Panel parentPanel = (Panel)menuFile.Parent;
            parentPanel.Children.Remove(menuFile);
            expander.Content = menuFile;

            eventHandlerRenamed = (sender, args) => dispatcher.BeginInvoke(
                 new Action(() => { Task_TaskRenamed(sender, args); }));
            eventHandlerDescriptionChanged = (sender, args) => dispatcher.BeginInvoke(
                 new Action(() => { Task_TaskDescriptionChanged(sender, args); }));
            eventHandlerDateDueChanged = (sender, args) => dispatcher.BeginInvoke(
                new Action(() => { Task_TaskDateDueChanged(sender, args); }));

            sideWindowControls = new Control[] { textBoxTaskName, textBoxTaskDescription, datePickerDue, toggleButtonFolder};
            SideWindowEnable(false);

            webClient = new TaskWebClient(tasks);
            webClient.RecievedTasks += (sender, args) => dispatcher.BeginInvoke(
                new Action(() => { HandleTasksRecieve(sender, args); }));
            webClient.RecievedTask += (sender, args) => dispatcher.BeginInvoke(
                new Action(() => { HandleTaskRecieve(sender, args); }));
            webClient.Connect("185.16.95.101");

            gridSplitter.DragDelta += SplitterNameDragDelta;
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
            if (width > 175)
            {
                mainGrid.ColumnDefinitions[0].Width = new GridLength(mainGrid.ColumnDefinitions[0].ActualWidth + e.HorizontalChange);
            }
        }


        private void buttonNewTask_Click(object sender, RoutedEventArgs e)
        {
            Task task = new Task("");
            task.GenerateID(randomGenerator);
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
                task.GenerateID(randomGenerator);
                tasks.Add(task,parentTask);
                webClient.TaskAdd(task,parentTask);
                taskTree.AddNode(task,parentTask.ID,true);
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

        private void HandleTaskRecieve(object sender, RecievedTaskEventArgs e)
        {
            Task task = e.task;
            string parent = e.parentTask;
            if (parent == null)
            { taskTree.AddNode(task);}
            else
            { taskTree.AddNode(task, parent, false); }
            
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
            SideWindowRemoveEvents();
            sideWindowTask = null;
            textBoxTaskName.Text = "";
            textBoxTaskDescription.Text = "";
            datePickerDue.SelectedDate = null;
            SideWindowEnable(false);
        }
        private void SideWindowRemoveEvents()
        {
            if (sideWindowTask != null)
            {
                var dispatcher = System.Windows.Application.Current.MainWindow.Dispatcher;
                sideWindowTask.TaskRenamed -= eventHandlerRenamed;
                sideWindowTask.TaskDescriptionChanged -= eventHandlerDescriptionChanged;
                sideWindowTask.TaskTimeDueChanged -= eventHandlerDateDueChanged;
            }
        }

        private void SideWindowUpdate(Task task)
        {
            task.TaskRenamed += eventHandlerRenamed;
            task.TaskDescriptionChanged += eventHandlerDescriptionChanged;
            task.TaskTimeDueChanged += eventHandlerDateDueChanged;

            SideWindowRemoveEvents();
            sideWindowTask = task;
            textBoxTaskName.Text = task.name;
            textBoxTaskDescription.Text = task.description;
            datePickerDue.SelectedDate = task.timeDue;
            SideWindowEnable(true);
        }

        private void Task_TaskRenamed(object sender, TaskRenamedEventArgs e)
        {
            if (sender != this)
            {
                textBoxTaskName.Text = e.newName;
            }
        }
        private void Task_TaskDescriptionChanged(object sender, TaskDescriptionChangedEventArgs e)
        {
            if (sender != this)
            {
                textBoxTaskDescription.Text = e.newDescription;
            }
        }
        private void Task_TaskDateDueChanged(object sender, TaskDateChangedEventArgs e)
        {
            if (sender != this)
            {
                datePickerDue.SelectedDate = e.newDate;
            }
        }

        private void SideWindowEnable(bool enabled)
        {
            foreach (Control c in sideWindowControls)
             { c.IsEnabled = enabled; }
        }

        private void textBoxTaskName_LostFocus(object sender, RoutedEventArgs e)
        {
            string newTitle = textBoxTaskName.Text;
            if (sideWindowTask != null)
            {
                if (newTitle == "")
                { textBoxTaskName.Text = sideWindowTask.name; }
                else
                {
                    sideWindowTask.Rename(newTitle, this);
                }
            }
        }


        private void textBoxTaskDescription_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ResetFocus(this);
            }
        }

        private void textBoxTaskDescription_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sideWindowTask != null)
            {
                sideWindowTask.DescriptionChange(textBoxTaskDescription.Text, this);
            }
        }

        private void textBoxTaskName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ResetFocus(this);
            }
        }
        private void ResetFocus(DependencyObject obj)
        {
            //Unfocusing was tricker than it looked
            var scope = FocusManager.GetFocusScope(obj); // elem is the UIElement to unfocus
            FocusManager.SetFocusedElement(scope, null); // remove logical focus
            Keyboard.ClearFocus(); // remove keyboard focus
        }

        private void toggleButtonFolder_Checked(object sender, RoutedEventArgs e)
        {
            if (sideWindowTask != null)
            { sideWindowTask.FolderChange(true, this); }
        }

        private void toggleButtonFolder_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sideWindowTask != null)
            { sideWindowTask.FolderChange(false, this); }
        }

        private void menuItemExport_Click(object sender, RoutedEventArgs e)
        {
            tasks.ExportFile("tasks.tlf");
        }

        private void datePickerDue_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sideWindowTask != null)
            { sideWindowTask.DateDueChange(datePickerDue.SelectedDate, this); }
        }
    }
}
