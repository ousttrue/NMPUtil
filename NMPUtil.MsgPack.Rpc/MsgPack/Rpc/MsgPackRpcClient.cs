using NMPUtil.Streams;
using NMPUtil.Tcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;


namespace NMPUtil.MsgPack.Rpc
{
    public class MsgPackRpcClient
    {
        NMPUtil.Tcp.TcpSocketConnector _connector = new NMPUtil.Tcp.TcpSocketConnector();
        AsyncStream _asyncStream;

        public event EventHandler ConnectedEvent;
        void EmitConnectedEvent()
        {
            var tmp = ConnectedEvent;
            if (tmp != null)
            {
                tmp(this, EventArgs.Empty);
            }
        }

        public void Connect(String host, Int32 port)
        {
            _connector.ConnectedEvent += (Object o, TcpSocketEventArgs e) =>
            {
                var stream = new NetworkStream(e.Socket, true);
                _asyncStream = new AsyncStream(stream);
                _asyncStream.ReadEvent += onRead;
                _asyncStream.BeginRead();
                EmitConnectedEvent();
            };
            _connector.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
        }

        public delegate void ResponseCallback(MsgPackUnpacker unpacker);
        Dictionary<UInt32, ResponseCallback> _requestMap=new Dictionary<uint,ResponseCallback>();

        void onRead(Object o, StreamReadEventArgs e)
        {
            var unpacker = new MsgPackUnpacker(e.Bytes);
            if (!unpacker.Header.IsArray)
            {
                throw new InvalidOperationException("should array");
            }
            using (var sub = unpacker.GetSubUnpacker())
            {
                var type = sub.Unpack<Int32>();
                switch (type)
                {
                    case 0:
                        // request. what ?
                        throw new InvalidOperationException("request received");

                    case 1:
                        // response
                        {
                            var id=sub.Unpack<UInt32>();
                            var error=sub.Unpack<UInt32>();
                            if(error!=0){
                                // error occured
                                throw new InvalidOperationException("error result");
                            }
                            else{
                                // success
                                _requestMap[id](sub);
                            }
                        }
                        break;

                    case 2:
                        // notifycation
                        {
                            throw new NotImplementedException("notifycation is not");
                        }
                        break;
                }
            }
        }

        UInt32 _id = 1;

        public void Call(ResponseCallback callback, String func, params Object[] args)
        {
            if (_asyncStream == null)
            {
                throw new InvalidOperationException("not connected");
            }
            //Console.WriteLine(String.Format("call {0}({1}) ...", func, String.Join(", ", args)));

            // call
            var ms = new MemoryStream();
            var packer = new MsgPackPacker(ms);
            packer.Pack_Array(4);
            packer.Pack((Byte)0);
            var id=_id++;
            packer.Pack(id);
            _requestMap[id] = callback;
            packer.Pack(func);
            packer.Pack_Array((UInt32)args.Length);
            foreach (var a in args)
            {
                packer.Pack(a);
            }
            var bytes = ms.ToArray();
            _asyncStream.Stream.Write(bytes, 0, bytes.Length);
        }
    }
}
