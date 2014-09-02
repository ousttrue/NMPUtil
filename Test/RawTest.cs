using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using NMPUtil.MsgPack;
using System.IO;

namespace UnitTest
{
    [TestFixture]
    public class RawTest
    {
        [Test]
        public void fix_raw()
        {
            var src = new Byte[] { 0, 1, 2 };
            var ms = new MemoryStream();
            var packer = new MsgPackPacker(ms);
            packer.Pack(src);
            var bytes = ms.ToArray();

            var unpacker = new MsgPackUnpacker(bytes);
            var v=new Byte[unpacker.MemberCount];
            unpacker.Unpack(ref v);

            Assert.AreEqual(src, v);
        }

        [Test]
        public void raw16()
        {
            var src = new List<Byte>();
            for(Byte i=0; i<50; ++i){
                src.Add(i);
            }

            var ms = new MemoryStream();
            var packer = new MsgPackPacker(ms);
            packer.Pack(src);
            var bytes = ms.ToArray();

            var unpacker = new MsgPackUnpacker(bytes);
            var v=new Byte[unpacker.MemberCount];
            unpacker.Unpack(ref v);

            Assert.AreEqual(src.ToArray(), v);
        }

        /*
        [Test]
        public void raw32()
        {
            var src = new List<Byte>();
            for (Byte i = 0; i < (65535+10); ++i)
            {
                src.Add(i);
            }

            var ms = new MemoryStream();
            var packer = new MyMsgPack.Packer(ms);
            packer.Pack(src);
            var bytes = ms.ToArray();

            var r = new MemoryStream(bytes);
            var unpacker = new MyMsgPack.Unpacker(r);
            var v = unpacker.Unpack();

            Assert.AreEqual(src.ToArray(), v.ToBytes());
        }
        */
    }
}
