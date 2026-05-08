using System.Text;

namespace X264GUIv2
{
    internal static class Program
    {
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

            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}