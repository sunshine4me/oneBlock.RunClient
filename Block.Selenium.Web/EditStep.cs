using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Block.Selenium.Web {
    public class EditStep : BaseStep {
        public string inputText { get; set; }

        public bool pressEnter { get; set; }



        public override void Excite() {
            IWebElement element = runFactory.findElement(this);
            if (element != null) {
                element.Clear();
                element.SendKeys(this.inputText);

                if (pressEnter) {
                    Actions builder = new Actions(runFactory.driver);
                    builder.SendKeys(Keys.Enter).Perform();
                }

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
