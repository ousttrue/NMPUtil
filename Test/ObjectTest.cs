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
    public class ObjectTest
    {
        [Test]
        public void map_root()
        {
            var ms = new MemoryStream();
            var packer = new MsgPackPacker(ms);
            var src = new
            {
                Name = "Hoge"
                ,
                Number = 4
                ,
                Nest = new
                {
                    Name = "Nested"
                }
            };
            packer.Pack(src);
            var bytes = ms.ToArray();

            var unpacker = new MsgPackUnpacker(bytes);
            Assert.IsTrue(unpacker.IsMap);

            var dst = new Dictionary<String, Object>();
            unpacker.Unpack(ref dst);

            Assert.AreEqual(src.Name, Encoding.UTF8.GetString(dst["Name"] as Byte[]));
            Assert.AreEqual(src.Number, dst["Number"]);
            Assert.AreEqual(src.Nest.Name, Encoding.UTF8.GetString((dst["Nest"] as Dictionary<String, Object>)["Name"] as Byte[]));
        }
    }
}
