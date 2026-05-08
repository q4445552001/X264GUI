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
            int count = value / max * 100;

            TaskbarManager.Instance.SetProgressValue(count, 100);
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
