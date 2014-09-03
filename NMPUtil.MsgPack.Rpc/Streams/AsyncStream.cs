using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMPUtil.Streams
{
    public class AsyncStream
    {
        TaskCompletionSource<ArraySegment<Byte>> _readtcs;
        Task<ArraySegment<Byte>> ReadTask
        {
            get { return _readtcs.Task; }
        }
        Byte[] _readBuffer;

        public delegate void ReadAction(ArraySegment<Byte> bytes);
        List<ReadAction> _readActions = new List<ReadAction>();
        public void AddReadAction(ReadAction action)
        {
            _readActions.Add(action);
        }

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
            _readtcs.SetCanceled();
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

                _readtcs.SetResult(new ArraySegment<Byte>(_readBuffer, 0, readbytes));

                BeginRead();
            };
            _readtcs = new TaskCompletionSource<ArraySegment<byte>>();
            _readtcs.Task.ContinueWith(t =>
            {
                foreach (var action in _readActions)
                {
                    action(t.Result);
                }
            });
            _s.BeginRead(_readBuffer, 0, _readBuffer.Length, new AsyncCallback(callback), this);
        }
    }
}
