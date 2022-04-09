using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using OpenSocket;

public class TCP_ClientController : MonoBehaviour
{
    //ipアドレスとポート番号設定
    [SerializeField] string hostname ="127.0.0.1";
    [SerializeField] int    port = 12345;
    TCP_Client      socket = new TCP_Client();

    //Debug用(clientからデータを送りserverからデータを受け取る処理)
    [SerializeField] Text debugText = null;                                     //ここにサーバから送ってきたデバッグ用の文字列が入る
    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
    public bool doDebugSend { private get; set; } = false;                                                     //ここがtrueになるとデバッグ用の処理が走る

    void Start()
    {
        //通信設定
        socket.Init(hostname, port);                      //接続先サーバ情報
        socket.StartRecvThread();                     //非同期通信Recv用のスレッド作成
    }

    void Update()
    {
        //
        socket.Update();

        if (doDebugSend)
        {
            //タイマーのリセット
            timer.Restart();

            //テストデータ送信処理を次に記載
            DebugSend(GameHeader.ID.DEBUG, (byte)GameHeader.GameCode.DEBUGDATA);
            
        }

        while (socket.RecvDataSize() > 0)
        {
            RecvRoutine();
        }

    }

    private void RecvRoutine() {
        var recvData = socket.GetRecvData();
        GameHeader header = new GameHeader();

        //受信データのデコード
        header.DecodeHeader(recvData);

        //受信データごとの処理ヘッダーごとの処理


        //デバッグ用のデータを受け取った際の処理
        if (header.id == GameHeader.ID.DEBUG)
        {
            //意図していないタイミングのデータが来た場合処理中止
            if (!doDebugSend) return;

            //データ処理例
            int sum = BitConverter.ToInt32(recvData, GameHeader.HEADER_SIZE);
            debugText.text = $"人数:" + sum + "\nTCP応答時間:" + timer.ElapsedMilliseconds + "ミリ秒";

            //フラグ修正
            doDebugSend = false;
        }
    }

    void DebugSend(GameHeader.ID _id, byte _code = 0x0000)
    {
        //データ生成
        byte[] sendData;
        GameHeader header = new GameHeader();
        header.CreateNewData(_id,_code);
        sendData = header.GetHeader();

        //送信処理(非同期実行)
        var task = socket.Send(sendData, sendData.Length);
    }

}