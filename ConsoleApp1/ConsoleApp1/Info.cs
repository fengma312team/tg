using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    /// <summary>
    /// 详细信息
    /// </summary>
    public class Info
    {
        /// <summary>
        /// 赛事
        /// </summary>
        public Match match { get; set; }
        /// <summary>
        /// 类型:全场波胆或半场波胆
        /// </summary>
        public string types { get; set; }
        /// <summary>
        /// 得分 几比几
        /// </summary>
        public string score { get; set; }
        /// <summary>
        /// 赔率
        /// </summary>
        public decimal odds { get; set; }

    }
}
