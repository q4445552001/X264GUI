using System;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Text;
using System.Windows.Forms.VisualStyles;
using System.Collections.Generic;

namespace X264GUI
{
    public partial class Form1 : Form
    {
        List<string> XOCode = new List<string>(); //X264 One
        List<string> XTCode = new List<string>(); //X264 Two
        List<List<string>> NewData = new List<List<string>>(); //影片資料
        List<List<string>> DataOriginal = new List<List<string>>(); //原始影片資料
        List<string> InFileName = new List<string>(); //原始檔案
        List<string> OutFileName = new List<string>(); //輸出檔案
        List<string> Subtitles = new List<string>(); //字幕檔案

        int BitValueDef = 900000; //初始化彼特率

        Thread Nico_Init, Nico;
        Form2 f2 = new Form2();
        
        //***********************************************************************************************************
        //錯誤訊息
        public static void ShowError(string MessageText)
        {
            MessageBox.Show(MessageText, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //***********************************************************************************************************
        //處理DragEnter事件
        private void listView1_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
        }

        //處理拖放事件
        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            for (int i = 0; i < s.Length; i++)
            {
                string[] sdata = s[i].Split('.');
                if (sdata[sdata.Length - 1].ToLower() == "mp4" || sdata[sdata.Length - 1].ToLower() == "mkv")
                    Encode(s[i]);
            }
            button2.Enabled = true;
        }

        //***********************************************************************************************************
        //ffprobe
        public string[] ffprobe(string input)
        {
            string[] ffprobeArr = new string[7];
            var run = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\bin\ffmpeg\ffprobe.exe",
                    Arguments = @"-of json -show_streams -show_format -v quiet " + "\"" + input + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                }
            };
            run.Start();
            dynamic stuff = JObject.Parse(run.StandardOutput.ReadToEnd().Replace("\'", "").Replace("\"", "\'"));
            
            if (stuff.streams != null || stuff.format != null)
            {
                string isAAC = "";
                for (int i = 0; i < stuff.streams.Count; i++)
                    if (stuff.streams[i].codec_type.ToString().ToLower() == "audio")
                    {
                        isAAC = (stuff.streams[i].codec_name).ToString().ToLower() == "aac" ? "true" : "false";
                        break;
                    }

                for (int i = 0; i < stuff.streams.Count; i++)
                    if (stuff.streams[i].codec_type.ToString().ToLower() == "video")
                    {
                        dynamic Stuff_Streams = stuff.streams[i];
                        double bitrateTemp = Convert.ToDouble(Stuff_Streams.bit_rate != null ? Stuff_Streams.bit_rate : stuff.format.bit_rate);
                        ffprobeArr[0] = (bitrateTemp > BitValueDef ? bitrateTemp : bitrateTemp - 50000) + "";
                        ffprobeArr[1] = Stuff_Streams.r_frame_rate == Stuff_Streams.avg_frame_rate ? "CBR" : "VBR";
                        ffprobeArr[2] = Stuff_Streams.r_frame_rate;
                        ffprobeArr[3] = Stuff_Streams.width + "x" + Stuff_Streams.height;
                        ffprobeArr[4] = Stuff_Streams.duration != null ? Stuff_Streams.duration : stuff.format.duration;
                        ffprobeArr[5] = stuff.format.size;
                        ffprobeArr[6] = isAAC;
                        break;
                    }
            }
            return ffprobeArr;
        }

        //***********************************************************************************************************
        //X264 1 pass code
        static string Xonepass(string threads, double BitValue)
        {
            return "--x26x-binary \"" + AppDomain.CurrentDomain.SetupInformation.ApplicationBase 
                + "bin\\x264\\x264.exe\" --bframes 0 --bitrate " + Math.Round(BitValue / 1000,0) 
                + " --pass 1 --threads " + threads + " --stats \"x2642pass.stats\" -o NUL " + "\"avsTemp.avs\"";
        }

        //***********************************************************************************************************
        //X264 2 pass code
        static string Xtwopass(string threads, double BitValue, string outres)
        {
            return outres != "" ? 
                "--x26x-binary \"" + AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                    + "bin\\x264\\x264.exe\" --bframes 0 --bitrate " + Math.Round(BitValue / 1000, 0) + " --pass 2 --threads " + threads
                    + " --stats \"x2642pass.stats\" --vf resize:width=" 
                    + (outres.IndexOf(' ') == -1 ? outres.Split('x')[0] : outres.Split('x')[0].Split(' ')[1]) 
                    + ",height=" + outres.Split('x')[1] + ",sar=1:1 -o \"avsTemp.264\" \"avsTemp.avs\""
            :
                 "--x26x-binary \"" + AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                    + "bin\\x264\\x264.exe\" --bframes 0 --bitrate " + Math.Round(BitValue / 1000, 0) + " --pass 2 --threads " + threads
                    + " --stats \"x2642pass.stats\" -o " + "\"avsTemp.264\" \"avsTemp.avs\"";
        }

        //***********************************************************************************************************
        //移除暫存檔
        public void Delete()
        {
            for (int i = 0; i < listView1.Items.Count; i++)
                File.Delete(InFileName[i].Replace("//", "\\") + @".ffindex");
            File.Delete(@"./x2642pass.stats.temp");
            File.Delete(@"./x2642pass.stats.mbtree.temp");
            File.Delete(@"./x2642pass.stats");
            File.Delete(@"./x2642pass.stats.mbtree");
            File.Delete(@"./avsTemp.avs");
            File.Delete(@"./avsTemp.ass");
            File.Delete(@"./avsTemp.264");
            File.Delete(@"./avsTemp.aac");
            File.Delete(@"./avsTemp.mp4");
            progressBar1.Value = 0;
        }

        //***********************************************************************************************************
        public Form1()
        {
            InitializeComponent();
            if (!File.Exists(@".\bin\ffmpeg\ffprobe.exe"))
            {
                ShowError("缺少主要檔案，強制關閉。");
                System.Environment.Exit(System.Environment.ExitCode);
            }
            
            //Combox5
            comboBox5.Items.Add(new ComboboxItem("Auto", "Auto"));
            comboBox5.Items.Add(new ComboboxItem("23.976", "24000/1001"));
            comboBox5.Items.Add(new ComboboxItem("29.970", "30000/1001"));

            //CPU Processor
            int cpunumber = Environment.ProcessorCount;
            System.Object[] ItemObject = new System.Object[cpunumber + 1];
            for (int i = 0; i <= cpunumber; i++)
                ItemObject[i] = i;
            comboBox4.Items.AddRange(ItemObject);
            
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
            
            numericUpDown1.Value = BitValueDef / 1000;
        }

        //***********************************************************************************************************
        //讀檔
        private void button1_Click(object sender, EventArgs e)
        {
            //讀取檔案
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.Multiselect = true;
            openfile.Filter = "*.mov;*.mp4;*mkv|*.mov;*.mp4;*mkv;*.avi";

            if (openfile.ShowDialog() == DialogResult.OK)
                foreach (string file in openfile.FileNames)
                    Encode(file);
        }

        //***********************************************************************************************************
        //產生編碼
        private void Encode(string file)
        {
        restart:
            {
                try //檔案錯誤例外處理
                {
                    int count = listView1.Items.Count;
                    
                    string FilePath = Path.GetDirectoryName(file) + "\\" + Path.GetFileNameWithoutExtension(file).Replace("\\", "//") + "-0.mp4";
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(file)); //指定程式路徑
                    //textBox1.Text = System.Environment.CurrentDirectory; // 程式目前路徑
                    
                    string[] OldList = ffprobe(file);
                    DataOriginal.Add(new List<string>(OldList));

                    double BitValue = (Convert.ToDouble(OldList[0]) >= BitValueDef) ? BitValueDef : Convert.ToDouble(OldList[0]) - 50000;
                    string[] NewList = (string[]) OldList.Clone();
                    NewList[0] = BitValue.ToString();
                    NewData.Add(new List<string>(NewList));

                    InFileName.Add(file.Replace("\\", "//"));
                    OutFileName.Add(FilePath);

                    string outres = "";
                    if (NewList[3] == "640x480") outres = "-s 640x480 ";
                    else if (NewList[3] == "1280x720") outres = "-s 1280x720 ";
                    else if (NewList[3] == "1920x1080") outres = "-s 1920x1080 ";
                    
                    if (File.Exists(Path.GetDirectoryName(file) + @"\" + Path.GetFileNameWithoutExtension(file) + ".ass"))
                        Subtitles.Add(Path.GetDirectoryName(file) + @"\" + Path.GetFileNameWithoutExtension(file) + ".ass");
                    else
                        Subtitles.Add(null);

                    XOCode.Add(Xonepass(comboBox4.SelectedItem.ToString(), BitValue));
                    XTCode.Add(Xtwopass(comboBox4.SelectedItem.ToString(), BitValue, outres));

                    DataView(count, file, NewList, OldList);
                    ShowToForm2(count, file);
                }
                catch (IndexOutOfRangeException print)
                {
                    ShowError("格式錯誤\n" + print);
                    goto restart;
                }
                catch (NullReferenceException print)
                {
                    ShowError("格式錯誤\n" + print);
                    goto restart;
                }
            }

            textBox1.Text = "0/" + listView1.Items.Count.ToString();
            button2.Enabled = listView1.Items.Count == 0 ? false : true;
        }

        //***********************************************************************************************************
        //開始轉檔
        private void button2_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            if (listView1.Items.Count != 0)
            {
                Nico_Init = new Thread(() =>
                {
                    button1.Enabled = false;
                    button2.Enabled = false;
                    button3.Enabled = false;
                    button8.Enabled = false;
                    button9.Enabled = false;
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;
                    comboBox4.Enabled = false;
                    comboBox5.Enabled = false;
                    numericUpDown1.Enabled = false;
                    int listViewCount = listView1.Items.Count;
                    for (int i = 0; i < listViewCount; i++)
                    {
                        listView1.Items[i].SubItems[8].ForeColor = Color.Black;
                        listView1.Items[i].SubItems[8].Text = "初始化";
                        listView1.Items[i].UseItemStyleForSubItems = false;
                        textBox1.Text = (i + 1) + "/" + listViewCount.ToString();
                        sw.Reset();
                        sw.Start();
                        button5.Enabled = true;
                        Directory.SetCurrentDirectory(Path.GetDirectoryName(@InFileName[i]));
                        progressBar1.Value = 0;
                        
                        double prodatabar = 0;
                        string avs = "";
                        string[] fpsmode = NewData[i][2].Split('/');

                        if (Subtitles[i] != null)
                        {
                            avs = @"LoadPlugin(""" + AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\bin\x264\ffms2.dll"") FFVideoSource(""" +
                                Path.GetFileName(InFileName[i].Replace("//", "\\")) + @""", fpsnum=" + fpsmode[0] + ", fpsden=" + fpsmode[1] +
                                @") LoadPlugin(""" + AppDomain.CurrentDomain.SetupInformation.ApplicationBase +
                                @"\bin\x264\VSFilter.dll"") TextSub(""avsTemp.ass"",1) #deinterlace #crop #resize #denoise";
                            File.Copy(Subtitles[i], "avsTemp.ass", true);
                        }
                        else
                        {
                            avs = @"LoadPlugin(""" + AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\bin\x264\ffms2.dll"") FFVideoSource(""" +
                                Path.GetFileName(InFileName[i].Replace("//", "\\")) + @""", fpsnum=" + fpsmode[0] + ", fpsden=" + fpsmode[1] +
                                @") #deinterlace #crop #resize #denoise";
                        }

                        File.WriteAllText(@".\avsTemp.avs", avs);

                        //Audio Not aac
                        if (Path.GetExtension(InFileName[i]).ToString().ToLower() == ".mkv")
                        {
                            listView1.Items[i].SubItems[8].Text = "音效分離";
                            Nico = new Thread(() =>
                                {
                                    var run = new Process
                                    {
                                        StartInfo = new ProcessStartInfo
                                        {
                                            FileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @".\bin\x264\", "mkvextract.exe"),
                                            Arguments = "tracks \"" + InFileName[i].Replace("//", "\\") + "\" 1:\"" + Path.GetDirectoryName(InFileName[i]) + "\\avsTemp.aac\"",
                                            UseShellExecute = false,
                                            CreateNoWindow = true,
                                            RedirectStandardError = true
                                        }
                                    };
                                    run.Start();
                                    run.WaitForExit();
                                });
                            Nico.Start();
                            Nico.Join();

                            if (NewData[i][6] == "false")
                            {
                                listView1.Items[i].SubItems[8].Text = "音效處理";
                                Nico = new Thread(() =>
                                {
                                    var run = new Process
                                    {
                                        StartInfo = new ProcessStartInfo
                                        {
                                            FileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\bin\eac3to\", "eac3to.exe"),
                                            Arguments = @"-log=NUL """ + Path.GetDirectoryName(InFileName[i]) + @"\avsTemp.aac"" """ + Path.GetDirectoryName(InFileName[i]) + @"\avsTemp.mp4""",
                                            UseShellExecute = false,
                                            CreateNoWindow = true,
                                            RedirectStandardError = true
                                        }
                                    };
                                    run.Start();
                                    run.WaitForExit();
                                });
                                Nico.Start();
                                Nico.Join();
                            }
                        }

                        //One Pass
                        listView1.Items[i].SubItems[8].Text = "OnePass";
                        Nico = new Thread(() =>
                        {
                            var run = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @".\bin\x264\", "avs4x26x.exe"),
                                    Arguments = XOCode[i],
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                    RedirectStandardError = true
                                }
                            };
                            run.Start();

                            StreamReader sr = run.StandardError;
                            while (!sr.EndOfStream)
                            {
                                TimeSpan Timemint = TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds);
                                listView1.Items[i].SubItems[9].Text = string.Format("{0:D2}:{1:D2}:{2:D2}", Timemint.Hours, Timemint.Minutes, Timemint.Seconds);
                                string srstring = sr.ReadLine();
                                //f2.show = srstring;
                                if (srstring.IndexOf("frames,") != -1 && srstring.IndexOf("[") != -1)
                                {
                                    double prodata = Math.Round(Convert.ToDouble(srstring.Substring(srstring.IndexOf("[") + 1, srstring.LastIndexOf("%") - 1)) / 2, 2);
                                    if (prodata < 99.99)
                                    {
                                        progressBar1.Value = (int)prodata;
                                        listView1.Items[i].SubItems[7].Text = prodata + " %";
                                    }
                                    prodatabar = prodata;
                                }
                            }
                        });
                        Nico.Start();
                        Nico.Join();

                        //Two Pass
                        listView1.Items[i].SubItems[8].Text = "TwoPass";
                        Nico = new Thread(() =>
                        {
                            var run = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @".\bin\x264\", "avs4x26x.exe"),
                                    Arguments = XTCode[i],
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                    RedirectStandardError = true
                                }
                            };
                            run.Start();

                            StreamReader sr = run.StandardError;
                            while (!sr.EndOfStream)
                            {
                                TimeSpan Timemint = TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds);
                                listView1.Items[i].SubItems[9].Text = string.Format("{0:D2}:{1:D2}:{2:D2}", Timemint.Hours, Timemint.Minutes, Timemint.Seconds);
                                string srstring = sr.ReadLine();
                                //f2.show = srstring;
                                if (srstring.IndexOf("frames,") != -1 && srstring.IndexOf("[") != -1)
                                {
                                    double prodata = Math.Round((prodatabar + Convert.ToDouble(srstring.Substring(srstring.IndexOf("[") + 1, srstring.LastIndexOf("%") - 1)) / 2), 2);
                                    if (prodata < 99.99)
                                    {
                                        progressBar1.Value = (int)prodata;
                                        listView1.Items[i].SubItems[7].Text = prodata + " %";
                                    }
                                }
                            }
                        });
                        Nico.Start();
                        Nico.Join();

                        //MP4Box
                        listView1.Items[i].SubItems[8].Text = "合併中";
                        Nico = new Thread(() =>
                        {
                            var run = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @".\bin\x264\", "mp4box.exe"),
                                    Arguments =
                                        "-add \"avsTemp.264\" -add \"" +
                                        (Path.GetExtension(InFileName[i]).ToString().ToLower() == ".mkv" ? (NewData[i][6] == "false" ? "avsTemp.mp4" : "avsTemp.aac") : InFileName[i].Replace("//", "\\")) +
                                        "\"#audio \"" + OutFileName[i].Replace("//", "\\") + "\"",
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                    RedirectStandardError = true
                                }
                            };
                            run.Start();

                            StreamReader sr = run.StandardError;
                            string srstring = sr.ReadToEnd();
                            //f2.show = srstring;
                            if (srstring.IndexOf("Error") != -1)
                            {
                                listView1.Items[i].SubItems[8].Text = "錯誤";
                                listView1.Items[i].SubItems[8].ForeColor = Color.Red;
                            }
                            else if (sr.EndOfStream)
                            {
                                progressBar1.Value = 100;
                                listView1.Items[i].SubItems[7].Text = "100 %";
                                listView1.Items[i].SubItems[8].Text = "Done";
                                Delete();
                                TimeSpan Timemint = TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds);
                                listView1.Items[i].SubItems[9].Text = string.Format("{0:D2}:{1:D2}:{2:D2}", Timemint.Hours, Timemint.Minutes, Timemint.Seconds);
                            }
                        });
                        Nico.Start();
                        Nico.Join();
                        sw.Stop();
                    }
                    button1.Enabled = true;
                    button2.Enabled = true;
                    button3.Enabled = true;
                    button5.Enabled = false;
                    button8.Enabled = true;
                    button9.Enabled = true;
                    comboBox2.Enabled = true;
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = true;
                    numericUpDown1.Enabled = true;

                    if (checkBox1.Checked)
                    {
                        Form3 f3 = new Form3();
                        Application.Run(f3);
                    }
                });
                Nico_Init.Start();
            }
        }

        //***********************************************************************************************************
        //檢視詳細資料
        private void DataView(int count,string file, string[] NewData, string[] OldData)
        {
            listView1.Items.Add(Path.GetFileName(file));
            string OldCapacity = Math.Round(Convert.ToDouble(OldData[5]) / 1024 / 1024, 2).ToString(); //原始大小
            double Audio_capacity = Convert.ToDouble(OldData[5]) - (Convert.ToDouble(OldData[0]) * Convert.ToDouble(OldData[4]) / 8); //計算Audio大小
            string NewCapacity = Math.Round((Audio_capacity + (Convert.ToDouble(NewData[0]) * Convert.ToDouble(NewData[4])) / 8) / 1024 / 1024, 2).ToString() + " MB"; //Video預估大小

            //listView1.Items[count].SubItems.AddRange(NewData.ToArray());
            //bitrate
            listView1.Items[count].SubItems.Add(Math.Round(Convert.ToDouble(OldData[0]) / 1000, 0) + ">" + Math.Round((Convert.ToDouble(NewData[0]) / 1000), 0) + " kb/s");
            
            listView1.Items[count].SubItems.Add(OldData[1] + ">CBR");

            //fps
            listView1.Items[count].SubItems.Add(Math.Round(Convert.ToDouble(OldData[2].Split('/')[0]) / Convert.ToDouble(OldData[2].Split('/')[1]), 3) + ">"
                + Math.Round((Convert.ToDouble(NewData[2].Split('/')[0]) / Convert.ToDouble(NewData[2].Split('/')[1])), 3).ToString());

            listView1.Items[count].SubItems.Add(OldData[3] + ">" + NewData[3]);

            // 時間
            listView1.Items[count].SubItems.Add(TimeSpan.FromSeconds(Convert.ToDouble(NewData[4])).ToString(@"hh\:mm\:ss"));
            listView1.Items[count].SubItems.Add(OldCapacity + ">" + NewCapacity);
            listView1.Items[count].SubItems.Add("00.00 %");
            listView1.Items[count].SubItems.Add("IDEL");
            listView1.Items[count].SubItems.Add("00:00:00");
            listView1.Items[count].SubItems.Add(file.Replace("//", @"\"));
            listView1.Items[count].ToolTipText = Path.GetFileName(file) + 
                "\nBitRate: " + Math.Round(Convert.ToDouble(OldData[0]) / 1000, 0) + " kb/s" + 
                "\nFPS模式: " + OldData[1] + 
                "\nFPS: " + Math.Round(Convert.ToDouble(OldData[2].Split('/')[0]) / Convert.ToDouble(OldData[2].Split('/')[1]), 3) +
                "\n解析度: " + OldData[3] +
                "\n檔案大小: " + OldCapacity + " MB";
            
        }

        //***********************************************************************************************************
        //檢視Log
        private void ShowToForm2(int count, string file)
        {
            f2.ffencode = file.ToString() + "\n";
            f2.ffencode = "--------------------------------------------------------------\n";
            f2.ffencode = "One Pass :\n";
            f2.ffencode = XOCode[count] + "\n";
            f2.ffencode = "Two Pass :\n";
            f2.ffencode = XTCode[count] + "\n";
            f2.ffencode = "--------------------------------------------------------------\n";
            f2.ffencode = "\n";
        }

        //***********************************************************************************************************
        //Bitrate
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem.ToString() == "Auto")
                numericUpDown1.Enabled = false;

            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].Checked)
                {
                    if (comboBox2.SelectedItem.ToString() == "Auto")
                    {
                        numericUpDown1.Enabled = false;

                        if (comboBox3.SelectedItem.ToString() == "Auto" && comboBox5.SelectedItem.ToString() == "Auto")
                            NewData[i] = new List<string>(DataOriginal[i]);

                        double bitrateTemp = Convert.ToDouble(DataOriginal[i][0]) >= BitValueDef ? BitValueDef : Convert.ToDouble(DataOriginal[i][0]);
                        NewData[i][0] = (bitrateTemp >= BitValueDef ? bitrateTemp : bitrateTemp - 50000) + "";

                    }
                    else if (comboBox2.SelectedItem.ToString() == "Manual")
                    {
                        numericUpDown1.Enabled = true;
                        NewData[i][0] = (numericUpDown1.Value * 1000).ToString();
                    }

                    List<String> listdata = NewData[i];
                    List<String> OldData = new List<string>(DataOriginal[i]);

                    listView1.Items[i].SubItems[1].Text = Math.Round(Convert.ToDouble(OldData[0]) / 1000, 0) + ">" + Math.Round(Convert.ToDouble(listdata[0]) / 1000, 0).ToString() + " kb/s";

                    double Audio_capacity = Convert.ToDouble(OldData[5]) - (Convert.ToDouble(OldData[0]) * Convert.ToDouble(OldData[4]) / 8); //計算Audio大小

                    double Video_estimate_capacity = Convert.ToDouble(listdata[0]) * Convert.ToDouble(listdata[4]) / 8; //Video預估大小
                    listView1.Items[i].SubItems[6].Text = Math.Round(Convert.ToDouble(OldData[5]) / 1024 / 1024, 2).ToString() + ">" + Math.Round((Video_estimate_capacity + Audio_capacity) / 1024 / 1024, 2).ToString() + " MB";
                    
                    XOCode[i] = Xonepass(comboBox4.SelectedItem.ToString(), Convert.ToDouble(NewData[i][0]));
                    XTCode[i] = Xtwopass(comboBox4.SelectedItem.ToString(), Convert.ToDouble(NewData[i][0]), NewData[i][3]);
                    ShowToForm2(i, InFileName[i]);
                }
            }
        }

        //***********************************************************************************************************
        //Bitrate change
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].Checked)
                {
                    if (numericUpDown1.Value > 0)
                    {
                        NewData[i][0] = (numericUpDown1.Value * 1000).ToString();
                        
                        List<String> OldData = new List<string>(DataOriginal[i]);
                        listView1.Items[i].SubItems[1].Text = Math.Round(Convert.ToDouble(OldData[0]) / 1000, 0) + ">" + NewData[i][0].Substring(0, NewData[i][0].ToString().Length - 3) + " kb/s";
                        
                        double Audio_capacity = Convert.ToDouble(OldData[5]) - (Convert.ToDouble(OldData[0]) * Convert.ToDouble(OldData[4]) / 8); //計算Audio大小

                        double Video_estimate_capacity = Convert.ToDouble(numericUpDown1.Value * 1000) * Convert.ToDouble(NewData[i][4]) / 8; //Video預估大小
                        listView1.Items[i].SubItems[6].Text = Math.Round(Convert.ToDouble(OldData[5]) / 1024 / 1024, 2).ToString() + ">" + Math.Round((Video_estimate_capacity + Audio_capacity) / 1024 / 1024, 2).ToString() + " MB";
                        
                        XOCode[i] = Xonepass(comboBox4.SelectedItem.ToString(), Convert.ToDouble(NewData[i][0]));
                        XTCode[i] = Xtwopass(comboBox4.SelectedItem.ToString(), Convert.ToDouble(NewData[i][0]), NewData[i][3]);
                        ShowToForm2(i, InFileName[i]);
                    }
                }
            }
        }

        //***********************************************************************************************************
        //FPS
        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                ComboboxItem item = comboBox5.Items[comboBox5.SelectedIndex] as ComboboxItem;
                if (listView1.Items[i].Checked)
                {
                    NewData[i][2] = item.Value.ToString() == "Auto" ? DataOriginal[i][2] : NewData[i][2] = item.Value.ToString();
                    XOCode[i] = Xonepass(comboBox4.SelectedItem.ToString(), Convert.ToDouble(NewData[i][0]));
                    XTCode[i] = Xtwopass(comboBox4.SelectedItem.ToString(), Convert.ToDouble(NewData[i][0]), NewData[i][3]);
                    ShowToForm2(i, InFileName[i]);
                }
                try
                {
                    listView1.Items[i].SubItems[3].Text = Math.Round(Convert.ToDouble(DataOriginal[i][2].Split('/')[0]) / Convert.ToDouble(DataOriginal[i][2].Split('/')[1]), 3) + ">" 
                        + Math.Round(Convert.ToDouble(NewData[i][2].Split('/')[0]) / Convert.ToDouble(NewData[i][2].Split('/')[1]), 3).ToString();
                }
                catch
                {
                    listView1.Items[i].SubItems[3].Text = NewData[i][2] + ">" + NewData[i][2];
                }
            }
        }

        //***********************************************************************************************************
        //Resolution
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].Checked)
                {
                    double width = Convert.ToDouble(DataOriginal[i][3].Split('x')[0]);
                    double hiegh = Convert.ToDouble(DataOriginal[i][3].Split('x')[1]);

                    if (comboBox3.SelectedItem.ToString() == "Auto")
                        NewData[i][3] = DataOriginal[i][3];
                    else if (comboBox3.SelectedItem.ToString() == "360p")
                    {
                        double width640 = 640;
                        int newHiegh = Convert.ToInt16(Math.Floor(hiegh / (width / width640)));
                        NewData[i][3] = (width640 + "") + "x" + (newHiegh + "");
                    }
                    else if (comboBox3.SelectedItem.ToString() == "480p")
                    {
                        NewData[i][3] = "640x480";
                    }
                    else if (comboBox3.SelectedItem.ToString() == "720p")
                    {
                        double width1280 = 1280;
                        int newHiegh = Convert.ToInt16(Math.Floor(hiegh / (width / width1280)));
                        NewData[i][3] = (width1280 + "") + "x" + (newHiegh + "");
                    }
                    else if (comboBox3.SelectedItem.ToString() == "1080p")
                    {
                        double width1920 = 1920;
                        int newHiegh = Convert.ToInt16(Math.Floor(hiegh / (width / width1920)));
                        NewData[i][3] = (width1920 + "") + "x" + (newHiegh + "");
                    }
                    
                    XOCode[i] = Xonepass(comboBox4.SelectedItem.ToString(), Convert.ToDouble(NewData[i][0]));
                    XTCode[i] = Xtwopass(comboBox4.SelectedItem.ToString(), Convert.ToDouble(NewData[i][0]), NewData[i][3]);
                    ShowToForm2(i, InFileName[i]);
                }
                listView1.Items[i].SubItems[4].Text = DataOriginal[i][3] + ">" + NewData[i][3];
            }
        }
        
        //***********************************************************************************************************
        //CPU
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.Items.Count != 0)
                for (int i = 0; i < NewData.Count; i++)
                {
                    XOCode[i] = Xonepass(comboBox4.SelectedItem.ToString(), Convert.ToDouble(NewData[i][0]));
                    XTCode[i] = Xtwopass(comboBox4.SelectedItem.ToString(), Convert.ToDouble(NewData[i][0]), NewData[i][3]);
                    ShowToForm2(i, InFileName[i]);
                }
        }

        //***********************************************************************************************************
        //線程強制停止
        private void button5_Click(object sender, EventArgs e)
        {
        restart:
            {
                try
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
                                switch (p.ProcessName) {
                                    case "x264":
                                    case "avs4x26x":
                                    case "mp4box":
                                    case "eac3to":
                                        p.Kill();
                                        break;
                                }
                            }

                            button1.Enabled = true;
                            button2.Enabled = true;
                            button3.Enabled = true;
                            button5.Enabled = false;
                            button8.Enabled = true;
                            button9.Enabled = true;
                            comboBox2.Enabled = true;
                            comboBox3.Enabled = true;
                            comboBox4.Enabled = true;
                            comboBox5.Enabled = true;
                            numericUpDown1.Enabled = true;
                            Delete();
                            ShowError("已強制停止");
                            listView1.Items[Convert.ToInt32(textBox1.Text.Split('/')[0]) - 1].SubItems[8].Text = "強制停止";
                            break;
                        }
                    }
                }
                catch
                {
                    goto restart;
                }
            }
        }

        //***********************************************************************************************************
        //check debug window
        private void button6_Click(object sender, EventArgs e)
        {
            f2.Show();
        }

        //***********************************************************************************************************
        //檢查 mp4box 是否結束
        void process_Exited(object sender, EventArgs e)
        {
        }

        //***********************************************************************************************************
        private void Form1_Load(object sender, EventArgs e)
        {
            Form.CheckForIllegalCrossThreadCalls = false;
        }

        //***********************************************************************************************************
        //listView1 全選
        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
        }

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
                    (Convert.ToBoolean(e.Header.Tag) ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal));
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

        //***********************************************************************************************************
        //清空
        private void button3_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            f2.clear2();
            button2.Enabled = false;
            button5.Enabled = false;
            textBox1.Text = "0/0";
            listView1.Columns[0].Tag = false;
            listView1.Refresh();

            NewData.Clear();
            InFileName.Clear();
            OutFileName.Clear();
            Subtitles.Clear();
            DataOriginal.Clear();
            XOCode.Clear();
            XTCode.Clear();
        }

        //***********************************************************************************************************
        //移除
        private void button8_Click(object sender, EventArgs e)
        {
            for (int i = listView1.SelectedItems.Count - 1; i >= 0; i--)
            {
                int Index = listView1.SelectedIndices[i];
                NewData.RemoveAt(Index);
                InFileName.RemoveAt(Index);
                OutFileName.RemoveAt(Index);
                Subtitles.RemoveAt(Index);
                DataOriginal.RemoveAt(Index);
                XOCode.RemoveAt(Index);
                XTCode.RemoveAt(Index);
                listView1.Items.RemoveAt(Index);
            }

            textBox1.Text = "0/" + listView1.Items.Count.ToString();

            if (listView1.Items.Count == 0)
                button3_Click(sender, e);
        }

        //***********************************************************************************************************
        //限制關閉程式
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Nico_Init != null && Nico_Init.IsAlive && checkBox1.Checked.ToString() == "False")
            {
                ShowError("轉檔中，請先停止後再關閉程式");
                e.Cancel = true;
            }
            else
                e.Cancel = false;
        }
       

        //***********************************************************************************************************
        //Set combox5
        private class ComboboxItem
        {
            public ComboboxItem(string text, string value) { Text = text; Value = value; }
            public string Value { get; set; }
            public string Text { get; set; }
            public override string ToString() { return Text; }
        }

        //***********************************************************************************************************
        //回復預設值
        private void button9_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0;
            comboBox2_SelectedIndexChanged(sender, e);
            comboBox3.SelectedIndex = 0;
            comboBox3_SelectedIndexChanged(sender, e);
            comboBox4.SelectedIndex = 0;
            comboBox4_SelectedIndexChanged(sender, e);
            comboBox5.SelectedIndex = 0;
            comboBox5_SelectedIndexChanged(sender, e);
        }
    }

}