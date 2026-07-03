using System.Diagnostics;

namespace X264GUIv2
{
    public static class TaskHelperAddons
    {
        public static readonly Dictionary<int, string> ProcessPids = [];

        public static void killProcess(params string[] names)
        {
            foreach (KeyValuePair<int, string> pid in ProcessPids)
            {
                if (!names.Any(n => n == pid.Value)) continue;
                Process pro = Process.GetProcessById(pid.Key);
                pro.Kill();
            }
        }
    }
}
