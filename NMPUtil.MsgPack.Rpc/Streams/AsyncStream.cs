using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMPUtil.Streams
{
    class AsyncStream
    {
        public event EventHandler<StreamReadEventArgs> ReadEvent;
        void EmitReadEvent(ArraySegment<Byte> bytes)
        {
            var temp = ReadEvent;
            if (temp != null)
            {
                temp(this, new StreamReadEventArgs { Bytes = bytes });
            }
        }
        Byte[] _readBuffer;

        public event EventHandler CloseEvent;
        void EmitCloseEvent()
        {
            var temp = CloseEvent;
            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        Stream _s;
        public Stream Stream
        {
            get { return _s; }
        }

        public AsyncStream(Stream s, Int32 bufferSize = 1024)
        {
            _readBuffer = new Byte[bufferSize];
            this._s = s;
        }

        public void Close()
        {
            _s.Close();
            EmitCloseEvent();
        }

        public void BeginRead()
        {
            Action<IAsyncResult> callback = (IAsyncResult ar) =>
            {
                var self = ar.AsyncState as AsyncStream;
                var readbytes=self._s.EndRead(ar);
                if (readbytes == 0)
                {
                    // closed
                    Close();
                    return;
                }

                EmitReadEvent(new ArraySegment<Byte>(_readBuffer, 0, readbytes));

                BeginRead();
            };
            _s.BeginRead(_readBuffer, 0, _readBuffer.Length, new AsyncCallback(callback), this);
        }
    }
}
