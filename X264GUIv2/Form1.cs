using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms.VisualStyles;
using X264GUIv2.Enums;
using X264GUIv2.Models;

namespace X264GUIv2
{
    public partial class Form1 : Form
    {
        #region 變數
        private readonly VideoFunc videoFunc;
        private CancellationTokenSource? Cts { get; set; }
        private readonly Form1Control form1Control;

        /// <summary>
        /// 初始化彼特率
        /// </summary>
        public int bitRateDefault = 1000000;

        /// <summary>
        /// 目前使用的ListItem
        /// </summary>
        public int useIdx { get; set; } = -1;
        #endregion

        #region 內鍵元件
        public readonly Form2 f2;
        public readonly ContextMenuStrip listViewMenu;
        public readonly ToolStripMenuItem listDiffViewItem;
        public readonly ToolStripMenuItem listFolderViewItem;
        public readonly ToolStripMenuItem listUpViewItem;
        public readonly ToolStripMenuItem listDnViewItem;
        public readonly ToolStripMenuItem listRestViewItem;
        #endregion

        #region 進度分配設定
        public float weightAudio => AutoTrimToolStripMenuItem.Checked ? .10f : 0f;
        public float weightOnePass => (.96f - weightAudio) / 2;
        public float weightTwoPass => weightOnePass;
        public float weightMerge => 1f - (weightAudio + weightOnePass + weightTwoPass);
        #endregion

        #region 初始化

        public Form1()
        {
            InitializeComponent();
            f2 = new();
            videoFunc = new(this);
            form1Control = new(this);

            fpsCBox.Items.Add(new ComboboxItem("Auto", "Auto"));
            fpsCBox.Items.Add(new ComboboxItem("23.976", "24000/1001"));
            fpsCBox.Items.Add(new ComboboxItem("25", "25/1"));
            fpsCBox.Items.Add(new ComboboxItem("29.970", "30000/1001"));
            fpsCBox.Items.Add(new ComboboxItem("30", "30/1"));

            int cpunumber = Environment.ProcessorCount;
            for (int i = 0; i <= cpunumber; i++)
                coreCBox.Items.Add(i);

            bitrateNumeric.Increment = new decimal([100, 0, 0, 0]);
            bitrateNumeric.Maximum = new decimal([100000, 0, 0, 0]);
            bitrateNumeric.Minimum = new decimal([1, 0, 0, 0]);
            bitrateNumeric.Value = bitRateDefault / 1000;

            foreach (BitrateEnum e in Enum.GetValues<BitrateEnum>())
                bitrateCBox.Items.Add(new ComboboxItem(Enum.GetName(e)!, ((int)e).ToString()));

            foreach (ResolutionEnum e in Enum.GetValues<ResolutionEnum>())
            {
                if (e == ResolutionEnum.Auto)
                    resolutionCBox.Items.Add(new ComboboxItem("Auto", ((int)e).ToString()));
                else
                    resolutionCBox.Items.Add(new ComboboxItem(((int)e).ToString(), ((int)e).ToString()));
            }

            bitrateCBox.SelectedIndex = 0;
            fpsCBox.SelectedIndex = 1;
            resolutionCBox.SelectedIndex = 0;
            coreCBox.SelectedIndex = 0;
            listView1.View = View.Details;
            listView1.Columns.AddRange([.. new List<ColumnHeader>
            {
                new() { Text = "檔案", Width = 150 },
                new() { Text = "BitRate", Width = 140, TextAlign = HorizontalAlignment.Center },
                new() { Text = "FPS 模式", Width = 90, TextAlign = HorizontalAlignment.Center },
                new() { Text = "FPS", Width = 120, TextAlign = HorizontalAlignment.Center },
                new() { Text = "解析度", Width = 170, TextAlign = HorizontalAlignment.Center },
                new() { Text = "時間長度", Width = 80, TextAlign = HorizontalAlignment.Center },
                new() { Text = "檔案大小", Width = 140, TextAlign = HorizontalAlignment.Center },
                new() { Text = "進度", Width = 70, TextAlign = HorizontalAlignment.Center },
                new() { Text = "狀態", Width = 80, TextAlign = HorizontalAlignment.Center },
                new() { Text = "消耗時間", Width = 80, TextAlign = HorizontalAlignment.Center },
                new() { Text = "路徑", Width = 500 },
            }]);

            settingToolStripMenuItem.DropDown.Closing += settingToolStripMenuItem_DropDownClosing;

            #region ContextMenuStrip
            listViewMenu = new();

            listFolderViewItem = new() { Text = "檢視資料夾" };
            listFolderViewItem.Click += listFolderViewItem_Click;

            listRestViewItem = new() { Text = "重置狀態" };
            listRestViewItem.Click += listRestViewItem_Click;

            listUpViewItem = new() { Text = "上移" };
            listUpViewItem.Click += listUpViewItem_Click;

            listDnViewItem = new() { Text = "下移" };
            listDnViewItem.Click += listDnViewItem_Click;

            listDiffViewItem = new() { Text = "移除" };
            listDiffViewItem.Click += listDiffViewItem_Click;

            listViewMenu.Items.AddRange([
                listFolderViewItem,
                new ToolStripSeparator(),
                listRestViewItem,
                new ToolStripSeparator(),
                listDiffViewItem,
                new ToolStripSeparator(),
                listUpViewItem,
                listDnViewItem,
            ]);
            #endregion
        }

        private void Form1_Load(object sender, EventArgs e) => CheckForIllegalCrossThreadCalls = false;

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Cts != null && !Cts.Token.IsCancellationRequested)
            {
                OtherControlFunc.ShowError("轉檔中，請先停止後再關閉程式");
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
                //dbSaveToolStripMenuItem_Click(sender, e);
            }
        }

        #endregion

        #region Button

        private void runBtn_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count != 0)
            {
                Cts = new();
                Task.Run(() =>
                {
                    Stopwatch sw1 = new();
                    sw1.Start();

                    form1Control.btnControl(false);

                    Stopwatch sw2 = new();
                    List<ListViewItem> listViewItems = [.. listView1.Items.Cast<ListViewItem>()];
                    for (int idx = 0; idx < listViewItems.Count; idx++)
                    {
                        if (Cts.Token.IsCancellationRequested)
                            return;

                        sw2.Reset();
                        sw2.Start();

                        try
                        {
                            useIdx = idx;
                            int itemIdx = OtherControlFunc.findFfprobItem(videoFunc.ffprobeData, (Guid?)listViewItems[idx].Tag);
                            mainProcess(videoFunc.ffprobeData[itemIdx], sw1, sw2);
                            if (videoFunc.ffprobeData[itemIdx].run == RunEnum.Stop)
                                break;
                        }
                        catch (Exception ex)
                        {
                            errProcess(videoFunc.ffprobeData[idx], -1, out RunEnum isRun);
                            WriteFile.WriteLog(ex.Message);
                            continue;
                        }

                        sw2.Stop();
                    }

                    form1Control.btnControl(true);
                    Cts.Cancel();

                    sw1.Stop();
                    timeStripStatus.Text = OtherControlFunc.timeConv(sw1);
                }, Cts.Token);
            }
        }

        private void stopBtn_Click(object sender, EventArgs e)
        {
            if (Cts != null && !Cts.Token.IsCancellationRequested)
            {
                Cts.Cancel();
                listView1.Items[useIdx].UseItemStyleForSubItems = false;
                listView1.Items[useIdx].SubItems[8].Text = RunEnum.Stop.GetDisplayName();
                var idx = OtherControlFunc.findFfprobItem(videoFunc.ffprobeData, (Guid?)listView1.Items[useIdx].Tag);
                videoFunc.ffprobeData[idx].run = RunEnum.Stop;
                VideoFunc.Delete(videoFunc.ffprobeData[idx]);
                form1Control.btnControl(true);
                TaskbarProgress.Error();
                OtherControlFunc.ShowError("已強制停止");
            }
        }

        private void addBtn_Click(object sender, EventArgs e)
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
                    videoFunc.Encode(openfile.FileNames);
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void diffBtn_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = listView1.SelectedItems.Count - 1; i >= 0; i--)
                {
                    int item = listView1.SelectedIndices[i];
                    videoFunc.ffprobeData.RemoveAt(item);
                    listView1.Items.RemoveAt(item);
                }

                progressText.Text = $"{videoFunc.ffprobeData.Count(x => x.run == RunEnum.Done)}/{videoFunc.ffprobeData.Count}";
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }
        #endregion

        #region ToolStrip

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                addBtn_Click(sender, e);
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void loadAvsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openfile = new()
                {
                    Multiselect = true
                };

                string fileStr = $"*.{VideoExt.avs};";
                fileStr = fileStr[..^1];

                openfile.Filter = $"{fileStr}|{fileStr}";

                if (openfile.ShowDialog() == DialogResult.OK)
                    videoFunc.Encode(openfile.FileNames);
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void createMergeToolStripMenuItem_Click(object sender, EventArgs e)
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
                    videoFunc.Encode(openfile.FileNames);
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void diffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                diffBtn_Click(sender, e);
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                listView1.Items.Clear();
                videoFunc.ffprobeData.Clear();
                runBtn.Enabled = false;
                stopBtn.Enabled = false;
                progressText.Text = $"{videoFunc.ffprobeData.Count(x => x.run == RunEnum.Done)}/{videoFunc.ffprobeData.Count}";
                listView1.Columns[0].Tag = false;
                listView1.Refresh();
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void logViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                f2.Show();
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void logViewClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                f2.clear();
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void dbLoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using var sql = new sqlLiteFunc();
                List<FfprobeOutput> loadData = [.. sql.SelectTable().OrderBy(x => x.idx)];
                List<FfprobeOutput> cacheData = [];

                foreach (FfprobeOutput i in loadData)
                {
                    if (videoFunc.ffprobeData.Any(x => x.Guid == i.Guid))
                        continue;

                    cacheData.Add(i);
                }

                if (cacheData.Count == 0)
                    return;

                videoFunc.ffprobeData.AddRange(cacheData);

                foreach (FfprobeOutput ffprobeOutput in cacheData)
                    listView1.Items.Add(VideoFunc.DataViewObject(ffprobeOutput));

                progressText.Text = $"{videoFunc.ffprobeData.Count(x => x.run == RunEnum.Done)}/{videoFunc.ffprobeData.Count}";
                runBtn.Enabled = listView1.Items.Count != 0;
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void dbSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (videoFunc.ffprobeData.Count == 0)
                {
                    if (e is FormClosingEventArgs)
                        return;

                    OtherControlFunc.ShowError("無可儲存資料");
                    return;
                }

                if (!(MessageBox.Show("確定儲存進度?", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes))
                    return;

                for (int i = 0; i < videoFunc.ffprobeData.Count; i++)
                {
                    var idx = OtherControlFunc.findListItem(listView1, videoFunc.ffprobeData[i].Guid);
                    videoFunc.ffprobeData[i].idx = idx;
                }

                using var sql = new sqlLiteFunc();
                sql.Insert(videoFunc.ffprobeData);
                MessageBox.Show("儲存成功");
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }
        private void dbClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(MessageBox.Show("確定清除進度?", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes))
                    return;

                using var sql = new sqlLiteFunc();
                sql.DropTable();
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void installPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(WriteFile.WritePath))
                    OtherControlFunc.openFolder(WriteFile.WritePath);
                else
                    OtherControlFunc.openFolder(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void settingToolStripMenuItem_DropDownClosing(object? sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                e.Cancel = true;
        }
        #endregion

        #region listView
        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e) => e.DrawDefault = true;
        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e) => e.DrawDefault = true;

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 0)
                {
                    e.DrawBackground();
                    CheckBoxRenderer.DrawCheckBox(e.Graphics,
                        new Point(e.Bounds.Left + 4, e.Bounds.Top + 4),
                        new Rectangle(e.Bounds.X + 18, e.Bounds.Y + 4, e.Bounds.Width - 24, e.Bounds.Height - 4),
                        "檔案名稱",
                        new Font("微軟正黑體", 9.0f, FontStyle.Regular),
                        TextFormatFlags.Left,
                        false,
                        Convert.ToBoolean(e.Header?.Tag) ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal);
                }
                else
                    e.DrawDefault = true;
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
            }
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            try
            {
                if (e.Column == 0)
                {
                    bool value = Convert.ToBoolean(listView1.Columns[e.Column].Tag);
                    listView1.Columns[e.Column].Tag = !value;
                    foreach (ListViewItem item in listView1.Items)
                        item.Checked = !value;
                    listView1.Invalidate();
                }
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
            }
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data == null)
                    return;

                e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;

            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
            }
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data == null)
                {
                    runBtn.Enabled = true;
                    return;
                }

                if (e.Data.GetData(DataFormats.FileDrop, false) is not string[] files)
                {
                    runBtn.Enabled = true;
                    return;
                }

                foreach (string file in files)
                {
                    List<string> f = [];
                    string extension = Path.GetExtension(file);
                    if (VideoExt.GetVideoExt.Any(x => $".{x}" == extension))
                        f.Add(file);

                    videoFunc.Encode([.. f]);
                }

                runBtn.Enabled = true;
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
            }
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button != MouseButtons.Right)
                    return;

                ListViewItem? item = listView1.GetItemAt(e.X, e.Y);
                if (item == null)
                    return;

                listView1.FocusedItem = item;
                listViewMenu.Tag = item.Tag;
                listViewMenu.Show(listView1, e.Location);
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void listFolderViewItem_Click(object? sender, EventArgs e)
        {
            try
            {
                FfprobeOutput? ffprobeOutput = videoFunc.ffprobeData.FirstOrDefault(x => x.Guid == (Guid?)listViewMenu.Tag);
                if (ffprobeOutput == null)
                    return;
                OtherControlFunc.openFolder(ffprobeOutput.InFile);

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
                    var idx1 = OtherControlFunc.findFfprobItem(videoFunc.ffprobeData, (Guid?)item.Tag);
                    var idx2 = OtherControlFunc.findListItem(listView1, (Guid?)item.Tag);

                    videoFunc.ffprobeData.RemoveAt(idx1);
                    listView1.Items.RemoveAt(idx2);
                }

                progressText.Text = $"{videoFunc.ffprobeData.Count(x => x.run == RunEnum.Done)}/{videoFunc.ffprobeData.Count}";
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
                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    if (item.Index > 0)
                    {
                        int index = item.Index - 1;
                        listView1.Items.RemoveAt(item.Index);
                        listView1.Items.Insert(index, item);
                    }
                }
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
                for (int i = listView1.SelectedItems.Count - 1; i >= 0; i--)
                {
                    ListViewItem item = listView1.SelectedItems[i];

                    if (item.Index < listView1.Items.Count - 1)
                    {
                        int index = item.Index + 1;

                        listView1.Items.RemoveAt(item.Index);
                        listView1.Items.Insert(index, item);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void listRestViewItem_Click(object? sender, EventArgs e)
        {
            try
            {
                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    var idx1 = OtherControlFunc.findFfprobItem(videoFunc.ffprobeData, (Guid?)item.Tag);
                    var idx2 = OtherControlFunc.findListItem(listView1, (Guid?)item.Tag);

                    videoFunc.ffprobeData[idx1].run = RunEnum.Idel;
                    listView1.Items[idx2].SubItems[8].ForeColor = Color.Black;
                    listView1.Items[idx2].SubItems[8].Text = RunEnum.Idel.GetDisplayName();
                }
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }
        #endregion

        #region Changed
        private void bitrateCBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                form1Control.bitrateCBoxControl();

                OtherControlFunc.listViewCheck(this, videoFunc.ffprobeData, idx => videoFunc.ffprobeData[idx] = videoFunc.bitRateFunc(videoFunc.ffprobeData[idx]));
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
            }
        }

        private void fpsCBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                OtherControlFunc.listViewCheck(this, videoFunc.ffprobeData, idx => videoFunc.ffprobeData[idx] = videoFunc.fpsFunc(videoFunc.ffprobeData[idx]));
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
            }
        }

        private void resolutionCBox_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                OtherControlFunc.listViewCheck(this, videoFunc.ffprobeData, idx => videoFunc.ffprobeData[idx] = videoFunc.resolutionFunc(videoFunc.ffprobeData[idx]));
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
            }
        }

        private void resolutionCBox_Leave(object sender, EventArgs e)
        {
            try
            {
                resolutionCBox_SelectedValueChanged(sender, e);
            }
            catch
            {
            }
        }

        private void bitrateNumeric_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (bitrateNumeric.Value == 0)
                    return;

                OtherControlFunc.listViewCheck(this, videoFunc.ffprobeData, idx => videoFunc.ffprobeData[idx] = videoFunc.bitRateNumericFunc(videoFunc.ffprobeData[idx]));
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
            }
        }
        #endregion

        #region 轉換

        private void mainProcess(FfprobeOutput ffprobeOutput, Stopwatch sw1, Stopwatch sw2)
        {
            if (ffprobeOutput.run == RunEnum.Done)
                return;

            if (!File.Exists(ffprobeOutput.InFile))
                throw new Exception($"無效路徑 {ffprobeOutput.InFile}");

            if (string.IsNullOrWhiteSpace(ffprobeOutput.InFilePath))
                throw new Exception($"無效路徑 {ffprobeOutput.InFilePath}");

            Environment.CurrentDirectory = ffprobeOutput.InFilePath;

            int listViewCount = listView1.Items.Count;
            int exitCode = 0;

            #region 初始化
            listView1.Items[useIdx].SubItems[8].ForeColor = Color.Black;
            listView1.Items[useIdx].SubItems[8].Text = RunEnum.Init.GetDisplayName();
            ffprobeOutput.run = RunEnum.Init;
            progressText.Text = $"{videoFunc.ffprobeData.Count(x => x.run == RunEnum.Done)}/{videoFunc.ffprobeData.Count}";

            stopBtn.Enabled = true;
            form1Control.UpdateProgres(0, videoFunc.ffprobeData.Count);
            TaskbarProgress.Clear();
            TaskbarProgress.Set(videoFunc.ffprobeData.Count(x => x.run == RunEnum.Done), videoFunc.ffprobeData.Count);

            if (ffprobeOutput.videoType != VideoTypeEnum.Aviscript)
            {
                string avs;
                if (string.IsNullOrWhiteSpace(ffprobeOutput.SubtitlesFile))
                {
                    avs = $@"
LoadPlugin(""{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\ffms2.dll"") 
FFVideoSource(""{Path.GetFileName(ffprobeOutput.InFileName)}"", fpsnum={ffprobeOutput.NewDetail.fpsnum}, fpsden={ffprobeOutput.NewDetail.fpsden})
#deinterlace #crop #resize #denoise";
                }
                else
                {
                    avs = $@"
LoadPlugin(""{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\ffms2.dll"") 
FFVideoSource(""{Path.GetFileName(ffprobeOutput.InFileName)}"", fpsnum={ffprobeOutput.NewDetail.fpsnum}, fpsden={ffprobeOutput.NewDetail.fpsden})
LoadPlugin(""{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\VSFilter.dll"")
TextSub(""{ffprobeOutput.avsTempFile}.ass"",1)
#deinterlace #crop #resize #denoise";
                    File.Copy(ffprobeOutput.SubtitlesFile, $"{ffprobeOutput.avsTempFile}.ass", true);
                }

                File.WriteAllText(@$".\{ffprobeOutput.avsTempFile}.avs", avs);
            }
            #endregion

            TaskbarProgress.Set(videoFunc.ffprobeData.Count(x => x.run == RunEnum.Done), videoFunc.ffprobeData.Count);
            string mapMsg = string.Empty;
            if (ffprobeOutput.videoType != VideoTypeEnum.Aviscript)
            {
                ffprobeOutput = audioProcess(ffprobeOutput, sw1, sw2, out mapMsg);
                ffprobeOutput = errProcess(ffprobeOutput, 0, out RunEnum isRunAudio);
                switch (isRunAudio)
                {
                    case RunEnum.Stop: return;
                    case RunEnum.Error: throw new Exception($"{RunEnum.SoundProcessing.GetDisplayName()} : {mapMsg}");
                }
            }

            TaskbarProgress.Set(videoFunc.ffprobeData.Count(x => x.run == RunEnum.Done), videoFunc.ffprobeData.Count);
            string onePassMsg = string.Empty;
            if (ffprobeOutput.videoType == VideoTypeEnum.Aviscript)
                ffprobeOutput = onePassAvsProcess(ffprobeOutput, sw1, sw2, out exitCode, out onePassMsg);
            else
                ffprobeOutput = onePassProcess(ffprobeOutput, sw1, sw2, out exitCode, out onePassMsg);
            ffprobeOutput = errProcess(ffprobeOutput, exitCode, out RunEnum isRunOnePass);
            switch (isRunOnePass)
            {
                case RunEnum.Stop: return;
                case RunEnum.Error: throw new Exception($"{RunEnum.OnePass.GetDisplayName()} : {onePassMsg}");
            }

            TaskbarProgress.Set(videoFunc.ffprobeData.Count(x => x.run == RunEnum.Done), videoFunc.ffprobeData.Count);
            string twoPassMsg = string.Empty;
            if (ffprobeOutput.videoType == VideoTypeEnum.Aviscript)
                ffprobeOutput = twoPassAvsProcess(ffprobeOutput, sw1, sw2, out exitCode, out onePassMsg);
            else
                ffprobeOutput = twoPassProcess(ffprobeOutput, sw1, sw2, out exitCode, out twoPassMsg);
            ffprobeOutput = errProcess(ffprobeOutput, exitCode, out RunEnum isRunTwoPass);
            switch (isRunTwoPass)
            {
                case RunEnum.Stop: return;
                case RunEnum.Error: throw new Exception($"{RunEnum.TwoPass.GetDisplayName()} : {twoPassMsg}");
            }

            if (ffprobeOutput.videoType != VideoTypeEnum.Aviscript)
            {
                TaskbarProgress.Set(videoFunc.ffprobeData.Count(x => x.run == RunEnum.Done), videoFunc.ffprobeData.Count);
                ffprobeOutput = mergeProcess(ffprobeOutput, sw1, sw2, out exitCode, out string mergeMsg);
                ffprobeOutput = errProcess(ffprobeOutput, exitCode, out RunEnum isRunMerge);
                switch (isRunMerge)
                {
                    case RunEnum.Stop: return;
                    case RunEnum.Error: throw new Exception($"{RunEnum.Merge.GetDisplayName()} : {mergeMsg}");
                }
            }

            VideoFunc.Delete(ffprobeOutput);
            listView1.Items[useIdx].SubItems[7].Text = "100 %";
            listView1.Items[useIdx].SubItems[8].Text = RunEnum.Done.GetDisplayName();
            ffprobeOutput.run = RunEnum.Done;

            form1Control.UpdateProgres(100, 100);
            if (videoFunc.ffprobeData.Count(x => x.run == RunEnum.Done) == videoFunc.ffprobeData.Count)
                TaskbarProgress.Set(videoFunc.ffprobeData.Count, videoFunc.ffprobeData.Count);
            else
                TaskbarProgress.Error();

            TimeSpan Timemint = TimeSpan.FromSeconds(sw2.Elapsed.TotalSeconds);
            listView1.Items[useIdx].SubItems[9].Text = string.Format("{0:D2}:{1:D2}:{2:D2}", Timemint.Hours, Timemint.Minutes, Timemint.Seconds);
        }

        /// <summary>
        /// 音效處理
        /// </summary>
        private FfprobeOutput audioProcess(FfprobeOutput ffprobeOutput, Stopwatch sw1, Stopwatch sw2, out string isCloseMsg)
        {
            isCloseMsg = string.Empty;

            if (Cts == null || Cts.Token.IsCancellationRequested || ffprobeOutput.audioMap == 0)
                return ffprobeOutput;

            if (AutoTrimToolStripMenuItem.Checked)
            {
                listView1.Items[useIdx].SubItems[8].Text = RunEnum.AudioTrim.GetDisplayName();
                ffprobeOutput.run = RunEnum.AudioTrim;
                TaskHelper t = new()
                {
                    Cts = Cts,
                    RunPath = ffprobeOutput.InFilePath,
                    FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\ffmpeg\ffmpeg.exe",
                    ArgumentList = {
                        $@"-i ""{ffprobeOutput.InFilePath}\{ffprobeOutput.InFileName}""",
                        $@"-ar 48000",
                        $@"-ac 2",
                        $@"-af ""aresample=48000,asetpts=PTS-STARTPTS""",
                        $@"-c:a aac",
                        $@"-q:a 1 ""{ffprobeOutput.InFilePath}\{ffprobeOutput.avsTempFile}.aac""",
                        $@"-fflags +genpts",
                        $@"-avoid_negative_ts make_zero",
                        $@"-movflags +faststart",
                        $@"-progress pipe:1",
                        $@"-nostats",
                        $@"-loglevel error",
                    },
                    ActionOut = sr =>
                    {
                        f2.appendText = sr;
                        form1Control.ffmpegOutput(ffprobeOutput, sr, sw1, sw2);
                    },
                };

                int exitCode = t.RunTask();
                isCloseMsg = t.isCloseMsg;
                if (exitCode == 0)
                    ffprobeOutput.isAac = true;
            }
            else if (Path.GetExtension(ffprobeOutput.InFile).Equals($".{VideoExt.mkv}", StringComparison.CurrentCultureIgnoreCase))
            {
                if (Cts == null || Cts.Token.IsCancellationRequested)
                    return ffprobeOutput;
                listView1.Items[useIdx].SubItems[8].Text = RunEnum.SoundSeparation.GetDisplayName();
                ffprobeOutput.run = RunEnum.SoundSeparation;
                new TaskHelper
                {
                    Cts = Cts,
                    RunPath = ffprobeOutput.InFilePath,
                    FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\mkvextract.exe",
                    ArgumentList = {
                        $@"tracks ""{ffprobeOutput.InFile}""",
                        $@"1:""{ffprobeOutput.InFilePath}\{ffprobeOutput.avsTempFile}.aac""",
                    },
                    ActionOut = sr =>
                    {
                        timeStripStatus.Text = OtherControlFunc.timeConv(sw1);
                        f2.appendText = sr;
                    },
                }.RunTask();

                if (!ffprobeOutput.isAac)
                {
                    if (Cts.Token.IsCancellationRequested)
                        return ffprobeOutput;
                    listView1.Items[useIdx].SubItems[8].Text = RunEnum.SoundProcessing.GetDisplayName();
                    ffprobeOutput.run = RunEnum.SoundProcessing;
                    new TaskHelper
                    {
                        Cts = Cts,
                        RunPath = ffprobeOutput.InFilePath,
                        FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\eac3to\eac3to.exe",
                        ArgumentList = {
                            $@"-log=NUL",
                            $@"""{ffprobeOutput.InFilePath}\{ffprobeOutput.avsTempFile}.aac""",
                            $@"""{ffprobeOutput.InFilePath}\{ffprobeOutput.avsTempFile}.mp4""",
                         },
                        ActionOut = sr =>
                        {
                            timeStripStatus.Text = OtherControlFunc.timeConv(sw1);
                            f2.appendText = sr;
                        },
                    }.RunTask();
                }
            }

            timeStripStatus.Text = OtherControlFunc.timeConv(sw1);
            return ffprobeOutput;
        }

        /// <summary>
        /// OnePass
        /// </summary>
        private FfprobeOutput onePassProcess(FfprobeOutput ffprobeOutput, Stopwatch sw1, Stopwatch sw2, out int exitCode, out string isCloseMsg)
        {
            exitCode = 0;
            isCloseMsg = string.Empty;

            if (Cts == null || Cts.Token.IsCancellationRequested)
                return ffprobeOutput;

            listView1.Items[useIdx].SubItems[8].Text = RunEnum.OnePass.GetDisplayName();
            ffprobeOutput.run = RunEnum.OnePass;
            TaskHelper t = new()
            {
                Cts = Cts,
                RunPath = ffprobeOutput.InFilePath,
                AutoCloseDialogBox = "Warning",
                FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\avs4x26x.exe",
                ArgumentList = videoFunc.Xonepass(ffprobeOutput),
                ActionErr = sr =>
                {
                    f2.appendText = sr;
                    listView1.Items[useIdx].SubItems[9].Text = OtherControlFunc.timeConv(sw2);
                    timeStripStatus.Text = OtherControlFunc.timeConv(sw1);
                    if (sr.IndexOf("frames,") != -1 && sr.IndexOf('[') != -1)
                    {
                        double prodata = Math.Round(Convert.ToDouble(sr.Substring(sr.IndexOf('[') + 1, sr.LastIndexOf('%') - 1)), 2);
                        listView1.Items[useIdx].SubItems[7].Text = $"{prodata:F1} %";
                        form1Control.calculateProgres(ffprobeOutput, (float)prodata);
                    }

                    if (sr.IndexOf("[error]") != -1)
                    {
                        ffprobeOutput.run = RunEnum.Error;
                        throw new Exception(sr);
                    }
                }
            };
            exitCode = t.RunTask();
            isCloseMsg = t.isCloseMsg;

            return ffprobeOutput;
        }

        /// <summary>
        /// TwoPass
        /// </summary>
        private FfprobeOutput twoPassProcess(FfprobeOutput ffprobeOutput, Stopwatch sw1, Stopwatch sw2, out int exitCode, out string isCloseMsg)
        {
            exitCode = 0;
            isCloseMsg = string.Empty;

            if (Cts == null || Cts.Token.IsCancellationRequested)
                return ffprobeOutput;

            listView1.Items[useIdx].SubItems[8].Text = RunEnum.TwoPass.GetDisplayName();
            ffprobeOutput.run = RunEnum.TwoPass;
            TaskHelper t = new()
            {
                Cts = Cts,
                RunPath = ffprobeOutput.InFilePath,
                AutoCloseDialogBox = "Warning",
                FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\avs4x26x.exe",
                ArgumentList = videoFunc.Xtwopass(ffprobeOutput),
                ActionErr = sr =>
                {
                    f2.appendText = sr;
                    listView1.Items[useIdx].SubItems[9].Text = OtherControlFunc.timeConv(sw2);
                    timeStripStatus.Text = OtherControlFunc.timeConv(sw1);
                    if (sr.IndexOf("frames,") != -1 && sr.IndexOf('[') != -1)
                    {
                        double prodata = Math.Round(Convert.ToDouble(sr.Substring(sr.IndexOf('[') + 1, sr.LastIndexOf('%') - 1)), 2);
                        listView1.Items[useIdx].SubItems[7].Text = $"{prodata:F1} %";
                        form1Control.calculateProgres(ffprobeOutput, (float)prodata);
                    }

                    if (sr.IndexOf("[error]") != -1)
                    {
                        ffprobeOutput.run = RunEnum.Error;
                        throw new Exception(sr);
                    }
                }
            };
            exitCode = t.RunTask();
            isCloseMsg = t.isCloseMsg;

            return ffprobeOutput;
        }

        /// <summary>
        /// Merge
        /// </summary>
        private FfprobeOutput mergeProcess(FfprobeOutput ffprobeOutput, Stopwatch sw1, Stopwatch sw2, out int exitCode, out string isCloseMsg)
        {
            exitCode = 0;
            isCloseMsg = string.Empty;

            if (Cts == null || Cts.Token.IsCancellationRequested)
                return ffprobeOutput;

            listView1.Items[useIdx].SubItems[8].Text = RunEnum.Merge.GetDisplayName();
            ffprobeOutput.run = RunEnum.Merge;

            string audioFile;
            int step = 0;
            float part = 4f;
            if (Path.GetExtension(ffprobeOutput.InFile).Equals(VideoExt.mkv, StringComparison.CurrentCultureIgnoreCase) || ffprobeOutput.isAac)
            {
                if (File.Exists($@"{ffprobeOutput.InFilePath}\{ffprobeOutput.avsTempFile}.aac"))
                    audioFile = $"{ffprobeOutput.avsTempFile}.aac";
                else
                    audioFile = ffprobeOutput.InFile;
            }
            else
                audioFile = ffprobeOutput.InFile;

            TaskHelper t = new()
            {
                Cts = Cts,
                RunPath = ffprobeOutput.InFilePath,
                FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\mp4box.exe",
                ArgumentList = {
                    $@"-add ""{ffprobeOutput.avsTempFile}.264""",
                    $@"{(ffprobeOutput.audioMap > 0 ? $@"-add ""{audioFile}""#audio" : "")}",
                    $@"""{ffprobeOutput.OutFile}"""
                },
                ActionErr = sr =>
                {
                    f2.appendText = sr;
                    listView1.Items[useIdx].SubItems[9].Text = OtherControlFunc.timeConv(sw2);
                    timeStripStatus.Text = OtherControlFunc.timeConv(sw1);

                    Match match = Regex.Match(sr, @"\((\d+)/(\d+)\)");
                    if (match.Success)
                    {
                        float current = float.TryParse(match.Groups[1].Value, out float _current) ? _current : 0;
                        listView1.Items[useIdx].SubItems[7].Text = $"{current:F1} %";
                        float pro = (step * (100 / part)) + (current / part);
                        form1Control.calculateProgres(ffprobeOutput, pro);

                        if ((int)current == 100)
                            step++;
                    }
                },
            };
            exitCode = t.RunTask();
            isCloseMsg = t.isCloseMsg;

            return ffprobeOutput;
        }

        /// <summary>
        /// OnePass
        /// </summary>
        private FfprobeOutput onePassAvsProcess(FfprobeOutput ffprobeOutput, Stopwatch sw1, Stopwatch sw2, out int exitCode, out string isCloseMsg)
        {
            exitCode = 0;
            isCloseMsg = string.Empty;

            if (Cts == null || Cts.Token.IsCancellationRequested)
                return ffprobeOutput;

            listView1.Items[useIdx].SubItems[8].Text = RunEnum.OnePass.GetDisplayName();
            ffprobeOutput.run = RunEnum.OnePass;
            TaskHelper t = new()
            {
                Cts = Cts,
                RunPath = ffprobeOutput.InFilePath,
                AutoCloseDialogBox = "Warning",
                FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\ffmpeg\ffmpeg.exe",
                ArgumentList = videoFunc.XonepassAvs(ffprobeOutput),
                ActionOut = sr =>
                {
                    f2.appendText = sr;
                    form1Control.ffmpegOutput(ffprobeOutput, sr, sw1, sw2);
                },
                ActionErr = sr =>
                {
                    f2.appendText = sr;
                }
            };
            exitCode = t.RunTask();
            isCloseMsg = t.isCloseMsg;

            return ffprobeOutput;
        }

        /// <summary>
        /// TwoPass
        /// </summary>
        private FfprobeOutput twoPassAvsProcess(FfprobeOutput ffprobeOutput, Stopwatch sw1, Stopwatch sw2, out int exitCode, out string isCloseMsg)
        {
            exitCode = 0;
            isCloseMsg = string.Empty;

            if (Cts == null || Cts.Token.IsCancellationRequested)
                return ffprobeOutput;

            listView1.Items[useIdx].SubItems[8].Text = RunEnum.TwoPass.GetDisplayName();
            ffprobeOutput.run = RunEnum.TwoPass;
            TaskHelper t = new()
            {
                Cts = Cts,
                RunPath = ffprobeOutput.InFilePath,
                AutoCloseDialogBox = "Warning",
                FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\ffmpeg\ffmpeg.exe",
                ArgumentList = videoFunc.XtwopassAvs(ffprobeOutput),
                ActionOut = sr =>
                {
                    f2.appendText = sr;
                    form1Control.ffmpegOutput(ffprobeOutput, sr, sw1, sw2);
                },
                ActionErr = sr =>
                {
                    f2.appendText = sr;
                }
            };
            exitCode = t.RunTask();
            isCloseMsg = t.isCloseMsg;

            return ffprobeOutput;
        }


        private FfprobeOutput errProcess(FfprobeOutput ffprobeOutput, int exitCode, out RunEnum run)
        {
            if (exitCode != 0 && ffprobeOutput.run != RunEnum.Stop)
            {
                listView1.Items[useIdx].UseItemStyleForSubItems = false;
                listView1.Items[useIdx].SubItems[8].Text = RunEnum.Error.GetDisplayName();
                ffprobeOutput.run = RunEnum.Error;
                listView1.Items[useIdx].SubItems[8].ForeColor = Color.Red;
            }

            if (ffprobeOutput.run == RunEnum.Error)
            {
                run = RunEnum.Error;
                TaskbarProgress.Error();
            }
            else
                run = ffprobeOutput.run;

            return ffprobeOutput;
        }
        #endregion
    }
}
