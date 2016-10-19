using Block.Interface;
using log4net;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace RunClient.mission {
    public class runTestCase {

        public IRow row { get; set; }


        public Dictionary<string, string> attrs { get; set; }

        public string caseName { get; set; }
        


        public ICaseResult result { get; set; }

        

    }
}
