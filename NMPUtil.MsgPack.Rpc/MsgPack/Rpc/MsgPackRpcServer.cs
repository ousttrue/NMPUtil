using NMPUtil.Streams;
using NMPUtil.Tcp;
using System;
using System.Net.Sockets;

namespace NMPUtil.MsgPack.Rpc
{
    public class MsgPackRpcServer
    {
        StreamManager _serverStreamManager = new StreamManager();
        TcpSocketListener _listener = new TcpSocketListener();
        public MsgPackRpcDispatcher Dispatcher
        {
            get;
            private set;
        }

        public MsgPackRpcServer()
        {
            _listener.AcceptedEvent += (Object o, TcpSocketEventArgs e) =>
            {
                var stream = new NetworkStream(e.Socket, true);
                _serverStreamManager.AddStream(stream);
                Console.WriteLine("accepted");
            };

            _serverStreamManager.StreamReadEvent += (Object o, StreamReadEventArgs e) =>
            {
                var s = o as AsyncStream;
                Console.WriteLine(String.Format("read {0} bytes", e.Bytes.Count));
                Dispatcher.Enqueue(s.Stream, e.Bytes);
            };

            Dispatcher = new MsgPackRpcDispatcher();
        }

        public void Start(Int32 port)
        {
            _listener.Bind(TcpUtil.EndPoint("", port));
            _listener.BeginAccept();
        }

        public void Update()
        {
            Dispatcher.Update();
        }

        public void Shutdown()
        {
            _listener.ShutDown();
        }
    }
}
