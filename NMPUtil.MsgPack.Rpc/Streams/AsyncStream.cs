using System;
using System.IO;

namespace NMPUtil.Streams
{
    public class AsyncStream
    {
        public event EventHandler<StreamReadEventArgs> ReadEvent;
        void EmitReadEvent(ArraySegment<Byte> bytes)
        {
            var tmp = ReadEvent;
            if (tmp != null)
            {
                tmp(this, new StreamReadEventArgs { Bytes = bytes });
            }
        }
        Byte[] _readBuffer;
        IAsyncResult _readIR;

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
            if (_readIR != null)
            {
                _s.EndRead(_readIR);
            }
            _s.Close();
            EmitCloseEvent();
        }

        public void BeginRead()
        {
            Action<IAsyncResult> callback = (IAsyncResult ar) =>
            {
                var self = ar.AsyncState as AsyncStream;
                int readbytes;
                try
                {
                    readbytes = self._s.EndRead(ar);
                }
                catch(IOException ex)
                {
                    // ERROR
                    Close();
                    return;
                }
                self._readIR = null;
                if (readbytes == 0)
                {
                    // closed
                    Close();
                    return;
                }

                EmitReadEvent(new ArraySegment<Byte>(_readBuffer, 0, readbytes));

                BeginRead();
            };
            _readIR=_s.BeginRead(_readBuffer, 0, _readBuffer.Length, new AsyncCallback(callback), this);
        }
    }
}
