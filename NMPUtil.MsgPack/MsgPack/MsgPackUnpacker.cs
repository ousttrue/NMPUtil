﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NMPUtil.MsgPack
{
    public partial class MsgPackUnpacker
    {
        #region exception
        public class InvalidByteException : Exception
        {
            public InvalidByteException()
            {
            }
            public InvalidByteException(string message)
                : base(message)
            {
            }
            public InvalidByteException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public class NotEnoughByteException : Exception
        {
            public NotEnoughByteException()
            {
            }
            public NotEnoughByteException(string message)
                : base(message)
            {
            }
            public NotEnoughByteException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }
        #endregion

        #region read
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

        MsgPackUnpacker GetSubUnpacker()
        {
            return new MsgPackUnpacker(GetRemain());
        }

        public Byte HeadByte
        {
            get { return _view.First(); }
        }

        SByte ReadSByte()
        {
            var b = _view.Skip(Pos).First();
            Advance(1);
            return (SByte)b;
        }

        Int16 ReadInt16()
        {
            var b = _view.Skip(Pos).Take(2);
            Advance(2);
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(b.ToArray(), 0));
        }

        Int32 ReadInt32()
        {
            var b = _view.Skip(Pos).Take(4);
            Advance(4);
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(b.ToArray(), 0));
        }

        Int64 ReadInt64()
        {
            var b = _view.Skip(Pos).Take(8);
            Advance(8);
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(b.ToArray(), 0));
        }

        Byte ReadByte()
        {
            var b = _view.Skip(Pos).First();
            Advance(1);
            return b;
        }

        UInt16 ReadUInt16()
        {
            return (UInt16)ReadInt16();
        }

        UInt32 ReadUInt32()
        {
            return (UInt32)ReadInt32();
        }

        UInt64 ReadUInt64()
        {
            return (UInt64)ReadInt64();
        }

        Single ReadSingle()
        {
            var b = _view.Skip(Pos).Take(4);
            if (BitConverter.IsLittleEndian)
            {
                b = b.Reverse();
            }
            Advance(4);
            return BitConverter.ToSingle(b.ToArray(), 0);
        }

        Double ReadDouble()
        {
            var b = _view.Skip(Pos).Take(8);
            if (BitConverter.IsLittleEndian)
            {
                b = b.Reverse();
            }
            Advance(8);
            return BitConverter.ToDouble(b.ToArray(), 0);
        }

        IEnumerable<Byte> ReadBytes(Int32 size)
        {
            var b = _view.Skip(Pos).Take(size);
            Advance(size);
            return b;
        }
        #endregion

        #region format
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
        public UInt32 MemberCount
        {
            get;
            private set;
        }
        #endregion

        public Encoding StrEncoding
        {
            get;
            set;
        }

        public MsgPackUnpacker(IEnumerable<Byte> bytes)
            : this(new ArraySegment<Byte>(bytes.ToArray()))
        {}

        public MsgPackUnpacker(Byte[] bytes)
            : this(new ArraySegment<Byte>(bytes))
        {}

        public MsgPackUnpacker(ArraySegment<Byte> view)
        {
            this.StrEncoding = Encoding.UTF8;
            this._view = view;
            ReadByte();

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
                    return;

                case MsgPackFormat.BIN8:
                    Format = t;
                    MemberCount = ReadByte();
                    return;

                case MsgPackFormat.BIN16:
                    Format = t;
                    MemberCount = ReadUInt16();
                    return;

                case MsgPackFormat.BIN32:
                    Format = t;
                    MemberCount = ReadUInt32();
                    return;

                case MsgPackFormat.STR8:
                    Format = t;
                    MemberCount = ReadByte();
                    return;

                case MsgPackFormat.STR16:
                    Format = t;
                    MemberCount = ReadUInt16();
                    return;

                case MsgPackFormat.STR32:
                    Format = t;
                    MemberCount = ReadUInt32();
                    return;

                case MsgPackFormat.ARRAY16:
                    Format = t;
                    MemberCount = ReadUInt16();
                    return;

                case MsgPackFormat.ARRAY32:
                    throw new NotImplementedException();

                case MsgPackFormat.MAP16:
                    Format = t;
                    MemberCount = ReadUInt16();
                    return;

                case MsgPackFormat.MAP32:
                    throw new NotImplementedException();
            }

            if (HeadByte < MsgPackFormat.FIX_MAP.Mask())
            {
                Format = MsgPackFormat.POSITIVE_FIXNUM;
                return;
            }
            else if (HeadByte < MsgPackFormat.FIX_ARRAY.Mask())
            {
                Format = MsgPackFormat.FIX_MAP;
                MemberCount = (UInt32)(HeadByte & Convert.ToByte("00001111", 2));
                return;
            }
            else if (HeadByte < MsgPackFormat.FIX_STR.Mask())
            {
                Format = MsgPackFormat.FIX_ARRAY;
                MemberCount = (UInt32)(HeadByte & Convert.ToByte("00001111", 2));
                return;
            }
            else if (HeadByte < MsgPackFormat.NIL.Mask())
            {
                Format = MsgPackFormat.FIX_STR;
                MemberCount = (UInt32)(HeadByte & Convert.ToByte("00011111", 2));
                return;
            }
            else if (HeadByte >= MsgPackFormat.NEGATIVE_FIXNUM.Mask())
            {
                Format = MsgPackFormat.NEGATIVE_FIXNUM;
                return;
            }

            throw new InvalidOperationException("UNKNOWN FIRST BYTE");
        }

        public void UnpackGeneric(Object o, Type type)
        {
            var mi = GetType().GetMethod("Unpack", BindingFlags.Instance | BindingFlags.Public);
            var gmi = mi.MakeGenericMethod(new Type[] { type });
            gmi.Invoke(this, new Object[] { o });
        }

        public void UnpackSub<T>(ref T v)
        {
            var sub = GetSubUnpacker();
            sub.Unpack(ref v);
            Advance(sub.Pos);
        }

        public ArraySegment<Byte> Unpack<T>(ref T v)
        {
            //var t = v.GetType();
            var t = typeof(T);
            switch (Format)
            {
                case MsgPackFormat.UINT8:
                    v = (T)Convert.ChangeType(ReadByte(), t);
                    break;

                case MsgPackFormat.UINT16:
                    v = (T)Convert.ChangeType(ReadUInt16(), t);
                    break;

                case MsgPackFormat.UINT32:
                    v = (T)Convert.ChangeType(ReadUInt32(), t);
                    break;

                case MsgPackFormat.UINT64:
                    v = (T)Convert.ChangeType(ReadUInt64(), t);
                    break;

                case MsgPackFormat.INT8:
                    v = (T)Convert.ChangeType(ReadSByte(), t);
                    break;

                case MsgPackFormat.INT16:
                    v = (T)Convert.ChangeType(ReadInt16(), t);
                    break;

                case MsgPackFormat.INT32:
                    v = (T)Convert.ChangeType(ReadInt32(), t);
                    break;

                case MsgPackFormat.INT64:
                    v = (T)Convert.ChangeType(ReadInt64(), t);
                    break;

                case MsgPackFormat.NIL:
                    v = (T)Convert.ChangeType(null, t);
                    break;

                case MsgPackFormat.TRUE:
                    v = (T)Convert.ChangeType(true, t);
                    break;

                case MsgPackFormat.FALSE:
                    v = (T)Convert.ChangeType(false, t);
                    break;

                case MsgPackFormat.FLOAT:
                    v = (T)Convert.ChangeType(ReadSingle(), t);
                    break;

                case MsgPackFormat.DOUBLE:
                    v = (T)Convert.ChangeType(ReadDouble(), t);
                    break;

                case MsgPackFormat.POSITIVE_FIXNUM:
                    {
                        var o = HeadByte;
                        v = (T)Convert.ChangeType(o, t);
                    }
                    break;

                case MsgPackFormat.NEGATIVE_FIXNUM:
                    {
                        var o = (SByte)((HeadByte & MsgPackFormat.NEGATIVE_FIXNUM.InvMask()) - 32);
                        v = (T)Convert.ChangeType(o, t);
                    }
                    break;

                // str
                case MsgPackFormat.FIX_STR:
                case MsgPackFormat.STR8:
                case MsgPackFormat.STR16:
                case MsgPackFormat.STR32:
                    {
                        var buf = ReadBytes((Int32)MemberCount);
                        v = (T)Convert.ChangeType(StrEncoding.GetString(buf.ToArray()), t);
                    }
                    break;

                // bin
                case MsgPackFormat.BIN8:
                case MsgPackFormat.BIN16:
                case MsgPackFormat.BIN32:
                    {
                        var buf = ReadBytes((Int32)MemberCount);
                        if (t == typeof(String))
                        {
                            v = (T)Convert.ChangeType(Encoding.UTF8.GetString(buf.ToArray()), t);
                        }
                        else if (t.IsEnum)
                        {
                            String enumName = Encoding.UTF8.GetString(buf.ToArray());
                            v = (T)Enum.Parse(t, enumName);
                        }
                        else
                        {
                            if (t == typeof(Object))
                            {
                                // fail safe
                                v = (T)(Object)buf.ToArray();
                            }
                            else
                            {
                                v = (T)Convert.ChangeType(buf.ToArray(), t);
                            }
                        }
                    }
                    break;

                // array types
                case MsgPackFormat.FIX_ARRAY:
                case MsgPackFormat.ARRAY16:
                case MsgPackFormat.ARRAY32:
                    UnpackArray(ref v);
                    break;

                // map types
                case MsgPackFormat.FIX_MAP:
                case MsgPackFormat.MAP16:
                case MsgPackFormat.MAP32:
                    UnpackMap(ref v);
                    break;

                default:
                    throw new InvalidOperationException("NOT REACH HERE !");
            }

            return GetRemain();
        }

        public class Unpacker
        {
            public delegate void Obj<in T>(T target, MsgPackUnpacker unpacker, UInt32 count);
            public delegate void Ref<T>(ref T target, MsgPackUnpacker unpacker, UInt32 count);
        }

        #region UnpackArray
        static Dictionary<Type, Object> _unpackArrayMapVal =
            new Dictionary<Type, Object>();
        static Dictionary<Type, Object> _unpackArrayMapRef =
            new Dictionary<Type, Object>
        {
            {
                  typeof(Object[])
                , (Object)(Unpacker.Obj<Object[]>)( 
                (Object[] target, MsgPackUnpacker unpacker, UInt32 count)=>

                {
                    //var target = o as Object[];
                    if(target.Length<count){
                        throw new InvalidOperationException();
                    }
                    for (uint i = 0; i < count; ++i)
                    {
                        var sub = unpacker.GetSubUnpacker();
                        if (sub.IsArray)
                        {
                            var val=new List<Object>();
                            sub.Unpack(ref val);
                            unpacker.Advance(sub.Pos);
                            target[i] = val;
                        }
                        else if (sub.IsMap)
                        {
                            var val=new Dictionary<String, Object>();
                            sub.Unpack(ref val);
                            unpacker.Advance(sub.Pos);
                            target[i] = val;
                        }
                        else
                        {
                            var val = new Object();
                            sub.Unpack(ref val);
                            unpacker.Advance(sub.Pos);
                            target[i] = val;
                        }
                    }
                    Console.WriteLine(target);
                })
            }

            , {
                typeof(IList<Object>)
                , (Object)(Unpacker.Obj<IList<Object>>)(
                (IList<Object> target, MsgPackUnpacker unpacker, UInt32 count)=>

                {
                    for (uint i = 0; i < count; ++i)
                    {
                        {
                            var sub = unpacker.GetSubUnpacker();
                            if (sub.IsArray)
                            {
                                var val = new List<Object>();
                                sub.Unpack(ref val);
                                unpacker.Advance(sub.Pos);
                                target.Add(val);
                            }
                            else if (sub.IsMap)
                            {
                                var val = new Dictionary<String, Object>();
                                sub.Unpack(ref val);
                                unpacker.Advance(sub.Pos);
                                target.Add(val);
                            }
                            else
                            {
                                var val = new Object();
                                sub.Unpack(ref val);
                                unpacker.Advance(sub.Pos);
                                target.Add(val);
                            }
                        }
                    }
                    Console.WriteLine(target);
                }
                )
            }

        };
        static public void AddUnpackArray<T>(Unpacker.Obj<T> unpacker) where T : class
        {
            _unpackArrayMapRef.Add(typeof(T), unpacker);
        }
        static public void AddUnpackArray<T>(Unpacker.Ref<T> unpacker) where T : struct
        {
            _unpackArrayMapVal.Add(typeof(T), unpacker);
        }

        ArraySegment<Byte> UnpackArray<T>(ref T t)
        {
            //var type = t.GetType();
            var type = typeof(T);
            if (type.IsValueType)
            {
                foreach (var kv in _unpackArrayMapVal)
                {
                    if (kv.Key.IsAssignableFrom(type))
                    {
                        var unpackMap = (Unpacker.Ref<T>)kv.Value;
                        unpackMap(ref t, this, MemberCount);
                        return GetRemain();
                    }
                }
            }
            else
            {
                foreach (var kv in _unpackArrayMapRef)
                {
                    if (kv.Key.IsAssignableFrom(type))
                    {
                        var unpackMap = (Unpacker.Obj<T>)kv.Value;
                        unpackMap(t, this, MemberCount);
                        return GetRemain();
                    }
                }
            }


            throw new InvalidOperationException();
        }
        #endregion

        #region UnpackMap
        static Dictionary<Type, Object> _unpackMapMapVal =
            new Dictionary<Type, Object>();
        static Dictionary<Type, Object> _unpackMapMapRef =
            new Dictionary<Type, Object>
        {
            {
                typeof(IDictionary<String, Object>)
                , (Object)(Unpacker.Obj<IDictionary<String, Object>>)(
                (IDictionary<String, Object> o, MsgPackUnpacker unpacker, UInt32 count)=>

                {
                    var target = o as IDictionary<String, Object>;
                    for (uint i = 0; i < count; ++i)
                    {
                        var key=String.Empty;
                        {
                            var sub = unpacker.GetSubUnpacker();
                            sub.Unpack(ref key);
                            unpacker.Advance(sub.Pos);
                        }
                        {
                            var sub = unpacker.GetSubUnpacker();
                            if (sub.IsArray)
                            {
                                var val = new List<Object>();
                                sub.Unpack(ref val);
                                unpacker.Advance(sub.Pos);
                                target.Add(key, val);
                            }
                            else if (sub.IsMap)
                            {
                                var val = new Dictionary<String, Object>();
                                sub.Unpack(ref val);
                                unpacker.Advance(sub.Pos);
                                target.Add(key, val);
                            }
                            else
                            {
                                var val = new Object();
                                sub.Unpack(ref val);
                                unpacker.Advance(sub.Pos);
                                target.Add(key, val);
                            }
                        }
                    }
                    Console.WriteLine(target);
                })
            }
            
            , {
                typeof(IDictionary<Object, Object>)
                , (Object)(Unpacker.Obj<IDictionary<Object, Object>>)(
                (IDictionary<Object, Object> target, MsgPackUnpacker unpacker, UInt32 count)=>

                {
                    for (uint i = 0; i < count; ++i)
                    {
                        var key=String.Empty;
                        {
                            var sub = unpacker.GetSubUnpacker();
                            sub.Unpack(ref key);
                            unpacker.Advance(sub.Pos);
                        }
                        {
                            var sub = unpacker.GetSubUnpacker();
                            if (sub.IsArray)
                            {
                                var val = new List<Object>();
                                sub.Unpack(ref val);
                                unpacker.Advance(sub.Pos);
                                target.Add(key, val);
                            }
                            else if (sub.IsMap)
                            {
                                var val = new Dictionary<Object, Object>();
                                sub.Unpack(ref val);
                                unpacker.Advance(sub.Pos);
                                target.Add(key, val);
                            }
                            else
                            {
                                var val = new Object();
                                sub.Unpack(ref val);
                                unpacker.Advance(sub.Pos);
                                target.Add(key, val);
                            }
                        }
                    }
                    Console.WriteLine(target);
                }
                )
            }

            , {
                  typeof(Object)
                , (Object)(Unpacker.Obj<Object>)( 
                (Object o, MsgPackUnpacker unpacker, UInt32 count)=>

                {
                    var type = o.GetType();
                    for (UInt32 i = 0; i < count; ++i)
                    {
                        String key = "";
                        {
                            var sub = unpacker.GetSubUnpacker();
                            sub.Unpack(ref key);
                            unpacker.Advance(sub.Pos);
                        }
                        var pi = type.GetProperty(key);
                        {
                            var sub = unpacker.GetSubUnpacker();
                            if (pi != null){
                                var v = pi.GetValue(o);
                                if (v == null)
                                {
                                    if (pi.PropertyType == typeof(String))
                                    {
                                        v = String.Empty;
                                    }
                                    else { 
                                        v = Activator.CreateInstance(pi.PropertyType);
                                    }
                                }
                                sub.UnpackGeneric(v, pi.PropertyType);
                                unpacker.Advance(sub.Pos);
                                pi.SetValue(o, v, null);
                            }
                            else{
                                var v=new Object();
                                sub.Unpack(ref v);
                                unpacker.Advance(sub.Pos);
                                pi.SetValue(o, v, null);
                            }
                        }
                    }
                }
                )
           }
        };
        static public void AddUnpackMap<T>(Unpacker.Obj<T> unpacker)where T: class
        {
            _unpackMapMapRef.Add(typeof(T), unpacker);
        }
        static public void AddUnpackMap<T>(Unpacker.Ref<T> unpacker) where T : struct
        {
            _unpackMapMapVal.Add(typeof(T), unpacker);
        }

        ArraySegment<Byte> UnpackMap<T>(ref T t)
        {
            //var type = t.GetType();
            var type = typeof(T);
            if (type.IsValueType)
            {
                foreach (var kv in _unpackMapMapVal)
                {
                    if (kv.Key.IsAssignableFrom(type))
                    {
                        var unpackMap = (Unpacker.Ref<T>)kv.Value;
                        unpackMap(ref t, this, MemberCount);
                        return GetRemain();
                    }
                }

                // add generics
                throw new InvalidOperationException();
            }
            else
            {
                foreach (var kv in _unpackMapMapRef)
                {
                    if (kv.Key.IsAssignableFrom(type))
                    {
                        var unpackMap = (Unpacker.Obj<T>)kv.Value;
                        unpackMap(t, this, MemberCount);
                        return GetRemain();
                    }
                }
            }

            throw new InvalidOperationException();
        }
        #endregion
    }
}
