using System.Runtime.CompilerServices;

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

        #region 剩餘時間

        /// <summary>
        /// 目標進度
        /// </summary>
        public static double DoneTotle { get; set; } = 0;

        /// <summary>
        /// 已完成進度
        /// </summary>
        public static double DoneCount { get; set; } = 0;

        /// <summary>
        /// 上次進度
        /// </summary>
        private static double DoneRemainingTotle { get; set; } = 0d;

        /// <summary>
        /// 剩餘時間
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DoneRemaining(double now, double dur, CustomStopwatch sw1)
        {
            if (now <= 0)
                return DoneRemainingTotle;

            // 已完成總影片秒數
            double completed = DoneCount + (now * (dur / 100));
            //System.Diagnostics.Debug.WriteLine($"已完成總影片秒數: {completed} = {DoneCount} + ({now} * ({dur} / 100))");
            if (completed <= 0)
                return DoneRemainingTotle;

            // 每秒真實時間可處理多少影片秒數
            double speed = completed / sw1.Elapsed.TotalSeconds;
            //System.Diagnostics.Debug.WriteLine($"每秒真實時間可處理多少影片秒數: {speed} = {completed} / {sw1.Elapsed.TotalSeconds}");
            if (speed <= 0)
                return DoneRemainingTotle;

            // 剩餘影片秒數
            double remaining = DoneTotle - completed;
            //System.Diagnostics.Debug.WriteLine($"剩餘影片秒數: {remaining} = {DoneTotle} - {completed}");
            if (remaining <= 0)
                return 0;

            DoneRemainingTotle = remaining / speed;
            //System.Diagnostics.Debug.WriteLine($"剩餘秒數: {DoneRemainingTotle} = {remaining} / {speed}");

            return DoneRemainingTotle;
        }

        /// <summary>
        /// EstimationResult
        /// </summary>
        /// <param name="SpeedFactor">轉檔倍速 (例如 1.5x)</param>
        /// <param name="RemainingTime">預估剩餘時間</param>
        //private record EstimationResult(double SpeedFactor, TimeSpan RemainingTime);

        #endregion

        #region listview更新頻率限制
        public static readonly int _lastUiUpdateTime = 1000;
        public static DateTime _lastUiUpdate { get; set; } = DateTime.MinValue;
        #endregion
    }
}
