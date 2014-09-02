using System;
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
        #region read
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

        public SubMsgPackUnpacker GetSubUnpacker()
        {
            return new SubMsgPackUnpacker(GetRemain(), this);
        }

        public Byte HeadByte
        {
            get;
            private set;
        }

        IEnumerable<Byte> ReadBytes(Int32 size)
        {
            if(Pos+size>_view.Count){
                throw new NotEnoughBytesException("ReadBytes "+size);
            }
            var b = _view.Skip(Pos).Take(size);
            Advance(size);
            return b;
        }

        SByte ReadSByte()
        {
            return (SByte)ReadBytes(1).First();
        }

        Int16 ReadInt16()
        {
            return IPAddress.NetworkToHostOrder(
                BitConverter.ToInt16(ReadBytes(2).ToArray(), 0));
        }

        Int32 ReadInt32()
        {
            return IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(ReadBytes(4).ToArray(), 0));
        }

        Int64 ReadInt64()
        {
            return IPAddress.NetworkToHostOrder(
                BitConverter.ToInt64(ReadBytes(8).ToArray(), 0));
        }

        Byte ReadByte()
        {
            return ReadBytes(1).First();
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
            var b = ReadBytes(4);
            if (BitConverter.IsLittleEndian)
            {
                b = b.Reverse();
            }
            return BitConverter.ToSingle(b.ToArray(), 0);
        }

        Double ReadDouble()
        {
            var b = ReadBytes(8);
            if (BitConverter.IsLittleEndian)
            {
                b = b.Reverse();
            }
            return BitConverter.ToDouble(b.ToArray(), 0);
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
        #endregion

        public Encoding StrEncoding
        {
            get;
            set;
        }

        public MsgPackUnpacker(IEnumerable<Byte> bytes, bool doParseHeadByte=true)
            : this(new ArraySegment<Byte>(bytes.ToArray()), doParseHeadByte)
        {}

        public MsgPackUnpacker(Byte[] bytes, bool doParseHeadByte = true)
            : this(new ArraySegment<Byte>(bytes), doParseHeadByte)
        {}

        public MsgPackUnpacker(ArraySegment<Byte> view, bool doParseHeadByte = true)
        {
            this.StrEncoding = Encoding.UTF8;
            this._view = view;

            if (doParseHeadByte)
            {
                ParseHeadByte();
            }
        }

        public MsgPackFormat ParseHeadByte()
        {
            HeadByte = ReadByte();

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
                    MemberCount = ReadByte();
                    break;

                case MsgPackFormat.BIN16:
                    Format = t;
                    MemberCount = ReadUInt16();
                    break;

                case MsgPackFormat.BIN32:
                    Format = t;
                    MemberCount = ReadUInt32();
                    break;

                case MsgPackFormat.STR8:
                    Format = t;
                    MemberCount = ReadByte();
                    break;

                case MsgPackFormat.STR16:
                    Format = t;
                    MemberCount = ReadUInt16();
                    break;

                case MsgPackFormat.STR32:
                    Format = t;
                    MemberCount = ReadUInt32();
                    break;

                case MsgPackFormat.ARRAY16:
                    Format = t;
                    MemberCount = ReadUInt16();
                    break;

                case MsgPackFormat.ARRAY32:
                    Format = t;
                    MemberCount = ReadUInt32();
                    break;

                case MsgPackFormat.MAP16:
                    Format = t;
                    MemberCount = ReadUInt16();
                    break;

                case MsgPackFormat.MAP32:
                    Format = t;
                    MemberCount = ReadUInt32();
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

            return Format;
        }

        public Object UnpackGeneric(Type type)
        {
            foreach (var mi in GetType().GetMethods())
            {
                if (mi.Name == "Unpack")
                {
                    if (mi.ReturnType == typeof(void))
                    {
                    }
                    else
                    {
                        var gmi = mi.MakeGenericMethod(new Type[] { type });
                        return gmi.Invoke(this, new Object[] { });
                    }
                }
            }
            throw new InvalidOperationException();
        }

        public void UnpackGeneric(ref Object o, Type type)
        {
            foreach (var mi in GetType().GetMethods())
            {
                if (mi.Name == "Unpack")
                {
                    {
                        if (mi.ReturnType == typeof(void))
                        {
                            var gmi = mi.MakeGenericMethod(new Type[] { type });
                            var args=new Object[] { o };
                            gmi.Invoke(this, args);
                            o = args[0];
                            return;
                        }
                        else
                        {
                        }
                    }
                }
            }
            throw new InvalidOperationException();
        }

        public T Unpack<T>()where T:struct
        {
            var t = typeof(T);
            switch (Format)
            {
                case MsgPackFormat.UINT8:
                    return (T)Convert.ChangeType(ReadByte(), t);

                case MsgPackFormat.UINT16:
                    return (T)Convert.ChangeType(ReadUInt16(), t);

                case MsgPackFormat.UINT32:
                    return (T)Convert.ChangeType(ReadUInt32(), t);

                case MsgPackFormat.UINT64:
                    return (T)Convert.ChangeType(ReadUInt64(), t);

                case MsgPackFormat.INT8:
                    return (T)Convert.ChangeType(ReadSByte(), t);

                case MsgPackFormat.INT16:
                    return (T)Convert.ChangeType(ReadInt16(), t);

                case MsgPackFormat.INT32:
                    return (T)Convert.ChangeType(ReadInt32(), t);
                    
                case MsgPackFormat.INT64:
                    return (T)Convert.ChangeType(ReadInt64(), t);

                case MsgPackFormat.NIL:
                    return (T)Convert.ChangeType(null, t);

                case MsgPackFormat.TRUE:
                    return (T)Convert.ChangeType(true, t);

                case MsgPackFormat.FALSE:
                    return (T)Convert.ChangeType(false, t);

                case MsgPackFormat.FLOAT:
                    return (T)Convert.ChangeType(ReadSingle(), t);

                case MsgPackFormat.DOUBLE:
                    return (T)Convert.ChangeType(ReadDouble(), t);

                case MsgPackFormat.POSITIVE_FIXNUM:
                    {
                        var o = HeadByte;
                        return (T)Convert.ChangeType(o, t);
                    }

                case MsgPackFormat.NEGATIVE_FIXNUM:
                    {
                        var o = (SByte)((HeadByte & MsgPackFormat.NEGATIVE_FIXNUM.InvMask()) - 32);
                        return (T)Convert.ChangeType(o, t);
                    }

                // str
                case MsgPackFormat.FIX_STR:
                case MsgPackFormat.STR8:
                case MsgPackFormat.STR16:
                case MsgPackFormat.STR32:
                    {
                        var buf = ReadBytes((Int32)MemberCount);
                        return (T)Convert.ChangeType(StrEncoding.GetString(buf.ToArray()), t);
                    }

                // bin
                case MsgPackFormat.BIN8:
                case MsgPackFormat.BIN16:
                case MsgPackFormat.BIN32:
                    {
                        var buf = ReadBytes((Int32)MemberCount);
                        if (t == typeof(String))
                        {
                            return (T)Convert.ChangeType(Encoding.UTF8.GetString(buf.ToArray()), t);
                        }
                        else if (t.IsEnum)
                        {
                            String enumName = Encoding.UTF8.GetString(buf.ToArray());
                            return (T)Enum.Parse(t, enumName);
                        }
                        else
                        {
                            if (t == typeof(Object))
                            {
                                // fail safe
                                return (T)(Object)buf.ToArray();
                            }
                            else
                            {
                                return (T)Convert.ChangeType(buf.ToArray(), t);
                            }
                        }
                    }

                // array types
                case MsgPackFormat.FIX_ARRAY:
                case MsgPackFormat.ARRAY16:
                case MsgPackFormat.ARRAY32:
                    return UnpackArray<T>();

                // map types
                case MsgPackFormat.FIX_MAP:
                case MsgPackFormat.MAP16:
                case MsgPackFormat.MAP32:
                    return UnpackMap<T>();

                default:
                    throw new InvalidOperationException("invalid format "+Format);
            }
        }

        public void Unpack<T>(ref T v)where T: class
        {
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
                    UnpackArray(v);
                    break;

                // map types
                case MsgPackFormat.FIX_MAP:
                case MsgPackFormat.MAP16:
                case MsgPackFormat.MAP32:
                    UnpackMap(v);
                    break;

                default:
                    throw new InvalidOperationException("invalid format "+Format);
            }
        }


        public delegate void UnpackerForReferenceTypeDelegate<in T>(T target, SubMsgPackUnpacker unpacker, UInt32 count);
        public delegate T UnpackerForValueTypeDelegate<T>(SubMsgPackUnpacker unpacker, UInt32 count);

        #region UnpackArray
        static Dictionary<Type, Object> _unpackArrayMapVal =
            new Dictionary<Type, Object>();
        static Dictionary<Type, Object> _unpackArrayMapRef =
            new Dictionary<Type, Object>();


        static public void AddUnpackArray<T>(UnpackerForReferenceTypeDelegate<T> unpacker) where T : class
        {
            _unpackArrayMapRef.Add(typeof(T), unpacker);
        }
        static public void AddUnpackArray<T>(UnpackerForValueTypeDelegate<T> unpacker) where T : struct
        {
            _unpackArrayMapVal.Add(typeof(T), unpacker);
        }

        T UnpackArray<T>() where T : struct
        {
            foreach (var kv in _unpackArrayMapVal)
            {
                if (kv.Key.IsAssignableFrom(typeof(T)))
                {
                    var handler = (UnpackerForValueTypeDelegate<T>)kv.Value;
                    using (var sub = GetSubUnpacker())
                    {
                        return handler(sub, MemberCount);
                    }
                }
            }

            foreach (var m in typeof(T).GetMethods())
            {
                var a = m.GetCustomAttribute<MsgPackArrayUnpackerAttribute>();
                if (a != null)
                {
                    var handler = (UnpackerForValueTypeDelegate<T>)m.CreateDelegate(typeof(UnpackerForValueTypeDelegate<T>));
                    using (var sub = GetSubUnpacker())
                    {
                        var t = handler(sub, MemberCount);
                        AddUnpackArray<T>(handler);
                        return t;
                    }
                }
            }

            throw new InvalidOperationException("no handler for "+typeof(T));
        }

        static public void UnpackArrayAsArray<T>(T[] array, SubMsgPackUnpacker unpacker, UInt32 count) where T : class
        {
            for (int i = 0; i < count; ++i)
            {
                unpacker.ParseHeadByte();
                if (array[i] == null)
                {
                    array[i] = Activator.CreateInstance<T>();
                }
                unpacker.Unpack(ref array[i]);
            }
        }

        static public void UnpackArrayAsList<T>(List<T> list, SubMsgPackUnpacker unpacker, UInt32 count) where T : class
        {
            for (int i = 0; i < count; ++i)
            {
                unpacker.ParseHeadByte();
                var o=Activator.CreateInstance<T>();
                unpacker.Unpack(ref o);
                list.Add(o);
            }
        }

        void UnpackArray<T>(T t) where T : class
        {
            foreach (var kv in _unpackArrayMapRef)
            {
                if (kv.Key.IsAssignableFrom(typeof(T)))
                {
                    using (var sub = GetSubUnpacker())
                    {
                        var handler = (UnpackerForReferenceTypeDelegate<T>)kv.Value;
                        handler(t, sub, MemberCount);
                        return;
                    }
                }
            }

            if (t is Array)
            {
                var et = typeof(T).GetElementType();
                var gmi = GetType().GetMethod("UnpackArrayAsArray");
                var mi = gmi.MakeGenericMethod(new Type[] { et });
                var handler = (UnpackerForReferenceTypeDelegate<T>)mi.CreateDelegate(typeof(UnpackerForReferenceTypeDelegate<T>));

                using (var sub = GetSubUnpacker())
                {
                    handler(t, sub, MemberCount);
                    AddUnpackArray<T>(handler);
                    return;
                }
            }

            if (t is IList)
            {
                var et = typeof(T).GetGenericArguments()[0];
                var gmi = GetType().GetMethod("UnpackArrayAsList");
                var mi = gmi.MakeGenericMethod(new Type[] { et });
                var handler = (UnpackerForReferenceTypeDelegate<T>)mi.CreateDelegate(typeof(UnpackerForReferenceTypeDelegate<T>));

                using (var sub = GetSubUnpacker())
                {
                    handler(t, sub, MemberCount);
                    AddUnpackArray<T>(handler);
                    return;
                }
            }

            throw new InvalidOperationException("no handle for " + typeof(T));
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
                , (Object)(UnpackerForReferenceTypeDelegate<IDictionary<String, Object>>)(
                (IDictionary<String, Object> o, SubMsgPackUnpacker unpacker, UInt32 count)=>

                {
                    var target = o as IDictionary<String, Object>;
                    for (uint i = 0; i < count; ++i)
                    {
                        var key=String.Empty;
                        {
                            unpacker.ParseHeadByte();
                            unpacker.Unpack(ref key);
                        }
                        {
                            unpacker.ParseHeadByte();
                            if (unpacker.IsArray)
                            {
                                var val = new List<Object>();
                                unpacker.Unpack(ref val);
                                target.Add(key, val);
                            }
                            else if (unpacker.IsMap)
                            {
                                var val = new Dictionary<String, Object>();
                                unpacker.Unpack(ref val);
                                target.Add(key, val);
                            }
                            else
                            {
                                var val = new Object();
                                unpacker.Unpack(ref val);
                                target.Add(key, val);
                            }
                        }
                    }
                    Console.WriteLine(target);
                })
            }
            
            , {
                typeof(IDictionary<Object, Object>)
                , (Object)(UnpackerForReferenceTypeDelegate<IDictionary<Object, Object>>)(
                (IDictionary<Object, Object> target, SubMsgPackUnpacker unpacker, UInt32 count)=>

                {
                    for (uint i = 0; i < count; ++i)
                    {
                        var key=String.Empty;
                        {
                            unpacker.ParseHeadByte();
                            unpacker.Unpack(ref key);
                        }
                        {
                            unpacker.ParseHeadByte();
                            if (unpacker.IsArray)
                            {
                                var val = new List<Object>();
                                unpacker.Unpack(ref val);
                                target.Add(key, val);
                            }
                            else if (unpacker.IsMap)
                            {
                                var val = new Dictionary<Object, Object>();
                                unpacker.Unpack(ref val);
                                target.Add(key, val);
                            }
                            else
                            {
                                var val = new Object();
                                unpacker.Unpack(ref val);
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
                , (Object)(UnpackerForReferenceTypeDelegate<Object>)( 
                (Object o, SubMsgPackUnpacker unpacker, UInt32 count)=>

                {
                    var type = o.GetType();
                    for (UInt32 i = 0; i < count; ++i)
                    {
                        String key = "";
                        {
                            unpacker.ParseHeadByte();
                            unpacker.Unpack(ref key);
                        }
                        var pi = type.GetProperty(key);
                        {
                            unpacker.ParseHeadByte();
                            if (pi != null){
                                if (pi.PropertyType.IsValueType)
                                {
                                    var v=unpacker.UnpackGeneric(pi.PropertyType);
                                    pi.SetValue(o, v, null);
                                }
                                else
                                {
                                    var v = pi.GetValue(o);
                                    if (v == null)
                                    {
                                        if (pi.PropertyType == typeof(String))
                                        {
                                            v = String.Empty;
                                        }
                                        else
                                        {
                                            v = Activator.CreateInstance(pi.PropertyType);
                                        }
                                    }
                                    unpacker.UnpackGeneric(ref v, pi.PropertyType);
                                    pi.SetValue(o, v, null);
                                }
                            }
                            else{
                                var v=new Object();
                                unpacker.Unpack(ref v);
                            }
                        }
                    }
                }
                )
           }
        };
        static public void AddUnpackMap<T>(UnpackerForReferenceTypeDelegate<T> unpacker)where T: class
        {
            _unpackMapMapRef.Add(typeof(T), unpacker);
        }
        static public void AddUnpackMap<T>(UnpackerForValueTypeDelegate<T> unpacker) where T : struct
        {
            _unpackMapMapVal.Add(typeof(T), unpacker);
        }

        T UnpackMap<T>() where T : struct
        {
            foreach (var kv in _unpackMapMapVal)
            {
                if (kv.Key.IsAssignableFrom(typeof(T)))
                {
                    var handler= (UnpackerForValueTypeDelegate<T>)kv.Value;
                    using (var sub = GetSubUnpacker())
                    {
                        return handler(sub, MemberCount);
                    }
                }
            }

            // add generics
            throw new InvalidOperationException("no handler for "+typeof(T));
        }

        void UnpackMap<T>(T t) where T : class
        {
            foreach (var kv in _unpackMapMapRef)
            {
                if (kv.Key.IsAssignableFrom(typeof(T)))
                {
                    var unpackMap = (UnpackerForReferenceTypeDelegate<T>)kv.Value;
                    using (var sub = GetSubUnpacker())
                    {
                        unpackMap(t, sub, MemberCount);
                    }
                    return;
                }
            }

            throw new InvalidOperationException("no handler for "+typeof(T));
        }
        #endregion
    }


    public class SubMsgPackUnpacker : MsgPackUnpacker, IDisposable
    {
        MsgPackUnpacker _parent;

        public SubMsgPackUnpacker(ArraySegment<Byte> view, MsgPackUnpacker parent):base(view, false)
        {
            this._parent = parent;
        }

        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                this._parent.Advance(Pos);
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }
    }
}
