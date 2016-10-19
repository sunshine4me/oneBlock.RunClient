using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Block.Selenium.Web {
    public class SwitchToFrameStep : BaseStep {


        public string frame { get; set; }

        public override void Excite() {
            
            if(string.IsNullOrEmpty(frame))
                runFactory.driver.SwitchTo().DefaultContent();
            else
                runFactory.driver.SwitchTo().Frame(frame);
            base.Excite();
        }



    }


}
