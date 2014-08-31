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
    public class StringTest
    {
        [Test]
        public void str()
        {
            var ms = new MemoryStream();
            var packer = new MsgPackPacker(ms);
            packer.Pack("文字列");
            var bytes = ms.ToArray();

            var unpacker = new MsgPackUnpacker(bytes);
            var v="";
            unpacker.Unpack(ref v);

            Assert.AreEqual("文字列", v);
        }
    }
}
