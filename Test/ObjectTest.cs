﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using NMPUtil.MsgPack;
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
            Assert.IsTrue(unpacker.Header.IsMap);

            var dst=unpacker.Unpack<Dictionary<String, Object>>();

            Assert.AreEqual(src.Name, dst["Name"]);
            Assert.AreEqual(src.Number, dst["Number"]);
            Assert.AreEqual(src.Nest.Name, (dst["Nest"] as Dictionary<String, Object>)["Name"]);
        }
    }
}
