using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StuRemotePty.Client
{
    public interface IConsoInputContent
    {
        int Init();
        char Read();
        int CloseRead();
    }
}
