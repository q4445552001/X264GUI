using System.Diagnostics;
using X264GUIv2.Enums;
using X264GUIv2.Models;

namespace X264GUIv2
{
    public class Form1Control(Form1 form)
    {
        public void bitrateCBoxControl(bool isClose)
        {
            if (!isClose)
            {
                form.bitrateNumeric.Enabled = false;
                return;
            }

            if (!int.TryParse((form.bitrateCBox.SelectedItem as ComboboxItem)?.Value, out int v))
                return;

            BitrateEnum bitrateEnum = (BitrateEnum)v;
            form.bitrateNumeric.Enabled = bitrateEnum == BitrateEnum.Manual;
        }

        public void ffmpegOutput(FfprobeOutput ffprobeOutput, string sr, Stopwatch sw1, Stopwatch sw2, WeighAllot weighAllot)
        {
            form.timeStripStatus.Text = OtherControlFunc.timeConv(sw1);

            int str = sr.IndexOf('=');
            if (str <= 0)
                return;

            string key = sr[..str];
            string value = sr[(str + 1)..];

            if (key == "out_time_ms" && double.TryParse(value, out double outTimeMs))
            {
                if (!OtherControlFunc.listViewIsRefresh())
                    return;

                form.listView1.Items[form.useIdx].SubItems[form.subTimeIdx]!.Text = OtherControlFunc.timeConv(sw2);

                double currentSeconds = outTimeMs / 1_000_000.0;
                double pro = currentSeconds / ffprobeOutput.MainData.duration * 100.0;
                form.listView1.Items[form.useIdx].SubItems[form.subProgressIdx]!.Text = $"{pro:F1} %";
                calculateProgres(ffprobeOutput, (float)pro, weighAllot);
            }
        }

        public void avs4x26xOutput(FfprobeOutput ffprobeOutput, string sr, Stopwatch sw1, Stopwatch sw2, WeighAllot weighAllot)
        {
            form.timeStripStatus.Text = OtherControlFunc.timeConv(sw1);

            if (sr.Contains("[error]"))
            {
                ffprobeOutput.MainData.run = RunEnum.Error;
                throw new Exception(sr);
            }

            if (!OtherControlFunc.listViewIsRefresh())
                return;

            form.listView1.Items[form.useIdx].SubItems[form.subTimeIdx]!.Text = OtherControlFunc.timeConv(sw2);
            if (sr.IndexOf("frames,") != -1 && sr.IndexOf('[') != -1)
            {
                double prodata = Math.Round(Convert.ToDouble(sr.Substring(sr.IndexOf('[') + 1, sr.LastIndexOf('%') - 1)), 2);
                form.listView1.Items[form.useIdx].SubItems[form.subProgressIdx]!.Text = $"{prodata:F1} %";
                calculateProgres(ffprobeOutput, (float)prodata, weighAllot);
            }
        }

        public int AudioCalculate(FfprobeOutput ffprobeOutput)
        {
            AudioHz audioHz = AudioHz.Default;
            if (form.kHzDefaultToolStripMenuItem.Checked)
            {
                audioHz = form.kHzDefaultToolStripMenuItem.getAudio_khz();
            }
            else if (form.kHz441ToolStripMenuItem.Checked)
            {
                audioHz = form.kHz441ToolStripMenuItem.getAudio_khz();
            }
            else if (form.kHz480ToolStripMenuItem.Checked)
            {
                audioHz = form.kHz480ToolStripMenuItem.getAudio_khz();
            }

            if (audioHz == AudioHz.Default && ffprobeOutput.MainData.audioMap > 0)
            {
                string[] audioSamplineRate = ffprobeOutput.MainData.AudioSamplineRate.Split("/");
                if (audioSamplineRate.Length == 2 && int.TryParse(audioSamplineRate[1], out int rate))
                    return rate;
            }

            return (int)audioHz;
        }

        public void calculateProgres(FfprobeOutput ffprobeOutput, float pro, WeighAllot weighAllot)
        {
            //WeighAllot weighAllot = new(
            //    ffprobeOutput.MainData.isLocalEncode &&
            //    ffprobeOutput.MainData.videoType == VideoTypeEnum.Normal &&
            //    ffprobeOutput.MainData.audioMap > 0 && form.AutoTrimToolStripMenuItem.Checked
            //);

            float audioWeight = ffprobeOutput.MainData.videoType switch
            {
                VideoTypeEnum.Normal or VideoTypeEnum.Aviscript => weighAllot.weightAudio,
                VideoTypeEnum.Merge => weighAllot.weightAudio / 2,
                _ => 0f
            };

            float mergeWeight = 0f;
            if (ffprobeOutput.MainData.videoType == VideoTypeEnum.Merge || !ffprobeOutput.MainData.isLocalEncode)
                mergeWeight = weighAllot.weightMerge / 2;

            float completedWeight = 0f;
            float currentWeight = 0f;

            if (!ffprobeOutput.MainData.isLocalEncode)
            {
                currentWeight += weighAllot.weightAudio / 2f;
                currentWeight += weighAllot.weightMerge / 2f;
            }

            switch (ffprobeOutput.MainData.run)
            {
                case RunEnum.AudioTrim:
                    completedWeight = mergeWeight;
                    currentWeight += audioWeight;
                    break;

                case RunEnum.OnePass:
                    if (!ffprobeOutput.MainData.isLocalEncode)
                        completedWeight = 0f;
                    else
                        completedWeight = audioWeight + mergeWeight;
                    currentWeight += weighAllot.weightOnePass;
                    break;

                case RunEnum.TwoPass:
                    completedWeight = audioWeight + weighAllot.weightOnePass + mergeWeight;
                    currentWeight += weighAllot.weightTwoPass;
                    break;

                case RunEnum.Merge:
                    completedWeight = audioWeight + weighAllot.weightOnePass + weighAllot.weightTwoPass + mergeWeight;
                    currentWeight += weighAllot.weightMerge;
                    break;
            }

            float totalProgress = (completedWeight * 100f) + (currentWeight * pro);
            UpdateProgres(totalProgress, 100);
        }

        public void btnControl(bool isClose)
        {
            form.stopBtn.Enabled = !isClose;
            form.runBtn.Enabled = isClose;
            form.upBtn.Enabled = isClose;
            form.dnBtn.Enabled = isClose;
            form.bitrateCBox.Enabled = isClose;
            bitrateCBoxControl(isClose);
            form.fpsCBox.Enabled = isClose;
            form.resolutionCBox.Enabled = isClose;
            form.coreCBox.Enabled = isClose;

            form.addToolStripMenuItem.Enabled = isClose;
            form.diffToolStripMenuItem.Enabled = isClose;
            form.clearToolStripMenuItem.Enabled = isClose;
            form.dbLoadToolStripMenuItem.Enabled = isClose;
            form.dbClearToolStripMenuItem.Enabled = isClose;
            form.dbSaveToolStripMenuItem.Enabled = isClose;
            form.AutoTrimToolStripMenuItem.Enabled = isClose;
            form.loadAvsToolStripMenuItem.Enabled = isClose;
            form.createMergeToolStripMenuItem.Enabled = isClose;
            form.kHzToolStripMenuItem.Enabled = isClose;
            form.HASHToolStripMenuItem.Enabled = isClose;
            form.HASHPathToolStripMenuItem.Enabled = isClose;

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
            if (now > 100 || now < 0)
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
        public void UpdateProgresLoop(string str, CancellationTokenSource cts)
        {
            int spinnerIndex = 0;
            char[] spinnerChars = ['|', '/', '-', '\\'];

            void del()
            {
                Graphics BarGraphics = form.progressBar1.CreateGraphics();
                form.progressBar1.PerformStep();
                char spinner = spinnerChars[spinnerIndex];
                spinnerIndex = (spinnerIndex + 1) % spinnerChars.Length;
                Font font = new("Consolas", 12, FontStyle.Bold);
                string showStr = $"{spinner} {str}...";
                PointF pt = new(form.progressBar1.Width / 2 - (showStr.Length * 4), form.progressBar1.Height / 2 - 10);
                form.progressBar1.Value = 100;
                BarGraphics.DrawString(showStr, font, Brushes.White, pt);
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
