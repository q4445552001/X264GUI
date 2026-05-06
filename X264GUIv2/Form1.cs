using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms.VisualStyles;
using X264GUIv2.Enums;
using X264GUIv2.Models;

namespace X264GUIv2
{
    public partial class Form1 : Form
    {
        private readonly Graphics BarGraphics;
        private readonly VideoFunc videoFunc;
        private CancellationTokenSource? Cts { get; set; }
        public int bitRateDefault = 1000000; //初始化彼特率
        public readonly Form2 f2;

        private float weightAudio => AutoTrimToolStripMenuItem.Checked ? .10f : 0f;
        private float weightOnePass => .96f - weightAudio;
        private float weightTwoPass => weightOnePass / 2;
        private float weightMerge => 100f - weightOnePass;

        #region 初始化

        public Form1()
        {
            InitializeComponent();
            f2 = new();
            BarGraphics = progressBar1.CreateGraphics();

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
                resolutionCBox.Items.Add(new ComboboxItem(((int)e).ToString(), ((int)e).ToString()));

            bitrateCBox.SelectedIndex = 0;
            fpsCBox.SelectedIndex = 1;
            resolutionCBox.SelectedIndex = resolutionCBox.FindString(((int)ResolutionEnum.Normal).ToString());
            coreCBox.SelectedIndex = 0;

            listView1.View = View.Details;
            listView1.Columns.AddRange([.. new List<ColumnHeader>
            {
                new() { Text = "檔案", Width = 130 },
                new() { Text = "BitRate", Width = 140, TextAlign = HorizontalAlignment.Center },
                new() { Text = "FPS 模式", Width = 100, TextAlign = HorizontalAlignment.Center },
                new() { Text = "FPS", Width = 120, TextAlign = HorizontalAlignment.Center },
                new() { Text = "解析度", Width = 170, TextAlign = HorizontalAlignment.Center },
                new() { Text = "時間長度", Width = 80, TextAlign = HorizontalAlignment.Center },
                new() { Text = "檔案大小", Width = 140, TextAlign = HorizontalAlignment.Center },
                new() { Text = "進度", Width = 70, TextAlign = HorizontalAlignment.Center },
                new() { Text = "狀態", Width = 100, TextAlign = HorizontalAlignment.Center },
                new() { Text = "消耗時間", Width = 80, TextAlign = HorizontalAlignment.Center },
                new() { Text = "路徑", Width = 300 },
            }]);

            videoFunc = new(this);
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
                e.Cancel = false;
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

                    runBtn.Enabled = false;
                    addBtn.Enabled = false;
                    diffBtn.Enabled = false;
                    bitrateCBox.Enabled = false;
                    fpsCBox.Enabled = false;
                    resolutionCBox.Enabled = false;
                    bitrateNumeric.Enabled = false;
                    menuStrip1.Enabled = false;

                    Stopwatch sw2 = new();
                    for (int i = 0; i < videoFunc.ffprobeData.Count; i++)
                    {
                        if (Cts.Token.IsCancellationRequested)
                            return;

                        sw2.Reset();
                        sw2.Start();

                        try
                        {
                            mainProcess(videoFunc.ffprobeData[i], sw1, sw2);
                            if (videoFunc.ffprobeData[i].run == RunEnum.Stop)
                                break;
                        }
                        catch
                        {
                            continue;
                        }

                        sw2.Stop();
                    }

                    stopBtn.Enabled = false;
                    runBtn.Enabled = true;
                    addBtn.Enabled = true;
                    diffBtn.Enabled = true;
                    bitrateCBox.Enabled = true;
                    fpsCBox.Enabled = true;
                    resolutionCBox.Enabled = true;
                    bitrateNumeric.Enabled = true;
                    menuStrip1.Enabled = true;
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
                Process[] localAll = Process.GetProcesses();
                foreach (Process p in localAll)
                {
                    switch (p.ProcessName)
                    {
                        case "x264":
                        case "avs4x26x":
                        case "mp4box":
                        case "eac3to":
                            p.Kill();
                            break;
                    }
                }

                int idx = Convert.ToInt32(progressText.Text?.Split('/')[0]) - 1;
                listView1.Items[idx].SubItems[8].Text = RunEnum.Stop.GetDisplayName();
                int idx2 = videoFunc.ffprobeData.FindIndex(x => x.Guid == (Guid?)listView1.Items[idx].Tag);
                videoFunc.ffprobeData[idx2].run = RunEnum.Stop;
                VideoFunc.Delete(videoFunc.ffprobeData[idx2]);
                stopBtn.Enabled = false;
                runBtn.Enabled = true;
                addBtn.Enabled = true;
                diffBtn.Enabled = true;
                bitrateCBox.Enabled = true;
                fpsCBox.Enabled = true;
                resolutionCBox.Enabled = true;
                bitrateNumeric.Enabled = true;
                menuStrip1.Enabled = true;
                OtherControlFunc.ShowError("已強制停止");
            }
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                addBtn_Click(sender, e);
            }
            catch
            {
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
                OtherControlFunc.WriteLog(ex.Message);
            }
        }

        private void diffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                diffBtn_Click(sender, e);
            }
            catch
            {
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

                progressText.Text = $"0/{listView1.Items.Count}";
            }
            catch (Exception ex)
            {
                OtherControlFunc.WriteLog(ex.Message);
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                listView1.Items.Clear();
                runBtn.Enabled = false;
                stopBtn.Enabled = false;
                progressText.Text = "0/0";
                listView1.Columns[0].Tag = false;
                listView1.Refresh();

                videoFunc.ffprobeData.Clear();
            }
            catch (Exception ex)
            {
                OtherControlFunc.WriteLog(ex.Message);
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
                OtherControlFunc.WriteLog(ex.Message);
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
                OtherControlFunc.WriteLog(ex.Message);
            }
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
                OtherControlFunc.WriteLog(ex.Message);
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
                OtherControlFunc.WriteLog(ex.Message);
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
                OtherControlFunc.WriteLog(ex.Message);
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
                OtherControlFunc.WriteLog(ex.Message);
            }
        }

        private void dbLoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(MessageBox.Show("確定讀取?", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes))
                    return;

                using var sql = new sqlLiteFunc();
                videoFunc.ffprobeData = sql.SelectTable();
                if (videoFunc.ffprobeData.Count == 0)
                {
                    OtherControlFunc.ShowError("無存檔");
                    return;
                }

                foreach (FfprobeOutput ffprobeOutput in videoFunc.ffprobeData)
                    listView1.Items.Add(VideoFunc.DataViewObject(ffprobeOutput));
            }
            catch (Exception ex)
            {
                OtherControlFunc.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void dbSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (videoFunc.ffprobeData.Count == 0)
                {
                    OtherControlFunc.ShowError("無可儲存資料");
                    return;
                }

                if (!(MessageBox.Show("確定儲存?", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes))
                    return;

                for (int i = 0; i < videoFunc.ffprobeData.Count; i++)
                {
                    int idx = listView1.Items.Cast<ListViewItem>().ToList().FindIndex(x => (Guid?)x.Tag == videoFunc.ffprobeData[i].Guid);
                    videoFunc.ffprobeData[i].index = idx;
                }

                using var sql = new sqlLiteFunc();
                sql.Insert(videoFunc.ffprobeData);
                MessageBox.Show("儲存成功");
            }
            catch (Exception ex)
            {
                OtherControlFunc.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void dbClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(MessageBox.Show("確定清除?", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes))
                    return;

                using var sql = new sqlLiteFunc();
                sql.DropTable();

                clearToolStripMenuItem_Click(sender, e);
            }
            catch (Exception ex)
            {
                OtherControlFunc.WriteLog(ex.Message);
                OtherControlFunc.ShowError(ex.Message);
            }
        }
        #endregion

        #region Changed
        private void bitrateCBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                bitrateNumeric.Enabled = bitrateCBox.SelectedItem?.ToString() == "Manual";

                if (videoFunc != null)
                    OtherControlFunc.listViewCheck(this, videoFunc.ffprobeData, idx => videoFunc.ffprobeData[idx] = videoFunc.bitRateFunc(videoFunc.ffprobeData[idx]));
            }
            catch (Exception ex)
            {
                OtherControlFunc.WriteLog(ex.Message);
            }
        }

        private void fpsCBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (videoFunc != null)
                    OtherControlFunc.listViewCheck(this, videoFunc.ffprobeData, idx => videoFunc.ffprobeData[idx] = videoFunc.fpsFunc(videoFunc.ffprobeData[idx]));
            }
            catch (Exception ex)
            {
                OtherControlFunc.WriteLog(ex.Message);
            }
        }

        private void resolutionCBox_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (videoFunc != null)
                    OtherControlFunc.listViewCheck(this, videoFunc.ffprobeData, idx => videoFunc.ffprobeData[idx] = videoFunc.resolutionFunc(videoFunc.ffprobeData[idx]));
            }
            catch (Exception ex)
            {
                OtherControlFunc.WriteLog(ex.Message);
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

                if (videoFunc != null)
                    OtherControlFunc.listViewCheck(this, videoFunc.ffprobeData, idx => videoFunc.ffprobeData[idx] = videoFunc.bitRateNumericFunc(videoFunc.ffprobeData[idx]));
            }
            catch (Exception ex)
            {
                OtherControlFunc.WriteLog(ex.Message);
            }
        }

        private void settingToolStripMenuItem_DropDownClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                e.Cancel = true;
        }
        #endregion

        #region 進度條
        /// <summary>
        /// 進度條
        /// </summary>
        public void UpdateProgres(float now, float count, bool isPercentage = true)
        {
            if (now > 100)
                return;

            void del()
            {
                progressBar1.PerformStep();
                var v = now / count * 100;
                var str = isPercentage ? Math.Round(v, 2).ToString("#0.00") + " %" : $"{now:#,##0}/{count:#,##0}";
                var font = new Font("Consolas", 12, FontStyle.Bold);
                var pt = new PointF(progressBar1.Width / 2 - (str.Length * 4), progressBar1.Height / 2 - 10);
                progressBar1.Value = v >= 100 ? 100 : (int)v;
                BarGraphics.DrawString(str, font, v >= 50 ? Brushes.White : Brushes.Blue, pt);
            }
            Invoke(del);
        }


        /// <summary>
        /// 進度條轉圈圈
        /// </summary>
        public void UpdateProgresLoop(CancellationTokenSource cts)
        {
            var spinnerIndex = 0;
            var spinnerChars = new[] { '|', '/', '-', '\\' };

            void del()
            {
                progressBar1.PerformStep();
                var spinner = spinnerChars[spinnerIndex];
                spinnerIndex = (spinnerIndex + 1) % spinnerChars.Length;
                var font = new Font("Consolas", 12, FontStyle.Bold);
                var pt = new PointF(progressBar1.Width / 2 - 20, progressBar1.Height / 2 - 10);
                progressBar1.Value = 100;
                BarGraphics.DrawString($"{spinner} Loading...", font, Brushes.White, pt);
            }

            while (true)
            {
                if (cts.Token.IsCancellationRequested)
                {
                    Invoke(progressBar1.PerformStep);
                    return;
                }

                Invoke(del);
                Thread.Sleep(500);
            }
        }
        #endregion

        #region 轉換

        private void mainProcess(FfprobeOutput ffprobeOutput, Stopwatch sw1, Stopwatch sw2)
        {
            if (ffprobeOutput.run == RunEnum.Done)
                throw new Exception($"{ffprobeOutput.InFileName} 已完成");

            string? path = Path.GetDirectoryName(ffprobeOutput.InFile);
            if (string.IsNullOrWhiteSpace(path))
                throw new Exception($"無效路徑 {path}");

            Environment.CurrentDirectory = path;

            int listViewCount = listView1.Items.Count;

            #region 初始化
            int idx = listView1.Items.Cast<ListViewItem>().ToList().FindIndex(x => (Guid?)x.Tag == ffprobeOutput.Guid);
            listView1.Items[idx].SubItems[8].ForeColor = Color.Black;
            listView1.Items[idx].SubItems[8].Text = RunEnum.Init.GetDisplayName();
            ffprobeOutput.run = RunEnum.Init;
            listView1.Items[idx].UseItemStyleForSubItems = false;
            progressText.Text = $"{idx + 1}/{listViewCount}";

            stopBtn.Enabled = true;
            UpdateProgres(0, 100);

            double prodatabar = 0;
            string avs = "";

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

            if (File.Exists(@$".\{Path.GetFileNameWithoutExtension(ffprobeOutput.InFile)}.avs"))
                File.Copy(@$".\{Path.GetFileNameWithoutExtension(ffprobeOutput.InFile)}.avs", @$".\{ffprobeOutput.avsTempFile}.avs", true);
            else
                File.WriteAllText(@$".\{ffprobeOutput.avsTempFile}.avs", avs);
            #endregion

            ffprobeOutput = audioProcess(ffprobeOutput, idx, path, sw1, sw2);
            ffprobeOutput = errProcess(ffprobeOutput, idx, 0, out RunEnum isRun0);
            switch (isRun0)
            {
                case RunEnum.Stop: return;
                case RunEnum.Error: throw new Exception($"{RunEnum.SoundProcessing.GetDisplayName()} Error");
            }
            prodatabar = 100 * weightAudio;

            ffprobeOutput = onePassProcess(ffprobeOutput, idx, path, sw1, sw2, prodatabar, out int exitCode);
            ffprobeOutput = errProcess(ffprobeOutput, idx, exitCode, out RunEnum isRun1);
            switch (isRun1)
            {
                case RunEnum.Stop: return;
                case RunEnum.Error: throw new Exception($"{RunEnum.Merge.GetDisplayName()} Error");
            }
            prodatabar = 100 * (weightAudio + weightOnePass);

            ffprobeOutput = twoPassProcess(ffprobeOutput, idx, path, sw1, sw2, prodatabar, out exitCode);
            ffprobeOutput = errProcess(ffprobeOutput, idx, exitCode, out RunEnum isRun2);
            switch (isRun2)
            {
                case RunEnum.Stop: return;
                case RunEnum.Error: throw new Exception($"{RunEnum.Merge.GetDisplayName()} Error");
            }
            prodatabar = 100 * (weightAudio + weightOnePass + weightTwoPass);

            ffprobeOutput = mergeProcess(ffprobeOutput, idx, path, sw1, sw2, prodatabar, out exitCode);
            ffprobeOutput = errProcess(ffprobeOutput, idx, exitCode, out RunEnum isRun3);
            switch (isRun3)
            {
                case RunEnum.Stop: return;
                case RunEnum.Error: throw new Exception($"{RunEnum.Merge.GetDisplayName()} Error");
            }

            UpdateProgres(100, 100);
            listView1.Items[idx].SubItems[7].Text = "100 %";
            listView1.Items[idx].SubItems[8].Text = RunEnum.Done.GetDisplayName();
            ffprobeOutput.run = RunEnum.Done;
            VideoFunc.Delete(ffprobeOutput);
            TimeSpan Timemint = TimeSpan.FromSeconds(sw2.Elapsed.TotalSeconds);
            listView1.Items[idx].SubItems[9].Text = string.Format("{0:D2}:{1:D2}:{2:D2}", Timemint.Hours, Timemint.Minutes, Timemint.Seconds);
        }

        /// <summary>
        /// 音效處理
        /// </summary>
        private FfprobeOutput audioProcess(FfprobeOutput ffprobeOutput, int idx, string path, Stopwatch sw1, Stopwatch sw2)
        {
            if (AutoTrimToolStripMenuItem.Checked)
            {
                if (Cts == null || Cts.Token.IsCancellationRequested)
                    return ffprobeOutput;

                listView1.Items[idx].SubItems[8].Text = RunEnum.AudioTrim.GetDisplayName();
                ffprobeOutput.run = RunEnum.AudioTrim;
                int exitCode = new TaskHelper
                {
                    Cts = Cts,
                    RunPath = path,
                    FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\ffmpeg\ffmpeg.exe",
                    ArgumentList = {
                        $@"-i ""{Path.GetDirectoryName(ffprobeOutput.InFile)}\{ffprobeOutput.InFileName}""",
                        $@"-ar 48000",
                        $@"-ac 2",
                        $@"-af ""aresample=48000,asetpts=PTS-STARTPTS""",
                        $@"-c:a aac",
                        $@"-q:a 1 ""{Path.GetDirectoryName(ffprobeOutput.InFile)}\{ffprobeOutput.avsTempFile}.aac""",
                        $@"-progress pipe:1",
                        $@"-nostats",
                        $@"-loglevel error",
                    },
                    ActionOut = sr =>
                    {
                        listView1.Items[idx].SubItems[9].Text = OtherControlFunc.timeConv(sw2);
                        timeStripStatus.Text = OtherControlFunc.timeConv(sw1);
                        int str = sr.IndexOf('=');

                        if (str <= 0)
                            return;

                        string key = sr[..str];
                        string value = sr[(str + 1)..];

                        if (key == "out_time_ms" && double.TryParse(value, out double outTimeMs))
                        {
                            double currentSeconds = outTimeMs / 1_000_000.0;
                            double pro = currentSeconds / ffprobeOutput.duration * 100.0;
                            double proA = pro * weightAudio;
                            listView1.Items[idx].SubItems[7].Text = $"{pro:F1} %";
                            UpdateProgres((float)proA, 100);
                        }

                        f2.appendText = sr;
                    },
                }.RunTask();

                if (exitCode == 0)
                    ffprobeOutput.isAcc = true;
            }
            else if (Path.GetExtension(ffprobeOutput.InFile).Equals($".{VideoExt.mkv}", StringComparison.CurrentCultureIgnoreCase))
            {
                if (Cts == null || Cts.Token.IsCancellationRequested)
                    return ffprobeOutput;
                listView1.Items[idx].SubItems[8].Text = RunEnum.SoundSeparation.GetDisplayName();
                ffprobeOutput.run = RunEnum.SoundSeparation;
                new TaskHelper
                {
                    Cts = Cts,
                    RunPath = path,
                    FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\mkvextract.exe",
                    ArgumentList = {
                        $@"tracks ""{ffprobeOutput.InFile}""",
                        $@"1:""{Path.GetDirectoryName(ffprobeOutput.InFile)}""{ffprobeOutput.avsTempFile}.aac""",
                    },
                    ActionOut = sr =>
                    {
                        timeStripStatus.Text = OtherControlFunc.timeConv(sw1);
                        f2.appendText = sr;
                    },
                }.RunTask();

                if (!ffprobeOutput.isAcc)
                {
                    if (Cts.Token.IsCancellationRequested)
                        return ffprobeOutput;
                    listView1.Items[idx].SubItems[8].Text = RunEnum.SoundProcessing.GetDisplayName();
                    ffprobeOutput.run = RunEnum.SoundProcessing;
                    new TaskHelper
                    {
                        Cts = Cts,
                        RunPath = path,
                        FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\eac3to\eac3to.exe",
                        ArgumentList = {
                            $@"-log=NUL",
                            $@"{Path.GetDirectoryName(ffprobeOutput.InFile)}\{ffprobeOutput.avsTempFile}.aac""",
                            $@"""{Path.GetDirectoryName(ffprobeOutput.InFile)}\{ffprobeOutput.avsTempFile}.mp4""",
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
        private FfprobeOutput onePassProcess(FfprobeOutput ffprobeOutput, int idx, string path, Stopwatch sw1, Stopwatch sw2, double prodatabar, out int exitCode)
        {
            exitCode = 0;

            if (Cts == null || Cts.Token.IsCancellationRequested)
                return ffprobeOutput;
            listView1.Items[idx].SubItems[8].Text = RunEnum.OnePass.GetDisplayName();
            ffprobeOutput.run = RunEnum.OnePass;
            exitCode = new TaskHelper
            {
                Cts = Cts,
                RunPath = path,
                AutoCloseDialogBox = "Warning",
                FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\avs4x26x.exe",
                ArgumentList = videoFunc.Xonepass(ffprobeOutput),
                ActionErr = sr =>
                {
                    f2.appendText = sr;
                    listView1.Items[idx].SubItems[9].Text = OtherControlFunc.timeConv(sw2);
                    timeStripStatus.Text = OtherControlFunc.timeConv(sw1);
                    if (sr.IndexOf("frames,") != -1 && sr.IndexOf('[') != -1)
                    {
                        double prodata1 = Math.Round(Convert.ToDouble(sr.Substring(sr.IndexOf('[') + 1, sr.LastIndexOf('%') - 1)), 2);
                        double proA = Math.Round(prodatabar + (prodata1 * weightOnePass), 2);
                        listView1.Items[idx].SubItems[7].Text = $"{prodata1:F1} %";
                        UpdateProgres((float)proA, 100);
                    }

                    if (sr.IndexOf("[error]") != -1)
                    {
                        listView1.Items[idx].SubItems[8].Text = RunEnum.Error.GetDisplayName();
                        ffprobeOutput.run = RunEnum.Error;
                        listView1.Items[idx].SubItems[8].ForeColor = Color.Red;
                    }
                }
            }.RunTask();

            return ffprobeOutput;
        }

        /// <summary>
        /// TwoPass
        /// </summary>
        private FfprobeOutput twoPassProcess(FfprobeOutput ffprobeOutput, int idx, string path, Stopwatch sw1, Stopwatch sw2, double prodatabar, out int exitCode)
        {
            exitCode = 0;

            if (Cts == null || Cts.Token.IsCancellationRequested)
                return ffprobeOutput;
            listView1.Items[idx].SubItems[8].Text = RunEnum.TwoPass.GetDisplayName();
            ffprobeOutput.run = RunEnum.TwoPass;
            exitCode = new TaskHelper
            {
                Cts = Cts,
                RunPath = path,
                AutoCloseDialogBox = "Warning",
                FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\avs4x26x.exe",
                ArgumentList = videoFunc.Xtwopass(ffprobeOutput),
                ActionErr = sr =>
                {
                    f2.appendText = sr;
                    listView1.Items[idx].SubItems[9].Text = OtherControlFunc.timeConv(sw2);
                    timeStripStatus.Text = OtherControlFunc.timeConv(sw1);
                    if (sr.IndexOf("frames,") != -1 && sr.IndexOf('[') != -1)
                    {
                        double prodata1 = Math.Round(Convert.ToDouble(sr.Substring(sr.IndexOf('[') + 1, sr.LastIndexOf('%') - 1)), 2);
                        double proB = Math.Round(prodatabar + (prodata1 * weightTwoPass), 2);
                        listView1.Items[idx].SubItems[7].Text = $"{prodata1:F1} %";
                        UpdateProgres((float)proB, 100);
                    }

                    if (sr.IndexOf("[error]") != -1)
                    {
                        listView1.Items[idx].SubItems[8].Text = RunEnum.Error.GetDisplayName();
                        ffprobeOutput.run = RunEnum.Error;
                        listView1.Items[idx].SubItems[8].ForeColor = Color.Red;
                    }
                }
            }.RunTask();

            return ffprobeOutput;
        }

        /// <summary>
        /// Merge
        /// </summary>
        private FfprobeOutput mergeProcess(FfprobeOutput ffprobeOutput, int idx, string path, Stopwatch sw1, Stopwatch sw2, double prodatabar, out int exitCode)
        {
            exitCode = 0;

            if (Cts == null || Cts.Token.IsCancellationRequested)
                return ffprobeOutput;
            listView1.Items[idx].SubItems[8].Text = RunEnum.Merge.GetDisplayName();
            ffprobeOutput.run = RunEnum.Merge;
            exitCode = new TaskHelper
            {
                Cts = Cts,
                RunPath = path,
                FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\mp4box.exe",
                ArgumentList = {
                    $@"-add ""{ffprobeOutput.avsTempFile}.264""",
                    $@"-add ""{(Path.GetExtension(ffprobeOutput.InFile).Equals(VideoExt.mkv, StringComparison.CurrentCultureIgnoreCase) ? (!ffprobeOutput.isAcc ? $"{ffprobeOutput.avsTempFile}.mp4" : $"{ffprobeOutput.avsTempFile}.aac") : ffprobeOutput.InFile)}""#audio",
                    $@"""{ffprobeOutput.OutFile}"""
                },
                ActionErr = sr =>
                {
                    f2.appendText = sr;
                    timeStripStatus.Text = OtherControlFunc.timeConv(sw1);
                    if (sr.IndexOf("Error") > -1)
                    {
                        listView1.Items[idx].SubItems[8].Text = RunEnum.Error.GetDisplayName();
                        ffprobeOutput.run = RunEnum.Error;
                        listView1.Items[idx].SubItems[8].ForeColor = Color.Red;
                        return;
                    }

                    Match match = Regex.Match(sr, @"\((\d+)/(\d+)\)");
                    if (match.Success)
                    {
                        listView1.Items[idx].SubItems[9].Text = OtherControlFunc.timeConv(sw2);
                        float current = float.TryParse(match.Groups[1].Value, out float _current) ? _current : 0;
                        float percentage = (float)prodatabar + (current * weightMerge);
                        listView1.Items[idx].SubItems[7].Text = $"{current:F1} %";
                        UpdateProgres(percentage, 100);
                    }
                },
            }.RunTask();

            return ffprobeOutput;
        }

        private FfprobeOutput errProcess(FfprobeOutput ffprobeOutput, int idx, int exitCode, out RunEnum run)
        {
            if (exitCode != 0 && ffprobeOutput.run != RunEnum.Stop)
            {
                listView1.Items[idx].SubItems[8].Text = RunEnum.Error.GetDisplayName();
                ffprobeOutput.run = RunEnum.Error;
                listView1.Items[idx].SubItems[8].ForeColor = Color.Red;
            }

            if (ffprobeOutput.run == RunEnum.Error)
                run = RunEnum.Error;
            else
                run = ffprobeOutput.run;

            return ffprobeOutput;
        }
        #endregion
    }
}
