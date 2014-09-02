using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NMPUtil.Streams
{
    public class SocketEventArgs : EventArgs
    {
        public Socket Socket;
    }


    public class TcpListener
    {
        public event EventHandler<SocketEventArgs> AcceptedEvent;
        void EmitAcceptedEvent(Socket socket)
        {
            var temp = AcceptedEvent;
            if (temp != null)
            {
                temp(this, new SocketEventArgs { Socket = socket });
            }
        }

        IPEndPoint _endpoint;
        Socket _listener;


        public void Listen(String host, Int32 port)
        {
            if (String.IsNullOrEmpty(host))
            {
                Listen(new IPAddress(0), port);
            }
            else
            {
                Listen(IPAddress.Parse(host), port);
            }
        }

        public void Listen(IPAddress address, Int32 port)
        {
            Listen(new IPEndPoint(address, port));
        }

        public void Listen(IPEndPoint endpoint)
        {
            this._endpoint=endpoint;
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(endpoint);
            _listener.Listen(10);

            BeginAccept();
            Console.WriteLine(String.Format("Begin Listen {0} ...", endpoint));
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
        }

        public void ShutDown()
        {
            _listener.Close();
            _listener = null;
        }
    }
}
