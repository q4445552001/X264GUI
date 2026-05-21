using System.Diagnostics;
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
        /// 單位
        /// </summary>
        public static double DoneRemainingUnit { get; set; } = 1;

        /// <summary>
        /// 上次進度
        /// </summary>
        public static double DoneRemainingTotle { get; set; } = 0d;

        /// <summary>
        /// 剩餘時間
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DoneRemaining(double now, Stopwatch sw)
        {
#pragma warning disable IDE0054 // 使用複合指派

            if (now > 0)
            {
                double elapsedSeconds = sw.ElapsedTicks / (double)Stopwatch.Frequency;
                DoneRemainingTotle = ((DoneTotle * 100d) - ((now * DoneRemainingUnit) + (100d * DoneCount))) * elapsedSeconds;
                DoneRemainingTotle = DoneRemainingTotle / ((now * DoneRemainingUnit) + (100d * DoneCount));
            }
            return DoneRemainingTotle;

#pragma warning restore IDE0054 // 使用複合指派
        }

        #endregion

        #region listview更新頻率限制
        public static readonly int _lastUiUpdateTime = 1000;
        public static DateTime _lastUiUpdate { get; set; } = DateTime.MinValue;
        #endregion
    }
}
