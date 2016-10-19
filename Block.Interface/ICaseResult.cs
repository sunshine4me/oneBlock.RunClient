using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Block.Interface {
    public interface ICaseResult {


        string message { get; set; }

        CaseResultCode statusCode { get; set; }

        /// <summary>
        /// 结果目录
        /// </summary>
        string resultPath { get; set; }
    }
    public enum CaseResultCode {
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
        warn = 3,
        /// <summary>
        /// 未知错误
        /// </summary>
        fatal = 4
    }
}
