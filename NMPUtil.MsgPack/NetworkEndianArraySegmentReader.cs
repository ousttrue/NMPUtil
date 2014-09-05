using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NMPUtil
{
    public class NetworkEndianArraySegmentReader
    {
        public class NotEnoughBytesException : InvalidOperationException
        {
            public NotEnoughBytesException()
            {
            }

            public NotEnoughBytesException(string message)
                : base(message)
            {
            }

            public NotEnoughBytesException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        ArraySegment<Byte> _view;
        public Int32 Pos
        {
            get;
            private set;
        }
        public void Advance(Int32 d)
        {
            Pos = Pos + d;
        }
        public ArraySegment<Byte> GetRemain()
        {
            return new ArraySegment<byte>(_view.Array, _view.Offset + Pos, _view.Count - Pos);
        }

        public NetworkEndianArraySegmentReader(ArraySegment<Byte> view)
        {
            this._view = view;
        }

        public IEnumerable<Byte> ReadBytes(Int32 size)
        {
            if (Pos + size > _view.Count)
            {
                throw new NotEnoughBytesException("ReadBytes " + size);
            }
            var b = _view.Skip(Pos).Take(size);
            Advance(size);
            return b;
        }

        public SByte ReadSByte()
        {
            return (SByte)ReadBytes(1).First();
        }

        public Int16 ReadInt16()
        {
            return IPAddress.NetworkToHostOrder(
                BitConverter.ToInt16(ReadBytes(2).ToArray(), 0));
        }

        public Int32 ReadInt32()
        {
            return IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(ReadBytes(4).ToArray(), 0));
        }

        public Int64 ReadInt64()
        {
            return IPAddress.NetworkToHostOrder(
                BitConverter.ToInt64(ReadBytes(8).ToArray(), 0));
        }

        public Byte ReadByte()
        {
            return ReadBytes(1).First();
        }

        public UInt16 ReadUInt16()
        {
            return (UInt16)ReadInt16();
        }

        public UInt32 ReadUInt32()
        {
            return (UInt32)ReadInt32();
        }

        public UInt64 ReadUInt64()
        {
            return (UInt64)ReadInt64();
        }

        public Single ReadSingle()
        {
            var b = ReadBytes(4);
            if (BitConverter.IsLittleEndian)
            {
                b = b.Reverse();
            }
            return BitConverter.ToSingle(b.ToArray(), 0);
        }

        public Double ReadDouble()
        {
            var b = ReadBytes(8);
            if (BitConverter.IsLittleEndian)
            {
                b = b.Reverse();
            }
            return BitConverter.ToDouble(b.ToArray(), 0);
        }
    }
}
