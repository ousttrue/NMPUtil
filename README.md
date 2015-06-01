# NMPUtil(.Net MsgPack Utility)
* https://github.com/msgpack/msgpack/blob/master/spec.md
MsgPackのC#実装。

# History
* 20150601: rename NMPUtil.MsgPack to NMPUtil

# ToDo
* 例外を使うのをやめる
* NuGetに登録する
* UnityのSampleプロジェクト

# UnityMemo
いくつか使えない要素を置き換えた。

* System.Threading.Tasks -> event
* MethodInfo.GetCustomAttributes<> -> GetCustomAttributes
* MethodInfo.CreateDelegate -> Invoke
* ArraySegmentでLINQが効かない件

# 使い方
## Pack

sample

    var ms = new MemoryStream();
    var packer = new MsgPackPacker(ms);
    packer.Pack_Array(4);
    packer.Pack(0);
    packer.Pack(1);
    packer.Pack(false);
    packer.PackNil();
    var bytes = ms.ToArray();
    
## Unpack

sample

    var unpacker = new MsgPackUnpacker(bytes);
    
    var a=unpacker.Unpack<Object[]>();
    
    Assert.AreEqual(4, a.Length);
    Assert.AreEqual(0, a[0]);
    Assert.AreEqual(1, a[1]);
    Assert.False((Boolean)a[2]);
    Assert.AreEqual(null, a[3]);

## Traverse

# 既存の型のPack, Unpack

    // register pack System.Drawing.Color
    MsgPackPacker.AddPack<System.Drawing.Color>(
        (MsgPackPacker p, Object o) =>
        {
            var color = (System.Drawing.Color)o;
            p.Pack_Array(4);
            p.Pack(color.A);
            p.Pack(color.R);
            p.Pack(color.G);
            p.Pack(color.B);
        });
    MsgPackUnpacker.AddUnpackArray<System.Drawing.Color>(
        (MsgPackUnpacker u, UInt32 size) =>
        {
            if (size != 4)
            {
                throw new ArgumentException("size");
            }
            var a = u.Unpack<Byte>();
            var r = u.Unpack<Byte>();
            var g = u.Unpack<Byte>();
            var b = u.Unpack<Byte>();
            return System.Drawing.Color.FromArgb(a, r, g, b);
        });

# 自前の型のPack, Unpack

    struct Vector3
    {
        Single _x;
        public Single X
        {
            get { return _x;  }
            set { _x = value; }
        }
        Single _y;
        public Single Y
        {
            get { return _y; }
            set { _y = value; }
        }
        Single _z;
        public Single Z
        {
            get { return _z; }
            set { _z = value; }
        }
    
        public override bool Equals(object obj)
        {
            if (obj is Vector3)
            {
                var s = (Vector3)obj;
                return s.X == X && s.Y == Y && s.Z == Z;
            }
            return base.Equals(obj);
        }
    
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    
        [MsgPackPacker]
        static public void Packer(MsgPackPacker p, Object o)
        {
            var v = (Vector3)o;
            p.Pack_Array(3);
            p.Pack(v._x);
            p.Pack(v._y);
            p.Pack(v._z);
        }
    
        [MsgPackArrayUnpacker]
        static public Vector3 Unpack(MsgPackUnpacker u, UInt32 count)
        {
            if (count != 3)
            {
                throw new ArgumentException("count");
            }
    
            var v = new Vector3();
            v._x = u.Unpack<Single>();
            v._y = u.Unpack<Single>();
            v._z = u.Unpack<Single>();
    
            return v;
        }
    }

# RPC

* https://github.com/msgpack-rpc/msgpack-rpc/blob/master/spec.md

sample
 
    // server
    var server = new MsgPackRpcServer();
    server.Dispatcher.RegisterFunc("add", (int a, int b) => { return a + b; });
    server.Start(8080);
    
    // client
    var client=new MsgPackRpcClient();
    var task=client.Connect("127.0.0.1", 8080);
    task.Wait();
    Console.WriteLine("connected");
    
    var callTask=client.Call<Int32>("add", 2244, 1234);
    callTask.Wait();
    Console.WriteLine("response: " + callTask.Result);
    
    Thread.Sleep(2000);

