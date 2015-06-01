using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


/// LINQでParserを組み立てる
/// http://karino2.livejournal.com/277374.html
namespace NMPUtil.MsgPack
{
    public struct Input
    {
        ArraySegment<Byte> m_buffer;

        public Input(Byte[] buffer)
            : this(buffer, 0, buffer.Length)
        {
        }

        public Input(Byte[] buffer, int offset, int count)
        {
            m_buffer = new ArraySegment<byte>(buffer, offset, count);
        }

        public Int32 Count
        {
            get
            {
                return m_buffer.Count;
            }
        }

        public IEnumerable<Byte> Values
        {
            get
            {
                return m_buffer;
            }
        }

        public Input Advance(int d = 1)
        {
            return new Input(m_buffer.Array, m_buffer.Offset + d, m_buffer.Count - d);
        }

        #region Singed
        public SByte SByte()
        {
            return (SByte)Values.First();
        }

        public Int16 ReadInt16()
        {
            return IPAddress.NetworkToHostOrder(
                BitConverter.ToInt16(Values.Take(2).ToArray(), 0));
        }

        public Int32 ReadInt32()
        {
            return IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(Values.Take(4).ToArray(), 0));
        }

        public Int64 ReadInt64()
        {
            return IPAddress.NetworkToHostOrder(
                BitConverter.ToInt64(Values.Take(8).ToArray(), 0));
        }
        #endregion

        #region Unsinged
        public Byte ReadByte()
        {
            return Values.First();
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
        #endregion

        #region Float
        public Single ReadSingle()
        {
            var b = Values.Take(4);
            if (BitConverter.IsLittleEndian)
            {
                b = b.Reverse();
            }
            return BitConverter.ToSingle(b.ToArray(), 0);
        }

        public Double ReadDouble()
        {
            var b = Values.Take(8);
            if (BitConverter.IsLittleEndian)
            {
                b = b.Reverse();
            }
            return BitConverter.ToDouble(b.ToArray(), 0);
        }
        #endregion
    }

    public interface IResult<out T>
    {
        T Value { get; }
        Input Reminder { get; }
        bool WasSuccess { get; }
    }
    public class Result<T> : IResult<T>
    {
        public T Value { get; set; }
        public Input Reminder { get; set; }
        public bool WasSuccess { get; set; }
        public static IResult<T> Success(T val, Input rem)
        {
            return new Result<T>() { Value = val, Reminder = rem, WasSuccess = true };
        }
        public static IResult<T> Fail(Input rem)
        {
            return new Result<T>() { Value = default(T), Reminder = rem, WasSuccess = false };
        }
    }

    public delegate IResult<T> MsgPackParser<out T>(Input input);

    public static class BParse
    {
        public static MsgPackParser<U> Select<T, U>(this MsgPackParser<T> first, Func<T, U> convert)
        {
            return i =>
            {
                var res = first(i);
                if (res.WasSuccess)
                    return Result<U>.Success(convert(res.Value), res.Reminder);
                return Result<U>.Fail(i);
            };
        }
        public static MsgPackParser<V> SelectMany<T, U, V>(
                    this MsgPackParser<T> parser,
                    Func<T, MsgPackParser<U>> selector,
                    Func<T, U, V> projector)
        {
            return (i) =>
            {
                var res = parser(i);
                if (res.WasSuccess)
                {
                    var parser2 = selector(res.Value);
                    return parser2.Select(u => projector(res.Value, u))(res.Reminder);
                }
                return Result<V>.Fail(i);
            };
        }
    }

    public static class MsgPackParse
    {
        public static MsgPackParser<Byte> Byte()
        {
            return i =>
            {
                const int size = 1;
                if (i.Count < size) return Result<byte>.Fail(i);
                return Result<byte>.Success(i.ReadByte(), i.Advance(size));
            };
        }

        public static MsgPackParser<UInt16> UInt16()
        {
            return i =>
            {
                const int size = 2;
                if (i.Count < size) return Result<UInt16>.Fail(i);
                return Result<UInt16>.Success(i.ReadUInt16(), i.Advance(size));
            };
        }

        public static MsgPackParser<UInt32> UInt32()
        {
            return i =>
            {
                const int size = 4;
                if (i.Count < size) return Result<UInt32>.Fail(i);
                return Result<UInt32>.Success(i.ReadUInt32(), i.Advance(size));
            };
        }

        public static MsgPackParser<MsgPackFormat> Format(Byte headbyte)
        {
            return i =>
            {
                var t = (MsgPackFormat)headbyte;
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
                    case MsgPackFormat.BIN8:
                    case MsgPackFormat.BIN16:
                    case MsgPackFormat.BIN32:
                    case MsgPackFormat.STR8:
                    case MsgPackFormat.STR16:
                    case MsgPackFormat.STR32:
                    case MsgPackFormat.ARRAY16:
                    case MsgPackFormat.ARRAY32:
                    case MsgPackFormat.MAP16:
                    case MsgPackFormat.MAP32:
                        return Result<MsgPackFormat>.Success(t, i);

                    default:
                        {
                            if (headbyte < MsgPackFormat.FIX_MAP.Mask())
                            {
                                return Result<MsgPackFormat>.Success(MsgPackFormat.POSITIVE_FIXNUM, i);
                            }
                            else if (headbyte < MsgPackFormat.FIX_ARRAY.Mask())
                            {
                                return Result<MsgPackFormat>.Success(MsgPackFormat.FIX_MAP, i);
                            }
                            else if (headbyte < MsgPackFormat.FIX_STR.Mask())
                            {
                                return Result<MsgPackFormat>.Success(MsgPackFormat.FIX_ARRAY, i);
                            }
                            else if (headbyte < MsgPackFormat.NIL.Mask())
                            {
                                return Result<MsgPackFormat>.Success(MsgPackFormat.FIX_STR, i);
                            }
                            else if (headbyte >= MsgPackFormat.NEGATIVE_FIXNUM.Mask())
                            {
                                return Result<MsgPackFormat>.Success(MsgPackFormat.NEGATIVE_FIXNUM, i);
                            }
                            else
                            {
                                return Result<MsgPackFormat>.Fail(i);
                            }
                        }
                }
            };
        }

        public static MsgPackParser<UInt32> MemberCount(Byte headbyte, MsgPackFormat format)
        {
            return i =>
            {
                switch (format)
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
                        return Result<UInt32>.Success(0, i);

                    case MsgPackFormat.STR8:
                    case MsgPackFormat.BIN8:
                        {
                            var parser = from count in MsgPackParse.Byte()
                                         select count
                                       ;
                            var result = parser(i);
                            return new Result<UInt32>
                            {
                                Value = result.Value,
                                Reminder = result.Reminder,
                                WasSuccess = result.WasSuccess,
                            };
                        }

                    case MsgPackFormat.MAP16:
                    case MsgPackFormat.ARRAY16:
                    case MsgPackFormat.STR16:
                    case MsgPackFormat.BIN16:
                        {
                            var parser = from count in MsgPackParse.UInt16()
                                         select count
                                       ;
                            var result = parser(i);
                            return new Result<UInt32>
                            {
                                Value = result.Value,
                                Reminder = result.Reminder,
                                WasSuccess = result.WasSuccess,
                            };
                        }

                    case MsgPackFormat.MAP32:
                    case MsgPackFormat.ARRAY32:
                    case MsgPackFormat.STR32:
                    case MsgPackFormat.BIN32:
                        {
                            var parser = from count in MsgPackParse.UInt32()
                                         select count
                                       ;
                            var result = parser(i);
                            return new Result<UInt32>
                            {
                                Value = result.Value,
                                Reminder = result.Reminder,
                                WasSuccess = result.WasSuccess,
                            };
                        }

                    default:
                        {

                            if (headbyte < MsgPackFormat.FIX_MAP.Mask())
                            {
                                return Result<UInt32>.Success(0, i);
                            }
                            else if (headbyte < MsgPackFormat.FIX_ARRAY.Mask())
                            {
                                return Result<UInt32>.Success(
                                (UInt32)(headbyte & Convert.ToByte("00001111", 2)), i);
                            }
                            else if (headbyte < MsgPackFormat.FIX_STR.Mask())
                            {
                                return Result<UInt32>.Success(
                                 (UInt32)(headbyte & Convert.ToByte("00001111", 2)), i);
                            }
                            else if (headbyte < MsgPackFormat.NIL.Mask())
                            {
                                return Result<UInt32>.Success(
                                 (UInt32)(headbyte & Convert.ToByte("00011111", 2)), i);
                            }
                            else if (headbyte >= MsgPackFormat.NEGATIVE_FIXNUM.Mask())
                            {
                                return Result<UInt32>.Success(0, i);
                            }
                            else
                            {
                                return Result<UInt32>.Fail(i);
                            }
                        }
                }

            };
        }

        public static MsgPackParser<MsgPackHeader> Header()
        {
            return from headbyte in MsgPackParse.Byte()
                   from format in MsgPackParse.Format(headbyte)
                   from membercount in MsgPackParse.MemberCount(headbyte, format)
                   select new MsgPackHeader
                   {
                       HeadByte = headbyte,
                       Format = format,
                       MemberCount = membercount,
                   };
        }
    }
}
