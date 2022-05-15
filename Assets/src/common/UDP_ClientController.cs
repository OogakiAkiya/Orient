﻿using System.Collections;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;
using OpenSocket;

public class UDP_ClientController : MonoBehaviour
{
    [SerializeField] string hostname = "127.0.0.1";
    [SerializeField] int destPort = 12344;
    [SerializeField] Text debugText = null;                                     //ここにサーバから送ってきたデバッグ用の文字列が入る

    uint nowSequence = 0;
    UDP_Client socket = new UDP_Client();

    byte[] recvData;
    GameHeader header = new GameHeader();

    void Start()
    {
        //ソケット初期化(UDPのClientなので送信ポートのみの指定)
        socket.Init(destPort);

        //UDPのデータを受け取るためにサーバからデータを受け取る前に通信経路確保のためクライアントからデータを送っておく
        DebugSend(GameHeader.ID.DEBUG);
        //send処理
    }

    void Update()
    {
        DebugSend(GameHeader.ID.DEBUG);

        while (socket.server.GetRecvDataSize() > 0)
        {
            RecvRoutine();
        }

        //送信頻度を調節したい場合以下を使用する
        //if (!IsInvoking("Second30FPSInvoke")) Invoke("Second30FPSInvoke", 1f / 30);
    }

    private void Second30FPSInvoke()
    {
        //送信処理
    }



    private void RecvRoutine()
    {
        KeyValuePair<IPEndPoint, byte[]> recvPair = socket.server.GetRecvData();
        recvData = recvPair.Value;
        uint sequence = BitConverter.ToUInt32(recvData, 0);

        //シーケンス番号処理
        if (nowSequence > sequence)
        {
            if (Math.Abs(nowSequence - sequence) < 2000000000) return;
            if (nowSequence < 1000000000 && sequence > 3000000000) return;
        }
        nowSequence = sequence;

        //受信データのデコード
        header.DecodeHeader(recvData, sizeof(uint));

        //ヘッダーごとの処理

    }
    void DebugSend(GameHeader.ID _id, byte _code = 0x0000)
    {
        //データ生成
        byte[] sendData;
        GameHeader header = new GameHeader();
        header.CreateNewData((GameHeader.ID)_id,_code);
        sendData = header.GetHeader();

        //送信処理(非同期実行)
        socket.Send(sendData, hostname, destPort);
    }
}