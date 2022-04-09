using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

public static class CompressionWrapper
{
    public static byte[] Encode(byte[] _originalData)
    {
        MemoryStream memory = new MemoryStream();                                               //圧縮結果を格納
        DeflateStream deflate = new DeflateStream(memory, CompressionMode.Compress, true);      //memoryとdeflateを関連付ける
        deflate.Write(_originalData, 0, _originalData.Length);                                  //圧縮
        deflate.Close();
        memory.Close();

        return memory.ToArray();
    }
    public static byte[] Decode(byte[] _originalData)
    {
        MemoryStream encodeData = new MemoryStream(_originalData);
        DeflateStream deflate = new DeflateStream(encodeData, CompressionMode.Decompress);    //memoryとdeflateを関連付ける
        byte[] buffer=new byte[2048];
        int size=deflate.Read(buffer,0,buffer.Length);
        byte[] returnData = new byte[size];
        System.Array.Copy(buffer, 0, returnData, 0, size);
        deflate.Close();
        encodeData.Close();

        return returnData;
    }
}
