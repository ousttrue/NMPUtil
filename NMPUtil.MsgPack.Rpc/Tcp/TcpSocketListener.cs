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
        TaskCompletionSource<NetworkStream> _tcs = new TaskCompletionSource<NetworkStream>();

        public Task<NetworkStream> Task
        {
            get
            {
                return _tcs.Task;
            }
        }

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

                _tcs.SetResult(new NetworkStream(socket, true));

                BeginAccept();
            };
            _listener.BeginAccept(new AsyncCallback(callback), _listener);
        }

        public void ShutDown()
        {
            _tcs.SetCanceled();
        }
    }
}
