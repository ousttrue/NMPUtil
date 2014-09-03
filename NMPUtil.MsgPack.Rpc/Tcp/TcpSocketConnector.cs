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
    public class TcpSocketConnector
    {
        TaskCompletionSource<NetworkStream> _tcs = new TaskCompletionSource<NetworkStream>();

        public Task<NetworkStream> Task
        {
            get { return _tcs.Task; }
        }

        public void Connect(IPEndPoint endpoint)
        {
            Action<IAsyncResult> callback = (IAsyncResult ar) =>
            {
                var socket = ar.AsyncState as Socket;
                socket.EndConnect(ar);

                _tcs.SetResult(new NetworkStream(socket, true));
            };

            var newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            newSocket.BeginConnect(endpoint, new AsyncCallback(callback), newSocket);
            Console.WriteLine(String.Format("connect to {0}...", endpoint));
        }
    }
}
