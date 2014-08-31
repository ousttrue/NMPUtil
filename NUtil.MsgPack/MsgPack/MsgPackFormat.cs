using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUtil.MsgPack
{
    public enum MsgPackFormat
    {
        POSITIVE_FIXNUM = 0,
        FIX_MAP = 0x80,
        FIX_ARRAY = 0x90,
        FIX_RAW = 0xA0,

        NIL = 0xC0,

        FALSE = 0xC2,
        TRUE = 0xC3,

        FLOAT = 0xCA,
        DOUBLE = 0xCB,
        UINT8 = 0xCC,
        UINT16 = 0xCD,
        UINT32 = 0xCE,
        UINT64 = 0xCF,
        INT8 = 0xD0,
        INT16 = 0xD1,
        INT32 = 0xD2,
        INT64 = 0xD3,

        RAW16 = 0xDA,
        RAW32 = 0xDB,
        ARRAY16 = 0xDC,
        ARRAY32 = 0xDD,
        MAP16 = 0xDE,
        MAP32 = 0xDF,
        NEGATIVE_FIXNUM = 0xE0, // 1110 0000
    }

    public static class MsgPackFormatMask
    {
        public static Byte Mask(this MsgPackFormat type)
        {
            return (Byte)type;
        }

        public static Byte InvMask(this MsgPackFormat type)
        {
            return (Byte)~type;
        }
    }
}
