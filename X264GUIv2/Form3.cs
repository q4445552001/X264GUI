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

        #region 初始化
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
        #endregion

        #region Form
        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!(MessageBox.Show("確定儲存?", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes))
                return;
            save();
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
        #endregion

        #region listView1
        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button != MouseButtons.Right)
                    return;

                ListViewItem? item = listView1.GetItemAt(e.X, e.Y);
                listDiffViewItem.Enabled = listView1.Items.Count > 1 && item != null;
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
        #endregion

        #region listViewItem
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
                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    var idx2 = listView1.findListItem((Guid?)item.Tag);
                    listView1.Items.RemoveAt(idx2);
                }
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }
        #endregion

        #region Btn
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
        #endregion

        #region 副程式
        private void save()
        {
            Guid? listGuid = (Guid?)form.listViewMenu.Tag;
            if (listGuid is null)
                return;

            Dictionary<Guid, int> editGuids =
                listView1.Items.Cast<ListViewItem>().Select(x => (Guid?)x.Tag)
                .Where(x => x is not null)
                .Select((x, idx) => new { guid = x, idx })
                .OrderBy(x => x.idx)
                .ToDictionary(x => (Guid)x.guid!, x => x.idx);

            int itemIdx = form.videoFunc.ffprobeData.findFfprobItem(listGuid);
            FfprobeOutput item = form.videoFunc.ffprobeData[itemIdx];

            if (item.MergeData is null)
                return;

            FfprobeOutputMain? newFfprobeOutputMain = item.MergeData.FirstOrDefault(x => x.Guid == editGuids.FirstOrDefault().Key);
            if (newFfprobeOutputMain is null)
                return;

            newFfprobeOutputMain.OriDetail = item.MainData.OriDetail;
            newFfprobeOutputMain.NewDetail = item.MainData.NewDetail;

            List<FfprobeOutputMain> newFfprobeOutputMerges = [];
            for (int i = 0; i < item.MergeData.Count; i++)
            {
                item.MergeData[i].idx = -editGuids[item.MergeData[i].Guid];
                item.MergeData[i].MergeGuid = newFfprobeOutputMain.Guid;
                newFfprobeOutputMerges.Add(item.MergeData[i]);
            }

            form.videoFunc.ffprobeData[itemIdx] = new()
            {
                MainData = newFfprobeOutputMain,
                MergeData = newFfprobeOutputMerges,
            };

            int listIdx = form.listView1.findListItem(listGuid);
            form.listView1.Items[listIdx].Tag = newFfprobeOutputMain.Guid;

            var temp = form.videoFunc.ffprobeData[itemIdx];
        }
        #endregion
    }
}
