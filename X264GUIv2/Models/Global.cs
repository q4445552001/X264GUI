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

        /// <summary>
        /// 已處理的總時間
        /// </summary>
        public static double TotleTimeConsuming { get; set; } = 0;

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
        /// 上次剩餘時間
        /// </summary>
        public static double DoneRemainingTotle { get; set; } = 0d;

        /// <summary>
        /// 上次完成進度
        /// </summary>
        private static double LastCompleted { get; set; } = 0d;

        /// <summary>
        /// 上次計算時間
        /// </summary>
        private static double LastElapsedSeconds { get; set; } = 0d;

        /// <summary>
        /// 平滑速度
        /// </summary>
        private static double SmoothSpeed { get; set; } = 0d;

        /// <summary>
        /// 剩餘時間
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DoneRemaining(double now, double dur, Stopwatch sw)
        {
            if (now <= 0 || dur <= 0)
                return DoneRemainingTotle;

            double elapsed = sw.Elapsed.TotalSeconds;

            if (elapsed <= 0)
                return DoneRemainingTotle;

            // 目前已完成工作量
            double completed =
                DoneCount + (now * dur / 100.0);

            if (completed <= 0)
                return DoneRemainingTotle;

            // 第一次執行，只記錄基準
            if (LastElapsedSeconds <= 0)
            {
                LastCompleted = completed;
                LastElapsedSeconds = elapsed;

                return DoneRemainingTotle;
            }

            // 本次增加的工作量
            double deltaCompleted = completed - LastCompleted;

            // 本次實際經過時間
            double deltaTime = elapsed - LastElapsedSeconds;

            if (deltaCompleted <= 0 || deltaTime <= 0)
                return DoneRemainingTotle;

            // 瞬間速度
            double instantSpeed = deltaCompleted / deltaTime;

            if (instantSpeed <= 0)
                return DoneRemainingTotle;

            // 更新基準
            LastCompleted = completed;
            LastElapsedSeconds = elapsed;

            // ==========================================
            // 速度平滑
            // 新速度佔 30%，舊速度佔 70%
            // ==========================================
            if (SmoothSpeed <= 0)
                SmoothSpeed = instantSpeed;
            else
                SmoothSpeed = (SmoothSpeed * 0.7) + (instantSpeed * 0.3);

            // 總工作量
            // 例如影片 3600 秒 → 7200 工作量
            double remaining = DoneTotle - completed;

            if (remaining <= 0)
            {
                DoneRemainingTotle = 0;
                return 0;
            }

            // 預估剩餘時間
            DoneRemainingTotle = remaining / SmoothSpeed;

            return DoneRemainingTotle;
        }

        #endregion

        #region listview更新頻率限制
        public static readonly int _lastUiUpdateTime = 1000;
        public static DateTime _lastUiUpdate { get; set; } = DateTime.MinValue;
        #endregion
    }
}
