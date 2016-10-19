using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunClient.mission {
    public class testCase {

        public List<runTestCase> runCases;
        
       

        public IRow configRow { get; set; }

        public string name { get; set; }
        public int ID { get; set; }

        
    }
}
