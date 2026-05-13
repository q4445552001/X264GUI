namespace X264GUIv2
{
    partial class Form3
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form3));
            listView1 = new ListView();
            dnBtn = new Button();
            upBtn = new Button();
            SuspendLayout();
            // 
            // listView1
            // 
            listView1.AllowDrop = true;
            listView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listView1.Font = new Font("Microsoft JhengHei UI", 10F);
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.Location = new Point(12, 46);
            listView1.Name = "listView1";
            listView1.Size = new Size(540, 283);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            listView1.MouseUp += listView1_MouseUp;
            // 
            // dnBtn
            // 
            dnBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            dnBtn.Location = new Point(500, 12);
            dnBtn.Name = "dnBtn";
            dnBtn.Size = new Size(52, 28);
            dnBtn.TabIndex = 24;
            dnBtn.Text = "下移";
            dnBtn.UseVisualStyleBackColor = true;
            dnBtn.Click += dnBtn_Click;
            // 
            // upBtn
            // 
            upBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            upBtn.Location = new Point(442, 12);
            upBtn.Name = "upBtn";
            upBtn.Size = new Size(52, 28);
            upBtn.TabIndex = 23;
            upBtn.Text = "上移";
            upBtn.UseVisualStyleBackColor = true;
            upBtn.Click += upBtn_Click;
            // 
            // Form3
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(564, 341);
            Controls.Add(dnBtn);
            Controls.Add(upBtn);
            Controls.Add(listView1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form3";
            Text = "編輯合併項目";
            FormClosed += Form3_FormClosed;
            SizeChanged += Form3_SizeChanged;
            ResumeLayout(false);
        }

        #endregion

        public ListView listView1;
        public Button dnBtn;
        public Button upBtn;
    }
}