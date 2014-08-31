using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using NUtil.MsgPack;
using System.IO;

namespace UnitTest
{
    [Serializable]
    struct Vector3
    {
        public Single X
        {
            get;
            set;
        }
        public Single Y
        {
            get;
            set;
        }
        public Single Z
        {
            get;
            set;
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
            MsgPackUnpacker.UnpackArrayMap.Add(typeof(System.Drawing.Color)
                , (ref Object o, MsgPackUnpacker u, UInt32 size) =>
                {
                    // check map size
                    if (size != 4)
                    {
                        throw new ArgumentException("invalid map size");
                    }
                    Byte a = 0;
                    u.UnpackSub(ref a);
                    Byte r = 0;
                    u.UnpackSub(ref r);
                    Byte g = 0;
                    u.UnpackSub(ref g);
                    Byte b = 0;
                    u.UnpackSub(ref b);
                    o = System.Drawing.Color.FromArgb(a, r, g, b);
                    int x = 0;
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
