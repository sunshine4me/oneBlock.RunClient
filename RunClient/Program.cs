using Block.Interface;
using log4net;
using System;
using System.IO;

namespace RunClient {
    class Program {

        static string dic;
        static void Main(string[] args) {

            //Uri.IsWellFormedUriString(@"fill///D:/github/seleniumBlock/seleniumBlock/bin/Debug/Result/2016-10-13/20161013110558", UriKind.RelativeOrAbsolute);



            var logger = LogManager.GetLogger(typeof(Program));
            //测试代码
            //string[] arrayA = { @"C:\Users\ASUS\Desktop\数据场景.xlsx", @"http://localhost:5000" };
            //args = arrayA;




            if (args.Length > 1) {
                dic = $"{Environment.CurrentDirectory}\\Run\\{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(dic)) {
                    Directory.CreateDirectory(dic);
                }
                logger.Info("run模式启动");
                Uri uri = new Uri(args[1]);

                try {
                    missionRun(args[0], uri, dic);
                } catch (Exception e) {
                    logger.Error($"{e.Message} \r\n{e.StackTrace}");
                }
                logger.Debug($"run模式执行完成!");
            } else {
                blockServer bs = new blockServer();
                dic = $"{Environment.CurrentDirectory}\\Debug";
                bs.debugEvent += startRun;
                logger.Info("Debug模式启动");
                bs.StartListener(8500);
                
            }

        }

        private static ICaseResult startRun(OneBlockCase tc) {

  
          
            //执行结果存放文件夹
            string resultPath = Path.Combine(dic, DateTime.Now.ToString("yyyyMMddhhmmss"));
            if (!Directory.Exists(resultPath)) {
                Directory.CreateDirectory(resultPath);
            }

            var runFactory = new Block.Selenium.Web.RunFactory();
            return runFactory.startRun(tc, resultPath);
        }
        private static void missionRun(string excelFile, Uri url, string dic) {

            mission.mission msr = new mission.mission();
            msr.excelFile = excelFile;
            msr.url = url;
            msr.dic = dic;
            msr.Run();
        }

        
    }
}
