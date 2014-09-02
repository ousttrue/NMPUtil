using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public void OnConnected(Object o, EventArgs e)
        {
            var s = o as Stream;
            if (s == null)
            {
                return;
            }
            AddStream(s);
        }
    }
}
