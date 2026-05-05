using System.Diagnostics;
using System.Text;

namespace X264GUIv2
{
    internal class TaskHelper
    {
        /// <summary>
        /// 檔案名稱
        /// </summary>
        public required string FileName { get; set; }

        /// <summary>
        /// 參數
        /// </summary>
        public IList<string> ArgumentList { get; set; } = [];

        /// <summary>
        /// 預設路徑
        /// </summary>
        public string? RunPath { get; set; }

        /// <summary>
        /// 是否等待
        /// </summary>
        public bool isWait { get; set; } = false;

        /// <summary>
        /// 輸出委派
        /// </summary>
        public Action<string>? ActionOut { get; set; }

        /// <summary>
        /// 輸出委派
        /// </summary>
        public Action<string>? ActionErr { get; set; }

        public required CancellationTokenSource Cts { get; set; }

        public ProcessWindowStyle WindowStyle { get; set; } = ProcessWindowStyle.Hidden;

        /// <summary>
        /// 執行程序
        /// </summary>
        protected internal bool RunTask()
        {
            if (!string.IsNullOrWhiteSpace(RunPath))
            {
                if (!Directory.Exists(RunPath))
                    throw new Exception($"{RunPath} 無效路徑");

                Environment.CurrentDirectory = RunPath;
            }

            if (!File.Exists(FileName ?? ""))
                throw new Exception($"{FileName} 無效路徑");

            string argument = ArgumentList.Count > 0 ? string.Join(" ", ArgumentList).Replace("^", "").Replace("\r\n", " ") : "";

            var p = new Process()
            {
                StartInfo =
                {
                    FileName = FileName,
                    CreateNoWindow = true, //是否要在新視窗中啟動處理序。
                    WindowStyle = WindowStyle, //取得或設定視窗狀態，用於啟動處理序時。
                    UseShellExecute = false, //是否要使用作業系統 Shell 來啟動處理序。
                    RedirectStandardInput = true, //應用程式的輸入是否從 StandardInput 資料流讀取。
                    RedirectStandardError = true, //應用程式的錯誤輸出是否寫入至 StandardError 資料流。
                    RedirectStandardOutput = true, //應用程式的文字輸出是否寫入至 StandardOutput 資料流。
                    WorkingDirectory = Environment.CurrentDirectory, //當 UseShellExecute 屬性為 false 時，取得或設定要啟動之處理序的工作目錄。 當 UseShellExecute 為 true 時，取得或設定包含要啟動之處理序的目錄。
                    Arguments = argument, //參數
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardInputEncoding = new UTF8Encoding(false),
                }
            };

            p.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Debug.WriteLine(e.Data);
                    ActionOut?.Invoke(e.Data);
                }
            };

            p.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Debug.WriteLine(e.Data);
                    ActionErr?.Invoke(e.Data);
                }
            };

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            if (isWait)
                p.WaitForExit();

            do
            {
                if (!p.HasExited)
                {
                    if (Cts.Token.IsCancellationRequested)
                        p.Kill();
                }
            }
            while (!p.WaitForExit(1000));

            Environment.CurrentDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? throw new Exception("路徑失敗");

            return p.ExitCode == 0;
        }
    }
}
