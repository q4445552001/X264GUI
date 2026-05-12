using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using X264GUIv2.Enums;
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

        public static List<FfprobeOutput> SortIdx(ListView listView, List<FfprobeOutput> ffprobeOutputs)
        {
            for (int i = 0; i < ffprobeOutputs.Count; i++)
            {
                var idx = listView.findListItem(ffprobeOutputs[i].MainData.Guid);
                ffprobeOutputs[i].MainData.idx = idx;
            }

            return ffprobeOutputs;
        }

        public static void listViewCheck(ListView listView, List<FfprobeOutput> ffprobeOutputs, Action<int> action)
        {
            IList<int> idxs = [];

            IList<ListViewItem> listViews = [.. listView.CheckedItems.Cast<ListViewItem>()];
            foreach (ListViewItem item in listViews)
            {
                var idx = findFfprobItem(ffprobeOutputs, (Guid?)item.Tag);
                idxs.Add(idx);
                action.Invoke(idx);

                listView.Items[ffprobeOutputs[idx].MainData.idx] = listView.DataViewObject(ffprobeOutputs[idx]);
            }

            foreach (int idx in idxs)
                listView.Items[ffprobeOutputs[idx].MainData.idx].Checked = true;
        }

        public static DetailsItem DataViewText(FfprobeOutput ffprobeOutput)
        {
            DetailsItem detailsItem = new();

            string OldCapacity = Math.Round(Convert.ToDouble(ffprobeOutput.MainData.size) / 1024 / 1024, 2).ToString(); //原始大小
            double Audio_capacity = Convert.ToDouble(ffprobeOutput.MainData.size) - (Convert.ToDouble(ffprobeOutput.MainData.OriDetail.bitrate) * Convert.ToDouble(ffprobeOutput.MainData.duration) / 8); //計算Audio大小
            string NewCapacity = Math.Round((Audio_capacity + Convert.ToDouble(ffprobeOutput.MainData.NewDetail.bitrate) * ffprobeOutput.MainData.duration / 8) / 1024 / 1024, 2).ToString() + " MB"; //Video預估大小

            detailsItem.FileName = ffprobeOutput.MainData.InFileName;
            detailsItem.BitRate = $"{(ffprobeOutput.MainData.OriDetail.bitrate == 0 ? "NUL" : ffprobeOutput.MainData.OriDetail.bitrate / 1000)} > {ffprobeOutput.MainData.NewDetail.bitrate / 1000} kb/s";
            detailsItem.FpsMode = $"{(ffprobeOutput.MainData.videoType == VideoTypeEnum.Aviscript ? "NUL" : ffprobeOutput.MainData.OriDetail.frameMode)} > {FrameModeEnum.CBR}";
            detailsItem.Fps = $"{Math.Round(ffprobeOutput.MainData.OriDetail.frameRate, 3)} > {Math.Round(ffprobeOutput.MainData.NewDetail.frameRate, 3)}";
            detailsItem.Resolution = $"{ffprobeOutput.MainData.OriDetail.resolution} > {ffprobeOutput.MainData.NewDetail.resolution}";
            detailsItem.Duration = TimeSpan.FromSeconds(ffprobeOutput.MainData.duration).ToString(@"hh\:mm\:ss");
            detailsItem.Size = $"{(OldCapacity == "0" ? "NUL" : OldCapacity)} > {NewCapacity}";
            detailsItem.Progress = "00.00 %";
            detailsItem.Status = ffprobeOutput.MainData.run.GetDisplayName();
            detailsItem.Time = "00:00:00";
            detailsItem.Path = ffprobeOutput.MainData.InFile ?? "";
            detailsItem.VideoType = ffprobeOutput.MainData.videoType.GetDisplayName();

            detailsItem.Text = Path.GetFileName(ffprobeOutput.MainData.InFile) +
                    $"\nBitRate: {(ffprobeOutput.MainData.OriDetail.bitrate == 0 ? "NUL" : ffprobeOutput.MainData.OriDetail.bitrate / 1000)} kb/s" +
                    $"\nFPS模式: {Enum.GetName(ffprobeOutput.MainData.OriDetail.frameMode)}" +
                    $"\nFPS: {Math.Round(ffprobeOutput.MainData.OriDetail.frameRate, 3)}" +
                    $"\n解析度: {ffprobeOutput.MainData.OriDetail.resolution}" +
                    $"\n檔案大小: {(OldCapacity == "0" ? "NUL" : OldCapacity)} MB";

            return detailsItem;
        }

        public static ListViewItem DataViewObject(this ListView listView, FfprobeOutput ffprobeOutput)
        {
            List<string> row = [];
            DetailsItem detailsItem = DataViewText(ffprobeOutput);

            foreach (ColumnHeader item in listView.Columns.Cast<ColumnHeader>())
            {
                if (item.Name == null)
                    continue;
                row.Add(typeof(DetailsItem).GetProperty(item.Name)?.GetValue(detailsItem) as string ?? string.Empty);
            }

            ListViewItem lis = new([.. row])
            {
                Tag = ffprobeOutput.MainData.Guid,
                ToolTipText = detailsItem.Text,
                UseItemStyleForSubItems = false,
            };

            return lis;
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
        public static (int, int) GetGCD(float w = 1920, float h = 1080)
        {
            float _w = w;
            float _h = h;

            while (h != 0)
            {
                float temp = h;
                h = w % h;
                w = temp;
            }

            float ratioW = _w / w;
            float ratioH = _h / w;
            return ((int)ratioW, (int)ratioH);
        }

        public static int FixEven(int value) => value % 2 == 0 ? value : value + 1;

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

        public static int findFfprobItem(List<FfprobeOutput> ffprobeOutputs, Guid? guid)
        {
            if (guid == null)
                return -1;

            int idx = ffprobeOutputs.FindIndex(x => x.MainData.Guid == guid);
            return idx;
        }

        public static int findListItem(this ListView listView, Guid? guid)
        {
            if (guid == null)
                return -1;

            int idx = listView.Items.Cast<ListViewItem>().ToList().FindIndex(x => (Guid?)x.Tag == guid);
            return idx;
        }

        public static int findSubitemIdx(this ListView listView, string name)
        {
            int idx = listView.Columns.Cast<ColumnHeader>().Select((x, idx) => new { name = x.Name, idx }).FirstOrDefault(x => x.name == name)?.idx ?? -1;
            return idx;
        }
    }
}
