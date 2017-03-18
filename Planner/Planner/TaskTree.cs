using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Planner
{
    partial class TaskTree : UserControl
    {
        public List<TaskTreeNode> nodes;
        public List<TaskTreeNode> visibleNodes;
        public TaskTreeNode selectedNode;

        public bool editing;
        
        private TextBox inputBox;

        public TaskTree()
        {
            InitializeComponent();
            nodes = new List<TaskTreeNode>();
            visibleNodes = new List<TaskTreeNode>();

            inputBox = new TextBox();
            inputBox.TextChanged += new EventHandler(inputBox_TextChanged);
            inputBox.KeyDown += new KeyEventHandler(inputBox_KeyDown);
            inputBox.Font = new Font("Arial", 9);
            inputBox.BorderStyle = BorderStyle.None;
        }

        private void inputBox_TextChanged(object sender, EventArgs e)
        {
            inputBox.Width = TextRenderer.MeasureText(inputBox.Text,inputBox.Font).Width + 5;
            selectedNode.task.title = inputBox.Text;
        }

        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                selectedNode.task.title = inputBox.Text;
                this.Controls.Remove(inputBox);
                OnTextChanged(new TextChangedEventArgs());

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int index = e.Y / 20;
            if (index >= 0 && index < visibleNodes.Count)
            {
                selectedNode = visibleNodes[index];
            }
            this.Refresh();
        }
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            RenameTask();
        }


        public void RenameTask()
        {
            if (selectedNode != null)
            {
                inputBox.Location = new Point(selectedNode.x, selectedNode.y);
                inputBox.Text = selectedNode.task.title;
                this.Controls.Add(inputBox);
                inputBox.SelectAll();
                inputBox.Focus();
            }
        }

        public void RemoveNode(string nodeID)
        {
            RemoveNode(nodes, nodeID);
        }
        public void RemoveNode(List<TaskTreeNode> nodes, string nodeID)
        {
            for (int i=0;i<nodes.Count;i++)
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

        public bool HasSelection
        {
            get { return selectedNode != null; }
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Font drawFont = new Font("Arial", 9);
            SolidBrush normalBrush = new SolidBrush(Color.Black);
            SolidBrush selectedBrush = new SolidBrush(Color.Gray);
            DrawArguments arguments = new DrawArguments(0,drawFont,normalBrush,selectedBrush,e);
            visibleNodes.Clear();

            foreach (TaskTreeNode n in nodes)
            {
                drawNode(n, arguments, 0);
            }

        }

        private void drawNode(TaskTreeNode node, DrawArguments arguments, int iterations)
        {
            SolidBrush brush = arguments.normalBrush;
            if (selectedNode != null && selectedNode.ID == node.ID)
            { brush = arguments.selectedBrush; }

            node.y = arguments.height;
            node.x = 20 + iterations * 20;

            arguments.eventArgs.Graphics.DrawString(node.task.title, arguments.font, brush, node.x, node.y);

            visibleNodes.Add(node);
            arguments.height += 20;
            foreach (TaskTreeNode n in node.children)
            { drawNode(n, arguments, iterations+1); }
        }

        private class DrawArguments
        {
            public int height;
            public Font font;
            public SolidBrush normalBrush;
            public SolidBrush selectedBrush;
            public PaintEventArgs eventArgs;

            public DrawArguments(int height, Font font, SolidBrush normalBrush, SolidBrush selectedBrush, PaintEventArgs eventArgs)
            {
                this.height = height;
                this.font = font;
                this.normalBrush = normalBrush;
                this.selectedBrush = selectedBrush;
                this.eventArgs = eventArgs;
            }
        }
        
        public event EventHandler<TextChangedEventArgs> TextChanged;
        protected virtual void OnTextChanged(TextChangedEventArgs e)
        {
            if (selectedNode != null)
            {
                e.taskID = selectedNode.task.ID;
                e.newText = selectedNode.task.title;
                TextChanged(this,e);
            }
        }
    }

    public class TextChangedEventArgs : EventArgs
    {
        public string taskID { get; set; }
        public string newText { get; set; }
    }
}
