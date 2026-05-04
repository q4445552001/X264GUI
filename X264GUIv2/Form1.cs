using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using X264GUIv2.Enums;
using X264GUIv2.Models;

namespace X264GUIv2
{
    public partial class Form1 : Form
    {
        public static string[] videoExt => [VideoExt.mp4, VideoExt.mkv, VideoExt.avi, VideoExt.mov];

        private readonly VideoFunc videoFunc;
        private readonly OtherControlFunc otherControlFunc;
        private readonly ControlFunc controlFunc;

        public int BitValueDef = 900000; //初始化彼特率
        Thread Nico_Init, Nico;

        #region 初始化

        public Form1()
        {
            InitializeComponent();

            fpsCBox.Items.Add(new ComboboxItem("Auto", "Auto"));
            fpsCBox.Items.Add(new ComboboxItem("23.976", "24000/1001"));
            fpsCBox.Items.Add(new ComboboxItem("25", "25"));
            fpsCBox.Items.Add(new ComboboxItem("29.970", "30000/1001"));
            fpsCBox.Items.Add(new ComboboxItem("30", "30"));

            int cpunumber = Environment.ProcessorCount;
            for (int i = 0; i <= cpunumber; i++)
                coreCBox.Items.Add(i);

            bitrateNumeric.Increment = new decimal([100, 0, 0, 0]);
            bitrateNumeric.Maximum = new decimal([100000, 0, 0, 0]);
            bitrateNumeric.Minimum = new decimal([1, 0, 0, 0]);
            bitrateNumeric.Value = BitValueDef / 1000;

            foreach (BitrateEnum e in Enum.GetValues<BitrateEnum>())
                bitrateCBox.Items.Add(new ComboboxItem(Enum.GetName(e)!, ((int)e).ToString()));

            foreach (ResolutionEnum e in Enum.GetValues<ResolutionEnum>())
                resolutionCBox.Items.Add(new ComboboxItem(((int)e).ToString(), ((int)e).ToString()));

            bitrateCBox.SelectedIndex = 0;
            fpsCBox.SelectedIndex = 0;
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
                new() { Text = "時間長度", Width = 170, TextAlign = HorizontalAlignment.Center },
                new() { Text = "檔案大小", Width = 140, TextAlign = HorizontalAlignment.Center },
                new() { Text = "進度", Width = 140, TextAlign = HorizontalAlignment.Center },
                new() { Text = "狀態", TextAlign = HorizontalAlignment.Center },
                new() { Text = "消耗時間", Width = 170, TextAlign = HorizontalAlignment.Center },
                new() { Text = "路徑", Width = 300 },
            }]);

            videoFunc = new(this);
            otherControlFunc = new(this);
            controlFunc = new(this);
        }

        private void Form1_Load(object sender, EventArgs e) => CheckForIllegalCrossThreadCalls = false;

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Nico_Init != null && Nico_Init.IsAlive /*&& checkBox1.Checked.ToString() == "False"*/)
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
            Stopwatch sw = new Stopwatch();
            if (listView1.Items.Count != 0)
            {
                Nico_Init = new Thread(() =>
                {
                    runBtn.Enabled = false;
                    addBtn.Enabled = false;
                    clearBtn.Enabled = false;
                    diffBtn.Enabled = false;
                    bitrateCBox.Enabled = false;
                    fpsCBox.Enabled = false;
                    resolutionCBox.Enabled = false;
                    bitrateNumeric.Enabled = false;

                    int listViewCount = listView1.Items.Count;
                    foreach (FfprobeOutput ffprobeOutput in videoFunc.ffprobeDataOriginal)
                    {
                        string? path = Path.GetDirectoryName(ffprobeOutput.InFile);
                        if (!string.IsNullOrWhiteSpace(path))
                            throw new Exception("查無路徑");

                        Directory.SetCurrentDirectory(path!);

                        var item = listView1.Items.Cast<ListView>().FirstOrDefault(x => (Guid?)x.Tag == ffprobeOutput.Guid);
                        int idx = 1;
                        listView1.Items[idx].SubItems[8].ForeColor = Color.Black;
                        listView1.Items[idx].SubItems[8].Text = "初始化";
                        listView1.Items[idx].UseItemStyleForSubItems = false;
                        progressText.Text = $"{idx + 1}//{listViewCount}";

                        sw.Reset();
                        sw.Start();

                        stopBtn.Enabled = true;
                        progressBar1.Value = 0;

                        double prodatabar = 0;
                        string avs = "";

                        if (string.IsNullOrWhiteSpace(ffprobeOutput.SubtitlesFile))
                        {
                            avs = $@"
LoadPlugin(""{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}\\bin\\x264\\ffms2.dll"") FFVideoSource(""
{Path.GetFileName(ffprobeOutput.InFileName)}"", fpsnum={ffprobeOutput.NewDetail.fpsnum}, fpsden={ffprobeOutput.NewDetail.fpsden})
#deinterlace #crop #resize #denoise";
                        }
                        else
                        {
                            avs = $@"
LoadPlugin(""{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}\\bin\\x264\\ffms2.dll"") FFVideoSource(""
{Path.GetFileName(ffprobeOutput.InFileName)}"", fpsnum={ffprobeOutput.NewDetail.fpsnum}, fpsden={ffprobeOutput.NewDetail.fpsden})
LoadPlugin(""{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}\\bin\\264\\VSFilter.dll"")
TextSub(""avsTemp.ass"",1)
#deinterlace #crop #resize #denoise";
                            File.Copy(ffprobeOutput.SubtitlesFile, "avsTemp.ass", true);
                        }

                        File.WriteAllText(@".\avsTemp.avs", avs);
                    }

                    stopBtn.Enabled = false;
                    runBtn.Enabled = true;
                    addBtn.Enabled = true;
                    clearBtn.Enabled = true;
                    diffBtn.Enabled = true;
                    bitrateCBox.Enabled = true;
                    fpsCBox.Enabled = true;
                    resolutionCBox.Enabled = true;
                    bitrateNumeric.Enabled = true;

                    #region
                    //for (int i = 0; i < listViewCount; i++)
                    //{

                    //    //Audio Not aac
                    //    if (Path.GetExtension(InFileName[i]).ToString().ToLower() == ".mkv")
                    //    {
                    //        listView1.Items[i].SubItems[8].Text = "音效分離";
                    //        Nico = new Thread(() =>
                    //        {
                    //            var run = new Process
                    //            {
                    //                StartInfo = new ProcessStartInfo
                    //                {
                    //                    FileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @".\bin\x264\", "mkvextract.exe"),
                    //                    Arguments = "tracks \"" + InFileName[i].Replace("//", "\\") + "\" 1:\"" + Path.GetDirectoryName(InFileName[i]) + "\\avsTemp.aac\"",
                    //                    UseShellExecute = false,
                    //                    CreateNoWindow = true,
                    //                    RedirectStandardError = true
                    //                }
                    //            };
                    //            run.Start();
                    //            run.WaitForExit();
                    //        });
                    //        Nico.Start();
                    //        Nico.Join();

                    //        if (NewData[i][6] == "false")
                    //        {
                    //            listView1.Items[i].SubItems[8].Text = "音效處理";
                    //            Nico = new Thread(() =>
                    //            {
                    //                var run = new Process
                    //                {
                    //                    StartInfo = new ProcessStartInfo
                    //                    {
                    //                        FileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\bin\eac3to\", "eac3to.exe"),
                    //                        Arguments = @"-log=NUL """ + Path.GetDirectoryName(InFileName[i]) + @"\avsTemp.aac"" """ + Path.GetDirectoryName(InFileName[i]) + @"\avsTemp.mp4""",
                    //                        UseShellExecute = false,
                    //                        CreateNoWindow = true,
                    //                        RedirectStandardError = true
                    //                    }
                    //                };
                    //                run.Start();
                    //                run.WaitForExit();
                    //            });
                    //            Nico.Start();
                    //            Nico.Join();
                    //        }
                    //    }

                    //    //One Pass
                    //    listView1.Items[i].SubItems[8].Text = "OnePass";
                    //    Nico = new Thread(() =>
                    //    {
                    //        var run = new Process
                    //        {
                    //            StartInfo = new ProcessStartInfo
                    //            {
                    //                FileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @".\bin\x264\", "avs4x26x.exe"),
                    //                Arguments = XOCode[i],
                    //                UseShellExecute = false,
                    //                CreateNoWindow = true,
                    //                RedirectStandardError = true
                    //            }
                    //        };
                    //        run.Start();

                    //        StreamReader sr = run.StandardError;
                    //        while (!sr.EndOfStream)
                    //        {
                    //            TimeSpan Timemint = TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds);
                    //            listView1.Items[i].SubItems[9].Text = string.Format("{0:D2}:{1:D2}:{2:D2}", Timemint.Hours, Timemint.Minutes, Timemint.Seconds);
                    //            string srstring = sr.ReadLine();
                    //            //f2.show = srstring;
                    //            if (srstring.IndexOf("frames,") != -1 && srstring.IndexOf("[") != -1)
                    //            {
                    //                double prodata = Math.Round(Convert.ToDouble(srstring.Substring(srstring.IndexOf("[") + 1, srstring.LastIndexOf("%") - 1)) / 2, 2);
                    //                if (prodata < 99.99)
                    //                {
                    //                    progressBar1.Value = (int)prodata;
                    //                    listView1.Items[i].SubItems[7].Text = prodata + " %";
                    //                }
                    //                prodatabar = prodata;
                    //            }
                    //        }
                    //    });
                    //    Nico.Start();
                    //    Nico.Join();

                    //    //Two Pass
                    //    listView1.Items[i].SubItems[8].Text = "TwoPass";
                    //    Nico = new Thread(() =>
                    //    {
                    //        var run = new Process
                    //        {
                    //            StartInfo = new ProcessStartInfo
                    //            {
                    //                FileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @".\bin\x264\", "avs4x26x.exe"),
                    //                Arguments = XTCode[i],
                    //                UseShellExecute = false,
                    //                CreateNoWindow = true,
                    //                RedirectStandardError = true
                    //            }
                    //        };
                    //        run.Start();

                    //        StreamReader sr = run.StandardError;
                    //        while (!sr.EndOfStream)
                    //        {
                    //            TimeSpan Timemint = TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds);
                    //            listView1.Items[i].SubItems[9].Text = string.Format("{0:D2}:{1:D2}:{2:D2}", Timemint.Hours, Timemint.Minutes, Timemint.Seconds);
                    //            string srstring = sr.ReadLine();
                    //            //f2.show = srstring;
                    //            if (srstring.IndexOf("frames,") != -1 && srstring.IndexOf("[") != -1)
                    //            {
                    //                double prodata = Math.Round((prodatabar + Convert.ToDouble(srstring.Substring(srstring.IndexOf("[") + 1, srstring.LastIndexOf("%") - 1)) / 2), 2);
                    //                if (prodata < 99.99)
                    //                {
                    //                    progressBar1.Value = (int)prodata;
                    //                    listView1.Items[i].SubItems[7].Text = prodata + " %";
                    //                }
                    //            }
                    //        }
                    //    });
                    //    Nico.Start();
                    //    Nico.Join();

                    //    //MP4Box
                    //    listView1.Items[i].SubItems[8].Text = "合併中";
                    //    Nico = new Thread(() =>
                    //    {
                    //        var run = new Process
                    //        {
                    //            StartInfo = new ProcessStartInfo
                    //            {
                    //                FileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @".\bin\x264\", "mp4box.exe"),
                    //                Arguments =
                    //                    "-add \"avsTemp.264\" -add \"" +
                    //                    (Path.GetExtension(InFileName[i]).ToString().ToLower() == ".mkv" ? (NewData[i][6] == "false" ? "avsTemp.mp4" : "avsTemp.aac") : InFileName[i].Replace("//", "\\")) +
                    //                    "\"#audio \"" + OutFileName[i].Replace("//", "\\") + "\"",
                    //                UseShellExecute = false,
                    //                CreateNoWindow = true,
                    //                RedirectStandardError = true
                    //            }
                    //        };
                    //        run.Start();

                    //        StreamReader sr = run.StandardError;
                    //        string srstring = sr.ReadToEnd();
                    //        //f2.show = srstring;
                    //        if (srstring.IndexOf("Error") != -1)
                    //        {
                    //            listView1.Items[i].SubItems[8].Text = "錯誤";
                    //            listView1.Items[i].SubItems[8].ForeColor = Color.Red;
                    //        }
                    //        else if (sr.EndOfStream)
                    //        {
                    //            progressBar1.Value = 100;
                    //            listView1.Items[i].SubItems[7].Text = "100 %";
                    //            listView1.Items[i].SubItems[8].Text = "Done";
                    //            Delete();
                    //            TimeSpan Timemint = TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds);
                    //            listView1.Items[i].SubItems[9].Text = string.Format("{0:D2}:{1:D2}:{2:D2}", Timemint.Hours, Timemint.Minutes, Timemint.Seconds);
                    //        }
                    //    });
                    //    Nico.Start();
                    //    Nico.Join();
                    //    sw.Stop();
                    //}
                    #endregion
                });
                Nico_Init.Start();
            }
        }

        private void stopBtn_Click(object sender, EventArgs e)
        {
            while (true)
            {
                if (Nico_Init != null && Nico_Init.IsAlive) Nico_Init.Abort();
                if (Nico != null && Nico.IsAlive) Nico.Abort();
                if (!Nico_Init.IsAlive)
                {
                    //foreach (Process p in Process.GetProcessesByName("eac3to")) p.Kill();
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
                    addBtn.Enabled = true;
                    clearBtn.Enabled = true;
                    diffBtn.Enabled = true;
                    bitrateCBox.Enabled = true;
                    fpsCBox.Enabled = true;
                    resolutionCBox.Enabled = true;
                    bitrateNumeric.Enabled = true;
                    videoFunc.Delete();
                    OtherControlFunc.ShowError("已強制停止");
                    listView1.Items[Convert.ToInt32(progressText.Text.Split('/')[0]) - 1].SubItems[8].Text = "強制停止";
                    break;
                }
            }
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            //讀取檔案
            OpenFileDialog openfile = new();
            openfile.Multiselect = true;

            string fileStr = "";
            foreach (string ext in videoExt)
                fileStr += $"*.{ext};";

            fileStr = fileStr[..^1];

            openfile.Filter = $"{fileStr}|{fileStr}";

            if (openfile.ShowDialog() == DialogResult.OK)
                foreach (string file in openfile.FileNames)
                    videoFunc.Encode(file);
        }

        private void diffBtn_Click(object sender, EventArgs e)
        {
            for (int i = listView1.SelectedItems.Count - 1; i >= 0; i--)
            {
                int item = listView1.SelectedIndices[i];
                videoFunc.ffprobeDataOriginal.RemoveAt(item);
                listView1.Items.RemoveAt(item);
            }

            progressText.Text = $"0/{listView1.Items.Count}";
        }

        private void clearBtm_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            runBtn.Enabled = false;
            stopBtn.Enabled = false;
            progressText.Text = "0/0";
            listView1.Columns[0].Tag = false;
            listView1.Refresh();

            videoFunc.ffprobeDataOriginal.Clear();
        }

        private void logBtn_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #region listView
        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e) => e.DrawDefault = true;
        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e) => e.DrawDefault = true;

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
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

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
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

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data == null)
                return;

            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
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
                string extension = Path.GetExtension(file);
                if (videoExt.Any(x => x == extension))
                    videoFunc.Encode(file);
            }

            runBtn.Enabled = true;
        }
        #endregion

        #region Changed
        private void bitrateCBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bitrateNumeric.Enabled = bitrateCBox.SelectedItem?.ToString() == "Manual";

            IList<ListViewItem> listViews = [.. listView1.CheckedItems.Cast<ListViewItem>()];
            foreach (ListViewItem listView in listViews)
            {
                FfprobeOutput? data = videoFunc.ffprobeDataOriginal.FirstOrDefault(x => x.Guid == (Guid?)listView.Tag);
                if (data == null) continue;

                if (bitrateCBox.SelectedItem?.ToString() == "Auto")
                    data.NewDetail.bitrate = data.OriDetail.bitrate - 100000;
                else if (bitrateCBox.SelectedItem?.ToString() == "Manual")
                    data.NewDetail.bitrate = (int)bitrateNumeric.Value * 1000;
            }
        }

        private void fpsCBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void resolutionCBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void bitrateNumeric_ValueChanged(object sender, EventArgs e)
        {

        }
        #endregion
    }
}
