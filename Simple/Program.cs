﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMPUtil.MsgPack;
using System.IO;


namespace Simple
{
    class Program
    {
        static void Main(string[] args)
        {
            var ms=new MemoryStream();
            var packer = new MsgPackPacker(ms); ;
            packer.Pack(1);

            var bytes=ms.ToArray();
            Console.WriteLine(String.Join(",", bytes));

            int result=0;
            var unpacker=new MsgPackUnpacker(new ArraySegment<Byte>(bytes));
            var view=unpacker.Unpack(ref result);
            Console.WriteLine(view.Count);
            Console.WriteLine(result);
        }
    }
}
