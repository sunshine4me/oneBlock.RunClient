using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Block.Interface {
    public interface IRunFactory {

        IBlockLog log { get; set; }

        ICaseResult startRun(OneBlockCase _case,string path);
    }


    
}
