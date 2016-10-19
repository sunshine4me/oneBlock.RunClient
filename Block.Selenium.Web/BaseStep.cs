using Block.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Block.Selenium.Web {
    public class BaseStep {
        
        //为了避免重名 使用两个下划线区分
        public string __describe { get; set; }

        #region 属性
        
        public string id { get; set; }
        public string name { get; set; }
        public string text { get; set; }
        public string className { get; set; }
        public string tagName { get; set; }
        public int index { get; set; }
        public int sleepTime { get; set; }
        public string xpath { get; set; }
        

        #endregion 属性

        public RunFactory runFactory;


        public OneBlockStep oneStep { get; set; }

        /// <summary>
        /// step启动执行
        /// </summary>
        public virtual void Excite() {
        
            Thread.Sleep(this.sleepTime * 1000);
            oneStep.result.screenshot = runFactory.snapshot();
            oneStep.result.statusCode = StepResultCode.sucess;
            oneStep.result.message = "执行成功";


        }

    

    }
}
