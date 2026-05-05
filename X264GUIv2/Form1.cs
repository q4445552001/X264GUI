using System.Diagnostics;
using System.Text;
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
        readonly Form2 f2 = new();

        #region 初始化

        public Form1()
        {
            InitializeComponent();

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

            BarGraphics = progressBar1.CreateGraphics();
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

        public void TaskAction(Action obj) => Invoke(obj);

        #region Button

        private void runBtn_Click(object sender, EventArgs e)
        {
            float weightA = 0.48f;
            float weightB = 0.48f;
            float weightC = 0.04f;

            Stopwatch sw = new();
            if (listView1.Items.Count != 0)
            {
                Cts = new();
                Task.Run(() =>
                {
                    runBtn.Enabled = false;
                    addToolStripMenuItem.Enabled = false;
                    addBtn.Enabled = false;
                    clearToolStripMenuItem.Enabled = false;
                    diffToolStripMenuItem.Enabled = false;
                    diffBtn.Enabled = false;
                    bitrateCBox.Enabled = false;
                    fpsCBox.Enabled = false;
                    resolutionCBox.Enabled = false;
                    bitrateNumeric.Enabled = false;

                    int listViewCount = listView1.Items.Count;
                    foreach (FfprobeOutput ffprobeOutput in videoFunc.ffprobeData)
                    {
                        if (Cts.Token.IsCancellationRequested)
                            return;

                        if (ffprobeOutput.run == RunEnum.Done)
                            continue;

                        string? path = Path.GetDirectoryName(ffprobeOutput.InFile);
                        if (string.IsNullOrWhiteSpace(path))
                            throw new Exception($"無效路徑 {path}");

                        Environment.CurrentDirectory = path;

                        #region 初始化
                        int idx = listView1.Items.Cast<ListViewItem>().ToList().FindIndex(x => (Guid?)x.Tag == ffprobeOutput.Guid);
                        listView1.Items[idx].SubItems[8].ForeColor = Color.Black;
                        listView1.Items[idx].SubItems[8].Text = RunEnum.Init.GetDisplayName();
                        ffprobeOutput.run = RunEnum.Init;
                        listView1.Items[idx].UseItemStyleForSubItems = false;
                        progressText.Text = $"{idx + 1}/{listViewCount}";

                        sw.Reset();
                        sw.Start();

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

                        string avsFile = @$".\{ffprobeOutput.avsTempFile}.avs";
                        //if (!File.Exists(avsFile))
                        File.WriteAllText(avsFile, avs);
                        #endregion

                        #region 音效處理
                        if (Path.GetExtension(ffprobeOutput.InFile).ToLower() == $".{VideoExt.mkv}")
                        {
                            if (Cts.Token.IsCancellationRequested)
                                return;
                            listView1.Items[idx].SubItems[8].Text = RunEnum.SoundSeparation.GetDisplayName();
                            ffprobeOutput.run = RunEnum.SoundSeparation;
                            new TaskHelper
                            {
                                Cts = Cts,
                                RunPath = path,
                                FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\mkvextract.exe",
                                ArgumentList = {
                                    $@"tracks ""{ffprobeOutput.InFile}"" 1:""{Path.GetDirectoryName(ffprobeOutput.InFile)}""{ffprobeOutput.avsTempFile}.aac""",
                                },
                            }.RunTask();

                            if (!ffprobeOutput.isAcc)
                            {
                                if (Cts.Token.IsCancellationRequested)
                                    return;
                                listView1.Items[idx].SubItems[8].Text = RunEnum.SoundProcessing.GetDisplayName();
                                ffprobeOutput.run = RunEnum.SoundProcessing;
                                new TaskHelper
                                {
                                    Cts = Cts,
                                    RunPath = path,
                                    FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\eac3to\eac3to.exe",
                                    ArgumentList = {
                                        @"-log=NUL """ + Path.GetDirectoryName(ffprobeOutput.InFile) + @$"\{ffprobeOutput.avsTempFile}.aac"" """ + Path.GetDirectoryName(ffprobeOutput.InFile) + @$"\{ffprobeOutput.avsTempFile}.mp4""",
                                    },
                                }.RunTask();
                            }
                        }
                        #endregion

                        #region OnePass
                        if (Cts.Token.IsCancellationRequested)
                            return;
                        listView1.Items[idx].SubItems[8].Text = RunEnum.OnePass.GetDisplayName();
                        ffprobeOutput.run = RunEnum.OnePass;
                        double proA = 0;
                        new TaskHelper
                        {
                            Cts = Cts,
                            RunPath = path,
                            AutoDisabledDialogBox = "Warning",
                            FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\avs4x26x.exe",
                            ArgumentList = { videoFunc.Xonepass(ffprobeOutput) },
                            ActionErr = sr =>
                            {
                                f2.show = sr;
                                TimeSpan Timemint = TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds);
                                listView1.Items[idx].SubItems[9].Text = string.Format("{0:D2}:{1:D2}:{2:D2}", Timemint.Hours, Timemint.Minutes, Timemint.Seconds);
                                if (sr.IndexOf("frames,") != -1 && sr.IndexOf("[") != -1)
                                {
                                    double prodata1 = Math.Round(Convert.ToDouble(sr.Substring(sr.IndexOf("[") + 1, sr.LastIndexOf("%") - 1)), 2);
                                    proA = Math.Round(prodata1 * weightA, 2);
                                    listView1.Items[idx].SubItems[7].Text = $"{prodata1} %";
                                    if (proA <= 100)
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
                        prodatabar = 100 * weightA;
                        if (ffprobeOutput.run == RunEnum.Error)
                            continue;
                        #endregion

                        #region TwoPass
                        if (Cts.Token.IsCancellationRequested)
                            return;
                        listView1.Items[idx].SubItems[8].Text = RunEnum.TwoPass.GetDisplayName();
                        ffprobeOutput.run = RunEnum.TwoPass;
                        double proB = 0;
                        new TaskHelper
                        {
                            Cts = Cts,
                            RunPath = path,
                            AutoDisabledDialogBox = "Warning",
                            FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\avs4x26x.exe",
                            ArgumentList = { videoFunc.Xtwopass(ffprobeOutput) },
                            ActionErr = sr =>
                            {
                                f2.show = sr;
                                TimeSpan Timemint = TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds);
                                listView1.Items[idx].SubItems[9].Text = string.Format("{0:D2}:{1:D2}:{2:D2}", Timemint.Hours, Timemint.Minutes, Timemint.Seconds);
                                if (sr.IndexOf("frames,") != -1 && sr.IndexOf("[") != -1)
                                {
                                    double prodata1 = Math.Round(Convert.ToDouble(sr.Substring(sr.IndexOf("[") + 1, sr.LastIndexOf("%") - 1)), 2);
                                    proB = Math.Round(prodatabar + (prodata1 * weightB), 2);
                                    listView1.Items[idx].SubItems[7].Text = $"{prodata1} %";
                                    if (proB <= 100)
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
                        prodatabar = 100 * (weightA + weightB);
                        if (ffprobeOutput.run == RunEnum.Error)
                            continue;
                        #endregion

                        #region MP4Box
                        if (Cts.Token.IsCancellationRequested)
                            return;
                        listView1.Items[idx].SubItems[8].Text = RunEnum.Merge.GetDisplayName();
                        ffprobeOutput.run = RunEnum.Merge;
                        new TaskHelper
                        {
                            Cts = Cts,
                            RunPath = path,
                            FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\mp4box.exe",
                            ArgumentList = {
                                        $@"-add ""{ffprobeOutput.avsTempFile}.264"" ^
                                        -add ""{(Path.GetExtension(ffprobeOutput.InFile).ToLower() == VideoExt.mkv ? (!ffprobeOutput.isAcc ? $"{ffprobeOutput.avsTempFile}.mp4" : $"{ffprobeOutput.avsTempFile}.aac") : ffprobeOutput.InFile)}""#audio ^
                                        ""{ffprobeOutput.OutFile}""" },
                            ActionErr = sr =>
                            {
                                f2.show = sr;
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
                                    float current = float.TryParse(match.Groups[1].Value, out float _current) ? _current : 0;
                                    float percentage = (float)prodatabar + (current * weightC);
                                    listView1.Items[idx].SubItems[7].Text = $"{current:F1} %";
                                    UpdateProgres(percentage, 100);
                                }
                            },
                        }.RunTask();
                        if (ffprobeOutput.run == RunEnum.Error)
                            continue;
                        #endregion

                        UpdateProgres(100, 100);
                        listView1.Items[idx].SubItems[7].Text = "100 %";
                        listView1.Items[idx].SubItems[8].Text = RunEnum.Done.GetDisplayName();
                        ffprobeOutput.run = RunEnum.Done;
                        videoFunc.Delete(ffprobeOutput);
                        TimeSpan Timemint = TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds);
                        listView1.Items[idx].SubItems[9].Text = string.Format("{0:D2}:{1:D2}:{2:D2}", Timemint.Hours, Timemint.Minutes, Timemint.Seconds);
                        sw.Stop();
                    }

                    stopBtn.Enabled = false;
                    runBtn.Enabled = true;
                    addToolStripMenuItem.Enabled = true;
                    addBtn.Enabled = true;
                    clearToolStripMenuItem.Enabled = true;
                    diffToolStripMenuItem.Enabled = true;
                    diffBtn.Enabled = true;
                    bitrateCBox.Enabled = true;
                    fpsCBox.Enabled = true;
                    resolutionCBox.Enabled = true;
                    bitrateNumeric.Enabled = true;
                    Cts.Cancel();
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

                stopBtn.Enabled = false;
                runBtn.Enabled = true;
                addToolStripMenuItem.Enabled = true;
                addBtn.Enabled = true;
                clearToolStripMenuItem.Enabled = true;
                diffToolStripMenuItem.Enabled = true;
                diffBtn.Enabled = true;
                bitrateCBox.Enabled = true;
                fpsCBox.Enabled = true;
                resolutionCBox.Enabled = true;
                bitrateNumeric.Enabled = true;
                int idx = Convert.ToInt32(progressText.Text?.Split('/')[0]) - 1;
                listView1.Items[idx].SubItems[8].Text = RunEnum.Stop.GetDisplayName();
                int idx2 = videoFunc.ffprobeData.FindIndex(x => x.Guid == (Guid?)listView1.Items[idx].Tag);
                videoFunc.ffprobeData[idx2].run = RunEnum.Stop;
                videoFunc.Delete(videoFunc.ffprobeData[idx2]);
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
                OtherControlFunc.ShowError(ex.Message);
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
                OtherControlFunc.ShowError(ex.Message);
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
                OtherControlFunc.ShowError(ex.Message);
            }
        }

        private void logViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            f2.Show();

            try
            {
            }
            catch (Exception ex)
            {
                OtherControlFunc.ShowError(ex.Message);
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
                OtherControlFunc.ShowError(ex.Message);
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
                OtherControlFunc.ShowError(ex.Message);
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
                OtherControlFunc.ShowError(ex.Message);
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
                OtherControlFunc.ShowError(ex.Message);
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
                OtherControlFunc.ShowError(ex.Message);
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
                OtherControlFunc.ShowError(ex.Message);
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
                OtherControlFunc.ShowError(ex.Message);
            }
        }
        #endregion

        #region 進度條
        /// <summary>
        /// 進度條
        /// </summary>
        public void UpdateProgres(float now, float count, bool isPercentage = true)
        {
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
    }
}
