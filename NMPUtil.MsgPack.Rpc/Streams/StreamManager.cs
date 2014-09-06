using System;
using System.Collections.Generic;
using System.IO;

namespace NMPUtil.Streams
{
    public class StreamManager
    {
        List<AsyncStream> _streams = new List<AsyncStream>();

        public event EventHandler<StreamReadEventArgs> StreamReadEvent;
        void EmitStreamReadEvent(ArraySegment<Byte> bytes)
        {
            var tmp = StreamReadEvent;
            if (tmp != null)
            {
                tmp(this, new StreamReadEventArgs { Bytes = bytes });
            }
        }
        void OnStreamRead(Object o, StreamReadEventArgs e)
        {
            var tmp = StreamReadEvent;
            if (tmp != null)
            {
                tmp(o, e);
            }
        }

        public StreamManager()
        {

        }

        public AsyncStream AddStream(Stream s)
        {
            var stream = new AsyncStream(s);

            _streams.Add(stream);
            stream.ReadEvent += OnStreamRead;

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
