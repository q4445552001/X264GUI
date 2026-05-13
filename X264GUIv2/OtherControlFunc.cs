using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using X264GUIv2.Enums;
using X264GUIv2.Models;
using static System.Windows.Forms.ListView;

namespace X264GUIv2
{
    internal static class OtherControlFunc
    {
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        /// <param name="MessageText"></param>
        public static void ShowError(string MessageText) => MessageBox.Show(MessageText, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public static List<FfprobeOutput> SortIdx(this ListView listView, List<FfprobeOutput> ffprobeOutputs)
        {
            for (int i = 0; i < ffprobeOutputs.Count; i++)
            {
                var idx = listView.findListItem(ffprobeOutputs[i].MainData.Guid);
                ffprobeOutputs[i].MainData.idx = idx;
            }

            return ffprobeOutputs;
        }

        public static void listViewCheck(this ListView listView, List<FfprobeOutput> ffprobeOutputs, Action<int> action)
        {
            IList<int> idxs = [];

            IList<ListViewItem> listViews = [.. listView.CheckedItems.Cast<ListViewItem>()];
            foreach (ListViewItem item in listViews)
            {
                var idx = ffprobeOutputs.findFfprobItem((Guid?)item.Tag);
                idxs.Add(idx);
                action.Invoke(idx);

                listView.Items[ffprobeOutputs[idx].MainData.idx] = listView.DataViewObject(ffprobeOutputs[idx].MainData);
            }

            foreach (int idx in idxs)
                listView.Items[ffprobeOutputs[idx].MainData.idx].Checked = true;
        }

        public static DetailsItem DataViewText(FfprobeOutputMain FfprobeOutputMain)
        {
            DetailsItem detailsItem = new();

            string OldCapacity = Math.Round(Convert.ToDouble(FfprobeOutputMain.videoSize) / 1024.0 / 1024.0, 2).ToString(); //原始大小
            double audioCapacity = Math.Round(Convert.ToDouble(FfprobeOutputMain.audioSize) / 1024.0 / 1024.0, 2); //計算Audio大小
            string NewCapacity = Math.Round((audioCapacity + Convert.ToDouble(FfprobeOutputMain.NewDetail.bitrate) * FfprobeOutputMain.duration / 8) / 1024 / 1024, 2).ToString() + " MB"; //Video預估大小

            detailsItem.FileName = FfprobeOutputMain.InFileName;
            detailsItem.BitRate = $"{(FfprobeOutputMain.OriDetail.bitrate == 0 ? "NUL" : FfprobeOutputMain.OriDetail.bitrate / 1000)} > {FfprobeOutputMain.NewDetail.bitrate / 1000} kb/s";
            detailsItem.FpsMode = $"{(FfprobeOutputMain.videoType == VideoTypeEnum.Aviscript ? "NUL" : FfprobeOutputMain.OriDetail.frameMode)} > {FrameModeEnum.CBR}";
            detailsItem.Fps = $"{Math.Round(FfprobeOutputMain.OriDetail.frameRate, 3)} > {Math.Round(FfprobeOutputMain.NewDetail.frameRate, 3)}";
            detailsItem.Resolution = $"{FfprobeOutputMain.OriDetail.resolution} > {FfprobeOutputMain.NewDetail.resolution}";
            detailsItem.Duration = TimeSpan.FromSeconds(FfprobeOutputMain.duration).ToString(@"hh\:mm\:ss");
            detailsItem.Size = $"{(OldCapacity == "0" ? "NUL" : OldCapacity)} > {NewCapacity}";
            detailsItem.Progress = "00.00 %";
            detailsItem.Status = FfprobeOutputMain.run.GetDisplayName();
            detailsItem.Time = "00:00:00";
            detailsItem.Path = FfprobeOutputMain.InFile ?? "";

            string videoType = $"{FfprobeOutputMain.videoCodeName}/{Path.GetExtension(FfprobeOutputMain.InFileName).Replace(".", "")}";
            detailsItem.VideoType = string.Format(FfprobeOutputMain.videoType.GetDisplayName(), videoType.ToUpper());

            detailsItem.Text = Path.GetFileName(FfprobeOutputMain.InFile) +
                    $"\nBitRate: {(FfprobeOutputMain.OriDetail.bitrate == 0 ? "NUL" : FfprobeOutputMain.OriDetail.bitrate / 1000)} kb/s" +
                    $"\nFPS模式: {Enum.GetName(FfprobeOutputMain.OriDetail.frameMode)}" +
                    $"\nFPS: {Math.Round(FfprobeOutputMain.OriDetail.frameRate, 3)}" +
                    $"\n解析度: {FfprobeOutputMain.OriDetail.resolution}" +
                    $"\n檔案大小: {(OldCapacity == "0" ? "NUL" : OldCapacity)} MB";

            return detailsItem;
        }

        private static readonly Dictionary<string, Func<DetailsItem, string>> _map = new()
        {
            ["FileName"] = x => x.FileName,
            ["BitRate"] = x => x.BitRate,
            ["FpsMode"] = x => x.FpsMode,
            ["Fps"] = x => x.Fps,
            ["Resolution"] = x => x.Resolution,
            ["Duration"] = x => x.Duration,
            ["Size"] = x => x.Size,
            ["Progress"] = x => x.Progress,
            ["Status"] = x => x.Status,
            ["Time"] = x => x.Time,
            ["Path"] = x => x.Path,
            ["Text"] = x => x.Text,
            ["VideoType"] = x => x.VideoType,
        };
        public static ListViewItem DataViewObject(this ListView listView, FfprobeOutputMain ffprobeOutputMain)
        {
            List<string> row = [];
            DetailsItem detailsItem = DataViewText(ffprobeOutputMain);

            foreach (ColumnHeader item in listView.Columns)
            {
                if (string.IsNullOrEmpty(item.Name))
                    continue;

                if (_map.TryGetValue(item.Name, out var getter))
                    row.Add(getter(detailsItem) ?? string.Empty);
                else
                    row.Add(string.Empty);
            }

            ListViewItem lis = new([.. row])
            {
                Tag = ffprobeOutputMain.Guid,
                ToolTipText = detailsItem.Text,
                UseItemStyleForSubItems = false,
            };

            return lis;
        }

        public static void UpItem(this ListView listView)
        {
            foreach (ListViewItem item in listView.SelectedItems)
            {
                if (item.Index > 0)
                {
                    int index = item.Index - 1;
                    listView.Items.RemoveAt(item.Index);
                    listView.Items.Insert(index, item);
                }
            }
        }

        public static void DnItem(this ListView listView)
        {
            for (int i = listView.SelectedItems.Count - 1; i >= 0; i--)
            {
                ListViewItem item = listView.SelectedItems[i];

                if (item.Index < listView.Items.Count - 1)
                {
                    int index = item.Index + 1;

                    listView.Items.RemoveAt(item.Index);
                    listView.Items.Insert(index, item);
                }
            }
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
                path = Path.GetDirectoryName(path);

            //explorer select 會常態占用 shell
            //if (File.Exists(path))
            //    Process.Start("explorer.exe", $@"/select,""{path}""")?.Dispose();
            //else
            if (Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true,
                });
            }
        }

        public static int findFfprobItem(this List<FfprobeOutput> ffprobeOutputs, Guid? guid)
        {
            if (guid == null)
                return -1;

            int idx = ffprobeOutputs.FindIndex(x => x.MainData.Guid == guid);
            return idx;
        }

        public static int findListItem(this ListView listView, Guid? guid)
        {
            ListViewItemCollection items = listView.Items;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Tag is Guid itemGuid && guid is not null && itemGuid == guid.Value)
                    return i;
            }

            return -1;
        }

        public static int findSubitemIdx(this ListView listView, string name)
        {
            ColumnHeaderCollection columns = listView.Columns;

            for (int i = 0; i < columns.Count; i++)
            {
                if (columns[i].Name == name)
                    return i;
            }

            return -1;
        }

        public static bool HasNonLocalCodePageChar(string text)
        {
            Encoding enc = Encoding.GetEncoding(Global.CodePage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);

            try
            {
                enc.GetBytes(text);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
