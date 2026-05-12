using System.ComponentModel.DataAnnotations;

namespace X264GUIv2.Enums
{
    public enum VideoTypeEnum
    {
        [Display(Name = "一般")]
        Normal,
        [Display(Name = "AVS")]
        Aviscript,
        [Display(Name = "合併")]
        Merge,
    }
}
