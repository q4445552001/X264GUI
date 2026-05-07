using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using X264GUIv2.Models;

namespace X264GUIv2
{
    internal static class OtherControlFunc
    {
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        /// <param name="MessageText"></param>
        public static void ShowError(string MessageText) => MessageBox.Show(MessageText, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public static void listViewCheck(Form1 form, List<FfprobeOutput> ffprobeOutputs, Action<int> action)
        {
            IList<int> idxs = [];

            IList<ListViewItem> listViews = [.. form.listView1.CheckedItems.Cast<ListViewItem>()];
            foreach (ListViewItem listView in listViews)
            {
                var idx = findFfprobItem(ffprobeOutputs, (Guid?)listView.Tag);
                idxs.Add(idx);
                action.Invoke(idx);

                form.listView1.Items[idx] = VideoFunc.DataViewObject(ffprobeOutputs[idx]);
            }

            foreach (int idx in idxs)
                form.listView1.Items[idx].Checked = true;
        }

        /// <summary>
        /// 取得Enum DisplayName
        /// </summary>
        public static string GetDisplayName(this Enum value)
        {
            var attr = value.GetType().GetMember(value.ToString())
                     .FirstOrDefault()?.GetCustomAttributes<DisplayAttribute>();

            return attr?.FirstOrDefault()?.Name ?? "";
        }

        /// <summary>
        /// 歐幾里得演算法 (GCD)
        /// </summary>
        public static (int, int) GetGCD(int w = 1920, int h = 1080)
        {
            int _w = w;
            int _h = h;

            while (h != 0)
            {
                int temp = h;
                h = w % h;
                w = temp;
            }

            int ratioW = _w / w;
            int ratioH = _h / w;
            return (ratioW, ratioH);
        }

        public static string timeConv(Stopwatch sw)
        {
            TimeSpan Timemint = TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds);
            return string.Format("{0:D2}:{1:D2}:{2:D2}", Timemint.Hours, Timemint.Minutes, Timemint.Seconds);
        }

        public static void openFolder(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            if (File.Exists(path))
                Process.Start("explorer.exe", $@"/select,""{path}""");
            else if (Directory.Exists(path))
                Process.Start("explorer.exe", $@"""{path}""");
        }

        /// <summary>
        /// 寫入log
        /// </summary>
        public static void WriteLog(string Str, Action<string>? action = null)
        {
            try
            {
                var str = $"[{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}] {Str}";

#if DEBUG
                Debug.WriteLine(str);
#endif
                action?.Invoke(str);

                using StreamWriter sw = new($"{AppDomain.CurrentDomain.BaseDirectory}\\{Assembly.GetExecutingAssembly().EntryPoint?.DeclaringType?.Namespace}_log.txt", true);
                sw.WriteLine(str);
                sw.Close();
            }
            catch { }
        }

        public static int findFfprobItem(List<FfprobeOutput> ffprobeOutputs, Guid? guid)
        {
            if (guid == null)
                return -1;

            int idx = ffprobeOutputs.FindIndex(x => x.Guid == guid);
            return idx;
        }

        public static int findListItem(ListView listView, Guid? guid)
        {
            if (guid == null)
                return -1;

            int idx = listView.Items.Cast<ListViewItem>().ToList().FindIndex(x => (Guid?)x.Tag == guid);
            return idx;
        }
    }
}
