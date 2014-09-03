using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NMPUtil.MsgPack;
using NMPUtil.MsgPack.Rpc;
using System.Diagnostics;
using System.Net.Sockets;


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
                var streamManager = new NMPUtil.Streams.StreamManager();
                streamManager.StreamReadEvent += (Object o, NMPUtil.Streams.StreamReadEventArgs e) =>
                {
                    Console.WriteLine(String.Format("read {0} bytes", e.Bytes.Count));
                };

                var server = new NMPUtil.Tcp.TcpSocketListener();
                server.AcceptedEvent += streamManager.OnTcpSocketConnected;
                server.AcceptedEvent += (Object o, NMPUtil.Tcp.TcpSocketEventArgs e) =>
                {
                    Console.WriteLine("accepted");
                };

                server.Bind(NMPUtil.Tcp.TcpUtil.EndPoint("", 8080));
                server.BeginAccept();

                var client = new NMPUtil.Tcp.TcpSocketConnector();
                client.ConnectedEvent += streamManager.OnTcpSocketConnected;
                client.ConnectedEvent += (Object o, NMPUtil.Tcp.TcpSocketEventArgs e) =>
                {
                    Console.WriteLine("connected");
                };

                client.Connect(NMPUtil.Tcp.TcpUtil.EndPoint("127.0.0.1", 8080));

                for (int i = 0; i < 10; ++i)
                {
                    Console.WriteLine(i);
                    System.Threading.Thread.Sleep(1000);

                    var msg = new Byte[] { (Byte)i };
                    var sendbytes=client.Socket.Send(msg, 0, msg.Length, SocketFlags.None);
                }
            }
        }
    }
}
