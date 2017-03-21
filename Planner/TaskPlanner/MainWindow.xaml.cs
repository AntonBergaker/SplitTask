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
        Control[] sideWindowControls;

        public MainWindow()
        {
            InitializeComponent();

            var dispatcher = System.Windows.Application.Current.MainWindow.Dispatcher;

            Panel parentPanel = (Panel)menuFile.Parent;
            parentPanel.Children.Remove(menuFile);
            expander.Content = menuFile;

            sideWindowControls = new Control[] { textBoxTaskName, textBoxTaskDescription, datePickerDue};

            webClient = new TaskWebClient(tasks);
            Action realAction1 = HandleTasksRecieve;
            webClient.RecievedTasks += (sender, args) => dispatcher.BeginInvoke(realAction1);
            Action realAction2 = HandleTaskRecieve;
            webClient.RecievedTask += (sender, args) => dispatcher.BeginInvoke(realAction2);
            Action realAction3 = HandleTaskRename;
            webClient.RenamedTask += (sender, args) => dispatcher.BeginInvoke(realAction3);
            webClient.Connect("185.16.95.101");

            gridSplitter.DragDelta += SplitterNameDragDelta;
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
        }

        private void buttonNewSubtask_Click(object sender, RoutedEventArgs e)
        {
            if (taskTree.HasSelection)
            {
                Task task = new Task("");
                task.chooseID(randomGenerator);
                tasks.Add(task);
                webClient.TaskAdd(task);
                taskTree.AddNode(task,taskTree.selectedNodes[0].ID);
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

        #region Event Handlers

        private void HandleTasksRecieve()
        {
            PopulateList();
        }
        private void HandleTaskRecieve()
        {
            PopulateList();
        }
        private void HandleTaskRename()
        {
            //taskTree.Refresh();
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
        private void SideWindowClear()
        {
            textBoxTaskName.Text = "";
            textBoxTaskDescription.Text = "";
            datePickerDue.SelectedDate = null;
            SideWindowEnabled(false);
        }
        private void SideWindowUpdate(Task task)
        {
            textBoxTaskName.Text = task.title;
            textBoxTaskDescription.Text = task.description;
            datePickerDue.SelectedDate = task.timeDue;
            SideWindowEnabled(true);
        }
        private void SideWindowEnabled(bool enabled)
        {
            foreach (Control c in sideWindowControls)
             { c.IsEnabled = enabled; }
        }
    }
}
