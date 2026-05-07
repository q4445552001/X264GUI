using System.ComponentModel.DataAnnotations.Schema;
using X264GUIv2.Enums;

namespace X264GUIv2.Models
{
    public class FfprobeOutput
    {
        public Guid Guid { get; set; } = Guid.NewGuid();

        [NotMapped]
        public string avsTempFile = $"_avsTemp_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        public bool isAac { get; set; } = false;

        public double duration { get; set; } = 0;

        public int size { get; set; } = 0;

        [NotMapped]
        public Detail NewDetail { get; set; } = new();

        [NotMapped]
        public Detail OriDetail { get; set; } = new();

        public string InFile { get; set; } = "";

        [NotMapped]
        public string InFileName => Path.GetFileName(InFile);

        [NotMapped]
        public string OutFile => $@"{Path.GetDirectoryName(InFile)}\{Path.GetFileNameWithoutExtension(InFile)}-0.mp4";

        public int idx { get; set; } = 0;

        public RunEnum run { get; set; } = RunEnum.Idel;

        [NotMapped]
        public string SubtitlesFile
        {
            get
            {
                if (File.Exists(Path.GetDirectoryName(InFile) + "\\" + Path.GetFileNameWithoutExtension(InFile) + ".ass"))
                    return $@"{Path.GetDirectoryName(InFile)}\{Path.GetFileNameWithoutExtension(InFile)}.ass";
                else
                    return "";
            }
        }

        public class Detail
        {
            public int bitrate { get; set; } = 0;

            public FrameModeEnum frameMode { get; set; } = FrameModeEnum.CBR;

            public string frameStr { get; set; } = "24000/1001";

            [NotMapped]
            public decimal fpsnum => decimal.TryParse(frameStr.Split("/")[0], out decimal _fps) ? _fps : 1;

            [NotMapped]
            public decimal fpsden => decimal.TryParse(frameStr.Split("/")[1], out decimal _frames) ? _frames : 1;

            [NotMapped]
            public decimal frameRate => fpsnum / fpsden;

            public int? resolutionW { get; set; } = 1920;

            public int? resolutionH { get; set; } = 1080;

            [NotMapped]
            public string resolution => $"{resolutionW}x{resolutionH}";

            //DB
            public Guid Guid { get; set; }
            public int? isNew { get; set; }
        }
    }
}
