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
    public class BooleanTest
    {
        [Test]
        public void nil()
        {
            var ms=new MemoryStream();
            var packer = new MsgPackPacker(ms);
            packer.PackNil();
            var bytes = ms.ToArray();
            Assert.AreEqual(new Byte[] { 0xC0 }, bytes);

            var unpacker = new MsgPackUnpacker(bytes);

            Assert.IsTrue(unpacker.IsNil);
        }

        [Test]
        public void True()
        {
            var ms=new MemoryStream();
            var packer = new MsgPackPacker(ms);
            packer.Pack(true);
            var bytes = ms.ToArray();
            Assert.AreEqual(new Byte[] { 0xC3 }, bytes);

            var unpacker = new MsgPackUnpacker(bytes);

            var j=default(Boolean);
            unpacker.Unpack(ref j);
            Assert.AreEqual(true, j);
        }

        [Test]
        public void False()
        {
            var ms=new MemoryStream();
            var packer = new MsgPackPacker(ms);
            packer.Pack(false);
            var bytes = ms.ToArray();
            Assert.AreEqual(new Byte[] { 0xC2 }, bytes);

            var unpacker = new MsgPackUnpacker(bytes);

            var j = default(Boolean);
            unpacker.Unpack(ref j);
            Assert.AreEqual(false, j);
        }
    }
}
