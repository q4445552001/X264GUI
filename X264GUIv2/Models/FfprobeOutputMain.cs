using System.ComponentModel.DataAnnotations.Schema;
using X264GUIv2.Enums;

namespace X264GUIv2.Models
{
    public class FfprobeOutputMain
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Guid? MergeGuid { get; set; }

        public string InFile { get; set; } = "";

        [NotMapped]
        public string InFileName => Path.GetFileName(InFile);

        [NotMapped]
        public string InFilePath => $@"{Path.GetDirectoryName(InFile) ?? "."}";

        [NotMapped]
        public string avsTempFile = $"_avsTemp_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        [NotMapped]
        public string OutFile => $@"{InFilePath}\{Path.GetFileNameWithoutExtension(InFile)}-0.mp4";

        public bool isAac { get; set; } = false;

        public int audioMap { get; set; } = 0;

        [NotMapped]
        private VideoTypeEnum? _videoType { get; set; }

        public VideoTypeEnum videoType
        {
            get
            {
                if (_videoType == null)
                    if (Path.GetExtension(InFile).Equals($".{VideoExt.avs}", StringComparison.CurrentCultureIgnoreCase))
                        return VideoTypeEnum.Aviscript;

                return _videoType ?? VideoTypeEnum.Normal;
            }
            set => _videoType = value;
        }

        public double duration { get; set; } = 0;

        public int videoSize { get; set; } = 0;
        public int audioSize { get; set; } = 0;

        public int idx { get; set; } = 0;

        public RunEnum run { get; set; } = RunEnum.Idel;

        [NotMapped]
        public FfprobeOutputDetail NewDetail { get; set; } = new();

        [NotMapped]
        public FfprobeOutputDetail OriDetail { get; set; } = new();

        [NotMapped]
        public string SubtitlesFile
        {
            get
            {
                if (File.Exists(InFilePath + "\\" + Path.GetFileNameWithoutExtension(InFile) + ".ass"))
                    return $@"{InFilePath}\{Path.GetFileNameWithoutExtension(InFile)}.ass";
                else
                    return string.Empty;
            }
        }
    }

    public class FfprobeOutputDetail
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

    public class FfprobeOutput : IEquatable<FfprobeOutput>
    {
        public FfprobeOutputMain MainData { get; set; } = new();
        public List<FfprobeOutputMain>? MergeData { get; set; }

        public override int GetHashCode() => HashCode.Combine(MainData.Guid);
        public bool Equals(FfprobeOutput? other) => Equals(other);
        public override bool Equals(object? obj) => Equals(obj as FfprobeOutput);
    }
}
