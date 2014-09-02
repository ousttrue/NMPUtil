using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NMPUtil.Streams
{
    public class StreamReadEventArgs : EventArgs
    {
        public Byte[] Bytes
        {
            get;
            set;
        }
    }


    public class StreamManager
    {
        List<Stream> _strams = new List<Stream>();

        public event EventHandler<StreamReadEventArgs> StreamReadEvent;

        public StreamManager()
        {

        }

        public void AddStream(Stream s)
        {
            _strams.Add(s);
        }

        public void OnConnected(Object o, SocketEventArgs e)
        {
            var s = e.Socket;
            if (s == null)
            {
                return;
            }
            if(o is TcpConnector)
            {
                Console.WriteLine("connected");
            }
            if(o is TcpListener)
            {
                Console.WriteLine("accepted");
            }
            AddStream(new NetworkStream(s, true));
        }
    }
}
