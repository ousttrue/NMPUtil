using NMPUtil.Streams;
using NMPUtil.Tcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NMPUtil.MsgPack.Rpc
{
    public class MsgPackRpcClient
    {
        NMPUtil.Tcp.TcpSocketConnector _connector = new NMPUtil.Tcp.TcpSocketConnector();
        AsyncStream _asyncStream;

        public Task<AsyncStream> Connect(String host, Int32 port)
        {
            var tcs = new TaskCompletionSource<AsyncStream>();
            _connector.Connect(TcpUtil.EndPoint("127.0.0.1", 8080));
            _connector.ConnectedEvent += (Object o, TcpSocketEventArgs e) =>
            {
                var stream = new NetworkStream(e.Socket, true);
                _asyncStream = new AsyncStream(stream);
                _asyncStream.BeginRead();
                tcs.SetResult(_asyncStream);
            };
            return tcs.Task;
        }

        public Task<T> Call<T>(String func, params Object[] args)
        {
            if (_asyncStream == null)
            {
                throw new InvalidOperationException("not connected");
            }
            Console.WriteLine(String.Format("call {0}({1}) ...", func, String.Join(", ", args)));

            // read task
            var tcs = new TaskCompletionSource<T>();
            var onRead = new EventHandler<StreamReadEventArgs>(
                (Object o, StreamReadEventArgs e) =>
                {
                    var unpacker = new MsgPackUnpacker(e.Bytes);
                    if (!unpacker.Header.IsArray)
                    {
                        throw new InvalidOperationException("should array");
                    }
                    using (var sub = unpacker.GetSubUnpacker())
                    {
                        sub.Unpack<Int32>();
                        sub.Unpack<UInt32>();
                        sub.Unpack<Int32>();
                        var result = sub.Unpack<Int32>();
                        tcs.SetResult((T)(Object)result);
                    }
                });

            _asyncStream.ReadEvent += onRead;

            // call
            var ms = new MemoryStream();
            var packer = new MsgPackPacker(ms);
            packer.Pack_Array(4);
            packer.Pack((Byte)0);
            packer.Pack(1);
            packer.Pack(func);
            packer.Pack_Array((UInt32)args.Length);
            foreach (var a in args)
            {
                packer.Pack(a);
            }
            var bytes = ms.ToArray();
            _asyncStream.Stream.Write(bytes, 0, bytes.Length);

            return tcs.Task.ContinueWith(t =>
            {
                _asyncStream.ReadEvent -= onRead;
                return t.Result;
            });
        }
    }

}
