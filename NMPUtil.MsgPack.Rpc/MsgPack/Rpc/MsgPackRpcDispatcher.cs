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
            if (!unpacker.IsArray)
            {
                throw new ArgumentException("is not array");
            }
            if(unpacker.MemberCount!=4)
            {
                throw new ArgumentException("array member is not 4. " + unpacker.MemberCount);
            }
            using (var sub = unpacker.GetSubUnpacker())
            {
                sub.ParseHeadByte();
                var type = sub.Unpack<Int32>();
                if (type != 0)
                {
                    throw new ArgumentException("is not request. " + type);
                }

                sub.ParseHeadByte();
                var id = sub.Unpack<UInt32>();

                sub.ParseHeadByte();
                var key = String.Empty;
                sub.Unpack(ref key);

                sub.ParseHeadByte();
                if (!sub.IsArray)
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
                    func(args, sub.MemberCount, response);
                }

                var data=ms.ToArray();
                s.Write(data, 0, data.Length);
            }
        }

        public delegate void RpcCall(SubMsgPackUnpacker args, UInt32 count, MsgPackPacker result);

        Dictionary<String, RpcCall> _funcMap = new Dictionary<string, RpcCall>();

        public void RegisterFunc<A1, A2, R>(String key, Func<A1, A2, R> func)
        {
            RpcCall msgpackCall = (SubMsgPackUnpacker args, UInt32 count, MsgPackPacker result) =>
            {
                args.ParseHeadByte();
                var a1 = default(A1);
                if (typeof(A1).IsValueType)
                {
                    var gmi=MsgPackUnpacker.GenericValueUnpacker.MakeGenericMethod(new[] { typeof(A1) });
                    a1 = (A1)gmi.Invoke(args, null);
                }
                else
                {
                    var gmi = MsgPackUnpacker.GenericReferenceUnpacker.MakeGenericMethod(new[] { typeof(A1) });
                    var invokeArgs = new Object[] { Activator.CreateInstance<A1>() };
                    gmi.Invoke(args, invokeArgs);
                    a1 = (A1)invokeArgs[0];
                }

                args.ParseHeadByte();
                var a2 = default(A2);
                if (typeof(A2).IsValueType)
                {
                    var gmi = MsgPackUnpacker.GenericValueUnpacker.MakeGenericMethod(new[] { typeof(A2) });
                    a2 = (A2)gmi.Invoke(args, null);
                }
                else
                {
                    var gmi = MsgPackUnpacker.GenericReferenceUnpacker.MakeGenericMethod(new[] { typeof(A2) });
                    var invokeArgs = new Object[] { Activator.CreateInstance<A2>() };
                    gmi.Invoke(args, invokeArgs);
                    a2 = (A2)invokeArgs[0];
                }

                R r = func(a1, a2);
                result.Pack(r);
            };

            _funcMap.Add(key, msgpackCall);
        }
    }
}
