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
    public class PipelineTest
    {
        [Test]
        public void pipeline()
        {
            var ms = new MemoryStream();
            var packer = new MsgPackPacker(ms);
            packer.Pack_Array(4);
            packer.Pack((Byte)0);
            packer.Pack((Byte)1);
            packer.Pack(false);
            packer.PackNil();
            var bytes = ms.ToArray();

            Assert.AreEqual(new Byte[]{
                0x94, 0x00, 0x01, 0xc2, 0xc0
            }, bytes);

            var unpacker = new MsgPackUnpacker(bytes.Take(2));
            
            Assert.Throws<NMPUtil.NetworkEndianArraySegmentReader.NotEnoughBytesException>(() =>
                {
                    var o=new Object[unpacker.Header.MemberCount];
                    unpacker.Unpack(ref o);
                }
               );

            unpacker=new MsgPackUnpacker(bytes);

            var a=new Object[unpacker.Header.MemberCount];
            unpacker.Unpack(ref a);

            Assert.AreEqual(4, a.Length);
            Assert.AreEqual(0, a[0]);
            Assert.AreEqual(1, a[1]);
            Assert.False((Boolean)a[2]);
            Assert.AreEqual(null, a[3]);
        }
    }
}
