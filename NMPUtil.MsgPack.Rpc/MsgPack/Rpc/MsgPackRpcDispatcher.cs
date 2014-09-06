using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMPUtil.MsgPack.Rpc
{
    public partial class MsgPackRpcDispatcher
    {
        public MsgPackRpcDispatcher()
        {
           
        }

        public void Process(System.IO.Stream s, ArraySegment<Byte> bytes)
        {
            var unpacker=new MsgPackUnpacker(bytes);
            if (!unpacker.Header.IsArray)
            {
                throw new ArgumentException("is not array");
            }
            if(unpacker.Header.MemberCount!=4)
            {
                throw new ArgumentException("array member is not 4. " + unpacker.Header.MemberCount);
            }
            using (var sub = unpacker.GetSubUnpacker())
            {
                var type = sub.Unpack<Int32>();
                if (type != 0)
                {
                    throw new ArgumentException("is not request. " + type);
                }

                var id = sub.Unpack<UInt32>();

                var key=sub.Unpack<String>();

                if (!sub.Header.IsArray)
                {
                    throw new ArgumentException("parameter is not array !");
                }
                var func = _funcMap[key];

                var ms = new MemoryStream();
                var response = new MsgPackPacker(ms);
                response.Pack_Array(4);
                // response type
                response.Pack((Byte)1);
                // response id
                response.Pack(id);
                // not error
                response.Pack(0);

                using (var args = sub.GetSubUnpacker())
                {
                    func(args, sub.Header.MemberCount, response);
                }

                var data=ms.ToArray();
                s.Write(data, 0, data.Length);
                Console.WriteLine(String.Format("write {0} bytes", data.Length));
            }
        }

        public delegate void RpcCall(MsgPackUnpacker args, UInt32 count, MsgPackPacker result);

        Dictionary<String, RpcCall> _funcMap = new Dictionary<string, RpcCall>();

        public void RegisterFunc<A1, A2, R>(String key, Func<A1, A2, R> func)
        {
            var gmi1 = MsgPackUnpacker.GenericUnpacker.MakeGenericMethod(new[] { typeof(A1) });
            var gmi2 = MsgPackUnpacker.GenericUnpacker.MakeGenericMethod(new[] { typeof(A2) });
            RpcCall msgpackCall = (MsgPackUnpacker args, UInt32 count, MsgPackPacker result) =>
            {
                var a1 = (A1)gmi1.Invoke(args, null);
                var a2 = (A2)gmi2.Invoke(args, null);
                R r = func(a1, a2);
                result.Pack(r);
            };
            _funcMap.Add(key, msgpackCall);
        }
    }
}
