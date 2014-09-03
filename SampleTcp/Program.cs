using NMPUtil.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
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

            // setup server
            var server = new NMPUtil.Tcp.TcpSocketListener();
            {
                server.AddAcceptAction(
                stream =>
                {
                    var s = streamManager.AddStream(stream);
                    Console.WriteLine("accepted");
                    s.AddReadAction((ArraySegment<Byte> bytes) =>
                    {
                        Console.WriteLine(String.Format("read {0} bytes", bytes.Count));
                    });
                })
                ;

                server.Bind(TcpUtil.EndPoint("", 8080));
                server.BeginAccept();
            }

            // setup client
            var client = new NMPUtil.Tcp.TcpSocketConnector();
            client.Connect(TcpUtil.EndPoint("127.0.0.1", 8080));

            {
                var task = client.Task
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
            }
            {
                var task = client.Task
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
            }

        }
    }
}
