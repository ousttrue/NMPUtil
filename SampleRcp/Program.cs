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
            var port = 18080;

            // server
            var server = new MsgPackRpcServer();
            server.Dispatcher.RegisterFunc("add", (int a, int b) => { return a + b; });
            server.Start(port);

            // client
            var client=new MsgPackRpcClient();
            var tcs = new TaskCompletionSource<int>();
            client.ConnectedEvent += (Object o, EventArgs e) =>
            {
                tcs.SetResult(0);
            };
            client.Connect("127.0.0.1", port);
            tcs.Task.Wait();
            Console.WriteLine("connected");

            // call & recv
            var recvTcs= new TaskCompletionSource<Int32>();
            MsgPackRpcClient.ResponseCallback callback=(MsgPackUnpacker unpacker)=>{
                recvTcs.SetResult(unpacker.Unpack<Int32>());
            };
            var task = recvTcs.Task;
            client.Call(callback, "add", 2244, 1234);

            // polling
            for (int i = 0; true; ++i )
            {
                Console.WriteLine(String.Format("[{0}]", i));

                if (task.IsCompleted)
                {
                    Console.WriteLine("response: " + task.Result);
                    break;
                }
                if (task.IsCanceled)
                {
                    break;
                }
                if (task.IsFaulted)
                {
                    break;
                }

                Thread.Sleep(1000);

                // execute in main process
                server.Update();
            }

            Thread.Sleep(2000);
        }
    }
}
