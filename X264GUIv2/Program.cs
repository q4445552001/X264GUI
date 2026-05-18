using System.Text;
using X264GUIv2.Models;

namespace X264GUIv2
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
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

            #region args
            var codePage = args.Where(x => x.IndexOf("code") > -1).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(codePage))
            {
                var dt = codePage.Split('=');
                if (dt.Length == 2 && int.TryParse(dt[1], out int _codePage))
                    Global.CodePage = _codePage;
            }
            #endregion

            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}