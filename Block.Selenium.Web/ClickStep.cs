using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Block.Selenium.Web {
    public class ClickStep : BaseStep {


        public override void Excite() {
            IWebElement element = runFactory.findElement(this);

            if (element != null) {
                element.Click();
            } else {
                oneStep.result.screenshot = runFactory.snapshot();
                oneStep.result.statusCode = Interface.StepResultCode.notfound;
                oneStep.result.message = "找不到控件";
                return;
            }

            base.Excite();
        }



    }


}
