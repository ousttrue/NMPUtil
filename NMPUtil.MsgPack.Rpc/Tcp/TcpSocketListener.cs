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
        Task _task;

        public void Bind(IPEndPoint endpoint)
        {
            this._endpoint = endpoint;
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(endpoint);
            _listener.Listen(10);

            Console.WriteLine(String.Format("bind {0} ...", endpoint));
        }

        public void BeginAccept()
        {
            if (_task != null)
            {
                return;
            }
            _task = Task<Socket>.Factory.FromAsync(_listener.BeginAccept, _listener.EndAccept, null)
                .ContinueWith(t => EmitAcceptedEvent(t.Result))
                .ContinueWith(t => _task=null)
                .ContinueWith(t => BeginAccept())
                ;
        }

        public void ShutDown()
        {
            _task = null;
            _listener.Close();
            _listener = null;
        }
    }
}
