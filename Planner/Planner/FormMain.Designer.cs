namespace Planner
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonNewTask = new System.Windows.Forms.Button();
            this.buttonNewSubtask = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.exportFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.contextTaskTree = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.taskTree = new Planner.TaskTree();
            this.toolStripMenuItemRename = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.contextTaskTree.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonNewTask
            // 
            this.buttonNewTask.Location = new System.Drawing.Point(12, 27);
            this.buttonNewTask.Name = "buttonNewTask";
            this.buttonNewTask.Size = new System.Drawing.Size(75, 23);
            this.buttonNewTask.TabIndex = 1;
            this.buttonNewTask.Text = "New Task";
            this.buttonNewTask.UseVisualStyleBackColor = true;
            this.buttonNewTask.Click += new System.EventHandler(this.buttonNewTask_Click);
            // 
            // buttonNewSubtask
            // 
            this.buttonNewSubtask.Location = new System.Drawing.Point(93, 27);
            this.buttonNewSubtask.Name = "buttonNewSubtask";
            this.buttonNewSubtask.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.buttonNewSubtask.Size = new System.Drawing.Size(91, 23);
            this.buttonNewSubtask.TabIndex = 2;
            this.buttonNewSubtask.Text = "New Subtask";
            this.buttonNewSubtask.UseVisualStyleBackColor = true;
            this.buttonNewSubtask.Click += new System.EventHandler(this.buttonNewSubtask_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(643, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // importFileDialog
            // 
            this.importFileDialog.DefaultExt = "tlf";
            this.importFileDialog.FileName = "importFileDialog";
            this.importFileDialog.Filter = "Tasklist Files|*.tlf";
            // 
            // exportFileDialog
            // 
            this.exportFileDialog.DefaultExt = "tlf";
            this.exportFileDialog.Filter = "Tasklist Files|*.tlf";
            // 
            // contextTaskTree
            // 
            this.contextTaskTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemRename,
            this.toolStripMenuItemDelete});
            this.contextTaskTree.Name = "contextTaskTree";
            this.contextTaskTree.Size = new System.Drawing.Size(153, 70);
            // 
            // toolStripMenuItemDelete
            // 
            this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
            this.toolStripMenuItemDelete.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItemDelete.Text = "Delete";
            this.toolStripMenuItemDelete.Click += new System.EventHandler(this.toolStripMenuItemDelete_Click);
            // 
            // taskTree
            // 
            this.taskTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.taskTree.AutoScroll = true;
            this.taskTree.AutoSize = true;
            this.taskTree.Location = new System.Drawing.Point(12, 56);
            this.taskTree.Name = "taskTree";
            this.taskTree.Size = new System.Drawing.Size(619, 361);
            this.taskTree.TabIndex = 3;
            this.taskTree.TextChanged += new System.EventHandler<Planner.TextChangedEventArgs>(this.taskTree_TextChanged);
            this.taskTree.MouseClick += new System.Windows.Forms.MouseEventHandler(this.taskTree_MouseClick);
            // 
            // toolStripMenuItemRename
            // 
            this.toolStripMenuItemRename.Name = "toolStripMenuItemRename";
            this.toolStripMenuItemRename.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItemRename.Text = "Rename";
            this.toolStripMenuItemRename.Click += new System.EventHandler(this.toolStripMenuItemRename_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(643, 429);
            this.Controls.Add(this.taskTree);
            this.Controls.Add(this.buttonNewSubtask);
            this.Controls.Add(this.buttonNewTask);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.Text = "Form1";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.contextTaskTree.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonNewTask;
        private System.Windows.Forms.Button buttonNewSubtask;
        private TaskTree taskTree;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog importFileDialog;
        private System.Windows.Forms.SaveFileDialog exportFileDialog;
        private System.Windows.Forms.ContextMenuStrip contextTaskTree;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRename;
    }
}

