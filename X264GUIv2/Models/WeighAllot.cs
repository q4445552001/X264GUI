namespace X264GUIv2.Models
{
    /// <summary>
    /// 進度分配設定
    /// </summary>
    public class WeighAllot(bool isTrim)
    {
        public float weightAudio => isTrim ? .10f : 0f;
        public float weightOnePass => (.96f - weightAudio) / 2;
        public float weightTwoPass => weightOnePass;
        public float weightMerge => 1f - (weightAudio + weightOnePass + weightTwoPass);
    }
}
