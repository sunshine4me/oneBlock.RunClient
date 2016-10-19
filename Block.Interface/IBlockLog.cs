using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Block.Interface {
    public interface IBlockLog {
        void Error(string msg);
        void Info(string msg);
        void Warn(string msg);
        void Debug(string msg);

    }
}
