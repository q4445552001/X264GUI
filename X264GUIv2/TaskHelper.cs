using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace X264GUIv2
{
    public class TaskHelper
    {
        public required CancellationTokenSource Cts { get; set; }

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
        /// 自動關閉錯誤視窗
        /// </summary>
        public string AutoCloseDialogBox { get; set; } = string.Empty;

        /// <summary>
        /// 輸出委派
        /// </summary>
        public Action<string>? ActionOut { get; set; }

        /// <summary>
        /// 輸出委派
        /// </summary>
        public Action<string>? ActionErr { get; set; }

        public ProcessWindowStyle WindowStyle { get; set; } = ProcessWindowStyle.Hidden;

        private bool isClose { get; set; } = false;
        public string isCloseMsg { get; private set; } = string.Empty;

        /// <summary>
        /// 執行程序
        /// </summary>
        protected internal int RunTask()
        {
            if (!string.IsNullOrWhiteSpace(RunPath))
            {
                if (!Directory.Exists(RunPath))
                    throw new Exception($"{RunPath} 無效路徑");

                Environment.CurrentDirectory = RunPath;
            }

            if (!File.Exists(FileName ?? ""))
                throw new Exception($"{FileName} 無效路徑");

            string argument = ArgumentList.Count > 0 ? string.Join(" ", ArgumentList).Replace("\r\n", " ") : "";
#if DEBUG
            //Debug.WriteLine($@"""{FileName}"" {argument}");
            WriteFile.WriteLog($@"""{FileName}"" {argument}");
#endif

            Process p = new()
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
                    StandardInputEncoding = Encoding.UTF8,
                }
            };

            AutoResetEvent outputWaitHandle = new(false);
            AutoResetEvent errorWaitHandle = new(false);

            p.OutputDataReceived += (s, e) =>
            {
                if (e.Data == null)
                {
                    outputWaitHandle.Set();
                    return;
                }

                if (string.IsNullOrWhiteSpace(e.Data))
                    return;

#if DEBUG
                //Debug.WriteLine($"Out: {e.Data}");
#endif

                try
                {
                    ActionOut?.Invoke(e.Data);
                }
                catch (Exception ex)
                {
                    WriteFile.WriteLog(ex.Message);
                }
            };

            p.ErrorDataReceived += (s, e) =>
            {
                if (e.Data == null)
                {
                    errorWaitHandle.Set();
                    return;
                }

                if (string.IsNullOrWhiteSpace(e.Data))
                    return;

#if DEBUG
                //Debug.WriteLine($"Err: {e.Data}");
#endif

                try
                {
                    ActionErr?.Invoke(e.Data);
                }
                catch (Exception ex)
                {
                    WriteFile.WriteLog(ex.Message);
                }
            };

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            closeDialogBox(p);

            do
            {
                if (!p.HasExited)
                {
                    if (Cts.Token.IsCancellationRequested || isClose)
                    {
                        p.Kill();
                    }
                }
            }
            while (!p.WaitForExit(500));

            outputWaitHandle.WaitOne();
            errorWaitHandle.WaitOne();
            p.WaitForExit();

            Environment.CurrentDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? throw new Exception("路徑失敗");

            return p.ExitCode;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        const uint WM_CLOSE = 0x0010;

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private void closeDialogBox(Process p)
        {
            if (!string.IsNullOrWhiteSpace(AutoCloseDialogBox))
            {
                _ = Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    var hWnd = FindWindow(null, AutoCloseDialogBox);
                    _ = GetWindowThreadProcessId(hWnd, out uint pid);
                    if (p.Id == pid && hWnd != IntPtr.Zero)
                    {
                        SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                        isClose = true;
                        isCloseMsg = $"{FileName} 檢查到 {AutoCloseDialogBox}，自動關閉";
                    }
                });
            }
        }
    }
}
