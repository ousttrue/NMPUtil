using NMPUtil.MsgPack;
using System;
using System.IO;


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

    struct Header
    {
        public MsgPackFormat Format
        {
            get;
            set;
        }

        public UInt32 MemberCount
        {
            get;
            set;
        }
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

            // unpack
            {
                var unpacker = new MsgPackUnpacker(new ArraySegment<Byte>(bytes));
                var result = unpacker.Unpack<int>();
                Console.WriteLine(result);
            }

            // DSL方式
            {
                var parser = from header in MsgPackParser.Header()
                             from value in MsgPackParser.Body(header)
                             select value;
                var result = parser(new Input(bytes));
                Console.WriteLine(result.Value);
            }
        }
    }
}
