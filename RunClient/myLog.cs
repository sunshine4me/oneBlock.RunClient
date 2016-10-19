using Block.Interface;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunClient {
    public class myLog : IBlockLog {
        ILog logger;
        public myLog() {
            logger = LogManager.GetLogger(typeof(Program));
        }

        public void Debug(string msg) {
           
            logger.Debug(msg);
        }

        public void Error(string msg) {
            logger.Error(msg);
        }

        public void Info(string msg) {
            logger.Info(msg);
        }

        public void Warn(string msg) {
            logger.Warn(msg);
        }
    }
}
