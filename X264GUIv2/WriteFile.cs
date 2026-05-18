using System.Diagnostics;
using System.Reflection;
using X264GUIv2.Models;

namespace X264GUIv2
{
    internal static class WriteFile
    {
        public static string WritePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Assembly.GetExecutingAssembly().EntryPoint?.DeclaringType?.Namespace}_{DateTime.Now:yyyyMMdd}_log.txt";

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

                using StreamWriter sw = new(WritePath, true);
                sw.WriteLine(str);
                sw.Close();
            }
            catch { }
        }

        /// <summary>
        /// 寫入csv
        /// </summary>
        public static void WriteHashCsv(string str, FfprobeOutput ffprobeOutput)
        {
            try
            {
                str = str.Replace("\t", ",");
                string filePath = $@"{ffprobeOutput.MainData.InFilePath}\_hash_{DateTime.Now:yyyyMMdd}.csv";
                using StreamWriter sw = new(filePath, true);
                sw.WriteLine(str);
                sw.Close();
            }
            catch
            {
                WriteLog($"[csv寫入失敗] {str}");
                throw;
            }
        }
    }
}
