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
