using System.Diagnostics;
using System.Reflection;

namespace X264GUIv2
{
    internal static class WriteFile
    {
        public static string WritePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Assembly.GetExecutingAssembly().EntryPoint?.DeclaringType?.Namespace}_log.txt";

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
    }
}
