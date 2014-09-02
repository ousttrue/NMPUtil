using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NMPUtil.Tcp
{
    public class TcpSocketConnector
    {
        public event EventHandler<TcpSocketEventArgs> ConnectedEvent;
        void EmitConnectedEvent(Socket socket)
        {
            var temp = ConnectedEvent;
            if (temp != null)
            {
                temp(this, new TcpSocketEventArgs { Socket = socket });
            }
        }

        public Socket Socket
        {
            get;
            private set;
        }

        public void Connect(IPEndPoint endpoint)
        {
            Action<IAsyncResult> callback = (IAsyncResult ar) =>
            {
                var socket = ar.AsyncState as Socket;
                this.Socket = socket;
                socket.EndConnect(ar);

                EmitConnectedEvent(socket);
            };

            var newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            newSocket.BeginConnect(endpoint, new AsyncCallback(callback), newSocket);
            Console.WriteLine(String.Format("connect to {0}...", endpoint));
        }
    }
}
