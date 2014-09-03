using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NMPUtil.MsgPack;
using NMPUtil.MsgPack.Rpc;
using System.Diagnostics;
using System.Net.Sockets;
using NMPUtil.Streams;
using NMPUtil.Tcp;
using System.Threading;


namespace Simple
{
    [Serializable]
    public class TestClass
    {
        public string MyProperty1 { get; set; }
        public int MyProperty2 { get; set; }
        //public DateTime MyProperty3 { get; set; }
        public bool MyProperty4 { get; set; }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var ms = new MemoryStream();
            var packer = new MsgPackPacker(ms); ;
            packer.Pack(1);

            var bytes = ms.ToArray();
            Console.WriteLine(String.Join(",", bytes));

            var unpacker = new MsgPackUnpacker(new ArraySegment<Byte>(bytes));
            var result = unpacker.Unpack<int>();

            Console.WriteLine(result);
        }
    }
}
