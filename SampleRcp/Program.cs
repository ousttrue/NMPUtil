using NMPUtil.MsgPack;
using NMPUtil.MsgPack.Rpc;
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

            // dispatcher
            var dispatcher = new MsgPackRpcDispatcher();
            dispatcher.RegisterFunc("add", (int a, int b) => { return a + b; });
            serverStreamManager.StreamReadEvent += (Object o, StreamReadEventArgs e) =>
            {
                var s = o as AsyncStream;
                Console.WriteLine(String.Format("read {0} bytes", e.Bytes.Count));
                dispatcher.Process(s.Stream, e.Bytes);
            };

            // client
            var client=new MsgPackRpcClient();
            var task=client.Connect("127.0.0.1", 8080);
            task.Wait();
            Console.WriteLine("connected");

            var callTask=client.Call<Int32>("add", 2244, 1234);
            callTask.Wait();
            Console.WriteLine("response: " + callTask.Result);
            Thread.Sleep(2000);
        }
    }
}
