using NMPUtil.Streams;
using NMPUtil.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SampleTcp
{
    class Program
    {
        static void Main(string[] args)
        {
            var streamManager = new NMPUtil.Streams.StreamManager();
            streamManager.StreamReadEvent += (Object o, StreamReadEventArgs e) =>
            {
                Console.WriteLine(String.Format("read {0} bytes", e.Bytes.Count));
            };

            // setup server
            var server = new NMPUtil.Tcp.TcpSocketListener();
            server.AcceptedEvent += (Object o, TcpSocketEventArgs e) =>
            {
                var stream = new NetworkStream(e.Socket, true);
                streamManager.AddStream(stream);
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
                streamManager.AddStream(stream);
                tcs.SetResult(stream);
            };
            client.Connect(TcpUtil.EndPoint("127.0.0.1", 8080));

            // sending...
            var task = tcs.Task
                .ContinueWith(t =>
                {
                    streamManager.AddStream(t.Result);
                    Console.WriteLine("connected");
                    return t.Result;
                })
                .ContinueWith(t =>
                {
                    var bytes = new Byte[] { 1 };
                    t.Result.Write(bytes, 0, bytes.Length);
                    Thread.Sleep(1000);
                    return t.Result;
                })
                .ContinueWith(t =>
                {
                    var bytes = new Byte[] { 1, 2 };
                    t.Result.Write(bytes, 0, bytes.Length);
                    Thread.Sleep(1000);
                    return t.Result;
                })
                .ContinueWith(t =>
                {
                    var bytes = new Byte[] { 1, 2, 3 };
                    t.Result.Write(bytes, 0, bytes.Length);
                    Thread.Sleep(1000);
                    return t.Result;
                })
                .ContinueWith(t =>
                {
                    var bytes = new Byte[] { 1, 2, 3, 4 };
                    t.Result.Write(bytes, 0, bytes.Length);
                    Thread.Sleep(1000);
                    return t.Result;
                })
                .ContinueWith(t =>
                {
                    var bytes = new Byte[] { 1, 2, 3, 4, 5 };
                    t.Result.Write(bytes, 0, bytes.Length);
                    Thread.Sleep(1000);
                    return t.Result;
                })
                ;

            task.Wait();
            Console.WriteLine("task completed");
            Thread.Sleep(1000);

            var task2 = Task.Factory.StartNew<NetworkStream>(() =>
            {
                Console.WriteLine("start new");
                return task.Result;
            })
            .ContinueWith(t =>
            {
                var bytes = new Byte[] { 1 };
                t.Result.Write(bytes, 0, bytes.Length);
                Thread.Sleep(1000);
                return t.Result;
            })
            .ContinueWith(t =>
            {
                var bytes = new Byte[] { 1, 2 };
                t.Result.Write(bytes, 0, bytes.Length);
                Thread.Sleep(1000);
                return t.Result;
            })
            .ContinueWith(t =>
            {
                var bytes = new Byte[] { 1, 2, 3 };
                t.Result.Write(bytes, 0, bytes.Length);
                Thread.Sleep(1000);
                return t.Result;
            })
            .ContinueWith(t =>
            {
                var bytes = new Byte[] { 1, 2, 3, 4 };
                t.Result.Write(bytes, 0, bytes.Length);
                Thread.Sleep(1000);
                return t.Result;
            })
            .ContinueWith(t =>
            {
                var bytes = new Byte[] { 1, 2, 3, 4, 5 };
                t.Result.Write(bytes, 0, bytes.Length);
                Thread.Sleep(1000);
                return t.Result;
            })
            ;

            task2.Wait();
            Console.WriteLine("task completed");
            Thread.Sleep(1000);
        }
    }
}
