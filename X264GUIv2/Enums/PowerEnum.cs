using System.ComponentModel.DataAnnotations;

namespace X264GUIv2.Enums
{
    internal enum PowerEnum
    {
        /// <summary>
        /// 停用
        /// </summary>
        [Display(Name = "停用")]
        Stop = 0,

        /// <summary>
        /// 休眠 (h)
        /// </summary>
        [Display(Name = "休眠")]
        Hibernate = 1,

        /// <summary>
        /// 睡眠 
        /// </summary>
        [Display(Name = "睡眠")]
        Sleep = 2,

        /// <summary>
        /// 登出 (l)
        /// </summary>
        [Display(Name = "登出")]
        Out = 3,

        /// <summary>
        /// 關機 (s)
        /// </summary>
        [Display(Name = "關機")]
        Off = 4,
    }
}
