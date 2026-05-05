using System.ComponentModel.DataAnnotations;

namespace X264GUIv2.Enums
{
    internal enum RunEnum
    {
        [Display(Name = "初始化")]
        Init,

        [Display(Name = "音效分離")]
        SoundSeparation,

        [Display(Name = "音效處理")]
        SoundProcessing,

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
