using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using NMPUtil.MsgPack;
using System.IO;

namespace UnitTest
{
    [Serializable]
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
            v._x = u.UnpackSub<Single>();
            v._y = u.UnpackSub<Single>();
            v._z = u.UnpackSub<Single>();
            return v;
        }
    }


    [Serializable]
    class UserType
    {
        public String Name
        {
            get;
            set;
        }

        public Vector3 Position
        {
            get;
            set;
        }

        public System.Drawing.Color Color
        {
            get;
            set;
        }
    }


    [TestFixture]
    public class UserTypeTest
    {
        [Test]
        public void pack_and_unpack()
        {
            var obj = new UserType
            {
                Name = "hoge"
                , Position = new Vector3 { X=1, Y=2, Z=3 }
                , Color = System.Drawing.Color.FromArgb(255, 128, 128, 255)
            };

            // register pack System.Drawing.Color
            MsgPackPacker.TypeMap.Add(typeof(System.Drawing.Color), (MsgPackPacker p, Object o) =>
            {
                var color=(System.Drawing.Color)o;
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
                    var a = u.UnpackSub<Byte>();
                    var r = u.UnpackSub<Byte>();
                    var g = u.UnpackSub<Byte>();
                    var b = u.UnpackSub<Byte>();
                    return System.Drawing.Color.FromArgb(a, r, g, b);
                });
      
            // pack
            var ms = new MemoryStream();
            var packer = new MsgPackPacker(ms);
            packer.Pack(obj);
            var bytes = ms.ToArray();

            // unpack
            var unpacker = new MsgPackUnpacker(bytes);

            var newObj=new UserType();
            unpacker.Unpack(ref newObj);

            Assert.AreEqual(obj.Name, newObj.Name);
            Assert.AreEqual(obj.Color, newObj.Color);
            Assert.AreEqual(obj.Position, newObj.Position);
        }
    }
}
