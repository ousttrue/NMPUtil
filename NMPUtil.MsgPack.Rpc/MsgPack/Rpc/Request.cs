using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NMPUtil.MsgPack.Rpc
{
    public class Request
    {
        public Byte TypeId
        {
            get { return 0; }
        }
        UInt32 id;
        String Method;
        Object[] Params;
    }
}
