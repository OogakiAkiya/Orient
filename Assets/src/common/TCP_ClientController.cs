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
    [SerializeField] int destPort = 12345;
    TCP_Client socket = new TCP_Client();

    //Debug用(clientからデータを送りserverからデータを受け取る処理)
    [SerializeField] Text debugText = null;                                     //ここにサーバから送ってきたデバッグ用の文字列が入る
    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
    public bool doDebugSend { private get; set; } = false;                                                     //ここがtrueになるとデバッグ用の処理が走る
    int count = 123;


    void Start()
    {
        //通信設定
        socket.Init(hostname, destPort);                      //接続先サーバ情報
        socket.StartRecvThread();                     //非同期通信Recv用のスレッド作成

        //フラグ起動(テスト用)
        doDebugSend = true;
    }

    void Update()
    {
        //TCP Clientのデータ更新処理(受信データの参照よりも先に行う必要あり)
        socket.Update();

        if (doDebugSend)
        {
            //タイマーのリセット
            timer.Restart();

            //テストデータ送信処理を次に記載
            DebugSend(GameHeader.ID.DEBUG, (byte)GameHeader.GameCode.DEBUGDATA);

            //フラグ修正
            doDebugSend = false;
            
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
            //データ処理例
            int data = BitConverter.ToInt32(recvData, GameHeader.HEADER_SIZE);
            if(debugText)debugText.text = $"DataSize:" + recvData.Length + "\nData:" + data + "\nTCP応答時間:" + timer.ElapsedMilliseconds + "ミリ秒";
            doDebugSend = true;
        }
    }

    void DebugSend(GameHeader.ID _id, byte _code = 0x0000)
    {
        //データ生成
        byte[] sendData = new byte[GameHeader.HEADER_SIZE+sizeof(int)];
        GameHeader header = new GameHeader();
        header.CreateNewData(_id,_code);
        Array.Copy(header.GetHeader(), 0, sendData, 0, header.GetHeader().Length);
        Array.Copy(Convert.ToArrayByte(count++), 0, sendData, header.GetHeader().Length,sizeof(int));


        //送信処理(非同期実行)
        var task = socket.Send(sendData, sendData.Length);
    }

}