using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Block.Selenium.Web {
    public class InitStep : BaseStep {
        public string url { get; set; }

        public string driverName { get; set; }

        public int implicitlyWait { get; set; }


        public override void Excite() {

            if (string.IsNullOrEmpty(driverName))
                driverName = "chrome";


            switch (driverName.ToLower()) {
                case "ie":
                    IEInit();
                    break;
                default:
                    ChromeInit();
                    break;
            }

       


            
        }


        private void ChromeInit() {
            string chromeDir = System.Environment.CurrentDirectory + "\\Resources\\";
            string chrome = chromeDir + "chromedriver.exe";
            ChromeDriverService cds = ChromeDriverService.CreateDefaultService(chromeDir);


            ChromeOptions co = new ChromeOptions();
            co.AddArgument("test-type");
            co.AddArgument("start-maximized");

            ChromeDriver driver;


            if (File.Exists(chrome)) {
                driver = new ChromeDriver(cds, co);
            } else {
                driver = new ChromeDriver(co);
            }

            runFactory.driver = driver;
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(implicitlyWait));//等待超时
            driver.Navigate().GoToUrl(url);


            runFactory.snapshot();

            base.Excite();
        }

        private void IEInit() {
            string driverDir = System.Environment.CurrentDirectory + "\\Resources\\";

            string driverPath = driverDir + "IEDriverServer.exe";


            InternetExplorerDriverService cds = InternetExplorerDriverService.CreateDefaultService(driverDir);


            InternetExplorerOptions co = new InternetExplorerOptions();
            co.IntroduceInstabilityByIgnoringProtectedModeSettings = true;
            

            InternetExplorerDriver driver;



            if (File.Exists(driverPath)) {
                driver = new InternetExplorerDriver(cds, co);
            } else {
                driver = new InternetExplorerDriver(co);
            }

            runFactory.driver = driver;
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(implicitlyWait));//等待超时
            driver.Navigate().GoToUrl(url);
          

            runFactory.snapshot();

            base.Excite();

        }

    }
}

