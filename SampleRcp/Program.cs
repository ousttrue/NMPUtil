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
