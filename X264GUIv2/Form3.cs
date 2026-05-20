using System.Collections.Concurrent;
using X264GUIv2.Models;

namespace X264GUIv2
{
    public partial class Form3 : Form
    {
        public readonly Form1 form;
        private FfprobeOutput formFfprobeOutput { get; set; } = new();
        private FfprobeOutput tempFfprobeOutput { get; set; } = new();

        #region 內建元件
        public readonly ContextMenuStrip listViewMenu;
        public readonly ToolStripMenuItem listFolderViewItem;
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
            listView1.Columns.Add("檔案名稱", listView1.ClientSize.Width);

            #region ContextMenuStrip
            listViewMenu = new();

            listFolderViewItem = new() { Text = "檢視資料夾" };
            listFolderViewItem.Click += listFolderViewItem_Click;

            listUpViewItem = new() { Text = "上移" };
            listUpViewItem.Click += listUpViewItem_Click;

            listDnViewItem = new() { Text = "下移" };
            listDnViewItem.Click += listDnViewItem_Click;

            listDiffViewItem = new() { Text = "移除" };
            listDiffViewItem.Click += listDiffViewItem_Click;

            listAddViewItem = new() { Text = "加入" };
            listAddViewItem.Click += listAddViewItem_Click;

            listViewMenu.Items.AddRange([
                listFolderViewItem,
                new ToolStripSeparator(),
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

            formFfprobeOutput = ffprobeOutput;

            listView1.Items.Clear();
            addItem(formFfprobeOutput.MergeData);
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
                listFolderViewItem.Enabled = item != null;

                if (item != null)
                {
                    listView1.FocusedItem = item;
                    listViewMenu.Tag = item.Tag;
                }
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
        private void listFolderViewItem_Click(object? sender, EventArgs e)
        {
            try
            {
                Guid? guid = (Guid?)listViewMenu.Tag;
                int idx = listView1.findListItem(guid);
                OtherControlFunc.openFolder(listView1.Items[idx].ToolTipText);
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

                if (openfile.ShowDialog() != DialogResult.OK || openfile.FileNames.Length == 0)
                    return;

                List<FfprobeOutputMain> ffprobeOutputMains = add(openfile.FileNames);
                tempFfprobeOutput.MainData = formFfprobeOutput.MainData.Clone();
                tempFfprobeOutput.MergeData = [];
                tempFfprobeOutput.MergeData.AddRange(ffprobeOutputMains);

                FfprobeOutput merge = VideoFunc.mergeFunc(tempFfprobeOutput.MergeData);
                tempFfprobeOutput.MergeData = merge.MergeData;

                addItem(ffprobeOutputMains);
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

            int itemIdx = form.videoFunc.ffprobeData.findFfprobItem(listGuid);
            int oriItemIdx = form.videoFunc.ffprobeData[itemIdx].MainData.Clone().idx;

            Dictionary<Guid, int> editGuids =
                listView1.Items.Cast<ListViewItem>().Select(x => (Guid?)x.Tag)
                .Where(x => x is not null)
                .Select((x, idx) => new { guid = x, idx })
                .OrderBy(x => x.idx)
                .ToDictionary(x => (Guid)x.guid!, x => x.idx);

            if (formFfprobeOutput.MergeData is null)
                return;

            if (tempFfprobeOutput.MergeData is not null)
            {
                formFfprobeOutput.MergeData.AddRange(tempFfprobeOutput.MergeData);
                tempFfprobeOutput.MergeData = [];
            }

            FfprobeOutputMain? newFfprobeOutputMain = formFfprobeOutput.MergeData.FirstOrDefault(x => x.Guid == editGuids.First().Key);
            if (newFfprobeOutputMain is null)
                return;

            List<FfprobeOutputMain> newFfprobeOutputMerges = [];
            for (int i = 0; i < formFfprobeOutput.MergeData.Count; i++)
            {
                if (!editGuids.ContainsKey(formFfprobeOutput.MergeData[i].Guid))
                    continue;
                formFfprobeOutput.MergeData[i].mergeIdx = editGuids[formFfprobeOutput.MergeData[i].Guid];
                newFfprobeOutputMerges.Add(formFfprobeOutput.MergeData[i]);
            }

            FfprobeOutput merge = VideoFunc.mergeFunc(newFfprobeOutputMerges);

            merge.MainData.idx = oriItemIdx;

            form.videoFunc.ffprobeData[itemIdx] = new()
            {
                MainData = merge.MainData,
                MergeData = merge.MergeData,
            };

            int listIdx = form.listView1.findListItem(listGuid);
            form.listView1.Items[listIdx] = form.listView1.DataViewObject(form.videoFunc.ffprobeData[itemIdx].MainData);
        }

        private List<FfprobeOutputMain> add(string[] files)
        {
            List<LoadFile> loadFiles = [.. files.Select((x, idx) => new LoadFile
            {
                File = x,
                index = idx,
            })];

            ConcurrentBag<FfprobeOutputMain> _ffprobe = [];
            Parallel.ForEach(loadFiles,
                () => new HashSet<object>(), // 每個 thread 一份
                (file, state, localVisited) =>
                {
                    FfprobeOutputMain main = VideoFunc.ffprobe(file);
                    main.mergeIdx = main.idx;
                    _ffprobe.Add(main);
                    return localVisited;
                },
                _ => { }
            );

            List<FfprobeOutputMain> ffprobeOutputMains = [.. _ffprobe];
            for (int i = 0; i < ffprobeOutputMains.Count; i++)
                ffprobeOutputMains[i] = form.videoFunc.changeFunc(ffprobeOutputMains[i]);

            return ffprobeOutputMains;
        }

        private void addItem(List<FfprobeOutputMain> ffprobeOutputMains)
        {
            foreach (FfprobeOutputMain ffprobe in ffprobeOutputMains.OrderBy(x => x.mergeIdx))
            {
                ListViewItem lis = new([ffprobe.InFileName])
                {
                    Tag = ffprobe.Guid,
                    ToolTipText = ffprobe.InFile,
                };

                listView1.Items.Add(lis);
            }
        }
        #endregion
    }
}
