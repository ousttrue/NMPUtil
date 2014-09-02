using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NMPUtil.MsgPack;
using NMPUtil.MsgPack.Rpc;


namespace Simple
{
    class Program
    {
        static void Main(string[] args)
        {
            {
                var ms = new MemoryStream();
                var packer = new MsgPackPacker(ms); ;
                packer.Pack(1);

                var bytes = ms.ToArray();
                Console.WriteLine(String.Join(",", bytes));

                var unpacker = new MsgPackUnpacker(new ArraySegment<Byte>(bytes));
                var result = unpacker.Unpack<int>();

                Console.WriteLine(result);
            }

            {
                var server = new NMPUtil.Streams.TcpListener();
                var dispatcher = new NMPUtil.MsgPack.Rpc.MsgPackRpcDispatcher();
                var streamManager = new NMPUtil.Streams.StreamManager();

                server.AcceptedEvent += streamManager.OnConnected;

                streamManager.StreamReadEvent += (Object o, NMPUtil.Streams.StreamReadEventArgs e) =>
                {

                    var s = o as Stream;
                    if (s == null)
                    {
                        return;
                    }
                    dispatcher.Process(s, e.Bytes);

                };

                Func<int, int, int> add = (int a, int b) =>
                {
                    return a + b;
                };

                MsgPackRpcDispatcher.RpcFunc func = (MsgPackPacker packer, SubMsgPackUnpacker unpacker, UInt32 count) =>
                {
                    if (count != 2)
                    {
                        throw new ArgumentException("count");
                    }
                    var lhs = unpacker.Unpack<int>();
                    var rhs = unpacker.Unpack<int>();

                    var result = add(lhs, rhs);
                    packer.Pack(result);
                };

                dispatcher.RegisterFunc("Add", func);

                // thread
                server.Listen(8080);

                var client = new NMPUtil.Streams.TcpClient();
                client.ConnectedEvent += streamManager.OnConnected;
                client.ConnectedEvent += (Object o, EventArgs e) =>
                {

                };

                client.Connect("127.0.0.1", 8080);

            }
        }
    }
}
