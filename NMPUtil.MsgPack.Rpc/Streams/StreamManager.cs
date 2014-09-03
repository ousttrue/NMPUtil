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

        public StreamManager()
        {

        }

        public AsyncStream AddStream(Stream s)
        {
            var stream = new AsyncStream(s);

            _streams.Add(stream);

            // async read
            stream.BeginRead();

            return stream;
        }

        AsyncStream GetStreamFrom(Stream stream)
        {
            return _streams.Find(s => s.Stream == stream);
        }
    }
}
