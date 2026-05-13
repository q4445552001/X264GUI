using X264GUIv2.Enums;

namespace X264GUIv2.Models
{
    public class LoadFile
    {
        public required string File { get; set; }
        public int index { get; set; } = 0;

        private VideoTypeEnum? _videoType { get; set; }

        public VideoTypeEnum videoType
        {
            get
            {
                if (_videoType == null)
                    if (Path.GetExtension(File).Equals($".{VideoExt.avs}", StringComparison.CurrentCultureIgnoreCase))
                        return VideoTypeEnum.Aviscript;

                return _videoType ?? VideoTypeEnum.Normal;
            }
            set => _videoType = value;
        }
    }
}
