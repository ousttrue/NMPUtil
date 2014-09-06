using System;
using System.Net;
using System.Net.Sockets;

namespace NMPUtil.Tcp
{
    public class TcpSocketConnector
    {
        public event EventHandler<TcpSocketEventArgs> ConnectedEvent;
        void EmitConnectedEvent(Socket socket)
        {
            var tmp = ConnectedEvent;
            if (tmp != null)
            {
                tmp(this, new TcpSocketEventArgs { Socket = socket });
            }
        }

        IAsyncResult _ar;

        public void Connect(IPEndPoint endpoint)
        {
            Action<IAsyncResult> callback = (IAsyncResult ar) =>
            {
                var socket = ar.AsyncState as Socket;
                socket.EndConnect(ar);

                EmitConnectedEvent(socket);
            };

            var newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _ar=newSocket.BeginConnect(endpoint, new AsyncCallback(callback), newSocket);
            Console.WriteLine(String.Format("connect to {0}...", endpoint));
        }
    }
}
