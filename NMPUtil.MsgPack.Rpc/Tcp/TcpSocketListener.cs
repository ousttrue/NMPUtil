using NMPUtil.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NMPUtil.Tcp
{
    public class TcpSocketListener
    {
        IPEndPoint _endpoint;
        Socket _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public EventHandler<TcpSocketEventArgs> AcceptedEvent;
        void EmitAcceptedEvent(Socket socket)
        {
            var tmp = AcceptedEvent;
            if (tmp != null)
            {
                tmp(this, new TcpSocketEventArgs { Socket = socket });
            }
        }
        IAsyncResult _ar;

        public TcpSocketListener()
        {
        }

        public void Bind(IPEndPoint endpoint)
        {
            this._endpoint = endpoint;
            _listener.Bind(endpoint);
            _listener.Listen(10);

            Console.WriteLine(String.Format("bind {0} ...", endpoint));
        }

        public void BeginAccept()
        {
            Action<IAsyncResult> callback = (IAsyncResult ar) =>
            {
                var listener = ar.AsyncState as Socket;
                var socket=listener.EndAccept(ar);

                EmitAcceptedEvent(socket);

                BeginAccept();
            };
            _ar=_listener.BeginAccept(new AsyncCallback(callback), _listener);
        }

        public void ShutDown()
        {
            if (_ar==null) {
                return;
            }
            _listener.EndAccept(_ar);
        }
    }
}
