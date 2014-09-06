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
    public partial class MsgPackUnpacker: IDisposable
    {
        NetworkEndianArraySegmentReader _reader;

        MsgPackHeader _header;
        public MsgPackHeader Header
        {
            get
            {
                if (_header == null)
                {
                    _header = new MsgPackHeader(_reader);
                }
                return _header;
            }
        }

        public Encoding StrEncoding
        {
            get;
            set;
        }

        static MethodInfo _genericUnpacker;
        static public MethodInfo GenericUnpacker
        {
            get
            {
                if (_genericUnpacker==null)
                {
                    _genericUnpacker=typeof(MsgPackUnpacker).GetMethods().First(mi => mi.Name == "Unpack");
                }
                return _genericUnpacker;
            }
        }

        public MsgPackUnpacker(IEnumerable<Byte> bytes, MsgPackUnpacker parent=null)
            : this(new ArraySegment<Byte>(bytes.ToArray()))
        {}

        public MsgPackUnpacker(Byte[] bytes, MsgPackUnpacker parent=null)
            : this(new ArraySegment<Byte>(bytes))
        {}

        MsgPackUnpacker _parent;
        public MsgPackUnpacker GetSubUnpacker()
        {
            var a=Header;
            return new MsgPackUnpacker(_reader.GetRemain(), this);
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
                if (_parent != null)
                {
                    this._parent._reader.Advance(_reader.Pos);
                }
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        public MsgPackUnpacker(ArraySegment<Byte> view, MsgPackUnpacker parent=null)
        {
            this.StrEncoding = Encoding.UTF8;
            this._parent = parent;
            this._reader = new NetworkEndianArraySegmentReader(view);
        }

        public T Unpack<T>()
        {
            var t = typeof(T);
            if (t.IsValueType || t == typeof(String))
            {
                Func<T> callback = () =>
                {
                    switch (Header.Format)
                    {
                        case MsgPackFormat.UINT8:
                            return (T)Convert.ChangeType(_reader.ReadByte(), t);

                        case MsgPackFormat.UINT16:
                            return (T)Convert.ChangeType(_reader.ReadUInt16(), t);

                        case MsgPackFormat.UINT32:
                            return (T)Convert.ChangeType(_reader.ReadUInt32(), t);

                        case MsgPackFormat.UINT64:
                            return (T)Convert.ChangeType(_reader.ReadUInt64(), t);

                        case MsgPackFormat.INT8:
                            return (T)Convert.ChangeType(_reader.ReadSByte(), t);

                        case MsgPackFormat.INT16:
                            return (T)Convert.ChangeType(_reader.ReadInt16(), t);

                        case MsgPackFormat.INT32:
                            return (T)Convert.ChangeType(_reader.ReadInt32(), t);

                        case MsgPackFormat.INT64:
                            return (T)Convert.ChangeType(_reader.ReadInt64(), t);

                        case MsgPackFormat.NIL:
                            return (T)Convert.ChangeType(null, t);

                        case MsgPackFormat.TRUE:
                            return (T)Convert.ChangeType(true, t);

                        case MsgPackFormat.FALSE:
                            return (T)Convert.ChangeType(false, t);

                        case MsgPackFormat.FLOAT:
                            return (T)Convert.ChangeType(_reader.ReadSingle(), t);

                        case MsgPackFormat.DOUBLE:
                            return (T)Convert.ChangeType(_reader.ReadDouble(), t);

                        case MsgPackFormat.POSITIVE_FIXNUM:
                            return (T)Convert.ChangeType(Header.FixNum, t);

                        case MsgPackFormat.NEGATIVE_FIXNUM:
                            return (T)Convert.ChangeType(Header.FixNum, t);

                        // str
                        case MsgPackFormat.FIX_STR:
                        case MsgPackFormat.STR8:
                        case MsgPackFormat.STR16:
                        case MsgPackFormat.STR32:
                            {
                                var buf = _reader.ReadBytes((Int32)Header.MemberCount);
                                return (T)Convert.ChangeType(StrEncoding.GetString(buf.ToArray()), t);
                            }

                        // bin
                        case MsgPackFormat.BIN8:
                        case MsgPackFormat.BIN16:
                        case MsgPackFormat.BIN32:
                            {
                                var buf = _reader.ReadBytes((Int32)Header.MemberCount);
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
                                    return (T)(Object)buf.ToArray();
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
                            throw new InvalidOperationException("invalid format " + Header.Format);
                    }
                };

                // clear header
                T val = callback();
                _header = null;
                return val;
            }
            else
            {
                if (typeof(T).IsArray)
                {
                    var o = (T)Activator.CreateInstance(typeof(T)
                        , new Object[]{ (Int32)Header.MemberCount });
                    UnpackByRef<T>(ref o);
                    return o;
                }
                else
                {
                    var o = (T)Activator.CreateInstance<T>();
                    UnpackByRef<T>(ref o);
                    return o;
                }
            }
        }

        void UnpackByRef<T>(ref T v)
        {
            if (v == null)
            {
                throw new ArgumentNullException("v");
            }

            var t = typeof(T);
            if (t.IsValueType)
            {
                throw new ArgumentException("v should not value type !");
            }

            switch (Header.Format)
            {
                case MsgPackFormat.UINT8:
                    v = (T)Convert.ChangeType(_reader.ReadByte(), t);
                    break;

                case MsgPackFormat.UINT16:
                    v = (T)Convert.ChangeType(_reader.ReadUInt16(), t);
                    break;

                case MsgPackFormat.UINT32:
                    v = (T)Convert.ChangeType(_reader.ReadUInt32(), t);
                    break;

                case MsgPackFormat.UINT64:
                    v = (T)Convert.ChangeType(_reader.ReadUInt64(), t);
                    break;

                case MsgPackFormat.INT8:
                    v = (T)Convert.ChangeType(_reader.ReadSByte(), t);
                    break;

                case MsgPackFormat.INT16:
                    v = (T)Convert.ChangeType(_reader.ReadInt16(), t);
                    break;

                case MsgPackFormat.INT32:
                    v = (T)Convert.ChangeType(_reader.ReadInt32(), t);
                    break;

                case MsgPackFormat.INT64:
                    v = (T)Convert.ChangeType(_reader.ReadInt64(), t);
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
                    v = (T)Convert.ChangeType(_reader.ReadSingle(), t);
                    break;

                case MsgPackFormat.DOUBLE:
                    v = (T)Convert.ChangeType(_reader.ReadDouble(), t);
                    break;

                case MsgPackFormat.POSITIVE_FIXNUM:
                    v = (T)Convert.ChangeType(Header.FixNum, t);
                    break;

                case MsgPackFormat.NEGATIVE_FIXNUM:
                    v = (T)Convert.ChangeType(Header.FixNum, t);
                    break;

                // str
                case MsgPackFormat.FIX_STR:
                case MsgPackFormat.STR8:
                case MsgPackFormat.STR16:
                case MsgPackFormat.STR32:
                    {
                        var buf = _reader.ReadBytes((Int32)Header.MemberCount);
                        v = (T)Convert.ChangeType(StrEncoding.GetString(buf.ToArray()), t);
                    }
                    break;

                // bin
                case MsgPackFormat.BIN8:
                case MsgPackFormat.BIN16:
                case MsgPackFormat.BIN32:
                    {
                        var buf = _reader.ReadBytes((Int32)Header.MemberCount);
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
                            v = (T)(Object)buf.ToArray();
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
                    throw new InvalidOperationException("invalid format "+Header.Format);
            }

            // clear header
            _header = null;
        }


        public delegate void UnpackerForReferenceTypeDelegate<in T>(T target, MsgPackUnpacker unpacker, UInt32 count);
        public delegate T UnpackerForValueTypeDelegate<T>(MsgPackUnpacker unpacker, UInt32 count);

        #region UnpackArrayCallbacks
        static Dictionary<Type, Object> _unpackArrayMapVal =
            new Dictionary<Type, Object>();
        static Dictionary<Type, Object> _unpackArrayMapRef =
            new Dictionary<Type, Object>();


        static public void AddUnpackArray<T>(UnpackerForReferenceTypeDelegate<T> unpacker)
        {
            _unpackArrayMapRef[typeof(T)]=unpacker;
        }

        static public void AddUnpackArray<T>(UnpackerForValueTypeDelegate<T> unpacker)
        {
            _unpackArrayMapVal[typeof(T)]=unpacker;
        }
        #endregion

        #region UnpackArray
        T UnpackArray<T>()
        {
            foreach (var kv in _unpackArrayMapVal)
            {
                if (kv.Key.IsAssignableFrom(typeof(T)))
                {
                    var handler = (UnpackerForValueTypeDelegate<T>)kv.Value;
                    using (var sub = GetSubUnpacker())
                    {
                        return handler(sub, Header.MemberCount);
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
                        var t = handler(sub, Header.MemberCount);
                        AddUnpackArray<T>(handler);
                        return t;
                    }
                }
            }

            throw new InvalidOperationException("no handler for "+typeof(T));
        }

        static public void UnpackArrayAsArray<T>(T[] array, MsgPackUnpacker unpacker, UInt32 count)
        {
            for (int i = 0; i < count; ++i)
            {
                array[i] = unpacker.Unpack<T>();
            }
        }

        static public void UnpackArrayAsList<T>(List<T> list, MsgPackUnpacker unpacker, UInt32 count)
        {
            for (int i = 0; i < count; ++i)
            {
                list.Add(unpacker.Unpack<T>());
            }
        }

        void UnpackArray<T>(T t)
        {
            foreach (var kv in _unpackArrayMapRef)
            {
                if (kv.Key.IsAssignableFrom(typeof(T)))
                {
                    using (var sub = GetSubUnpacker())
                    {
                        var handler = (UnpackerForReferenceTypeDelegate<T>)kv.Value;
                        handler(t, sub, Header.MemberCount);
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
                    handler(t, sub, Header.MemberCount);
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
                    handler(t, sub, Header.MemberCount);
                    AddUnpackArray<T>(handler);
                    return;
                }
            }

            throw new InvalidOperationException("no handle for " + typeof(T));
        }
        #endregion

        #region UnpackMapCallbacks
        static Dictionary<Type, Object> _unpackMapMapVal =
            new Dictionary<Type, Object>();
        static Dictionary<Type, Object> _unpackMapMapRef =
            new Dictionary<Type, Object>
        {
            {
                typeof(IDictionary<String, Object>)
                , (Object)(UnpackerForReferenceTypeDelegate<IDictionary<String, Object>>)(
                (IDictionary<String, Object> o, MsgPackUnpacker unpacker, UInt32 count)=>

                {
                    var target = o as IDictionary<String, Object>;
                    for (uint i = 0; i < count; ++i)
                    {
                        var key=unpacker.Unpack<String>();

                        if (unpacker.Header.IsArray)
                        {
                            target.Add(key, unpacker.Unpack<List<Object>>());
                        }
                        else if (unpacker.Header.IsMap)
                        {
                            target.Add(key, unpacker.Unpack<Dictionary<String, Object>>());
                        }
                        else
                        {
                            target.Add(key, unpacker.Unpack<Object>());
                        }
                    }
                    Console.WriteLine(target);
                })
            }
            
            , {
                typeof(IDictionary<Object, Object>)
                , (Object)(UnpackerForReferenceTypeDelegate<IDictionary<Object, Object>>)(
                (IDictionary<Object, Object> target, MsgPackUnpacker unpacker, UInt32 count)=>

                {
                    for (uint i = 0; i < count; ++i)
                    {
                        var key=unpacker.Unpack<String>();

                        if (unpacker.Header.IsArray)
                        {
                            target.Add(key, unpacker.Unpack<List<Object>>());
                        }
                        else if (unpacker.Header.IsMap)
                        {
                            target.Add(key, unpacker.Unpack<Dictionary<Object, Object>>());
                        }
                        else
                        {
                            target.Add(key, unpacker.Unpack<Object>());
                        }

                    }
                    Console.WriteLine(target);
                }
                )
            }

            , {
                  typeof(Object)
                , (Object)(UnpackerForReferenceTypeDelegate<Object>)( 
                (Object o, MsgPackUnpacker unpacker, UInt32 count)=>

                {
                    var type = o.GetType();
                    for (UInt32 i = 0; i < count; ++i)
                    {
                        var key =unpacker.Unpack<String>();

                        var pi = type.GetProperty(key);
                        {
                            if (pi != null){
                                var gmi =MsgPackUnpacker.GenericUnpacker.MakeGenericMethod(new Type[] { 
                                    pi.PropertyType
                                });
                                var v=gmi.Invoke(unpacker, null);
                                pi.SetValue(o, v, null);
                            }
                            else{
                                var v=unpacker.Unpack<Object>();
                            }
                        }
                    }
                }
                )
           }
        };
        static public void AddUnpackMap<T>(UnpackerForReferenceTypeDelegate<T> unpacker)
        {
            _unpackMapMapRef.Add(typeof(T), unpacker);
        }
        static public void AddUnpackMap<T>(UnpackerForValueTypeDelegate<T> unpacker)
        {
            _unpackMapMapVal.Add(typeof(T), unpacker);
        }
        #endregion

        #region UnpackMap
        T UnpackMap<T>()
        {
            foreach (var kv in _unpackMapMapVal)
            {
                if (kv.Key.IsAssignableFrom(typeof(T)))
                {
                    var handler= (UnpackerForValueTypeDelegate<T>)kv.Value;
                    using (var sub = GetSubUnpacker())
                    {
                        return handler(sub, Header.MemberCount);
                    }
                }
            }

            // add generics
            throw new InvalidOperationException("no handler for "+typeof(T));
        }

        void UnpackMap<T>(T t)
        {
            foreach (var kv in _unpackMapMapRef)
            {
                if (kv.Key.IsAssignableFrom(typeof(T)))
                {
                    var unpackMap = (UnpackerForReferenceTypeDelegate<T>)kv.Value;
                    using (var sub = GetSubUnpacker())
                    {
                        unpackMap(t, sub, Header.MemberCount);
                    }
                    return;
                }
            }

            throw new InvalidOperationException("no handler for "+typeof(T));
        }
        #endregion
    }
}
