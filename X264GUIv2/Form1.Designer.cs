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
            progressText = new TextBox();
            listView1 = new ListView();
            logBtn = new Button();
            runBtn = new Button();
            stopBtn = new Button();
            addBtn = new Button();
            diffBtn = new Button();
            clearBtn = new Button();
            ((System.ComponentModel.ISupportInitialize)bitrateNumeric).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(47, 10);
            label1.Name = "label1";
            label1.Size = new Size(96, 18);
            label1.TabIndex = 0;
            label1.Text = "BitRate (kb/s)";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(242, 10);
            label2.Name = "label2";
            label2.Size = new Size(31, 18);
            label2.TabIndex = 1;
            label2.Text = "FPS";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(299, 10);
            label3.Name = "label3";
            label3.Size = new Size(71, 18);
            label3.TabIndex = 2;
            label3.Text = "解析度 (p)";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(375, 10);
            label4.Name = "label4";
            label4.Size = new Size(78, 18);
            label4.TabIndex = 3;
            label4.Text = "編譯核心數";
            // 
            // bitrateCBox
            // 
            bitrateCBox.DropDownStyle = ComboBoxStyle.DropDownList;
            bitrateCBox.FormattingEnabled = true;
            bitrateCBox.Location = new Point(14, 31);
            bitrateCBox.Name = "bitrateCBox";
            bitrateCBox.Size = new Size(75, 25);
            bitrateCBox.TabIndex = 4;
            bitrateCBox.SelectedIndexChanged += bitrateCBox_SelectedIndexChanged;
            // 
            // bitrateNumeric
            // 
            bitrateNumeric.Location = new Point(96, 31);
            bitrateNumeric.Name = "bitrateNumeric";
            bitrateNumeric.Size = new Size(73, 24);
            bitrateNumeric.TabIndex = 5;
            bitrateNumeric.ValueChanged += bitrateNumeric_ValueChanged;
            // 
            // fpsCBox
            // 
            fpsCBox.DropDownStyle = ComboBoxStyle.DropDownList;
            fpsCBox.FormattingEnabled = true;
            fpsCBox.Location = new Point(222, 31);
            fpsCBox.Name = "fpsCBox";
            fpsCBox.Size = new Size(69, 25);
            fpsCBox.TabIndex = 6;
            fpsCBox.SelectedIndexChanged += fpsCBox_SelectedIndexChanged;
            // 
            // coreCBox
            // 
            coreCBox.DropDownStyle = ComboBoxStyle.DropDownList;
            coreCBox.FormattingEnabled = true;
            coreCBox.Location = new Point(375, 31);
            coreCBox.Name = "coreCBox";
            coreCBox.Size = new Size(78, 25);
            coreCBox.TabIndex = 7;
            // 
            // resolutionCBox
            // 
            resolutionCBox.FormattingEnabled = true;
            resolutionCBox.Location = new Point(297, 31);
            resolutionCBox.Name = "resolutionCBox";
            resolutionCBox.Size = new Size(72, 25);
            resolutionCBox.TabIndex = 8;
            resolutionCBox.SelectedIndexChanged += resolutionCBox_SelectedIndexChanged;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(459, 34);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(100, 23);
            progressBar1.TabIndex = 10;
            // 
            // progressText
            // 
            progressText.Location = new Point(565, 34);
            progressText.Name = "progressText";
            progressText.ReadOnly = true;
            progressText.Size = new Size(100, 24);
            progressText.TabIndex = 11;
            // 
            // listView1
            // 
            listView1.AllowDrop = true;
            listView1.CheckBoxes = true;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.Location = new Point(14, 62);
            listView1.Name = "listView1";
            listView1.OwnerDraw = true;
            listView1.ShowItemToolTips = true;
            listView1.Size = new Size(651, 417);
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
            // logBtn
            // 
            logBtn.Location = new Point(701, 55);
            logBtn.Name = "logBtn";
            logBtn.Size = new Size(75, 23);
            logBtn.TabIndex = 13;
            logBtn.Text = "LOG";
            logBtn.UseVisualStyleBackColor = true;
            logBtn.Click += logBtn_Click;
            // 
            // runBtn
            // 
            runBtn.Enabled = false;
            runBtn.Location = new Point(690, 105);
            runBtn.Name = "runBtn";
            runBtn.Size = new Size(75, 23);
            runBtn.TabIndex = 14;
            runBtn.Text = "開始轉檔";
            runBtn.UseVisualStyleBackColor = true;
            runBtn.Click += runBtn_Click;
            // 
            // stopBtn
            // 
            stopBtn.Enabled = false;
            stopBtn.Location = new Point(782, 55);
            stopBtn.Name = "stopBtn";
            stopBtn.Size = new Size(75, 23);
            stopBtn.TabIndex = 15;
            stopBtn.Text = "強制停止";
            stopBtn.UseVisualStyleBackColor = true;
            stopBtn.Click += stopBtn_Click;
            // 
            // addBtn
            // 
            addBtn.Location = new Point(690, 151);
            addBtn.Name = "addBtn";
            addBtn.Size = new Size(75, 23);
            addBtn.TabIndex = 16;
            addBtn.Text = "+";
            addBtn.UseVisualStyleBackColor = true;
            addBtn.Click += addBtn_Click;
            // 
            // diffBtn
            // 
            diffBtn.Location = new Point(690, 208);
            diffBtn.Name = "diffBtn";
            diffBtn.Size = new Size(75, 23);
            diffBtn.TabIndex = 17;
            diffBtn.Text = "-";
            diffBtn.UseVisualStyleBackColor = true;
            diffBtn.Click += diffBtn_Click;
            // 
            // clearBtn
            // 
            clearBtn.Location = new Point(690, 179);
            clearBtn.Name = "clearBtn";
            clearBtn.Size = new Size(75, 23);
            clearBtn.TabIndex = 18;
            clearBtn.Text = "清空";
            clearBtn.UseVisualStyleBackColor = true;
            clearBtn.Click += clearBtm_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoValidate = AutoValidate.EnableAllowFocusChange;
            ClientSize = new Size(914, 510);
            Controls.Add(clearBtn);
            Controls.Add(diffBtn);
            Controls.Add(addBtn);
            Controls.Add(stopBtn);
            Controls.Add(runBtn);
            Controls.Add(logBtn);
            Controls.Add(listView1);
            Controls.Add(progressText);
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
            Name = "Form1";
            Text = "X264GUI";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)bitrateNumeric).EndInit();
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
        public TextBox progressText;
        public ListView listView1;
        public Button logBtn;
        public Button runBtn;
        public Button stopBtn;
        public Button addBtn;
        public Button diffBtn;
        public Button clearBtn;
    }
}
