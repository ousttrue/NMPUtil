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
    public class IntTest 
    {
        [Test]
        public void mask() 
        {
            Assert.AreEqual(Convert.ToByte("11100000", 2), 
                    MsgPackFormat.NEGATIVE_FIXNUM.Mask());
            Assert.AreEqual(Convert.ToByte("00011111", 2), 
                    MsgPackFormat.NEGATIVE_FIXNUM.InvMask());

            Assert.AreEqual(Convert.ToByte("10100000", 2), 
                    MsgPackFormat.FIX_STR.Mask());
            Assert.AreEqual(Convert.ToByte("01011111", 2), 
                    MsgPackFormat.FIX_STR.InvMask());
        }

        [Test]
        public void positive_fixnum() 
        {
            for(Byte i=0; i<128; ++i){
                var ms=new MemoryStream();
                var packer = new MsgPackPacker(ms);
                packer.Pack(i);
                var bytes=ms.ToArray();
                Assert.AreEqual(new Byte[]{ i }, bytes);

                var unpacker=new MsgPackUnpacker(new ArraySegment<Byte>(bytes));

                var j=unpacker.Unpack<Byte>();
                Assert.AreEqual(i, j);
            }
        }

        [Test]
        public void negative_fixnum() 
        {
            Byte mask=Convert.ToByte("11100000", 2);
            for(SByte i=-32; i<0; ++i){
                var ms=new MemoryStream();
                var packer = new MsgPackPacker(ms);
                packer.Pack(i);
                var bytes=ms.ToArray();

                var unpacker = new MsgPackUnpacker(new ArraySegment<Byte>(bytes));
                Assert.AreEqual(MsgPackFormat.NEGATIVE_FIXNUM, unpacker.Format);

                var j=unpacker.Unpack<sbyte>();
                Assert.AreEqual(i, j);
            }
        }

        [Test]
        public void uint8() 
        {
            {
                Byte i=0x7F+20;

                var ms=new MemoryStream();
                var packer = new MsgPackPacker(ms);
                packer.Pack(i);
                var bytes=ms.ToArray();
                Assert.AreEqual(new Byte[]{
                        0xcc, 0x93,
                        }, bytes);

                var unpacker = new MsgPackUnpacker(new ArraySegment<Byte>(bytes));

                var j=unpacker.Unpack<Byte>();
                Assert.AreEqual(i, j);
            }
        }

        [Test]
        public void cast_large_type()
        {
            {
                Byte i = 0x7F + 20;

                var ms = new MemoryStream();
                var packer = new MsgPackPacker(ms);
                packer.Pack(i);
                var bytes = ms.ToArray();
                Assert.AreEqual(new Byte[]{
                        0xcc, 0x93,
                        }, bytes);

                var unpacker = new MsgPackUnpacker(new ArraySegment<Byte>(bytes));

                var j=unpacker.Unpack<UInt16>();
                Assert.AreEqual(i, j);
            }
        }

        [Test]
        public void uint16() 
        {
            {
                UInt16 i=0xFF+20;

                var ms=new MemoryStream();
                var packer = new MsgPackPacker(ms);
                packer.Pack(i);
                var bytes=ms.ToArray();
                Assert.AreEqual(new Byte[]{
                        0xcd, 0x01, 0x13
                        }, bytes);

                var unpacker = new MsgPackUnpacker(new ArraySegment<Byte>(bytes));

                var j=unpacker.Unpack<UInt16>();
                Assert.AreEqual(i, j);
            }
        }

        [Test]
        public void uint32() 
        {
            {
                UInt32 i=0xFFFF+20;

                var ms=new MemoryStream();
                var packer = new MsgPackPacker(ms);
                packer.Pack(i);
                var bytes=ms.ToArray();
                Assert.AreEqual(new Byte[]{
                        0xce, 0x00, 0x01, 0x00, 0x13
                        }, bytes);

                var unpacker = new MsgPackUnpacker(new ArraySegment<Byte>(bytes));

                var j=unpacker.Unpack<UInt32>();
                Assert.AreEqual(i, j);
            }
        }

        [Test]
        public void uint64() 
        {
            {
                UInt64 i=0xFFFFFFFF;
                i += 20;

                var ms=new MemoryStream();
                var packer = new MsgPackPacker(ms);
                packer.Pack(i);
                var bytes=ms.ToArray();
                Assert.AreEqual(new Byte[]{
                        0xcf, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x13
                        }, bytes);

                var unpacker = new MsgPackUnpacker(new ArraySegment<Byte>(bytes));

                var j=unpacker.Unpack<UInt64>();
                Assert.AreEqual(i, j);
            }
        }

        [Test]
        public void int8()
        {
            {
                SByte i = -64;

                var ms=new MemoryStream();
                var packer = new MsgPackPacker(ms);
                packer.Pack(i);
                var bytes = ms.ToArray();

                Assert.AreEqual(new Byte[]{
                        0xd0, 0xc0,
                        }, bytes);

                var unpacker = new MsgPackUnpacker(new ArraySegment<Byte>(bytes));

                var j=unpacker.Unpack<sbyte>();
                Assert.AreEqual(i, j);
            }
        }

        [Test]
        public void int16()
        {
            {
                Int16 i = -150;

                var ms=new MemoryStream();
                var packer = new MsgPackPacker(ms);
                packer.Pack(i);
                var bytes = ms.ToArray();

                Assert.AreEqual(new Byte[]{
                        0xd1, 0xFF, 0x6a
                        }, bytes);

                var unpacker = new MsgPackUnpacker(new ArraySegment<Byte>(bytes));

                var j=unpacker.Unpack<Int16>();
                Assert.AreEqual(i, j);
            }
        }

        [Test]
        public void int32()
        {
            {
                Int32 i = -35000;

                var ms=new MemoryStream();
                var packer = new MsgPackPacker(ms);
                packer.Pack(i);
                var bytes = ms.ToArray();

                Assert.AreEqual(new Byte[]{
                        0xd2, 0xff, 0xff, 0x77, 0x48
                        }, bytes);

                var unpacker = new MsgPackUnpacker(new ArraySegment<Byte>(bytes));

                var j=unpacker.Unpack<Int32>();
                Assert.AreEqual(i, j);
            }
        }

        [Test]
        public void int64()
        {
            {
                Int64 i = -2147483650;

                var ms=new MemoryStream();
                var packer = new MsgPackPacker(ms);
                packer.Pack(i);
                var bytes = ms.ToArray();

                Assert.AreEqual(new Byte[]{
                        0xd3, 0xff, 0xff, 0xff, 0xff, 0x7f, 0xff, 0xff, 0xfe
                        }, bytes);

                var unpacker = new MsgPackUnpacker(new ArraySegment<Byte>(bytes));

                var j=unpacker.Unpack<Int64>();
                Assert.AreEqual(i, j);
            }
        }
    }
}
