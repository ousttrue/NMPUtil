using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NMPUtil.MsgPack
{
    public class MsgPackPacker
    {
        public BinaryWriter Writer
        {
            get;
            private set;
        }

        public MsgPackPacker(BinaryWriter w)
        {
            Writer=w;
        }

        public MsgPackPacker(Stream s):this(new BinaryWriter(s))
        {
        }

        public static Dictionary<Type, Action<MsgPackPacker, Object>> TypeMap = new Dictionary<Type, Action<MsgPackPacker, Object>>()
                {
                    // float
                    {typeof(Double), (MsgPackPacker packer, Object o) =>{
                        packer.Pack((Double)o);
                    }},
                    {typeof(Single), (MsgPackPacker packer, Object o)=>{            
                        packer.Pack((Single)o);
                    }},
                    // int
                    {typeof(Int64), (MsgPackPacker packer, Object o)=>
                    {
                        packer.Pack((Int64)o);
                    }},
                    {typeof(Int32), (MsgPackPacker packer, Object o)=>
                    {
                        packer.Pack((Int32)o);
                    }},
                    {typeof(Int16), (MsgPackPacker packer, Object o)=>
                    {
                        packer.Pack((Int16)o);
                    }},
                    {typeof(SByte), (MsgPackPacker packer, Object o)=>
                    {
                        packer.Pack((SByte)o);
                    }},
                    //
                    {typeof(UInt64), (MsgPackPacker packer, Object o)=>
                    {
                        packer.Pack((UInt64)o);
                    }},
                    {typeof(UInt32), (MsgPackPacker packer, Object o)=>
                    {
                        packer.Pack((UInt32)o);
                    }},
                    {typeof(UInt16), (MsgPackPacker packer, Object o)=>
                    {
                        packer.Pack((UInt16)o);
                    }},
                    {typeof(Byte), (MsgPackPacker packer, Object o)=>
                    {
                        packer.Pack((Byte)o);
                    }},
                    //
                    {typeof(Boolean), (MsgPackPacker packer, Object o) =>
                    {
                        packer.Pack((Boolean)o);
                    }},
                    {typeof(Byte[]), (MsgPackPacker packer, Object o)=>
                    {
                        packer.Pack((Byte[])o);
                    }},
                    {typeof(String), (MsgPackPacker packer, Object o)=>
                    {
                        packer.Pack((String)o);
                    }}
                    , {typeof(Decimal), (MsgPackPacker packer, Object o)=>
                    {
                        packer.Pack((Double)o);
                    }}
                };

        public void PackNil()
        {
            Writer.Write(MsgPackFormat.NIL.Mask());
        }

        public void Pack(Boolean b)
        {
            if (b)
            {
                Writer.Write(MsgPackFormat.TRUE.Mask());
            }
            else
            {
                Writer.Write(MsgPackFormat.FALSE.Mask());
            }
        }

        public void Pack(Byte n)
        {
            Writer.Write(MsgPackFormat.UINT8.Mask());
            Writer.Write((Byte)n);
        }

        public void Pack(UInt16 n)
        {
            Writer.Write(MsgPackFormat.UINT16.Mask());
            Writer.Write(IPAddress.HostToNetworkOrder((Int16)n));
        }

        public void Pack(UInt32 n)
        {
            Writer.Write(MsgPackFormat.UINT32.Mask());
            Writer.Write(IPAddress.HostToNetworkOrder((Int32)n));
        }

        public void Pack(UInt64 n)
        {
            Writer.Write(MsgPackFormat.UINT64.Mask());
            Writer.Write(IPAddress.HostToNetworkOrder((Int64)n));
        }

        public void Pack(SByte n)
        {
            Writer.Write(MsgPackFormat.INT8.Mask());
            Writer.Write((Byte)(n));
        }

        public void Pack(Int16 n)
        {
            Writer.Write(MsgPackFormat.INT16.Mask());
            Writer.Write(IPAddress.HostToNetworkOrder(n));
        }

        public void Pack(Int32 n)
        {
            Writer.Write(MsgPackFormat.INT32.Mask());
            Writer.Write(IPAddress.HostToNetworkOrder(n));
        }

        public void Pack(Int64 n)
        {
            Writer.Write(MsgPackFormat.INT64.Mask());
            Writer.Write(IPAddress.HostToNetworkOrder(n));
        }

        public void Pack(Single n)
        {
            Writer.Write(MsgPackFormat.FLOAT.Mask());
            if (BitConverter.IsLittleEndian)
            {
                Writer.Write(BitConverter.GetBytes(n).Reverse().ToArray());
            }
            else
            {
                Writer.Write(n);
            }
        }

        public void Pack(Double n)
        {
            Writer.Write(MsgPackFormat.DOUBLE.Mask());
            if (BitConverter.IsLittleEndian)
            {
                Writer.Write(BitConverter.GetBytes(n).Reverse().ToArray());
            }
            else
            {
                Writer.Write(n);
            }
        }

        public void Pack(String s)
        {
            Pack(s, Encoding.UTF8);
        }

        public void Pack(String s, Encoding e)
        {
            var bytes = e.GetBytes(s);
            var size = (UInt32)bytes.Count();
            if (size < 32)
            {
                Writer.Write((Byte)(MsgPackFormat.FIX_STR.Mask() | size));
            }
            else if (size <= Byte.MaxValue)
            {
                Writer.Write(MsgPackFormat.STR8.Mask());
                Writer.Write((Byte)(size & 0xff));
            }
            else if (size <= UInt16.MaxValue)
            {
                Writer.Write(MsgPackFormat.STR16.Mask());
                Writer.Write((UInt16)size);
            }
            else
            {
                Writer.Write(MsgPackFormat.STR32.Mask());
                Writer.Write(size);
            }
            Writer.Write(bytes);
        }

        public void Pack(IEnumerable<Byte> bytes)
        {
            var size = (UInt32)bytes.Count();
            if (size <= Byte.MaxValue)
            {
                Writer.Write(MsgPackFormat.BIN8.Mask());
                Writer.Write((Byte)(size & 0xff));
            }
            else if (size <= UInt16.MaxValue)
            {
                Writer.Write(MsgPackFormat.BIN16.Mask());
                Writer.Write((UInt16)size);
            }
            else
            {
                Writer.Write(MsgPackFormat.BIN32.Mask());
                Writer.Write(size);
            }
            Writer.Write(bytes.ToArray());
        }

        public void Pack_Array(Int32 n)
        {
            if (n < 0x0F)
            {
                Writer.Write((Byte)(MsgPackFormat.FIX_ARRAY.Mask() | n));
            }
            else if (n < 0xFFFF)
            {
                Writer.Write(MsgPackFormat.ARRAY16.Mask());
                Writer.Write((Byte)(n >> 8 & 0xff));
                Writer.Write((Byte)(n & 0xff));
            }
            else
            {
                throw new NotImplementedException("not implemented");
            }
        }

        public void Pack_Map(Int32 n)
        {
            if (n < 0x0F)
            {
                Writer.Write((Byte)(MsgPackFormat.FIX_MAP.Mask() | n));
            }
            else if (n < 0xFFFF)
            {
                Writer.Write(MsgPackFormat.MAP16.Mask());
                Writer.Write((Byte)(n >> 8 & 0xff));
                Writer.Write((Byte)(n & 0xff));
            }
            else
            {
                throw new NotImplementedException("not implemented");
            }
        }

        public void Pack(Object o)
        {
            if (Object.ReferenceEquals(null, o))
            {
                PackNil();
                return;
            }

            var type = o.GetType();
            if (TypeMap.ContainsKey(type))
            {
                TypeMap[type](this, o);
                return;
            }

            // 汎用
            {
                var pilist = type.GetProperties(
                    BindingFlags.Public | BindingFlags.Instance).Where(pi =>
                {
                    var nsAttrs = pi.GetCustomAttributes(typeof(NonSerializedAttribute), false);
                    return nsAttrs.Length == 0;
                }).ToArray();

                if (pilist.Length > 0)
                {
                    Pack_Map(pilist.Length);
                    foreach (var pi in pilist)
                    {
                        Pack(pi.Name);
                        var value = pi.GetValue(o, null);
                        Pack(value);
                    }
                    return;
                }
            }

            throw new InvalidOperationException();
        }
    }
}
