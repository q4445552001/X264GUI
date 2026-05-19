namespace X264GUIv2.Models
{
    public static class Global
    {
        public static int CodePage { get; set; } = 950;

        /// <summary>
        /// 初始化彼特率
        /// </summary>
        public static readonly int BitRateDefault = 1000000;

        private static string _hashPath = "%temp%";
        /// <summary>
        /// HASH儲存位置
        /// </summary>
        public static string HASHPath
        {
            get
            {
                string path = Environment.ExpandEnvironmentVariables(_hashPath);
                return Directory.Exists(path) ? path : "%temp%";
            }
            set => _hashPath = value;
        }

        #region listview更新頻率限制
        public static readonly int _lastUiUpdateTime = 1000;
        public static DateTime _lastUiUpdate { get; set; } = DateTime.MinValue;
        #endregion
    }
}
