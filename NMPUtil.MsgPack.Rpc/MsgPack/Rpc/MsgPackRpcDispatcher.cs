using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMPUtil.MsgPack.Rpc
{
    public class MsgPackRpcDispatcher
    {
        public void Process(System.IO.Stream s, Byte[] bytes)
        {
        }

        public delegate void RpcFunc(MsgPackPacker result, SubMsgPackUnpacker args, UInt32 count);

        public void RegisterFunc(String name, RpcFunc func)
        {

        }
    }
}
