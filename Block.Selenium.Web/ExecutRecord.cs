using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Block.Selenium.Web {
    public class ExecutRecord {
        /// <summary>
        /// 执行状态
        /// </summary>
        public int recordType { get; set; }
        /// <summary>
        /// 结果描述
        /// </summary>
        public string recordMessage { get; set; }
        /// <summary>
        /// 截图
        /// </summary>
        public string screenshot { get; set; }
    }
}
