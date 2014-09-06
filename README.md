# MsgPack.Net
* https://github.com/msgpack/msgpack/blob/master/spec.md

MsgPackのC#実装。

# ToDo

* Unityで試す
* NuGetに登録する

# 使い方
## Pack

sample

    var ms = new MemoryStream();
    var packer = new MsgPackPacker(ms);
    packer.Pack_Array(4);
    packer.Pack(0);
    packer.Pack(1);
    packer.Pack(false);
    packer.PackNil();
    var bytes = ms.ToArray();
    
## Unpack

sample

    var unpacker = new MsgPackUnpacker(bytes);
    
    var a=unpacker.Unpack<Object[]>();
    
    Assert.AreEqual(4, a.Length);
    Assert.AreEqual(0, a[0]);
    Assert.AreEqual(1, a[1]);
    Assert.False((Boolean)a[2]);
    Assert.AreEqual(null, a[3]);

## Traverse

# 既存の型のPack, Unpack

# 自前の型のPack, Unpack

# RPC

* https://github.com/msgpack-rpc/msgpack-rpc/blob/master/spec.md

sample
 
    // server
    var server = new MsgPackRpcServer();
    server.Dispatcher.RegisterFunc("add", (int a, int b) => { return a + b; });
    server.Start(8080);
    
    // client
    var client=new MsgPackRpcClient();
    var task=client.Connect("127.0.0.1", 8080);
    task.Wait();
    Console.WriteLine("connected");
    
    var callTask=client.Call<Int32>("add", 2244, 1234);
    callTask.Wait();
    Console.WriteLine("response: " + callTask.Result);
    
    Thread.Sleep(2000);

