using Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public MainWindow()
        {
            InitializeComponent();

            var dispatcher = System.Windows.Application.Current.MainWindow.Dispatcher;

            Panel parentPanel = (Panel)menuFile.Parent;
            parentPanel.Children.Remove(menuFile);
            expander.Content = menuFile;

            Control[] sideWindow = new Control[] { textBoxTaskName, textBoxTaskDescription, datePickerDue};
            foreach (Control c in sideWindow)
            { c.IsEnabled = false; }

            webClient = new TaskWebClient(tasks);
            Action realAction1 = HandleTasksRecieve;
            webClient.RecievedTasks += (sender, args) => dispatcher.BeginInvoke(realAction1);
            Action realAction2 = HandleTaskRecieve;
            webClient.RecievedTask += (sender, args) => dispatcher.BeginInvoke(realAction2);
            Action realAction3 = HandleTaskRename;
            webClient.RenamedTask += (sender, args) => dispatcher.BeginInvoke(realAction3);
            webClient.Connect("185.16.95.101");

        }

        private void buttonNewTask_Click(object sender, RoutedEventArgs e)
        {
            Planner.Task task = new Planner.Task("");
            task.chooseID(randomGenerator);
            tasks.Add(task);
            webClient.TaskAdd(task);
            PopulateList();
            //taskTree.selectedNode = taskTree.nodes.Last();
           // taskTree.RenameTask();
        }

        private void buttonNewSubtask_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (taskTree.HasSelection)
            {
                Task newTask = new Task("subtask!");
                newTask.chooseID(randomGenerator);
                Task parentTask = tasks.IDDictionary[taskTree.selectedNode.ID];
                tasks.Add(newTask, parentTask);
                webClient.TaskAdd(newTask, parentTask);
                PopulateList();
            }*/
        }


        private void PopulateList()
        {
            /*List<TaskTreeNode> nodes = taskTree.nodes;
            nodes.Clear();
            foreach (Task t in tasks.tasks)
            {
                nodes.Add(BuildNodeFromTask(t));
            }
            taskTree.Refresh();*/

        }
        /*private TaskTreeNode BuildNodeFromTask(Task task)
        {
            TaskTreeNode node = new TaskTreeNode(task);
            foreach (Task t in task.subtasks)
            { node.children.Add(BuildNodeFromTask(t)); }
            return node;
        }*/

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
            { tasks.ImportFile(dialog.FileName); }
        }
    }
}
