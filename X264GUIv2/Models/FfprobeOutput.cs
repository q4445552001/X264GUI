using X264GUIv2.Enums;

namespace X264GUIv2.Models
{
    internal class FfprobeOutput
    {
        public Guid Guid = new();
        public bool isAcc { get; set; } = false;
        public double duration { get; set; } = 0;
        public int size { get; set; } = 0;
        public Detail NewDetail { get; set; } = new();
        public Detail OriDetail { get; set; } = new();
        public string InFile { get; set; } = "";
        public string InFileName => Path.GetFileName(InFile);
        public string OutFile => $"{Path.GetDirectoryName(InFile)!.Replace("\\", "//")}//{Path.GetFileNameWithoutExtension(InFile)}-0.mp4";
        public string SubtitlesFile
        {
            get
            {
                if (File.Exists(Path.GetDirectoryName(InFile) + "\\" + Path.GetFileNameWithoutExtension(InFile) + ".ass"))
                    return $"{Path.GetDirectoryName(InFile)?.Replace("\\", "//")}//{Path.GetFileNameWithoutExtension(InFile).Replace("\\", "//")}.ass";
                else
                    return "";
            }
        }

        internal class Detail
        {
            public int bitrate { get; set; } = 0;
            public FrameModeEnum frameMode { get; set; } = FrameModeEnum.CBR;
            public string frameStr { get; set; } = "24000/1001";
            public decimal fpsnum => decimal.TryParse(frameStr.Split("/")[0], out decimal _fps) ? _fps : 1;
            public decimal fpsden => decimal.TryParse(frameStr.Split("/")[1], out decimal _frames) ? _frames : 1;
            public decimal frameRate => fpsnum / fpsden;
            public int? resolutionW { get; set; } = 1920;
            public int? resolutionH { get; set; } = 1080;
            public string resolution => $"{resolutionW}x{resolutionH}";
        }
    }
}
