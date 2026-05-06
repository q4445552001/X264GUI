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
                int idx = ffprobeOutputs.FindIndex(x => x.Guid == (Guid?)listView.Tag);
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
    }
}
