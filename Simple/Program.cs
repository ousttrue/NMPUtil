using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NMPUtil.MsgPack;
using NMPUtil.MsgPack.Rpc;
using System.Diagnostics;


namespace Simple
{
    [Serializable]
    public class TestClass
    {
        public string MyProperty1 { get; set; }
        public int MyProperty2 { get; set; }
        //public DateTime MyProperty3 { get; set; }
        public bool MyProperty4 { get; set; }
    }

 
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
                // シンプルなPOCOとしての対象
                var obj = new TestClass
                {
                    MyProperty1 = "hoge",
                    MyProperty2 = 1,
                    //MyProperty3 = new DateTime(1999, 12, 11),
                    MyProperty4 = true
                };

                // オブジェクト配列としての対象
                var array = Enumerable.Range(1, 10)
                    .Select(i => new TestClass
                    {
                        MyProperty1 = "hoge" + i,
                        MyProperty2 = i,
                        //MyProperty3 = new DateTime(1999, 12, 11).AddDays(i),
                        MyProperty4 = i % 2 == 0
                    })
                    .ToArray();

                var sw = new Stopwatch();

                var ms = new MemoryStream();
                var tc=new TestClass();
                sw.Start();
                for (int i = 0; i < 10000; ++i)
                {
                    var packer = new MsgPackPacker(ms); ;
                    packer.Pack(array);

                    var unpacker = new MsgPackUnpacker(ms.ToArray());
                    var tca = new TestClass[unpacker.MemberCount];
                    unpacker.Unpack(ref tca);
                }
                sw.Stop();
                Console.WriteLine("{0} msec", sw.ElapsedMilliseconds);
                System.Threading.Thread.Sleep(3000);
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
