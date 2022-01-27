using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StuRemotePty.Client
{
    public class ConsoInputContentLinux : IConsoInputContent
    {
        private const string LibSystem = "libShowKey.so";

        [DllImport(LibSystem)]
        static extern int InitRead();

        [DllImport(LibSystem)]
        static extern char ReadChar();

        public int Init() => InitRead();
        public char Read() => ReadChar();
    }
}
