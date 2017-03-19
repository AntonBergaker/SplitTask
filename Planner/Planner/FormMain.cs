using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskFunctions;

namespace Planner
{
    public partial class FormMain : Form
    {
        public Random randomGenerator = new Random();
        TaskCollection tasks = new TaskCollection();
        TaskWebClient webClient;

        public FormMain()
        {
            InitializeComponent();

            webClient = new TaskWebClient(tasks);
            MethodInvoker realAction1 = HandleTasksRecieve;
            webClient.RecievedTasks += (sender, args) => this.BeginInvoke(realAction1);
            MethodInvoker realAction2 = HandleTaskRecieve;
            webClient.RecievedTask += (sender, args) => this.BeginInvoke(realAction2);
            MethodInvoker realAction3 = HandleTaskRename;
            webClient.RenamedTask += (sender, args) => this.BeginInvoke(realAction3);
            webClient.Connect("185.16.95.101");
        }

        private void buttonNewTask_Click(object sender, EventArgs e)
        {
            Task task = new Task("");
            task.chooseID(randomGenerator);
            tasks.Add(task);
            webClient.TaskAdd(task);
            PopulateList();
            taskTree.selectedNode = taskTree.nodes.Last();
            taskTree.RenameTask();
        }
        private void buttonNewSubtask_Click(object sender, EventArgs e)
        {
            if (taskTree.HasSelection)
            {
                Task newTask = new Task("subtask!");
                newTask.chooseID(randomGenerator);
                Task parentTask = tasks.IDDictionary[taskTree.selectedNode.ID];
                tasks.Add(newTask,parentTask);
                webClient.TaskAdd(newTask, parentTask);
                PopulateList();
            }
        }

        private void PopulateList()
        {
            List<TaskTreeNode> nodes = taskTree.nodes;
            nodes.Clear();
            foreach (Task t in tasks.tasks)
            {
                nodes.Add(BuildNodeFromTask(t));
            }
            taskTree.Refresh();
            
        }
        private TaskTreeNode BuildNodeFromTask(Task task)
        {
            TaskTreeNode node = new TaskTreeNode(task);
            foreach (Task t in task.subtasks)
            { node.children.Add(BuildNodeFromTask(t)); }
            return node;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (importFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(importFileDialog.FileName))
                {
                    tasks.ImportFile(importFileDialog.FileName);
                    PopulateList();
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (exportFileDialog.ShowDialog() == DialogResult.OK)
            {
                tasks.ExportFile(exportFileDialog.FileName);
            }
        }

        private void taskTree_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (taskTree.HasSelection)
                {
                    contextTaskTree.Show(this, new Point(e.X, e.Y + 50));
                }
            }
        }
        private void taskTree_TextChanged(object sender, TextChangedEventArgs e)
        {
            tasks.Rename(e.taskID, e.newText);
            webClient.TaskRename(e.taskID, e.newText);
        }



        #region toolStripMenu items

        private void toolStripMenuItemDelete_Click(object sender, EventArgs e)
        {
            if (taskTree.HasSelection)
            {
                string ID = taskTree.selectedNode.ID;
                taskTree.RemoveNode(ID);
                tasks.Remove(ID);
                taskTree.Refresh();
            }
        }
        private void toolStripMenuItemRename_Click(object sender, EventArgs e)
        {
            if (taskTree.HasSelection)
            {
                taskTree.RenameTask();
            }
        }

        #endregion
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
            taskTree.Refresh();
        }

        #endregion
    }
}
