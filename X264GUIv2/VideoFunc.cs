using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using X264GUIv2.Enums;
using X264GUIv2.Models;

namespace X264GUIv2
{
    public class VideoFunc(Form1 form)
    {
        public List<FfprobeOutput> ffprobeData = []; //原始影片資料

        /// <summary>
        /// 產生編碼
        /// </summary>
        /// <param name="file"></param>
        public void Encode(string[] files, bool isMerge = false)
        {
            try
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
                        if (file.videoType == VideoTypeEnum.Aviscript)
                            _ffprobe.Add(avsffprobe(file));
                        else
                        {
                            FfprobeOutputMain main = ffprobe(file);
                            main.videoType = isMerge ? VideoTypeEnum.Merge : main.videoType;
                            _ffprobe.Add(main);
                        }
                        return localVisited;
                    },
                    _ => { }
                );

                List<FfprobeOutput> data = [];
                if (_ffprobe.FirstOrDefault()?.videoType == VideoTypeEnum.Merge)
                {
                    FfprobeOutput merge = new()
                    {
                        MainData = _ffprobe.First(x => x.idx == 0),
                        MergeData = [.. _ffprobe],
                    };

                    for (int i = 0; i < merge.MergeData.Count; i++)
                    {
                        merge.MergeData[i].MergeGuid = merge.MainData.Guid;
                        merge.MergeData[i].idx = -merge.MergeData[i].idx;
                    }

                    merge.MainData.duration = merge.MergeData.Sum(x => x.duration);
                    merge.MainData.videoSize = merge.MergeData.Sum(x => x.videoSize);
                    merge.MainData.audioSize = merge.MergeData.Sum(x => x.audioSize);
                    merge.MainData.OriDetail.bitrate = merge.MergeData.Sum(x => x.OriDetail.bitrate) / merge.MergeData.Count;
                    data = [new FfprobeOutput { MainData = merge.MainData, MergeData = merge.MergeData }];
                }
                else
                {
                    data = [.. _ffprobe.OrderBy(x => x.idx).Select(x => new FfprobeOutput { MainData = x }).ToList()];
                }

                for (int i = 0; i < data.Count; i++)
                {
                    data[i] = bitRateFunc(data[i]);
                    data[i] = fpsFunc(data[i]);
                    data[i] = resolutionFunc(data[i]);
                    data[i] = bitRateNumericFunc(data[i]);
                    form.listView1.Items.Add(form.listView1.DataViewObject(data[i]));
                    ffprobeData.Add(data[i]);
                }
            }
            catch (IndexOutOfRangeException print)
            {
                WriteFile.WriteLog($"格式錯誤: {print}");
            }
            catch (NullReferenceException print)
            {
                WriteFile.WriteLog($"格式錯誤: {print}");
            }
            catch (Exception ex)
            {
                WriteFile.WriteLog(ex.Message);
            }

            form.progressText.Text = $"{ffprobeData.Count(x => x.MainData.run == RunEnum.Done)}/{ffprobeData.Count}";
            form.runBtn.Enabled = form.listView1.Items.Count != 0;
        }

        /// <summary>
        /// ffprobe
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static FfprobeOutputMain ffprobe(LoadFile input)
        {
            FfprobeOutputMain ffprobeOutput = new();
            string standardOutput = "";

            TaskHelper task = new()
            {
                Cts = new(),
                FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\ffmpeg\ffprobe.exe",
                ArgumentList = {
                    $@"-of json",
                    $@"-show_streams",
                    $@"-show_format",
                    $@"-v quiet ""{input.File}"""
                },
                ActionOut = sr => standardOutput += sr
            };
            task.RunTask();

            StandardOutput.Main? stuff = JsonSerializer.Deserialize<StandardOutput.Main>(standardOutput);
            if (stuff == null || stuff.streams == null)
                throw new Exception("解析失敗");

            #region Video
            StandardOutput.StreamData? video = stuff.streams.FirstOrDefault(x => x.codec_type?.ToLower() == "video") ?? throw new Exception("空視訊");
            if (!int.TryParse(video.bit_rate ?? stuff.format?.bit_rate, out int bitrateTemp))
                bitrateTemp = 0;
            #endregion

            #region Audio
            StandardOutput.StreamData? audio = stuff.streams.FirstOrDefault(x => x.codec_type?.ToLower() == "audio");
            int audioSize = 0;
            if (audio is not null)
            {
                int audioBitRate = int.TryParse(audio.bit_rate, out int _audioBitRate) ? _audioBitRate : 0;
                int audioDuration = double.TryParse(audio.duration, out double _audioDuration) ? (int)_audioDuration : 0;
                if (audioBitRate == 0)
                    audioBitRate = 1;
                if (audioDuration == 0)
                    audioDuration = 1;

                audioSize = audioBitRate * audioDuration / 8;
            }
            #endregion

            ffprobeOutput = new()
            {
                duration = double.TryParse(stuff.format?.duration, out double _duration) ? _duration : 0,
                videoSize = int.TryParse(stuff.format?.size, out int _vsize) ? _vsize : 0,
                audioSize = audioSize,
                isAac = audio?.codec_name == "aac",
                audioMap = audio?.index ?? 0,
                videoCodeName = video.codec_name ?? "",
                InFile = input.File,
                idx = input.index,
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

        /// <summary>
        /// avsffprobe
        /// </summary>
        public static FfprobeOutputMain avsffprobe(LoadFile input)
        {
            FfprobeOutputMain ffprobeOutput = new();
            double frames = 0;
            string fpsStr = "24000/1001";
            double fps = 0;
            int width = 0;
            int height = 0;

            TaskHelper task = new()
            {
                Cts = new(),
                FileName = $@"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\avs4x26x.exe",
                ArgumentList = {
                            $@"--info",
                            $@"""{input.File}""",
                        },
                ActionOut = sr =>
                {
                    // Framecount
                    var _frame = Regex.Match(sr, @"Video framecount:\s*(\d+)");
                    if (_frame.Success)
                        frames = double.Parse(_frame.Groups[1].Value);

                    // FPS (24000/1001)
                    var _fps = Regex.Match(sr, @"Video framerate:\s*(\d+)/(\d+)");
                    if (_fps.Success)
                    {
                        double num = double.Parse(_fps.Groups[1].Value);
                        double den = double.Parse(_fps.Groups[2].Value);
                        fpsStr = $"{_fps.Groups[1]}/{_fps.Groups[2]}";
                        fps = num / den;
                    }

                    var match = Regex.Match(sr, @"Video resolution:\s*(\d+)x(\d+)");
                    if (match.Success)
                    {
                        width = int.Parse(match.Groups[1].Value);
                        height = int.Parse(match.Groups[2].Value);
                    }
                }
            };
            task.RunTask();

            ffprobeOutput = new()
            {
                duration = frames / fps,
                InFile = input.File,
                idx = input.index,
                OriDetail = new()
                {
                    frameStr = fpsStr,
                    resolutionW = width,
                    resolutionH = height,
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

        public FfprobeOutput bitRateFunc(FfprobeOutput ffprobeOutput)
        {
            if (form.bitrateCBox.Items[form.bitrateCBox.SelectedIndex] is ComboboxItem item)
            {
                if (!int.TryParse(item.Value, out int v))
                    throw new Exception("無效選項");

                BitrateEnum bitrateEnum = (BitrateEnum)v;

                if (bitrateEnum == BitrateEnum.Auto)
                {
                    if (ffprobeOutput.MainData.videoType == VideoTypeEnum.Aviscript)
                        ffprobeOutput.MainData.NewDetail.bitrate = form.bitRateDefault;
                    else
                    {
                        if (ffprobeOutput.MainData.OriDetail.bitrate < form.bitRateDefault)
                            ffprobeOutput.MainData.NewDetail.bitrate = ffprobeOutput.MainData.OriDetail.bitrate - 100000;
                        else if ((ffprobeOutput.MainData.OriDetail.bitrate - 100000) > form.bitRateDefault)
                            ffprobeOutput.MainData.NewDetail.bitrate = form.bitRateDefault;
                        else
                            ffprobeOutput.MainData.NewDetail.bitrate = ffprobeOutput.MainData.OriDetail.bitrate - 100000;
                    }
                }
                else if (bitrateEnum == BitrateEnum.Manual)
                    ffprobeOutput.MainData.NewDetail.bitrate = (int)form.bitrateNumeric.Value * 1000;
            }

            return ffprobeOutput;
        }

        public FfprobeOutput fpsFunc(FfprobeOutput ffprobeOutput)
        {
            if (form.fpsCBox.Items[form.fpsCBox.SelectedIndex] is ComboboxItem item)
                ffprobeOutput.MainData.NewDetail.frameStr = item.Value == "Auto" ? ffprobeOutput.MainData.OriDetail.frameStr : item.Value;

            return ffprobeOutput;
        }

        public FfprobeOutput resolutionFunc(FfprobeOutput ffprobeOutput)
        {
            string text = "0";
            if (form.resolutionCBox.SelectedIndex == -1)
                text = form.resolutionCBox.Text;
            else if (form.resolutionCBox.Items[form.resolutionCBox.SelectedIndex] is ComboboxItem item)
                text = item.Value;

            if (!int.TryParse(text, out int v))
                throw new Exception("請輸入數值");

            int resolution;
            if (Enum.TryParse(text, out ResolutionEnum resolutionEnum))
                resolution = (int)resolutionEnum;
            else
                resolution = v;

            if (resolutionEnum == ResolutionEnum.Auto)
            {
                ffprobeOutput.MainData.NewDetail.resolutionW = ffprobeOutput.MainData.OriDetail.resolutionW;
                ffprobeOutput.MainData.NewDetail.resolutionH = ffprobeOutput.MainData.OriDetail.resolutionH;
            }
            else
            {
                (int, int) GCD = OtherControlFunc.GetGCD(ffprobeOutput.MainData.OriDetail.resolutionW ?? 0, ffprobeOutput.MainData.OriDetail.resolutionH ?? 0);
                ffprobeOutput.MainData.NewDetail.resolutionW = OtherControlFunc.FixEven(resolution / GCD.Item2 * GCD.Item1);
                ffprobeOutput.MainData.NewDetail.resolutionH = resolution;

                if (ffprobeOutput.MainData.NewDetail.resolutionW == 0)
                {
                    ffprobeOutput.MainData.NewDetail.resolutionW = ffprobeOutput.MainData.OriDetail.resolutionW;
                    ffprobeOutput.MainData.NewDetail.resolutionH = ffprobeOutput.MainData.OriDetail.resolutionH;
                }
            }

            return ffprobeOutput;
        }

        public FfprobeOutput bitRateNumericFunc(FfprobeOutput ffprobeOutput)
        {
            if (form.bitrateCBox.Items[form.bitrateCBox.SelectedIndex] is ComboboxItem item)
            {
                if (!int.TryParse(item.Value, out int v))
                    throw new Exception("無效選項");

                BitrateEnum bitrateEnum = (BitrateEnum)v;

                if (bitrateEnum == BitrateEnum.Manual)
                    ffprobeOutput.MainData.NewDetail.bitrate = (int)form.bitrateNumeric.Value * 1000;
            }

            return ffprobeOutput;
        }

        /// <summary>
        /// 刪除檔案
        /// </summary>
        public static void Delete(FfprobeOutput ffprobeOutput)
        {
            try
            {
                Thread.Sleep(1000);
                File.Delete($"{ffprobeOutput.MainData.InFile}.ffindex");
                File.Delete(@$"{ffprobeOutput.MainData.InFilePath}\x2642pass.stats.temp");
                File.Delete(@$"{ffprobeOutput.MainData.InFilePath}\x2642pass.stats.mbtree.temp");
                File.Delete(@$"{ffprobeOutput.MainData.InFilePath}\x2642pass.stats");
                File.Delete(@$"{ffprobeOutput.MainData.InFilePath}\x2642pass.stats.mbtree");
                File.Delete(@$"{ffprobeOutput.MainData.InFilePath}\{ffprobeOutput.MainData.avsTempFile}.avs");
                File.Delete(@$"{ffprobeOutput.MainData.InFilePath}\{ffprobeOutput.MainData.avsTempFile}.ass");
                File.Delete(@$"{ffprobeOutput.MainData.InFilePath}\{ffprobeOutput.MainData.avsTempFile}.264");
                File.Delete(@$"{ffprobeOutput.MainData.InFilePath}\{ffprobeOutput.MainData.avsTempFile}.aac");
                File.Delete(@$"{ffprobeOutput.MainData.InFilePath}\{ffprobeOutput.MainData.avsTempFile}.mp4");
            }
            catch
            {
            }
        }

        public string[] Xonepass(FfprobeOutput ffprobeOutput)
        {
            List<string> arr = [];

            string threads = form.coreCBox.SelectedItem?.ToString() ?? "0";

            arr.Add($@"--x26x-binary ""{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\x264.exe""");
            arr.Add($@"--bframes 0");
            arr.Add($@"--bitrate {ffprobeOutput.MainData.NewDetail.bitrate / 1000}");
            arr.Add($@"--pass 1");
            arr.Add($@"--threads {threads}");
            arr.Add($@"--stats ""x2642pass.stats""");
            arr.Add($@"-o NUL ""{ffprobeOutput.MainData.avsTempFile}.avs""");
            return [.. arr];
        }

        public string[] XonepassAvs(FfprobeOutput ffprobeOutput)
        {
            List<string> arr = [];

            string threads = form.coreCBox.SelectedItem?.ToString() ?? "0";

            arr.Add($@"-i {ffprobeOutput.MainData.InFile}");
            arr.Add($@"-c:v libx264");
            arr.Add($@"-b:v {ffprobeOutput.MainData.NewDetail.bitrate / 1000}k");
            arr.Add($@"-pass 1");
            arr.Add($@"-an");
            arr.Add($@"-f mp4 NUL");
            arr.Add($@"-y");
            arr.Add($@"-threads {threads}");
            arr.Add($@"-progress pipe:1");
            arr.Add($@"-nostats");
            arr.Add($@"-loglevel error");
            return [.. arr];
        }

        public string[] XonepassMerge(FfprobeOutput ffprobeOutput)
        {
            List<string> arr = [];

            string threads = form.coreCBox.SelectedItem?.ToString() ?? "0";

            arr.Add($@"-f concat");
            arr.Add($@"-safe 0");
            arr.Add($@"-i {ffprobeOutput.MainData.avsTempFile}.merge");
            arr.Add($@"-c:v libx264");
            arr.Add($@"-b:v {ffprobeOutput.MainData.NewDetail.bitrate / 1000}k");
            arr.Add($@"-pass 1");
            arr.Add($@"-an");
            arr.Add($@"-f mp4 NUL");
            arr.Add($@"-y");
            arr.Add($@"-threads {threads}");
            arr.Add($@"-progress pipe:1");
            arr.Add($@"-nostats");
            arr.Add($@"-loglevel error");
            return [.. arr];
        }

        public string[] Xtwopass(FfprobeOutput ffprobeOutput)
        {
            List<string> arr = [];

            string threads = form.coreCBox.SelectedItem?.ToString() ?? "0";
            if (ffprobeOutput.MainData.NewDetail.resolutionW == null || ffprobeOutput.MainData.NewDetail.resolutionH == null)
            {
                arr.Add($@"--x26x-binary ""{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\x264.exe""");
                arr.Add($@"--bframes 0");
                arr.Add($@"--bitrate {ffprobeOutput.MainData.NewDetail.bitrate / 1000}");
                arr.Add($@"--pass 2");
                arr.Add($@"--threads {threads}");
                arr.Add($@"--stats ""x2642pass.stats""");
                arr.Add($@"-o ""{ffprobeOutput.MainData.avsTempFile}.264"" ""{ffprobeOutput.MainData.avsTempFile}.avs""");
            }
            else
            {
                arr.Add($@"--x26x-binary ""{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin\x264\x264.exe""");
                arr.Add($@"--bframes 0");
                arr.Add($@"--bitrate {ffprobeOutput.MainData.NewDetail.bitrate / 1000}");
                arr.Add($@"--pass 2");
                arr.Add($@"--threads {threads}");
                arr.Add($@"--stats ""x2642pass.stats""");
                arr.Add($@"--vf resize:width={ffprobeOutput.MainData.NewDetail.resolutionW},height={ffprobeOutput.MainData.NewDetail.resolutionH},sar=1:1");
                arr.Add($@"-o ""{ffprobeOutput.MainData.avsTempFile}.264"" ""{ffprobeOutput.MainData.avsTempFile}.avs""");
            }

            return [.. arr];
        }

        public string[] XtwopassAvs(FfprobeOutput ffprobeOutput)
        {
            List<string> arr = [];

            string threads = form.coreCBox.SelectedItem?.ToString() ?? "0";

            arr.Add($@"-i {ffprobeOutput.MainData.InFile}");
            if (form.AutoTrimToolStripMenuItem.Checked)
            {
                arr.Add($@"-ar 48000");
                arr.Add($@"-ac 2");
                arr.Add($@"-af ""aresample=48000,asetpts=PTS-STARTPTS""");
                arr.Add($@"-c:a aac");
                arr.Add($@"-q:a 1");
            }
            else
            {
                arr.Add($@"-c:a aac");
            }
            arr.Add($@"-c:v libx264");
            arr.Add($@"-b:v {ffprobeOutput.MainData.NewDetail.bitrate / 1000}k");
            arr.Add($@"-pass 2");
            arr.Add($@"""{ffprobeOutput.MainData.OutFile}""");
            arr.Add($@"-y");
            arr.Add($@"-threads {threads}");
            if (form.AutoTrimToolStripMenuItem.Checked)
            {
                arr.Add($@"-fflags +genpts");
                arr.Add($@"-avoid_negative_ts make_zero");
                arr.Add($@"-movflags +faststart");
            }
            arr.Add($@"-progress pipe:1");
            arr.Add($@"-nostats");
            arr.Add($@"-loglevel error");
            return [.. arr];
        }

        public string[] XtwopassMerge(FfprobeOutput ffprobeOutput)
        {
            List<string> arr = [];

            string threads = form.coreCBox.SelectedItem?.ToString() ?? "0";

            arr.Add($@"-f concat");
            arr.Add($@"-safe 0");
            arr.Add($@"-i {ffprobeOutput.MainData.avsTempFile}.merge");
            if (form.AutoTrimToolStripMenuItem.Checked)
            {
                arr.Add($@"-ar 48000");
                arr.Add($@"-ac 2");
                arr.Add($@"-af ""aresample=48000,asetpts=PTS-STARTPTS""");
                arr.Add($@"-c:a aac");
                arr.Add($@"-q:a 1");
            }
            else
            {
                arr.Add($@"-c:a aac");
            }
            arr.Add($@"-c:v libx264");
            arr.Add($@"-b:v {ffprobeOutput.MainData.NewDetail.bitrate / 1000}k");
            arr.Add($@"-pass 2");
            arr.Add($@"""{ffprobeOutput.MainData.OutFile}""");
            arr.Add($@"-y");
            arr.Add($@"-threads {threads}");
            if (form.AutoTrimToolStripMenuItem.Checked)
            {
                arr.Add($@"-fflags +genpts");
                arr.Add($@"-avoid_negative_ts make_zero");
                arr.Add($@"-movflags +faststart");
            }
            arr.Add($@"-progress pipe:1");
            arr.Add($@"-nostats");
            arr.Add($@"-loglevel error");
            return [.. arr];
        }
    }
}
