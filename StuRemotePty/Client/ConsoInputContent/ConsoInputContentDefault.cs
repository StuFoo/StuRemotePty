using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StuRemotePty.Client
{
    public class ConsoInputContentDefault : IConsoInputContent
    {
        public int Init() => 0;

        public char Read() => Console.ReadKey(true).KeyChar;

        public int CloseRead() => 0;
    }
}
