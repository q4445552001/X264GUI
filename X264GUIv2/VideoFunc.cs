using System.Diagnostics;
using System.Text;
using System.Text.Json;
using X264GUIv2.Enums;
using X264GUIv2.Models;

namespace X264GUIv2
{
    internal class VideoFunc(Form1 form)
    {
        public readonly List<FfprobeOutput> ffprobeDataOriginal = []; //原始影片資料

        /// <summary>
        /// 產生編碼
        /// </summary>
        /// <param name="file"></param>
        public void Encode(string file)
        {
            try
            {
                FfprobeOutput ffprobeData = ffprobe(file);
                ffprobeDataOriginal.Add(ffprobeData);

                //XOCode.Add(Xonepass(form.coreCBox.SelectedItem?.ToString() ?? "0", ffprobeData.bitrate));
                //XTCode.Add(Xtwopass(form.coreCBox.SelectedItem?.ToString() ?? "0", ffprobeData.bitrate, outres));

                DataView(ffprobeData);
                //ShowToForm2(count, file);
            }
            catch (IndexOutOfRangeException print)
            {
                OtherControlFunc.ShowError("格式錯誤\n" + print);
            }
            catch (NullReferenceException print)
            {
                OtherControlFunc.ShowError("格式錯誤\n" + print);
            }
            catch (Exception ex)
            {
                OtherControlFunc.ShowError("格式錯誤\n" + ex.Message);
            }

            form.progressText.Text = $"0/{form.listView1.Items.Count}";
            form.runBtn.Enabled = form.listView1.Items.Count != 0;
        }

        /// <summary>
        /// ffprobe
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static FfprobeOutput ffprobe(string input)
        {
            FfprobeOutput ffprobeOutput = new();

            Process run = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}\bin\ffmpeg\ffprobe.exe",
                    Arguments = $@"-of json -show_streams -show_format -v quiet ""{input}""",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                }
            };
            run.Start();

            var standardOutput = run.StandardOutput.ReadToEnd();
            //Debug.WriteLine(standardOutput);

            StandardOutput.Main? stuff = JsonSerializer.Deserialize<StandardOutput.Main>(standardOutput);
            if (stuff == null || stuff.streams == null)
                throw new Exception("解析失敗");

            StandardOutput.StreamData? video = stuff.streams.FirstOrDefault(x => x.codec_type?.ToLower() == "video") ?? throw new Exception("空視訊");
            if (!int.TryParse(video.bit_rate ?? stuff.format?.bit_rate, out int bitrateTemp))
                bitrateTemp = 0;

            ffprobeOutput = new()
            {
                duration = double.TryParse(stuff.format?.duration, out double _duration) ? _duration : 0,
                size = int.TryParse(stuff.format?.size, out int _size) ? _size : 0,
                isAcc = stuff.streams.Any(x => x.codec_type?.ToLower() == "audio" && x.codec_name?.ToLower() == "aac"),
                InFile = stuff.format?.filename?.Replace("\\", "//")!,
                OriDetail = new()
                {
                    bitrate = bitrateTemp,
                    frameMode = video.r_frame_rate == video.avg_frame_rate ? FrameModeEnum.CBR : FrameModeEnum.VBR,
                    frameStr = video.r_frame_rate ?? video.avg_frame_rate ?? "24000/1001",
                    resolutionW = video.width,
                    resolutionH = video.height,
                }
            };

            ffprobeOutput.NewDetail = ffprobeOutput.OriDetail;

            return ffprobeOutput;
        }

        /// <summary>
        /// 檢視詳細資料
        /// </summary>
        public void DataView(FfprobeOutput ffprobeOutput)
        {
            List<string> row = [ffprobeOutput.InFileName];

            DetailsItem detailsItem = DataViewText(ffprobeOutput);
            row.Add(detailsItem.BitRate);
            row.Add(detailsItem.FpsMode);
            row.Add(detailsItem.Fps);
            row.Add(detailsItem.Resolution);
            row.Add(detailsItem.Duration);
            row.Add(detailsItem.Size);
            row.Add(detailsItem.Progress);
            row.Add(detailsItem.Status);
            row.Add(detailsItem.Time);
            row.Add(detailsItem.Path);

            ListViewItem lis = new([.. row])
            {
                Tag = ffprobeOutput.Guid,
                ToolTipText = detailsItem.Text
            };

            form.listView1.Items.Add(lis);
        }

        public static DetailsItem DataViewText(FfprobeOutput ffprobeOutput)
        {
            DetailsItem detailsItem = new();

            string OldCapacity = Math.Round(Convert.ToDouble(ffprobeOutput.size) / 1024 / 1024, 2).ToString(); //原始大小
            double Audio_capacity = Convert.ToDouble(ffprobeOutput.size) - (Convert.ToDouble(ffprobeOutput.OriDetail.bitrate) * Convert.ToDouble(ffprobeOutput.duration) / 8); //計算Audio大小
            string NewCapacity = Math.Round((Audio_capacity + Convert.ToDouble(ffprobeOutput.NewDetail.bitrate) * ffprobeOutput.duration / 8) / 1024 / 1024, 2).ToString() + " MB"; //Video預估大小

            detailsItem.BitRate = $"{ffprobeOutput.OriDetail.bitrate / 1000} > {ffprobeOutput.NewDetail.bitrate / 1000} kb/s";
            detailsItem.FpsMode = $"{ffprobeOutput.OriDetail.frameMode} > {ffprobeOutput.NewDetail.frameMode}";
            detailsItem.Fps = $"{Math.Round(ffprobeOutput.OriDetail.frameRate, 3)} > {Math.Round(ffprobeOutput.NewDetail.frameRate, 3)}";
            detailsItem.Resolution = $"{ffprobeOutput.OriDetail.resolution} > {ffprobeOutput.NewDetail.resolution}";
            detailsItem.Duration = TimeSpan.FromSeconds(ffprobeOutput.duration).ToString(@"hh\:mm\:ss");
            detailsItem.Size = $"{OldCapacity} > {NewCapacity}";
            detailsItem.Progress = "00.00 %";
            detailsItem.Status = "IDEL";
            detailsItem.Time = "00:00:00";
            detailsItem.Path = ffprobeOutput.InFile?.Replace("//", "\\") ?? "";

            detailsItem.Text = Path.GetFileName(ffprobeOutput.InFile) +
                    $"\nBitRate: {ffprobeOutput.OriDetail.bitrate / 1000} kb/s" +
                    $"\nFPS模式: {Enum.GetName(ffprobeOutput.OriDetail.frameMode)}" +
                    $"\nFPS: {Math.Round(ffprobeOutput.OriDetail.frameRate, 3)}" +
                    $"\n解析度: {ffprobeOutput.OriDetail.resolution}" +
                    $"\n檔案大小: {OldCapacity} MB";

            return detailsItem;
        }

        /// <summary>
        /// 刪除檔案
        /// </summary>
        public void Delete()
        {
            //for (int i = 0; i < form.listView1.Items.Count; i++)
            //    File.Delete(InFileName[i].Replace("//", "\\") + @".ffindex");
            File.Delete(@"./x2642pass.stats.temp");
            File.Delete(@"./x2642pass.stats.mbtree.temp");
            File.Delete(@"./x2642pass.stats");
            File.Delete(@"./x2642pass.stats.mbtree");
            File.Delete(@"./avsTemp.avs");
            File.Delete(@"./avsTemp.ass");
            File.Delete(@"./avsTemp.264");
            File.Delete(@"./avsTemp.aac");
            File.Delete(@"./avsTemp.mp4");
            form.progressBar1.Value = 0;
        }

        public string Xonepass(FfprobeOutput ffprobeOutput)
        {
            string threads = form.coreCBox.SelectedItem?.ToString() ?? "0";

            string str = $@"
--x26x-binary ""{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\x264.exe\"" ^
--bframes 0 --bitrate {ffprobeOutput.NewDetail.bitrate / 1000} --pass 1 --threads {threads} --stats ""x2642pass.stats"" ^
-o NUL ""avsTemp.avs""
";

            return str;
        }

        public string Xtwopass(FfprobeOutput ffprobeOutput)
        {
            string threads = form.coreCBox.SelectedItem?.ToString() ?? "0";
            string str;
            if (ffprobeOutput.NewDetail.resolutionW == null || ffprobeOutput.NewDetail.resolutionH == null)
                str = $@"
--x26x-binary ""{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\x264.exe"" ^
--bframes 0 --bitrate {ffprobeOutput.NewDetail.bitrate / 1000} --pass 2 --threads {threads} --stats ""x2642pass.stats"" ^
-o ""avsTemp.264"" ""avsTemp.avs""
";
            else
                str = $@"
--x26x-binary ""{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\x264.exe"" ^
--bframes 0 --bitrate {ffprobeOutput.NewDetail.bitrate / 1000} --pass 2 --threads {threads} --stats ""x2642pass.stats"" ^
--vf resize:width={ffprobeOutput.NewDetail.resolutionW},height={ffprobeOutput.NewDetail.resolutionH},sar=1:1
-o ""avsTemp.264"" ""avsTemp.avs""
";

            return str;
        }
    }
}
