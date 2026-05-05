using System.Text.Json;
using X264GUIv2.Enums;
using X264GUIv2.Models;

namespace X264GUIv2
{
    internal class VideoFunc(Form1 form)
    {
        public List<FfprobeOutput> ffprobeData = []; //原始影片資料

        /// <summary>
        /// 產生編碼
        /// </summary>
        /// <param name="file"></param>
        public void Encode(string[] files)
        {
            try
            {
                List<LoadFile> loadFiles = [.. files.Select((x, idx) => new LoadFile
                {
                    File = x,
                    index = idx,
                })];

                List<FfprobeOutput> _ffprobe = [];
                Parallel.ForEach(loadFiles,
                    () => new HashSet<object>(), // 每個 thread 一份
                    (file, state, localVisited) =>
                    {
                        _ffprobe.Add(ffprobe(file));
                        return localVisited;
                    },
                    _ => { }
                );

                _ffprobe = [.. _ffprobe.OrderBy(x => x.index)];

                for (int i = 0; i < _ffprobe.Count; i++)
                {
                    _ffprobe[i] = bitRateFunc(_ffprobe[i]);
                    _ffprobe[i] = bitRateFunc(_ffprobe[i]);
                    _ffprobe[i] = fpsFunc(_ffprobe[i]);
                    _ffprobe[i] = resolutionFunc(_ffprobe[i]);
                    _ffprobe[i] = bitRateNumericFunc(_ffprobe[i]);
                    form.listView1.Items.Add(DataViewObject(_ffprobe[i]));
                }

                ffprobeData.AddRange(_ffprobe);
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
        public static FfprobeOutput ffprobe(LoadFile input)
        {
            FfprobeOutput ffprobeOutput = new();
            string standardOutput = "";

            TaskHelper task = new()
            {
                Cts = new(),
                FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}\bin\ffmpeg\ffprobe.exe",
                ArgumentList = { $@"-of json -show_streams -show_format -v quiet ""{input.File}""" },
                isWait = true,
                ActionOut = sr => standardOutput += sr
            };
            task.RunTask();

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
                InFile = input.File,
                index = input.index,
                OriDetail = new()
                {
                    bitrate = bitrateTemp,
                    frameMode = video.r_frame_rate == video.avg_frame_rate ? FrameModeEnum.CBR : FrameModeEnum.VBR,
                    frameStr = video.r_frame_rate ?? video.avg_frame_rate ?? "24000/1001",
                    resolutionW = video.width,
                    resolutionH = video.height,
                }
            };

            ffprobeOutput.NewDetail = new()
            {
                bitrate = ffprobeOutput.OriDetail.bitrate,
                frameMode = ffprobeOutput.OriDetail.frameMode,
                frameStr = ffprobeOutput.OriDetail.frameStr,
                resolutionW = ffprobeOutput.OriDetail.resolutionW,
                resolutionH = ffprobeOutput.OriDetail.resolutionH,
            };

            return ffprobeOutput;
        }

        public static DetailsItem DataViewText(FfprobeOutput ffprobeOutput)
        {
            DetailsItem detailsItem = new();

            string OldCapacity = Math.Round(Convert.ToDouble(ffprobeOutput.size) / 1024 / 1024, 2).ToString(); //原始大小
            double Audio_capacity = Convert.ToDouble(ffprobeOutput.size) - (Convert.ToDouble(ffprobeOutput.OriDetail.bitrate) * Convert.ToDouble(ffprobeOutput.duration) / 8); //計算Audio大小
            string NewCapacity = Math.Round((Audio_capacity + Convert.ToDouble(ffprobeOutput.NewDetail.bitrate) * ffprobeOutput.duration / 8) / 1024 / 1024, 2).ToString() + " MB"; //Video預估大小

            detailsItem.BitRate = $"{ffprobeOutput.OriDetail.bitrate / 1000} > {ffprobeOutput.NewDetail.bitrate / 1000} kb/s";
            detailsItem.FpsMode = $"{ffprobeOutput.OriDetail.frameMode} > {FrameModeEnum.CBR}";
            detailsItem.Fps = $"{Math.Round(ffprobeOutput.OriDetail.frameRate, 3)} > {Math.Round(ffprobeOutput.NewDetail.frameRate, 3)}";
            detailsItem.Resolution = $"{ffprobeOutput.OriDetail.resolution} > {ffprobeOutput.NewDetail.resolution}";
            detailsItem.Duration = TimeSpan.FromSeconds(ffprobeOutput.duration).ToString(@"hh\:mm\:ss");
            detailsItem.Size = $"{OldCapacity} > {NewCapacity}";
            detailsItem.Progress = "00.00 %";
            detailsItem.Status = "IDEL";
            detailsItem.Time = "00:00:00";
            detailsItem.Path = ffprobeOutput.InFile ?? "";

            detailsItem.Text = Path.GetFileName(ffprobeOutput.InFile) +
                    $"\nBitRate: {ffprobeOutput.OriDetail.bitrate / 1000} kb/s" +
                    $"\nFPS模式: {Enum.GetName(ffprobeOutput.OriDetail.frameMode)}" +
                    $"\nFPS: {Math.Round(ffprobeOutput.OriDetail.frameRate, 3)}" +
                    $"\n解析度: {ffprobeOutput.OriDetail.resolution}" +
                    $"\n檔案大小: {OldCapacity} MB";

            return detailsItem;
        }

        public static ListViewItem DataViewObject(FfprobeOutput ffprobeOutput)
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

            return lis;
        }


        public FfprobeOutput bitRateFunc(FfprobeOutput ffprobeOutput)
        {
            if (form.bitrateCBox.Items[form.bitrateCBox.SelectedIndex] is ComboboxItem item)
            {
                if (!int.TryParse(item.Value, out int v))
                    throw new Exception("");

                BitrateEnum bitrateEnum = (BitrateEnum)v;

                if (bitrateEnum == BitrateEnum.Auto)
                {
                    if ((ffprobeOutput.OriDetail.bitrate - 100000) > form.bitRateDefault)
                        ffprobeOutput.NewDetail.bitrate = form.bitRateDefault;
                    else
                        ffprobeOutput.NewDetail.bitrate = ffprobeOutput.OriDetail.bitrate - 100000;
                }
                else if (bitrateEnum == BitrateEnum.Manual)
                    ffprobeOutput.NewDetail.bitrate = (int)form.bitrateNumeric.Value * 1000;
            }

            return ffprobeOutput;
        }

        public FfprobeOutput fpsFunc(FfprobeOutput ffprobeOutput)
        {
            if (form.fpsCBox.Items[form.fpsCBox.SelectedIndex] is ComboboxItem item)
            {
                ffprobeOutput.NewDetail.frameStr = item.Value == "Auto" ? ffprobeOutput.OriDetail.frameStr : item.Value;
            }

            return ffprobeOutput;
        }

        public FfprobeOutput resolutionFunc(FfprobeOutput ffprobeOutput)
        {
            if (form.resolutionCBox.Items[form.resolutionCBox.SelectedIndex] is ComboboxItem item)
            {
                if (!int.TryParse(item.Value, out int v))
                    throw new Exception("");

                ResolutionEnum resolutionEnum = (ResolutionEnum)v;

                if (resolutionEnum == ResolutionEnum.Auto)
                {
                    ffprobeOutput.NewDetail.resolutionW = ffprobeOutput.OriDetail.resolutionW;
                    ffprobeOutput.NewDetail.resolutionH = ffprobeOutput.OriDetail.resolutionH;
                }
                else
                {
                    (int, int) GCD = OtherControlFunc.GetGCD(ffprobeOutput.OriDetail.resolutionW ?? 0, ffprobeOutput.OriDetail.resolutionH ?? 0);
                    ffprobeOutput.NewDetail.resolutionW = (int)resolutionEnum / GCD.Item2 * GCD.Item1;
                    ffprobeOutput.NewDetail.resolutionH = (int)resolutionEnum;
                }
            }

            return ffprobeOutput;
        }

        public FfprobeOutput bitRateNumericFunc(FfprobeOutput ffprobeOutput)
        {
            ffprobeOutput.NewDetail.bitrate = (int)form.bitrateNumeric.Value * 1000;
            return ffprobeOutput;
        }

        /// <summary>
        /// 刪除檔案
        /// </summary>
        public void Delete(FfprobeOutput ffprobeOutput)
        {
            try
            {
                Thread.Sleep(1000);
                File.Delete($@"{ffprobeOutput.InFileName}.ffindex");
                File.Delete(@"./x2642pass.stats.temp");
                File.Delete(@"./x2642pass.stats.mbtree.temp");
                File.Delete(@"./x2642pass.stats");
                File.Delete(@"./x2642pass.stats.mbtree");
                File.Delete(@$"./{ffprobeOutput.avsTempFile}.avs");
                File.Delete(@$"./{ffprobeOutput.avsTempFile}.ass");
                File.Delete(@$"./{ffprobeOutput.avsTempFile}.264");
                File.Delete(@$"./{ffprobeOutput.avsTempFile}.aac");
                File.Delete(@$"./{ffprobeOutput.avsTempFile}.mp4");
            }
            catch
            {
            }
        }

        public string Xonepass(FfprobeOutput ffprobeOutput)
        {
            string threads = form.coreCBox.SelectedItem?.ToString() ?? "0";

            string str = $@"^
--x26x-binary ""{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\x264.exe"" ^
--bframes 0 --bitrate {ffprobeOutput.NewDetail.bitrate / 1000} --pass 1 --threads {threads} --stats ""x2642pass.stats"" ^
-o NUL ""{ffprobeOutput.avsTempFile}.avs""
";

            return str;
        }

        public string Xtwopass(FfprobeOutput ffprobeOutput)
        {
            string threads = form.coreCBox.SelectedItem?.ToString() ?? "0";
            string str;
            if (ffprobeOutput.NewDetail.resolutionW == null || ffprobeOutput.NewDetail.resolutionH == null)
                str = $@"^
--x26x-binary ""{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\x264.exe"" ^
--bframes 0 --bitrate {ffprobeOutput.NewDetail.bitrate / 1000} --pass 2 --threads {threads} --stats ""x2642pass.stats"" ^
-o ""{ffprobeOutput.avsTempFile}.264"" ""{ffprobeOutput.avsTempFile}.avs""
";
            else
                str = $@"^
--x26x-binary ""{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\x264.exe"" ^
--bframes 0 --bitrate {ffprobeOutput.NewDetail.bitrate / 1000} --pass 2 --threads {threads} --stats ""x2642pass.stats"" ^
--vf resize:width={ffprobeOutput.NewDetail.resolutionW},height={ffprobeOutput.NewDetail.resolutionH},sar=1:1 ^
-o ""{ffprobeOutput.avsTempFile}.264"" ""{ffprobeOutput.avsTempFile}.avs""
";

            return str;
        }
    }
}
