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
    public class ArrayTest
    {
        [Test]
        public void fix_array()
        {
            var ms = new MemoryStream();
            var packer = new MsgPackPacker(ms);
            packer.Pack_Array(4);
            packer.Pack(0);
            packer.Pack(1);
            packer.Pack(false);
            packer.PackNil();
            var bytes = ms.ToArray();

            /*
            Assert.AreEqual(new Byte[]{
                0x94, 0x00, 0x01, 0xc2, 0xc0
            }, bytes);
            */

            var unpacker = new MsgPackUnpacker(bytes);

            var a=new Object[unpacker.MemberCount];
            unpacker.Unpack(ref a);

            Assert.AreEqual(4, a.Length);
            Assert.AreEqual(0, a[0]);
            Assert.AreEqual(1, a[1]);
            Assert.False((Boolean)a[2]);
            Assert.AreEqual(null, a[3]);
        }

        [Test]
        public void array16()
        {
            var ms = new MemoryStream();
            var packer = new MsgPackPacker(ms);
            uint size = 20;
            packer.Pack_Array(size);
            for (int i = 0; i < size; ++i)
            {
                packer.Pack(i);
            }
            var bytes = ms.ToArray();

            var unpacker = new MsgPackUnpacker(bytes);

            var a=new List<Object>();
            unpacker.Unpack(ref a);
            for (int i = 0; i < size; ++i)
            {
                Assert.AreEqual(i, a[i]);
            }
        }
    }
}