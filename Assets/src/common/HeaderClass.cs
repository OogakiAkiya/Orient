using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

//GameHeader Data
// ID(1byte) - パケットの主な役割識別用
// code(1byte) - 機能ごとの役割識別用コード
// extendField(4byte) - 今後拡張用のフィールド
// 

public class GameHeader
{
    public static readonly int HEADER_SIZE = sizeof(ID) + sizeof(GameCode) + sizeof(int);
    public static readonly int INITIALIZE_CODE = 0x0000;

    public enum ID : byte
    {
        INIT = 0x0001,
        DEBUG = 0x0002,
    }

    //以下ゲーム用のコードを用意しているがそれ以外にランキング用などそれぞれのコードを複製して作成推奨
    public enum GameCode : byte
    {
        INITDATA = 0x0001,
        DEBUGDATA = 0x0002,
    }


    public ID id { get; private set; } = ID.INIT;
    public byte code { get; private set; } = 0x0000;
    //フォーマット合わせのために記述しているがここは今後フィールドを追加する際に修正
    public int extendField { get; private set; } = 0;


    public void CreateNewData(ID _id = ID.INIT, byte _code = 0x0000)
    {
        id = _id;
        code = _code;
    }

    public void DecodeHeader(byte[] _data,int _index=0)
    {
        int index = _index;
        //ID
        id = (ID)_data[index];
        index += sizeof(ID);

        //GameCode
        code = _data[index];
        index += sizeof(GameCode);
    }

    public byte[] GetHeader()
    {
        byte[] returnData = new byte[HEADER_SIZE];
        uint index = 0;

        returnData[index] = (byte)id;
        index += sizeof(ID);
        returnData[index] = code;
        index = sizeof(GameCode);
        Array.Copy(returnData, Convert.ToArrayByte(extendField), 0);


        return returnData;
    }
}
