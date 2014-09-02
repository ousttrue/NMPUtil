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
    public class FloatTest
    {
        [Test]
        public void Float32()
        {
            Single i = 1.1f;

            var ms=new MemoryStream();
            var packer = new MsgPackPacker(ms);
            packer.Pack(i);
            var bytes = ms.ToArray();

            var unpacker = new MsgPackUnpacker(bytes);

            var j=unpacker.Unpack<Single>();
            Assert.AreEqual(i, j);
        }

        [Test]
        public void Float64()
        {
            Double i = 1.1;

            var ms=new MemoryStream();
            var packer = new MsgPackPacker(ms);
            packer.Pack(i);
            var bytes = ms.ToArray();
            /*
            Assert.AreEqual(new Byte[]{
                0xcb, 0x40, 0xf0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0
            }, bytes);
            */

            var unpacker = new MsgPackUnpacker(bytes);

            var j=unpacker.Unpack<Double>();
            Assert.AreEqual(i, j);
        }
    }
}
