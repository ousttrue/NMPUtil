using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using NUtil.MsgPack;
using System.IO;

namespace UnitTest
{
    [TestFixture]
    public class MapTest
    {
        [Test]
        public void fix_map()
        {
            var ms = new MemoryStream();
            var packer = new MsgPackPacker(ms);
            packer.Pack_Map(2);
            packer.Pack(0);
            packer.Pack(1);
            packer.Pack(2);
            packer.Pack(3);
            var bytes = ms.ToArray();

            /*
            Assert.AreEqual(new Byte[]{
                0x82, 0x00, 0x01, 0x02, 0x03
            }, bytes);
            */

            var unpacker = new MsgPackUnpacker(bytes);

            var m=new Dictionary<dynamic, dynamic>();
            unpacker.Unpack(ref m);
            var a = m.ToArray();

            Assert.AreEqual(2, a.Count());
            Assert.AreEqual(1, a[0].Value);
            Assert.AreEqual(3, a[1].Value);
        }

        [Test]
        public void map16()
        {
            var ms = new MemoryStream();
            var packer = new MsgPackPacker(ms);
            int size=18;
            packer.Pack_Map(size);
            for (int i = 0; i < size; ++i)
            {
                packer.Pack(i);
                packer.Pack(i+5);
            }
            var bytes = ms.ToArray();

            /*
            Assert.AreEqual(
                new Byte[]{0xde, 0x0, 0x12, 0x0, 0x5, 0x1, 0x6, 0x2, 0x7, 0x3, 0x8, 0x4, 0x9, 0x5, 0xa, 0x6, 0xb, 0x7, 0xc, 0x8, 0xd, 0x9, 0xe, 0xa, 0xf, 0xb, 0x10, 0xc,
0x11, 0xd, 0x12, 0xe, 0x13, 0xf, 0x14, 0x10, 0x15, 0x11, 0x16},
            bytes);
            */
            var unpacker = new MsgPackUnpacker(bytes);

            var m=new Dictionary<dynamic, dynamic>();
            unpacker.Unpack(ref m);
            var a = m.ToArray();

            Assert.AreEqual(size, a.Count());
        }
    }
}
