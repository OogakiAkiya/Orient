using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace OpenSocket
{

    class ClientState
    {
        public UdpClient socket = null;
        public IPEndPoint endPoint;
        private List<KeyValuePair<IPEndPoint, byte[]>> recvDataList = new List<KeyValuePair<IPEndPoint, byte[]>>();
        private System.Object lockObject = new System.Object();

        ~ClientState()
        {
            if (socket != null)
            {
                socket.Close();
            }
        }

        public KeyValuePair<IPEndPoint, byte[]> GetRecvData()
        {
            KeyValuePair<IPEndPoint, byte[]> returnByte;
            returnByte = recvDataList[0];

            lock (lockObject)
            {
                recvDataList.RemoveAt(0);
            }
            return returnByte;
        }

        public void AddRecvData(IPEndPoint _iPEndPoint, byte[] _data)
        {
            lock (lockObject)
            {
                FileController file = FileController.GetInstance();
                KeyValuePair<IPEndPoint, byte[]> addData = new KeyValuePair<IPEndPoint, byte[]>(_iPEndPoint, new byte[_data.Length]);
                Array.Copy(_data, 0, addData.Value, 0, addData.Value.Length);
                recvDataList.Add(addData);
                //:以降はサーバIPアドレス,サーバポート番号,受信データサイズの順でログを書き出す
                file.Write("udp", "Recv data:" + _iPEndPoint.Address.ToString() + "," +_iPEndPoint.Port + "," + _data.Length, true);


                /*tcpと同じように複数のデータが一つのデータとして渡された時はこちらを使用する必要がありその場合ヘッダの先頭にデータサイズを格納
                 * 高負荷の挙動を確かめていないため暫定的に残す
                int count = 0;
                while (true)
                {
                    int size = System.BitConverter.ToInt32(_data, count);
                    if (_data.Length - count < size) return;
                    KeyValuePair<IPEndPoint, byte[]> addData = new KeyValuePair<IPEndPoint, byte[]>(_iPEndPoint, new byte[size - sizeof(int)]);
                    Array.Copy(_data, (count + sizeof(int)), addData.Value, 0, addData.Value.Length);
                    recvDataList.Add(addData);
                    count += size;
                    if (_data.Length - count < sizeof(int)) return;
                }
                */
            }
        }

        public int GetRecvDataSize()
        {
            int count = 0;
            lock (lockObject)
            {
                count = recvDataList.Count;
            }
            return count;
        }
    }

    class UDP_Client
    {
        
        public ClientState server { get; private set; } = new ClientState();    //serverはデータを待ち受けしつつ(クライアントで使用していないUDPポートを使用)送信にも使用している
        int destPort = 12344;                                                   //送信先ポート
        uint sequence = 0;                                                      //送信先ポート

        public void Init(int _destPort)
        {
            destPort = _destPort;
            server.socket = new UdpClient();

            //UDPサーバ起動
            server.socket.BeginReceive(new AsyncCallback(ReceiveCallback), server);
        }


        public void Send(KeyValuePair<IPEndPoint, byte[]> _data)
        {

            List<byte> sendData = new List<byte>();
            sendData.AddRange(BitConverter.GetBytes(sequence));
            sendData.AddRange(_data.Value);
            server.socket.SendAsync(sendData.ToArray(), sendData.ToArray().Length, _data.Key.Address.ToString(), destPort);

            CountUPSequence();

        }

        public void Send(byte[] _data, string _IP, int _port)
        {
            //送信前にシーケンス番号をパケットの先頭に配置する
            List<byte> sendData = new List<byte>();
            sendData.AddRange(BitConverter.GetBytes(sequence));
            sendData.AddRange(_data);
            //sender.socket.SendAsync(sendData.ToArray(), sendData.ToArray().Length, _IP, _port);
            server.socket.SendAsync(sendData.ToArray(), sendData.ToArray().Length, _IP, _port);
            CountUPSequence();

        }

        private void CountUPSequence()
        {
            sequence++;
            if (sequence > 4200000000)
            {
                sequence = 0;
            }
        }

        //圧縮処理が存在する場合使用する
        private void Decode_ReceiveCallback(IAsyncResult ar)
        {
            ClientState client = (ClientState)ar.AsyncState;
            byte[] decodeData = CompressionWrapper.Decode(client.socket.EndReceive(ar, ref client.endPoint));
            client.AddRecvData(client.endPoint, decodeData);
            client.socket.BeginReceive(Decode_ReceiveCallback, client);
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            FileController file = FileController.GetInstance();
            ClientState client = (ClientState)ar.AsyncState;
           
            //EndReceiveをAddRecvDataと同じタイミングで行うとclient.endPointがnullのままとなるので注意
            byte[] addData = client.socket.EndReceive(ar, ref client.endPoint);
            client.AddRecvData(client.endPoint, addData);
            

            //もう一度recv状態にする
            client.socket.BeginReceive(ReceiveCallback, client);
        }

    }
}