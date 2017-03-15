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

namespace Planner
{
    public partial class FormMain : Form
    {
        public Random randomGenerator = new Random();
        TaskCollection tasks = new TaskCollection();

        public FormMain()
        {
            InitializeComponent();
            
        }

        private void buttonNewTask_Click(object sender, EventArgs e)
        {
            Task task = new Task("");
            task.chooseID(randomGenerator);
            tasks.Add(task);
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
            TaskTreeNode node = new TaskTreeNode(task.title,task.ID);
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
                    tasks.Import(importFileDialog.FileName);
                    PopulateList();
                }
            }
        }

        private void taskTree_TextChanged(object sender, TextChangedEventArgs e)
        {
            tasks.IDDictionary[e.taskID].title = e.newText;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (exportFileDialog.ShowDialog() == DialogResult.OK)
            {
                tasks.Export(exportFileDialog.FileName);
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

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (taskTree.HasSelection)
            {
                string ID = taskTree.selectedNode.ID;
                taskTree.RemoveNode(ID);
                tasks.Remove(ID);
                taskTree.Refresh();
            }
        }
    }
}
