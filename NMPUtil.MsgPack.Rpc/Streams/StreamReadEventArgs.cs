using System;

namespace NMPUtil.Streams
{
    public class StreamReadEventArgs: EventArgs
    {
        public ArraySegment<Byte> Bytes;
    }
}
