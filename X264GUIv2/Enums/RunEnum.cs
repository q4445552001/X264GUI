using System.ComponentModel.DataAnnotations;

namespace X264GUIv2.Enums
{
    public enum RunEnum
    {
        [Display(Name = "初始化")]
        Init,

        [Display(Name = "音軌分離")]
        SoundSeparation,

        [Display(Name = "音軌處理")]
        SoundProcessing,

        [Display(Name = "音軌修剪")]
        AudioTrim,

        [Display(Name = "OnePass")]
        OnePass,

        [Display(Name = "TwoPass")]
        TwoPass,

        [Display(Name = "合併")]
        Merge,

        [Display(Name = "錯誤")]
        Error,

        [Display(Name = "完成")]
        Done,

        [Display(Name = "停止")]
        Stop,
    }
}
