namespace X264GUIv2.Models
{
    public static class Global
    {
        public static int CodePage { get; set; } = 950;

        #region listview更新頻率限制
        public static readonly int _lastUiUpdateTime = 1000;
        public static DateTime _lastUiUpdate { get; set; } = DateTime.MinValue;
        #endregion
    }
}
