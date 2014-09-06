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
            // server
            var server = new MsgPackRpcServer();
            server.Dispatcher.RegisterFunc("add", (int a, int b) => { return a + b; });
            server.Start(8080);

            // client
            var client=new MsgPackRpcClient();
            var tcs = new TaskCompletionSource<int>();
            client.ConnectedEvent += (Object o, EventArgs e) =>
            {
                tcs.SetResult(0);
            };
            client.Connect("127.0.0.1", 8080);
            tcs.Task.Wait();
            Console.WriteLine("connected");

            var recvTcs= new TaskCompletionSource<Int32>();
            MsgPackRpcClient.ResponseCallback callback=(MsgPackUnpacker unpacker)=>{
                recvTcs.SetResult(unpacker.Unpack<Int32>());
            };
            client.Call(callback, "add", 2244, 1234);

            var task = recvTcs.Task;
            task.Wait();
            Console.WriteLine("response: " + task.Result);

            Thread.Sleep(2000);
        }
    }
}
