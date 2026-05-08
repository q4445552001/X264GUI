using System.Diagnostics;
using X264GUIv2.Enums;
using X264GUIv2.Models;

namespace X264GUIv2
{
    public class Form1Control(Form1 form)
    {
        public void bitrateCBoxControl()
        {
            if (!int.TryParse((form.bitrateCBox.SelectedItem as ComboboxItem)?.Value, out int v))
                return;

            BitrateEnum bitrateEnum = (BitrateEnum)v;
            form.bitrateNumeric.Enabled = bitrateEnum == BitrateEnum.Manual;
        }

        public void ffmpegOutput(FfprobeOutput ffprobeOutput, string sr, Stopwatch sw1, Stopwatch sw2)
        {
            form.listView1.Items[form.useIdx].SubItems[9].Text = OtherControlFunc.timeConv(sw2);
            form.timeStripStatus.Text = OtherControlFunc.timeConv(sw1);
            int str = sr.IndexOf('=');

            if (str <= 0)
                return;

            string key = sr[..str];
            string value = sr[(str + 1)..];

            if (key == "out_time_ms" && double.TryParse(value, out double outTimeMs))
            {
                double currentSeconds = outTimeMs / 1_000_000.0;
                double pro = currentSeconds / ffprobeOutput.duration * 100.0;
                float weightPass = 1f;
                float weight = 0f;

                if (ffprobeOutput.run == RunEnum.OnePass)
                    weightPass = form.weightOnePass;
                else if (ffprobeOutput.run == RunEnum.TwoPass)
                {
                    weightPass = form.weightOnePass;
                    weight = (float)form.weightOnePass * 100;
                }

                if (ffprobeOutput.videoType == VideoTypeEnum.Aviscript && form.AutoTrimToolStripMenuItem.Checked)
                    weight += (float)form.weightAudio * 100;

                double proA;
                if (ffprobeOutput.videoType != VideoTypeEnum.Aviscript && ffprobeOutput.run == RunEnum.AudioTrim)
                    proA = pro * form.weightAudio;
                else
                    proA = (pro * weightPass) + weight;

                form.listView1.Items[form.useIdx].SubItems[7].Text = $"{pro:F1} %";
                UpdateProgres((float)proA, 100);
            }
        }

        public void btnControl(bool isClose)
        {
            form.stopBtn.Enabled = !isClose;
            form.runBtn.Enabled = isClose;
            form.addBtn.Enabled = isClose;
            form.diffBtn.Enabled = isClose;
            form.bitrateCBox.Enabled = isClose;
            bitrateCBoxControl();
            form.fpsCBox.Enabled = isClose;
            form.resolutionCBox.Enabled = isClose;
            form.coreCBox.Enabled = isClose;

            form.addToolStripMenuItem.Enabled = isClose;
            form.diffToolStripMenuItem.Enabled = isClose;
            form.clearToolStripMenuItem.Enabled = isClose;
            form.dbLoadToolStripMenuItem.Enabled = isClose;
            form.dbClearToolStripMenuItem.Enabled = isClose;
            form.AutoTrimToolStripMenuItem.Enabled = isClose;
            form.loadAvsToolStripMenuItem.Enabled = isClose;
            form.createMergeToolStripMenuItem.Enabled = isClose;

            form.listDiffViewItem.Enabled = isClose;
            form.listRestViewItem.Enabled = isClose;
            form.listUpViewItem.Enabled = isClose;
            form.listDnViewItem.Enabled = isClose;
        }

        #region 進度條
        /// <summary>
        /// 進度條
        /// </summary>
        public void UpdateProgres(float now, float count, bool isPercentage = true)
        {
            if (now > 100)
                return;

            void del()
            {
                Graphics BarGraphics = form.progressBar1.CreateGraphics();
                form.progressBar1.PerformStep();
                float v = now / count * 100;
                string str = isPercentage ? Math.Round(v, 2).ToString("#0.00") + " %" : $"{now:#,##0}/{count:#,##0}";
                Font font = new("Consolas", 12, FontStyle.Bold);
                PointF pt = new(form.progressBar1.Width / 2 - (str.Length * 4), form.progressBar1.Height / 2 - 10);
                form.progressBar1.Value = v >= 100 ? 100 : (int)v;
                BarGraphics.DrawString(str, font, v >= 50 ? Brushes.White : Brushes.Blue, pt);
            }
            form.Invoke(del);
        }

        /// <summary>
        /// 進度條轉圈圈
        /// </summary>
        public void UpdateProgresLoop(CancellationTokenSource cts)
        {
            int spinnerIndex = 0;
            char[] spinnerChars = ['|', '/', '-', '\\'];

            void del()
            {
                Graphics BarGraphics = form.progressBar1.CreateGraphics();
                form.progressBar1.PerformStep();
                int spinner = spinnerChars[spinnerIndex];
                spinnerIndex = (spinnerIndex + 1) % spinnerChars.Length;
                Font font = new("Consolas", 12, FontStyle.Bold);
                PointF pt = new(form.progressBar1.Width / 2 - 20, form.progressBar1.Height / 2 - 10);
                form.progressBar1.Value = 100;
                BarGraphics.DrawString($"{spinner} Loading...", font, Brushes.White, pt);
            }

            while (true)
            {
                if (cts.Token.IsCancellationRequested)
                {
                    form.Invoke(form.progressBar1.PerformStep);
                    return;
                }

                form.Invoke(del);
                Thread.Sleep(500);
            }
        }
        #endregion
    }
}
