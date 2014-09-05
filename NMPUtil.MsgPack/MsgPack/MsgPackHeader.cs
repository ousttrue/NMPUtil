using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMPUtil.MsgPack
{
    public class MsgPackHeader
    {
        public Byte HeadByte
        {
            get;
            private set;
        }

        public MsgPackFormat Format
        {
            get;
            private set;
        }

        public Boolean IsNil
        {
            get
            {
                return Format == MsgPackFormat.NIL;
            }
        }

        public Boolean IsArray
        {
            get
            {
                switch (Format)
                {
                    case MsgPackFormat.FIX_ARRAY:
                    case MsgPackFormat.ARRAY16:
                    case MsgPackFormat.ARRAY32:
                        return true;

                    default:
                        return false;
                }
            }
        }

        public Boolean IsMap
        {
            get
            {
                switch (Format)
                {
                    case MsgPackFormat.FIX_MAP:
                    case MsgPackFormat.MAP16:
                    case MsgPackFormat.MAP32:
                        return true;

                    default:
                        return false;
                }
            }
        }

        // for array / map / str / bin
        UInt32 _memberCount;
        public UInt32 MemberCount
        {
            get
            {
                return _memberCount;
            }
            private set
            {
                _memberCount = value;
            }
        }

        public SByte FixNum
        {
            get
            {
                switch (Format)
                {
                    case MsgPackFormat.POSITIVE_FIXNUM:
                    case MsgPackFormat.NEGATIVE_FIXNUM:
                        return (SByte)HeadByte;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public MsgPackHeader(NetworkEndianArraySegmentReader reader)
        {
            this.HeadByte = reader.ReadByte();

            var t = (MsgPackFormat)HeadByte;
            switch (t)
            {
                case MsgPackFormat.UINT8:
                case MsgPackFormat.UINT16:
                case MsgPackFormat.UINT32:
                case MsgPackFormat.UINT64:
                case MsgPackFormat.INT8:
                case MsgPackFormat.INT16:
                case MsgPackFormat.INT32:
                case MsgPackFormat.INT64:
                case MsgPackFormat.NIL:
                case MsgPackFormat.TRUE:
                case MsgPackFormat.FALSE:
                case MsgPackFormat.FLOAT:
                case MsgPackFormat.DOUBLE:
                    Format = t;
                    break;

                case MsgPackFormat.BIN8:
                    Format = t;
                    MemberCount = reader.ReadByte();
                    break;

                case MsgPackFormat.BIN16:
                    Format = t;
                    MemberCount = reader.ReadUInt16();
                    break;

                case MsgPackFormat.BIN32:
                    Format = t;
                    MemberCount = reader.ReadUInt32();
                    break;

                case MsgPackFormat.STR8:
                    Format = t;
                    MemberCount = reader.ReadByte();
                    break;

                case MsgPackFormat.STR16:
                    Format = t;
                    MemberCount = reader.ReadUInt16();
                    break;

                case MsgPackFormat.STR32:
                    Format = t;
                    MemberCount = reader.ReadUInt32();
                    break;

                case MsgPackFormat.ARRAY16:
                    Format = t;
                    MemberCount = reader.ReadUInt16();
                    break;

                case MsgPackFormat.ARRAY32:
                    Format = t;
                    MemberCount = reader.ReadUInt32();
                    break;

                case MsgPackFormat.MAP16:
                    Format = t;
                    MemberCount = reader.ReadUInt16();
                    break;

                case MsgPackFormat.MAP32:
                    Format = t;
                    MemberCount = reader.ReadUInt32();
                    break;

                default:
                    {

                        if (HeadByte < MsgPackFormat.FIX_MAP.Mask())
                        {
                            Format = MsgPackFormat.POSITIVE_FIXNUM;
                        }
                        else if (HeadByte < MsgPackFormat.FIX_ARRAY.Mask())
                        {
                            Format = MsgPackFormat.FIX_MAP;
                            MemberCount = (UInt32)(HeadByte & Convert.ToByte("00001111", 2));
                        }
                        else if (HeadByte < MsgPackFormat.FIX_STR.Mask())
                        {
                            Format = MsgPackFormat.FIX_ARRAY;
                            MemberCount = (UInt32)(HeadByte & Convert.ToByte("00001111", 2));
                        }
                        else if (HeadByte < MsgPackFormat.NIL.Mask())
                        {
                            Format = MsgPackFormat.FIX_STR;
                            MemberCount = (UInt32)(HeadByte & Convert.ToByte("00011111", 2));
                        }
                        else if (HeadByte >= MsgPackFormat.NEGATIVE_FIXNUM.Mask())
                        {
                            Format = MsgPackFormat.NEGATIVE_FIXNUM;
                        }
                        else
                        {
                            throw new InvalidOperationException("unknown HeadByte " + HeadByte);
                        }
                        break;
                    }
            }
        }

    }
}
