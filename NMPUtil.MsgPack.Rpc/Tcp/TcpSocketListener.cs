using System;
using System.Net;
using System.Net.Sockets;

namespace NMPUtil.Tcp
{
    public class TcpSocketListener
    {
        IPEndPoint _endpoint;
        Socket _listener;

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
            ShutDown();

            this._endpoint = endpoint;
            _listener=new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(endpoint);
            _listener.Listen(10);

            Console.WriteLine(String.Format("bind {0} ...", endpoint));
        }

        public void BeginAccept()
        {
            Action<IAsyncResult> callback = (IAsyncResult ar) =>
            {
                var listener = ar.AsyncState as Socket;
                Socket socket;
                try
                {
                    socket = listener.EndAccept(ar);
                }
                catch(ObjectDisposedException ex)
                {
                    // Closed
                    return;
                }

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
            _listener.Close();
        }
    }
}
