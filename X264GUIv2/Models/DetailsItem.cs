using X264GUIv2.Enums;

namespace X264GUIv2.Models
{
    internal class DetailsItem
    {
        public string FileName { get; set; } = string.Empty;
        public string BitRate { get; set; } = string.Empty;
        public string FpsMode { get; set; } = string.Empty;
        public string Fps { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Progress => StatusEnum == RunEnum.Done ? "100 %" : "0.00 %";
        public string Status => StatusEnum.GetDisplayName();
        public string Time { get; set; } = "00:00:00";
        public string Path { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string VideoType { get; set; } = string.Empty;

        public RunEnum StatusEnum { get; set; } = RunEnum.Idel;
    }
}
