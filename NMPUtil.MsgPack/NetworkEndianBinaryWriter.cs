using System;
using System.IO;
using System.Linq;
using System.Net;

namespace NMPUtil
{
    public class NetworkEndianBinaryWriter : BinaryWriter
    {
        public NetworkEndianBinaryWriter(Stream s)
            : base(s)
        { }

        public override void Write(Int16 value)
        {
            base.Write(IPAddress.HostToNetworkOrder(value));
        }

        public override void Write(Int32 value)
        {
            base.Write(IPAddress.HostToNetworkOrder(value));
        }

        public override void Write(Int64 value)
        {
            base.Write(IPAddress.HostToNetworkOrder(value));
        }

        public override void Write(UInt16 value)
        {
            base.Write(IPAddress.HostToNetworkOrder((Int16)value));
        }
    
        public override void Write(UInt32 value)
        {
            base.Write(IPAddress.HostToNetworkOrder((Int32)value));
        }

        public override void Write(UInt64 value)
        {
            base.Write(IPAddress.HostToNetworkOrder((Int64)value));
        }

        public override void Write(Single value)
        {
            if (BitConverter.IsLittleEndian)
            {
                base.Write(BitConverter.GetBytes(value).Reverse().ToArray());
            }
            else
            {
                base.Write(value);
            }

        }

        public override void Write(Double value)
        {
            if (BitConverter.IsLittleEndian)
            {
                base.Write(BitConverter.GetBytes(value).Reverse().ToArray());
            }
            else
            {
                base.Write(value);
            }
        }
    }
}
