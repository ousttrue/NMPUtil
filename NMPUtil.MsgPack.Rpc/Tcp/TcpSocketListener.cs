using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NMPUtil.Tcp
{
    public class TcpSocketListener
    {
        public event EventHandler<TcpSocketEventArgs> AcceptedEvent;
        void EmitAcceptedEvent(Socket socket)
        {
            var temp = AcceptedEvent;
            if (temp != null)
            {
                temp(this, new TcpSocketEventArgs { Socket = socket });
            }
        }

        IPEndPoint _endpoint;
        Socket _listener;


        public void Listen(IPEndPoint endpoint)
        {
            this._endpoint=endpoint;
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(endpoint);
            _listener.Listen(10);

            BeginAccept();
            Console.WriteLine(String.Format("begin Listen {0} ...", endpoint));
        }

        void BeginAccept()
        {
            AsyncCallback callback = (IAsyncResult ar) =>
            {
                var socket = ar.AsyncState as Socket;
                Socket newSocket = socket.EndAccept(ar);

                EmitAcceptedEvent(newSocket);   

                // next...
                BeginAccept();
            };

            _listener.BeginAccept(callback, _listener);
            Console.WriteLine("accepting...");
        }

        public void ShutDown()
        {
            _listener.Close();
            _listener = null;
        }
    }
}
