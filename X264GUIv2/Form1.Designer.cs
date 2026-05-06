namespace X264GUIv2
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
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            bitrateCBox = new ComboBox();
            bitrateNumeric = new NumericUpDown();
            fpsCBox = new ComboBox();
            coreCBox = new ComboBox();
            resolutionCBox = new ComboBox();
            progressBar1 = new ProgressBar();
            listView1 = new ListView();
            runBtn = new Button();
            stopBtn = new Button();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            progressText = new ToolStripStatusLabel();
            timeStripStatusLabel = new ToolStripStatusLabel();
            timeStripStatus = new ToolStripStatusLabel();
            menuStrip1 = new MenuStrip();
            FileToolStripMenuItem = new ToolStripMenuItem();
            addToolStripMenuItem = new ToolStripMenuItem();
            diffToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            clearToolStripMenuItem = new ToolStripMenuItem();
            dbToolStripMenuItem = new ToolStripMenuItem();
            dbLoadToolStripMenuItem = new ToolStripMenuItem();
            dbSaveToolStripMenuItem = new ToolStripMenuItem();
            dbClearToolStripMenuItem = new ToolStripMenuItem();
            ViewMenuItem = new ToolStripMenuItem();
            logViewToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            logViewClearToolStripMenuItem = new ToolStripMenuItem();
            settingToolStripMenuItem = new ToolStripMenuItem();
            AutoTrimToolStripMenuItem = new ToolStripMenuItem();
            addBtn = new Button();
            diffBtn = new Button();
            toolStripSeparator3 = new ToolStripSeparator();
            ((System.ComponentModel.ISupportInitialize)bitrateNumeric).BeginInit();
            statusStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(46, 37);
            label1.Name = "label1";
            label1.Size = new Size(96, 18);
            label1.TabIndex = 0;
            label1.Text = "BitRate (kb/s)";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(192, 37);
            label2.Name = "label2";
            label2.Size = new Size(31, 18);
            label2.TabIndex = 1;
            label2.Text = "FPS";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(249, 37);
            label3.Name = "label3";
            label3.Size = new Size(71, 18);
            label3.TabIndex = 2;
            label3.Text = "解析度 (p)";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(325, 37);
            label4.Name = "label4";
            label4.Size = new Size(78, 18);
            label4.TabIndex = 3;
            label4.Text = "編譯核心數";
            // 
            // bitrateCBox
            // 
            bitrateCBox.DropDownStyle = ComboBoxStyle.DropDownList;
            bitrateCBox.FormattingEnabled = true;
            bitrateCBox.Location = new Point(13, 58);
            bitrateCBox.Name = "bitrateCBox";
            bitrateCBox.Size = new Size(75, 25);
            bitrateCBox.TabIndex = 4;
            bitrateCBox.SelectedIndexChanged += bitrateCBox_SelectedIndexChanged;
            // 
            // bitrateNumeric
            // 
            bitrateNumeric.Location = new Point(95, 58);
            bitrateNumeric.Name = "bitrateNumeric";
            bitrateNumeric.Size = new Size(73, 24);
            bitrateNumeric.TabIndex = 5;
            bitrateNumeric.ValueChanged += bitrateNumeric_ValueChanged;
            // 
            // fpsCBox
            // 
            fpsCBox.DropDownStyle = ComboBoxStyle.DropDownList;
            fpsCBox.FormattingEnabled = true;
            fpsCBox.Location = new Point(172, 58);
            fpsCBox.Name = "fpsCBox";
            fpsCBox.Size = new Size(69, 25);
            fpsCBox.TabIndex = 6;
            fpsCBox.SelectedIndexChanged += fpsCBox_SelectedIndexChanged;
            // 
            // coreCBox
            // 
            coreCBox.DropDownStyle = ComboBoxStyle.DropDownList;
            coreCBox.FormattingEnabled = true;
            coreCBox.Location = new Point(325, 58);
            coreCBox.Name = "coreCBox";
            coreCBox.Size = new Size(78, 25);
            coreCBox.TabIndex = 7;
            // 
            // resolutionCBox
            // 
            resolutionCBox.FormattingEnabled = true;
            resolutionCBox.Location = new Point(247, 58);
            resolutionCBox.Name = "resolutionCBox";
            resolutionCBox.Size = new Size(72, 25);
            resolutionCBox.TabIndex = 8;
            resolutionCBox.SelectedValueChanged += resolutionCBox_SelectedValueChanged;
            resolutionCBox.Leave += resolutionCBox_Leave;
            // 
            // progressBar1
            // 
            progressBar1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar1.Location = new Point(409, 58);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(254, 25);
            progressBar1.TabIndex = 10;
            // 
            // listView1
            // 
            listView1.AllowDrop = true;
            listView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listView1.CheckBoxes = true;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.Location = new Point(12, 92);
            listView1.Name = "listView1";
            listView1.OwnerDraw = true;
            listView1.ShowItemToolTips = true;
            listView1.Size = new Size(651, 300);
            listView1.TabIndex = 12;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            listView1.ColumnClick += listView1_ColumnClick;
            listView1.DrawColumnHeader += listView1_DrawColumnHeader;
            listView1.DrawItem += listView1_DrawItem;
            listView1.DrawSubItem += listView1_DrawSubItem;
            listView1.DragDrop += listView1_DragDrop;
            listView1.DragEnter += listView1_DragEnter;
            // 
            // runBtn
            // 
            runBtn.Enabled = false;
            runBtn.Location = new Point(504, 27);
            runBtn.Name = "runBtn";
            runBtn.Size = new Size(75, 28);
            runBtn.TabIndex = 14;
            runBtn.Text = "開始轉檔";
            runBtn.UseVisualStyleBackColor = true;
            runBtn.Click += runBtn_Click;
            // 
            // stopBtn
            // 
            stopBtn.Enabled = false;
            stopBtn.Location = new Point(585, 27);
            stopBtn.Name = "stopBtn";
            stopBtn.Size = new Size(75, 28);
            stopBtn.TabIndex = 15;
            stopBtn.Text = "強制停止";
            stopBtn.UseVisualStyleBackColor = true;
            stopBtn.Click += stopBtn_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1, progressText, timeStripStatusLabel, timeStripStatus });
            statusStrip1.Location = new Point(0, 408);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(672, 22);
            statusStrip1.TabIndex = 19;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(34, 17);
            toolStripStatusLabel1.Text = "數量:";
            // 
            // progressText
            // 
            progressText.Name = "progressText";
            progressText.Size = new Size(26, 17);
            progressText.Text = "0/0";
            // 
            // timeStripStatusLabel
            // 
            timeStripStatusLabel.Name = "timeStripStatusLabel";
            timeStripStatusLabel.RightToLeft = RightToLeft.No;
            timeStripStatusLabel.Size = new Size(70, 17);
            timeStripStatusLabel.Text = "總消耗時間:";
            // 
            // timeStripStatus
            // 
            timeStripStatus.Name = "timeStripStatus";
            timeStripStatus.RightToLeft = RightToLeft.No;
            timeStripStatus.Size = new Size(55, 17);
            timeStripStatus.Text = "00:00:00";
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { FileToolStripMenuItem, dbToolStripMenuItem, ViewMenuItem, settingToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(672, 24);
            menuStrip1.TabIndex = 20;
            menuStrip1.Text = "menuStrip1";
            // 
            // FileToolStripMenuItem
            // 
            FileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { addToolStripMenuItem, diffToolStripMenuItem, toolStripSeparator1, clearToolStripMenuItem });
            FileToolStripMenuItem.Name = "FileToolStripMenuItem";
            FileToolStripMenuItem.Size = new Size(43, 20);
            FileToolStripMenuItem.Text = "檔案";
            // 
            // addToolStripMenuItem
            // 
            addToolStripMenuItem.Name = "addToolStripMenuItem";
            addToolStripMenuItem.Size = new Size(146, 22);
            addToolStripMenuItem.Text = "新增視訊檔案";
            addToolStripMenuItem.Click += addToolStripMenuItem_Click;
            // 
            // diffToolStripMenuItem
            // 
            diffToolStripMenuItem.Name = "diffToolStripMenuItem";
            diffToolStripMenuItem.Size = new Size(146, 22);
            diffToolStripMenuItem.Text = "移除視訊檔案";
            diffToolStripMenuItem.Click += diffToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(143, 6);
            // 
            // clearToolStripMenuItem
            // 
            clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            clearToolStripMenuItem.Size = new Size(146, 22);
            clearToolStripMenuItem.Text = "清空列表";
            clearToolStripMenuItem.Click += clearToolStripMenuItem_Click;
            // 
            // dbToolStripMenuItem
            // 
            dbToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { dbLoadToolStripMenuItem, dbSaveToolStripMenuItem, toolStripSeparator3, dbClearToolStripMenuItem });
            dbToolStripMenuItem.Name = "dbToolStripMenuItem";
            dbToolStripMenuItem.Size = new Size(55, 20);
            dbToolStripMenuItem.Text = "儲存體";
            // 
            // dbLoadToolStripMenuItem
            // 
            dbLoadToolStripMenuItem.Name = "dbLoadToolStripMenuItem";
            dbLoadToolStripMenuItem.Size = new Size(122, 22);
            dbLoadToolStripMenuItem.Text = "讀取存檔";
            dbLoadToolStripMenuItem.Click += dbLoadToolStripMenuItem_Click;
            // 
            // dbSaveToolStripMenuItem
            // 
            dbSaveToolStripMenuItem.Name = "dbSaveToolStripMenuItem";
            dbSaveToolStripMenuItem.Size = new Size(122, 22);
            dbSaveToolStripMenuItem.Text = "存檔";
            dbSaveToolStripMenuItem.Click += dbSaveToolStripMenuItem_Click;
            // 
            // dbClearToolStripMenuItem
            // 
            dbClearToolStripMenuItem.Name = "dbClearToolStripMenuItem";
            dbClearToolStripMenuItem.Size = new Size(122, 22);
            dbClearToolStripMenuItem.Text = "清除存檔";
            dbClearToolStripMenuItem.Click += dbClearToolStripMenuItem_Click;
            // 
            // ViewMenuItem
            // 
            ViewMenuItem.DropDownItems.AddRange(new ToolStripItem[] { logViewToolStripMenuItem, toolStripSeparator2, logViewClearToolStripMenuItem });
            ViewMenuItem.Name = "ViewMenuItem";
            ViewMenuItem.Size = new Size(43, 20);
            ViewMenuItem.Text = "檢視";
            // 
            // logViewToolStripMenuItem
            // 
            logViewToolStripMenuItem.Name = "logViewToolStripMenuItem";
            logViewToolStripMenuItem.Size = new Size(150, 22);
            logViewToolStripMenuItem.Text = "LogView";
            logViewToolStripMenuItem.Click += logViewToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(147, 6);
            // 
            // logViewClearToolStripMenuItem
            // 
            logViewClearToolStripMenuItem.Name = "logViewClearToolStripMenuItem";
            logViewClearToolStripMenuItem.Size = new Size(150, 22);
            logViewClearToolStripMenuItem.Text = "LogView 清除";
            logViewClearToolStripMenuItem.Click += logViewClearToolStripMenuItem_Click;
            // 
            // settingToolStripMenuItem
            // 
            settingToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { AutoTrimToolStripMenuItem });
            settingToolStripMenuItem.Name = "settingToolStripMenuItem";
            settingToolStripMenuItem.Size = new Size(43, 20);
            settingToolStripMenuItem.Text = "設定";
            // 
            // AutoTrimToolStripMenuItem
            // 
            AutoTrimToolStripMenuItem.CheckOnClick = true;
            AutoTrimToolStripMenuItem.Name = "AutoTrimToolStripMenuItem";
            AutoTrimToolStripMenuItem.Size = new Size(159, 22);
            AutoTrimToolStripMenuItem.Text = "啟用音訊軌Trim";
            // 
            // addBtn
            // 
            addBtn.Location = new Point(409, 27);
            addBtn.Name = "addBtn";
            addBtn.Size = new Size(28, 28);
            addBtn.TabIndex = 21;
            addBtn.Text = "+";
            addBtn.UseVisualStyleBackColor = true;
            addBtn.Click += addBtn_Click;
            // 
            // diffBtn
            // 
            diffBtn.Location = new Point(443, 27);
            diffBtn.Name = "diffBtn";
            diffBtn.Size = new Size(28, 28);
            diffBtn.TabIndex = 22;
            diffBtn.Text = "-";
            diffBtn.UseVisualStyleBackColor = true;
            diffBtn.Click += diffBtn_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(119, 6);
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoValidate = AutoValidate.EnableAllowFocusChange;
            ClientSize = new Size(672, 430);
            Controls.Add(diffBtn);
            Controls.Add(addBtn);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            Controls.Add(stopBtn);
            Controls.Add(runBtn);
            Controls.Add(listView1);
            Controls.Add(progressBar1);
            Controls.Add(resolutionCBox);
            Controls.Add(coreCBox);
            Controls.Add(fpsCBox);
            Controls.Add(bitrateNumeric);
            Controls.Add(bitrateCBox);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Font = new Font("Microsoft JhengHei UI", 10F);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "X264GUI";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)bitrateNumeric).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        public Label label1;
        public Label label2;
        public Label label3;
        public Label label4;
        public ComboBox bitrateCBox;
        public NumericUpDown bitrateNumeric;
        public ComboBox fpsCBox;
        public ComboBox coreCBox;
        public ComboBox resolutionCBox;
        public ProgressBar progressBar1;
        public ListView listView1;
        public Button logBtn;
        public Button runBtn;
        public Button stopBtn;
        public StatusStrip statusStrip1;
        public ToolStripStatusLabel progressText;
        public ToolStripStatusLabel toolStripStatusLabel1;
        public MenuStrip menuStrip1;
        public ToolStripMenuItem ViewMenuItem;
        public ToolStripMenuItem logViewToolStripMenuItem;
        public ToolStripMenuItem FileToolStripMenuItem;
        public ToolStripMenuItem addToolStripMenuItem;
        public ToolStripMenuItem clearToolStripMenuItem;
        private ToolStripMenuItem diffToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        public Button addBtn;
        public Button diffBtn;
        public ToolStripStatusLabel timeStripStatusLabel;
        public ToolStripStatusLabel timeStripStatus;
        private ToolStripMenuItem dbToolStripMenuItem;
        private ToolStripMenuItem dbSaveToolStripMenuItem;
        private ToolStripMenuItem dbClearToolStripMenuItem;
        private ToolStripMenuItem settingToolStripMenuItem;
        private ToolStripMenuItem AutoTrimToolStripMenuItem;
        private ToolStripMenuItem logViewClearToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem dbLoadToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
    }
}
