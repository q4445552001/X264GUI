using System.Text;

namespace X264GUIv2
{
    //TODO: trim 還有問題
    //TODO: 路徑語系錯誤
    //TODO: SQL改反射
    //TODO: 顏色設定改初始化實做
    //TODO: 進度條文字修正(超出範圍元範圍會被引截掉)
    //TODO: 2PASS的進度有問題
    //TODO: 儲存體只有一筆時無法儲存
    //TODO: bitrate Auto時轉換結束後手動會被打開
    //TODO: mkv的aac失效

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using var mutex = new Mutex(true, Application.ProductName, out var isFirstOpen);
            if (!isFirstOpen && MessageBox.Show("程式已開啟", "提示") == DialogResult.OK)
                Environment.Exit(1);

            string[] files = [
                @".\bin\ffmpeg\ffprobe.exe",
                @".\bin\ffmpeg\ffmpeg.exe",
            ];

            foreach (string file in files)
            {
                if (!File.Exists(file))
                {
                    OtherControlFunc.ShowError("缺少主要檔案，強制關閉。");
#if !DEBUG
                Environment.Exit(1);
#endif
                }
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}