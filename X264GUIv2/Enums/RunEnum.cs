using System.ComponentModel.DataAnnotations;

namespace X264GUIv2.Enums
{
    public enum RunEnum
    {
        [Display(Name = "Idel")]
        Idel = 0,

        [Display(Name = "初始化")]
        Init = 1,

        [Display(Name = "音軌分離")]
        SoundSeparation = 2,

        [Display(Name = "音軌處理")]
        SoundProcessing = 3,

        [Display(Name = "音軌修剪")]
        AudioTrim = 4,

        [Display(Name = "OnePass")]
        OnePass = 5,

        [Display(Name = "TwoPass")]
        TwoPass = 6,

        [Display(Name = "合併")]
        Merge = 7,

        [Display(Name = "Hash")]
        Hash = 8,

        [Display(Name = "錯誤")]
        Error = 9,

        [Display(Name = "完成")]
        Done = 10,

        [Display(Name = "停止")]
        Stop = 11,

        [Display(Name = "警告")]
        Warning = 12,
    }
}
