using X264GUIv2.Models;

namespace X264GUIv2
{
    public partial class Form3 : Form
    {
        public readonly Form1 form;

        #region 內建元件
        public readonly ContextMenuStrip listViewMenu;
        public readonly ToolStripMenuItem listAddViewItem;
        public readonly ToolStripMenuItem listDiffViewItem;
        public readonly ToolStripMenuItem listUpViewItem;
        public readonly ToolStripMenuItem listDnViewItem;
        #endregion

        public Form3(Form1 form)
        {
            InitializeComponent();
            this.form = form;

            #region ContextMenuStrip
            listViewMenu = new();

            listUpViewItem = new() { Text = "上移" };
            listUpViewItem.Click += listUpViewItem_Click;

            listDnViewItem = new() { Text = "下移" };
            listDnViewItem.Click += listDnViewItem_Click;

            listDiffViewItem = new() { Text = "移除" };
            listDiffViewItem.Click += listDiffViewItem_Click;

            listAddViewItem = new() { Text = "加入" };
            listAddViewItem.Click += listAddViewItem_Click;

            listViewMenu.Items.AddRange([
                listAddViewItem,
                listDiffViewItem,
                new ToolStripSeparator(),
                listUpViewItem,
                listDnViewItem,
            ]);
            #endregion
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (MessageBox.Show("確定儲存?", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            save();

            e.Cancel = true;

            //Hide();
        }

        public void ShowDialog(FfprobeOutput ffprobeOutput)
        {
            if (ffprobeOutput.MergeData == null)
                return;

            listView1.Items.Clear();
            listView1.Columns.Add("檔案名稱", listView1.ClientSize.Width);

            foreach (FfprobeOutputMain ffprobe in ffprobeOutput.MergeData.OrderByDescending(x => x.idx))
            {
                ListViewItem lis = new([ffprobe.InFileName])
                {
                    Tag = ffprobe.Guid,
                };

                listView1.Items.Add(lis);
            }

            ShowDialog();
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button != MouseButtons.Right)
                    return;

                ListViewItem? item = listView1.GetItemAt(e.X, e.Y);
                listDiffViewItem.Enabled = item != null;
                listUpViewItem.Enabled = item != null;
                listDnViewItem.Enabled = item != null;

                if (item != null)
                    listView1.FocusedItem = item;
                listViewMenu.Show(listView1, e.Location);
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void listUpViewItem_Click(object? sender, EventArgs e)
        {
            try
            {
                listView1.UpItem();
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void listDnViewItem_Click(object? sender, EventArgs e)
        {
            try
            {
                listView1.DnItem();
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void listAddViewItem_Click(object? sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openfile = new()
                {
                    Multiselect = true
                };

                string fileStr = "";
                foreach (string ext in VideoExt.GetVideoExt)
                    fileStr += $"*.{ext};";

                fileStr = fileStr[..^1];

                openfile.Filter = $"{fileStr}|{fileStr}";

                if (openfile.ShowDialog() == DialogResult.OK)
                    form.videoFunc.Encode(openfile.FileNames);

                //videoFunc.ffprobeData = OtherControlFunc.SortIdx(listView1, videoFunc.ffprobeData);
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void listDiffViewItem_Click(object? sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void save()
        {

            int idx = OtherControlFunc.findFfprobItem(form.videoFunc.ffprobeData, (Guid?)form.listViewMenu.Tag);
            var data = form.videoFunc.ffprobeData[idx].MergeData;
        }

        private void upBtn_Click(object sender, EventArgs e)
        {
            try
            {
                listUpViewItem_Click(sender, e);
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void dnBtn_Click(object sender, EventArgs e)
        {
            try
            {
                listDnViewItem_Click(sender, e);
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void Form3_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                listView1.Columns[0].Width = listView1.ClientSize.Width;
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }
    }
}
