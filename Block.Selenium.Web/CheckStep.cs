
using OpenQA.Selenium;

namespace Block.Selenium.Web {
    public class CheckStep : BaseStep {



        public override void Excite() {
            IWebElement element = runFactory.findElement(this);
            if (element == null) {

                oneStep.result.screenshot = runFactory.snapshot();
                oneStep.result.statusCode = Interface.StepResultCode.notfound;
                oneStep.result.message = "找不到控件";
                return;
            }
            runFactory.snapshot();

            base.Excite();
        }
    }

}
