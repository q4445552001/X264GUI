using System.Text;

namespace X264GUIv2
{
    //TODO: 新增AUDIO TRIM
    //TODO: 新增SQL LITE
    //TODO: Ass無警告模式

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!File.Exists(@".\bin\ffmpeg\ffprobe.exe"))
            {
                OtherControlFunc.ShowError("缺少主要檔案，強制關閉。");
#if !DEBUG
                Environment.Exit(1);
#endif
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}