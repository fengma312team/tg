using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    /// <summary>
    /// 基础赛事信息
    /// </summary>
    public class Match
    {
        /// <summary>
        /// 赛事ID
        /// </summary>
        public string gameid { get; set; }
        /// <summary>
        /// 应该是唯一标记(不确定)
        /// </summary>
        public string ga12 { get; set; }
        /// <summary>
        /// 比赛时间
        /// </summary>
        public string gametime { get; set; }
        /// <summary>
        /// 所属联赛
        /// </summary>
        public string alliance { get; set; }
        /// <summary>
        /// 球队双方
        /// </summary>
        public string team { get; set; }
    }
}
