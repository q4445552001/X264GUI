using Microsoft.WindowsAPICodePack.Taskbar;

namespace X264GUIv2
{
    public static class TaskbarProgress
    {
        public static void Set(int value, int max)
        {
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);

            if (value > max)
                value = max;
            float count = (float)value / max * 100f;

            //TaskbarManager.Instance.SetProgressValue((int)Math.Round(count, 0), 100);
            TaskbarManager.Instance.SetProgressValue(value, max);
        }

        public static void Error()
        {
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Error);
        }

        public static void Clear()
        {
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
        }
    }
}
