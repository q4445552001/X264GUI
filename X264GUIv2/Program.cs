using System.Text;

namespace X264GUIv2
{
    //TODO: trim 還有問題
    //TODO: 路徑語系錯誤

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string[] files = [@".\bin\ffmpeg\ffprobe.exe", @".\bin\ffmpeg\ffmpeg.exe"];

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