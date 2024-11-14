namespace DeepNestPort.Core
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            timer1 = new System.Windows.Forms.Timer(components);
            tableLayoutPanel1 = new TableLayoutPanel();
            panel1 = new Panel();
            tableLayoutPanel2 = new TableLayoutPanel();
            pictureBox3 = new PictureBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            label1 = new Label();
            objectListView2 = new BrightIdeasSoftware.ObjectListView();
            contextMenuStrip5 = new ContextMenuStrip(components);
            addSheetToolStripMenuItem = new ToolStripMenuItem();
            rectToolStripMenuItem = new ToolStripMenuItem();
            dxfToolStripMenuItem = new ToolStripMenuItem();
            deleteToolStripMenuItem1 = new ToolStripMenuItem();
            tableLayoutPanel4 = new TableLayoutPanel();
            label2 = new Label();
            objectListView1 = new BrightIdeasSoftware.ObjectListView();
            contextMenuStrip4 = new ContextMenuStrip(components);
            detailToolStripMenuItem = new ToolStripMenuItem();
            clearToolStripMenuItem = new ToolStripMenuItem();
            deleteToolStripMenuItem = new ToolStripMenuItem();
            quantityToolStripMenuItem = new ToolStripMenuItem();
            setToToolStripMenuItem = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolStripStatusLabel2 = new ToolStripStatusLabel();
            toolStripProgressBar1 = new ToolStripProgressBar();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)objectListView2).BeginInit();
            contextMenuStrip5.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)objectListView1).BeginInit();
            contextMenuStrip4.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 20;
            timer1.Tick += timer1_Tick;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18F));
            tableLayoutPanel1.Controls.Add(panel1, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(3, 2, 3, 2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1034, 543);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(tableLayoutPanel2);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 2);
            panel1.Margin = new Padding(3, 2, 3, 2);
            panel1.Name = "panel1";
            panel1.Size = new Size(1028, 539);
            panel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(pictureBox3, 1, 0);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel3, 0, 1);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel4, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 0);
            tableLayoutPanel2.Margin = new Padding(3, 2, 3, 2);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new Size(1028, 539);
            tableLayoutPanel2.TabIndex = 3;
            // 
            // pictureBox3
            // 
            pictureBox3.BorderStyle = BorderStyle.FixedSingle;
            pictureBox3.Dock = DockStyle.Fill;
            pictureBox3.Location = new Point(403, 2);
            pictureBox3.Margin = new Padding(3, 2, 3, 2);
            pictureBox3.Name = "pictureBox3";
            tableLayoutPanel2.SetRowSpan(pictureBox3, 2);
            pictureBox3.Size = new Size(622, 535);
            pictureBox3.TabIndex = 2;
            pictureBox3.TabStop = false;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(label1, 0, 0);
            tableLayoutPanel3.Controls.Add(objectListView2, 0, 1);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 271);
            tableLayoutPanel3.Margin = new Padding(3, 2, 3, 2);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(394, 266);
            tableLayoutPanel3.TabIndex = 4;
            // 
            // label1
            // 
            label1.BackColor = Color.LemonChiffon;
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(388, 15);
            label1.TabIndex = 0;
            label1.Text = "Sheets";
            // 
            // objectListView2
            // 
            objectListView2.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.DoubleClick;
            objectListView2.ContextMenuStrip = contextMenuStrip5;
            objectListView2.Dock = DockStyle.Fill;
            objectListView2.FullRowSelect = true;
            objectListView2.GridLines = true;
            objectListView2.Location = new Point(3, 17);
            objectListView2.Margin = new Padding(3, 2, 3, 2);
            objectListView2.Name = "objectListView2";
            objectListView2.ShowGroups = false;
            objectListView2.Size = new Size(388, 247);
            objectListView2.TabIndex = 3;
            objectListView2.View = View.Details;
            objectListView2.SelectedIndexChanged += objectListView2_SelectedIndexChanged;
            objectListView2.KeyDown += objectListView2_KeyDown;
            // 
            // contextMenuStrip5
            // 
            contextMenuStrip5.ImageScalingSize = new Size(20, 20);
            contextMenuStrip5.Items.AddRange(new ToolStripItem[] { addSheetToolStripMenuItem, deleteToolStripMenuItem1 });
            contextMenuStrip5.Name = "contextMenuStrip5";
            contextMenuStrip5.Size = new Size(126, 48);
            // 
            // addSheetToolStripMenuItem
            // 
            addSheetToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { rectToolStripMenuItem, dxfToolStripMenuItem });
            addSheetToolStripMenuItem.Name = "addSheetToolStripMenuItem";
            addSheetToolStripMenuItem.Size = new Size(125, 22);
            addSheetToolStripMenuItem.Text = "add sheet";
            // 
            // rectToolStripMenuItem
            // 
            rectToolStripMenuItem.Name = "rectToolStripMenuItem";
            rectToolStripMenuItem.Size = new Size(94, 22);
            rectToolStripMenuItem.Text = "rect";
            rectToolStripMenuItem.Click += rectToolStripMenuItem_Click;
            // 
            // dxfToolStripMenuItem
            // 
            dxfToolStripMenuItem.Name = "dxfToolStripMenuItem";
            dxfToolStripMenuItem.Size = new Size(94, 22);
            dxfToolStripMenuItem.Text = "dxf";
            dxfToolStripMenuItem.Click += dxfToolStripMenuItem_Click;
            // 
            // deleteToolStripMenuItem1
            // 
            deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            deleteToolStripMenuItem1.Size = new Size(125, 22);
            deleteToolStripMenuItem1.Text = "delete";
            deleteToolStripMenuItem1.Click += deleteToolStripMenuItem1_Click;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 1;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Controls.Add(label2, 0, 0);
            tableLayoutPanel4.Controls.Add(objectListView1, 0, 1);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(3, 3);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 2;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Size = new Size(394, 263);
            tableLayoutPanel4.TabIndex = 5;
            // 
            // label2
            // 
            label2.BackColor = Color.LemonChiffon;
            label2.Dock = DockStyle.Fill;
            label2.Location = new Point(3, 0);
            label2.Name = "label2";
            label2.Size = new Size(388, 15);
            label2.TabIndex = 1;
            label2.Text = "Parts";
            // 
            // objectListView1
            // 
            objectListView1.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.DoubleClick;
            objectListView1.ContextMenuStrip = contextMenuStrip4;
            objectListView1.Dock = DockStyle.Fill;
            objectListView1.FullRowSelect = true;
            objectListView1.GridLines = true;
            objectListView1.Location = new Point(3, 17);
            objectListView1.Margin = new Padding(3, 2, 3, 2);
            objectListView1.Name = "objectListView1";
            objectListView1.ShowGroups = false;
            objectListView1.Size = new Size(388, 244);
            objectListView1.TabIndex = 1;
            objectListView1.View = View.Details;
            objectListView1.SelectedIndexChanged += objectListView1_SelectedIndexChanged;
            objectListView1.KeyDown += objectListView1_KeyDown;
            // 
            // contextMenuStrip4
            // 
            contextMenuStrip4.ImageScalingSize = new Size(20, 20);
            contextMenuStrip4.Items.AddRange(new ToolStripItem[] { detailToolStripMenuItem, clearToolStripMenuItem, deleteToolStripMenuItem, quantityToolStripMenuItem });
            contextMenuStrip4.Name = "contextMenuStrip4";
            contextMenuStrip4.Size = new Size(131, 108);
            // 
            // detailToolStripMenuItem
            // 
            detailToolStripMenuItem.Image = Properties.Resources.plus;
            detailToolStripMenuItem.Name = "detailToolStripMenuItem";
            detailToolStripMenuItem.Size = new Size(130, 26);
            detailToolStripMenuItem.Text = "add detail";
            detailToolStripMenuItem.Click += detailToolStripMenuItem_Click;
            // 
            // clearToolStripMenuItem
            // 
            clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            clearToolStripMenuItem.Size = new Size(130, 26);
            clearToolStripMenuItem.Text = "clear";
            clearToolStripMenuItem.Click += clearToolStripMenuItem_Click;
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Size = new Size(130, 26);
            deleteToolStripMenuItem.Text = "delete";
            deleteToolStripMenuItem.Click += deleteToolStripMenuItem_Click;
            // 
            // quantityToolStripMenuItem
            // 
            quantityToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { setToToolStripMenuItem });
            quantityToolStripMenuItem.Name = "quantityToolStripMenuItem";
            quantityToolStripMenuItem.Size = new Size(130, 26);
            quantityToolStripMenuItem.Text = "quantity";
            // 
            // setToToolStripMenuItem
            // 
            setToToolStripMenuItem.Name = "setToToolStripMenuItem";
            setToToolStripMenuItem.Size = new Size(103, 22);
            setToToolStripMenuItem.Text = "set to";
            setToToolStripMenuItem.Click += setToToolStripMenuItem_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1, toolStripStatusLabel2, toolStripProgressBar1 });
            statusStrip1.Location = new Point(0, 543);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 13, 0);
            statusStrip1.Size = new Size(1034, 22);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(13, 17);
            toolStripStatusLabel1.Text = "..";
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new Size(917, 17);
            toolStripStatusLabel2.Spring = true;
            // 
            // toolStripProgressBar1
            // 
            toolStripProgressBar1.Name = "toolStripProgressBar1";
            toolStripProgressBar1.Size = new Size(88, 16);
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1034, 565);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(statusStrip1);
            Margin = new Padding(3, 2, 3, 2);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DeepNestPort";
            tableLayoutPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)objectListView2).EndInit();
            contextMenuStrip5.ResumeLayout(false);
            tableLayoutPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)objectListView1).EndInit();
            contextMenuStrip4.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private BrightIdeasSoftware.ObjectListView objectListView1;
        private PictureBox pictureBox3;
        private TableLayoutPanel tableLayoutPanel2;
        private ContextMenuStrip contextMenuStrip4;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private ToolStripProgressBar toolStripProgressBar1;
        private BrightIdeasSoftware.ObjectListView objectListView2;
        private ToolStripMenuItem detailToolStripMenuItem;
        private ToolStripMenuItem clearToolStripMenuItem;
        private ToolStripMenuItem quantityToolStripMenuItem;
        private ToolStripMenuItem setToToolStripMenuItem;
        private ContextMenuStrip contextMenuStrip5;
        private ToolStripMenuItem addSheetToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem1;
        private ToolStripMenuItem rectToolStripMenuItem;
        private ToolStripMenuItem dxfToolStripMenuItem;
        private TableLayoutPanel tableLayoutPanel3;
        private Label label1;
        private TableLayoutPanel tableLayoutPanel4;
        private Label label2;
    }
}