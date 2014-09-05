using NMPUtil.MsgPack;
using NMPUtil.Streams;
using NMPUtil.Tcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SampleRcp
{
    class Program
    {
        static void Main(string[] args)
        {
            var serverStreamManager = new StreamManager();
            var clientStreamManager = new StreamManager();

            // setup server
            var server = new TcpSocketListener();
            server.AcceptedEvent += (Object o, TcpSocketEventArgs e) =>
            {
                var stream = new NetworkStream(e.Socket, true);
                serverStreamManager.AddStream(stream);
                Console.WriteLine("accepted");
            };
            server.Bind(TcpUtil.EndPoint("", 8080));
            server.BeginAccept();

            // setup client
            var client = new NMPUtil.Tcp.TcpSocketConnector();
            var tcs = new TaskCompletionSource<NetworkStream>();

            client.ConnectedEvent += (Object o, TcpSocketEventArgs e) =>
            {
                var stream = new NetworkStream(e.Socket, true);
                clientStreamManager.AddStream(stream);
                tcs.SetResult(stream);
            };
            // call
            tcs.Task.ContinueWith(t =>
            {
                Thread.Sleep(500);

                Console.WriteLine("call...");
                var ms = new MemoryStream();
                var packer = new MsgPackPacker(ms);
                packer.Pack_Array(4);
                packer.Pack((Byte)0);
                packer.Pack(1);
                packer.Pack("add");
                packer.Pack_Array(2);
                packer.Pack(2244);
                packer.Pack(12345);
                var bytes = ms.ToArray();
                t.Result.Write(bytes, 0, bytes.Length);
            });

            // dispatcher
            var dispatcher = new NMPUtil.MsgPack.Rpc.MsgPackRpcDispatcher();
            dispatcher.RegisterFunc("add", (int a, int b) => { return a + b; });
            serverStreamManager.StreamReadEvent += (Object o, StreamReadEventArgs e) =>
            {
                var s = o as AsyncStream;
                Console.WriteLine(String.Format("read {0} bytes", e.Bytes.Count));
                dispatcher.Process(s.Stream, e.Bytes);
            };

            // result task
            var readTcs = new TaskCompletionSource<ArraySegment<Byte>>();
            var task = readTcs.Task.ContinueWith(t =>
            {
                var unpacker = new MsgPackUnpacker(t.Result);
                using (var sub = unpacker.GetSubUnpacker())
                {
                    sub.Unpack<Int32>();
                    sub.Unpack<UInt32>();
                    sub.Unpack<Int32>();
                    var result = sub.Unpack<Int32>();
                    Console.WriteLine("result " + result);
                }
            });
            clientStreamManager.StreamReadEvent += (Object o, StreamReadEventArgs e) =>
            {
                readTcs.SetResult(e.Bytes);
            };

            client.Connect(TcpUtil.EndPoint("127.0.0.1", 8080));
            task.Wait();
            Thread.Sleep(1000);
        }
    }
}
