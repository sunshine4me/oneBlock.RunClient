
using Block.Interface;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Block.Selenium.Web {
    /// <summary>
    /// 案例执行工厂
    /// </summary>
    public class RunFactory : IRunFactory {

        public IBlockLog log { get; set; }

        public RunFactory() {
            log = new myLog();
        }




        public RemoteWebDriver driver { get; set; }

        /// <summary>
        /// 执行结果存放文件夹
        /// </summary>
        public string resultPath { get; private set; }

        

        /// <summary>
        /// 查找控件
        /// </summary>
        public IWebElement findElement(BaseStep ts) {
            
            IWebElement we = null;
            String xpath = ts.xpath;
            if (string.IsNullOrEmpty(xpath)) {
                xpath = this.getNativeXpath(ts);
            }
            if (ts.index > 0)
                we = driver.FindElementsByXPath(xpath)[ts.index - 1];
            else
                we = driver.FindElementByXPath(xpath);

            //将界面移动到element上
            //第一中办法
            //((IJavaScriptExecutor)ch).ExecuteScript("arguments[0].scrollIntoView();", we);

            //第二中办法
            //int y = we.Location.Y;
            //String js = String.Format("window.scroll(0, {0})", y / 2);
            //((IJavaScriptExecutor)ch).ExecuteScript(js);


            //第三种 
            Actions action = new Actions(driver);
            action.MoveToElement(we).Perform();

            return we;
        }

        public string getNativeXpath(BaseStep step) {
            StringBuilder xp = new StringBuilder();
            if (string.IsNullOrEmpty(step.tagName)) {
                xp.Append("//*");
            } else {
                xp.Append($"//{step.tagName}");
            }

            if (!string.IsNullOrEmpty(step.className)) {
                xp.Append($"[@class='{step.className}']");
            }

            if (!string.IsNullOrEmpty(step.id)) {
                xp.Append($"[@id='{step.id}']");
            }

            if (!string.IsNullOrEmpty(step.name)) {
                xp.Append($"[@name='{step.name}']");
            }

            if (!string.IsNullOrEmpty(step.text)) {
                xp.Append($"[text()='{step.text}']");
            }

            return xp.ToString();

        }

        /// <summary>
        /// 截图
        /// </summary>
        public string snapshot() {
            var fileName = Path.Combine(resultPath, $"{ DateTime.Now.ToString("yyyyMMddhhmmss")}.jpg");
            return snapshot(fileName);
        }

        /// <summary>
        /// 截图
        /// </summary>
        public string snapshot(string fileName) {
            try {
                driver.GetScreenshot().SaveAsFile(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            } catch (Exception e) {
                log.Error($"{e.Message} \r\n{e.StackTrace}");
            }
            return fileName;
        }

      

        /// <summary>
        /// 杀掉相关进程并释放所有资源(为了艾泽拉斯)
        /// </summary>
        public void killThemAll() {
            //chromeDriver 关闭
            try {
                driver.Quit();
            } catch {
                //todo
            }
        }

        



        /// <summary>
        /// 创建step对象
        /// </summary>
        private BaseStep greateStep(OneBlockStep _step) {
            BaseStep tmp = null;

            try {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Object[] parameters = new Object[1];
                parameters[0] = _step;
                tmp = (BaseStep)assembly.CreateInstance($"Block.Selenium.Web.{_step.name}", true);
                if (tmp != null) {
                    tmp.__describe = _step.describe;
                    tmp.oneStep = _step;
                    tmp.oneStep.result = new StepResult();
                    assignPro(tmp, _step.attrs);
                    tmp.runFactory = this;
                }
            } catch (Exception e) {
                log.Warn($"{e.Message} \r\n{e.StackTrace}");
            }
            return tmp;
        }

        /// <summary>
        /// 赋值
        /// </summary>
        private void assignPro(BaseStep _basestep, Dictionary<string, string> attrs) {
            Type type = _basestep.GetType();
            var properties = type.GetProperties();
            foreach (var attr in attrs) {
                var pi = properties.FirstOrDefault(x => x.Name.ToLower() == attr.Key.ToLower());
                if (pi != null) {
                    //防止int bool 等参数在做空值转换时的错误
                    if (pi.PropertyType != typeof(string) && string.IsNullOrEmpty(attr.Value))
                        continue;
                    try {
                        pi.SetValue(_basestep, Convert.ChangeType(attr.Value, pi.PropertyType), null);
                    } catch (Exception e) {
                        log.Warn($"{e.Message} \r\n{e.StackTrace}");
                    }

                }

            }
        }

        public ICaseResult startRun(OneBlockCase _case, string path) {



            this.resultPath = path;

            List<BaseStep> runSteps = new List<BaseStep>();

            log.Debug("Step数据初始化...");
            foreach (var st in _case.steps) {
                var tmp = this.greateStep(st);
                if (tmp != null)
                    runSteps.Add(tmp);
            }
            log.Debug($"Step数据初始化完成,数量:{runSteps.Count}");


            log.Info($"开始执行,结果文件路径:{resultPath}.");



            int i = 0;
            foreach (var step in runSteps) {
                i++;
                try {
                    log.Debug($"Step.{i}[{step}:{step.__describe}]: 开始执行...");
                    step.Excite();
                    saveResult(_case);
                    log.Info($"Step.{i}[{step}:{step.__describe}]: 执行完成,状态:{step.oneStep.result.statusCode}!");
                    if (step.oneStep.result.statusCode != StepResultCode.sucess) break;
                } catch (Exception e) {
                    log.Error($"Step.{i}[{step}:{step.__describe}]: 执行错误!");
                    log.Error($"{e.Message} \r\n{e.StackTrace}");

                    step.oneStep.result.screenshot = this.snapshot();
                    step.oneStep.result.statusCode = StepResultCode.fatal;
                    step.oneStep.result.message = e.StackTrace;
                    break;//结束执行
                }
            }
            log.Info($"执行完成,结果文件路径:{resultPath}.");
            log.Debug("开始释放相关资源.");
            this.killThemAll();

            var result = new CaseResult();
            result.resultPath = resultPath;
            if (runSteps.Count(t => t.oneStep.result.statusCode == StepResultCode.sucess) == runSteps.Count) {
                result.statusCode = CaseResultCode.sucess;
                result.message = "完成";
            } else {
                result.statusCode = CaseResultCode.fail;
                result.message = "失败";
            }

            return result;
        }

        /// <summary>
        /// 保存结果文件
        /// </summary>
        private void saveResult(OneBlockCase _case) {

            using (FileStream fs3 = new FileStream(resultPath + "\\result.json", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)) {
                using (StreamWriter sw = new StreamWriter(fs3)) {
                    var sss = JsonConvert.SerializeObject(_case);
                    sw.WriteLine(sss);
                }
            }
        }
    }


    public class CaseResult : ICaseResult {
        public string message { get; set; }

        public CaseResultCode statusCode { get; set; }

        /// <summary>
        /// 结果目录
        /// </summary>
        public string resultPath { get; set; }
    }


    public class myLog : IBlockLog {
        public void Debug(string msg) {
            Console.WriteLine($"[debug] {msg}");
        }

        public void Error(string msg) {
            Console.WriteLine($"[error] {msg}");
        }

        public void Info(string msg) {
            Console.WriteLine($"[info] {msg}");
        }

        public void Warn(string msg) {
            Console.WriteLine($"[warn] {msg}");
        }
    }
}
