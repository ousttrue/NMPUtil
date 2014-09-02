using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NMPUtil.MsgPack.Rpc
{
    public class Request
    {
        public UInt32 ID
        {
            get;
            set;
        }

        public String Method
        {
            get;
            set;
        }

        public Object[] Params
        {
            get;
            set;
        }

        [MsgPackPacker]
        static public void Packer(MsgPackPacker p, Object o)
        {
            var v = (Request)o;
            p.Pack_Array(4);
            p.Pack((Byte)0);
            p.Pack(v.ID);
            p.Pack(v.Method);
            p.Pack(v.Params);
        }
    }
}
