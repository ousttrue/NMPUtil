using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NMPUtil.Streams
{
    public class StreamManager
    {
        List<AsyncStream> _streams = new List<AsyncStream>();

        public event EventHandler<StreamReadEventArgs> StreamReadEvent;
        void OnRead(Object o, StreamReadEventArgs args)
        {
            var temp = StreamReadEvent;
            if (temp != null)
            {
                // through
                temp(o, args);
            }
        }

        public StreamManager()
        {

        }

        public void AddStream(Stream s)
        {
            var stream = new AsyncStream(s);
            stream.ReadEvent += OnRead;
            _streams.Add(stream);

            // async read
            stream.BeginRead();
        }

        AsyncStream GetStreamFrom(Stream stream)
        {
            return _streams.Find(s => s.Stream == stream);
        }
    }
}
