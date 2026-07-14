namespace X264GUIv2
{
    public static class Version
    {
        public static string Hash { get; set; } = "無版控資料";
        public static string Time { get; set; } = "無版控資料";

        [AttributeUsage(AttributeTargets.Assembly)]
        public class HashAttribute : Attribute
        {
            public HashAttribute(string v) => Hash = string.IsNullOrWhiteSpace(v) ? Hash : v;
        }

        [AttributeUsage(AttributeTargets.Assembly)]
        public class TimeAttribute : Attribute
        {
            public TimeAttribute(string v) => Time = string.IsNullOrWhiteSpace(v) ? Time : v;
        }
    }
}
