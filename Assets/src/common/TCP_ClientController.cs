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
    [SerializeField] int destPort = 17600;
    TCP_Client socket = new TCP_Client();

    //Debug用(clientからデータを送りserverからデータを受け取る処理)
    [SerializeField] Text debugText = null;                                         //サーバから送ってきたデバッグ用の文字列が入る
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    public bool doDebugSend { private get; set; } = false;                          //trueになるとデバッグ用の処理が走る
    bool hasConnectionError = true;                                                 //trueがサーバへ接続できなかった状態
    int count = 0;                                                                  //サーバに送るテストデータ


    void Start()
    {
        //todo: 最終的に非同期で接続を試行しUpdateの関数を呼び出すタイミングで接続中などの文字を表示する

        Timer timer = new Timer();
        //サーバ接続
        while (true)
        {
            Debug.Log(timer.GetTime_Minutes().ToString());

            //接続テスト実施し成功したら正式に接続
            if (socket.TryConnect(hostname, destPort,5000))
            {
                socket.Init(hostname, destPort);
                break;
            }

            //接続タイムアウト
            if (timer.GetTime_Second() > 10)
            {
                Debug.Log("Server not found");
                return;
            }
        }

        //非同期通信Recv用のスレッド作成
        socket.StartRecvThread();                                     

        //フラグ起動(テスト用)
        doDebugSend = true;

        //正常にサーバへ接続できた状態へフラグ変更
        hasConnectionError = false;
    }

    void Update()
    {
        if (hasConnectionError) return;

        //TCP Clientのデータ更新処理(受信データの参照よりも先に行う必要あり)
        socket.Update();

        if (doDebugSend)
        {
            //タイマーのリセット
            stopwatch.Restart();

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
            if(debugText)debugText.text = $"DataSize:" + recvData.Length + "\nData:" + data + "\nTCP応答時間:" + stopwatch.ElapsedMilliseconds + "ミリ秒";
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