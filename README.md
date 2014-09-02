# MsgPack.Net
- https://github.com/msgpack/msgpack/blob/master/spec.md

MsgPackのC#実装。

# ToDo

* RPC実装
* Unityで試す
* NuGetに登録する

# 使い方
## Pack
    var ms = new MemoryStream();
    var packer = new MsgPackPacker(ms);
    packer.Pack_Array(4);
    packer.Pack(0);
    packer.Pack(1);
    packer.Pack(false);
    packer.PackNil();
    var bytes = ms.ToArray();
    
## Unpack(参照型)
    var unpacker = new MsgPackUnpacker(bytes);
    
    var a=new Object[unpacker.MemberCount];
    unpacker.Unpack(ref a);
    
    Assert.AreEqual(4, a.Length);
    Assert.AreEqual(0, a[0]);
    Assert.AreEqual(1, a[1]);
    Assert.False((Boolean)a[2]);
    Assert.AreEqual(null, a[3]);

## Unpack(値型)

## Traverse

# 既存の型のPack, Unpack

# 自前の型のPack, Unpack

# RPC
- https://github.com/msgpack-rpc/msgpack-rpc/blob/master/spec.md

