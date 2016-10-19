using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Block.Interface {
    public class OneBlockCase {

        public List<OneBlockStep> steps { get; set; }
    }

    public class OneBlockStep {
        public string name { get; set; }

        public string describe { get; set; }
        public Dictionary<string, string> attrs { get; set; }

        public StepResult result { get; set; }
    }




    public class StepResult {
        public string message { get; set; }

        public StepResultCode statusCode { get; set; }
        /// <summary>
        /// 截图
        /// </summary>
        public string screenshot { get; set; }
    }

    public enum StepResultCode {
        /// <summary>
        /// 未执行
        /// </summary>
        noRun = 0,
        /// <summary>
        /// 执行成功
        /// </summary>
        sucess = 1,
        /// <summary>
        /// 失败
        /// </summary>
        fail = 2,
        /// <summary>
        /// 警告
        /// </summary>
        notfound = 3,
        /// <summary>
        /// 未知错误
        /// </summary>
        fatal = 4
    }
}
