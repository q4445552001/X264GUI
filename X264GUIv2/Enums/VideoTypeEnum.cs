using System.ComponentModel.DataAnnotations;

namespace X264GUIv2.Enums
{
    public enum VideoTypeEnum
    {
        [Display(Name = "{0}")]
        Normal,
        [Display(Name = "Aviscript")]
        Aviscript,
        [Display(Name = "影片合併")]
        Merge,
    }
}
